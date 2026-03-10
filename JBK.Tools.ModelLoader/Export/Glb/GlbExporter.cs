using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class GlbExporter : IExporter
{
    private readonly Dictionary<FaceType, IIndexProcessor> _indexProcessors;
    private readonly Dictionary<VertexType, IMeshProcessor> _meshProcessors;
    private readonly GlbExporterOptions _options;

    public GlbExporter(GlbExporterOptions? options = null)
    {
        _options = options ?? new GlbExporterOptions();

        _indexProcessors = new Dictionary<FaceType, IIndexProcessor>
        {
            { FaceType.TriangleList, new ListIndexProcessor() },
            { FaceType.TriangleStrip, new StripIndexProcessor() }
        };

        _meshProcessors = new Dictionary<VertexType, IMeshProcessor>
        {
            { VertexType.Rigid, new RigidMeshProcessor() },
            { VertexType.RigidDouble, new RigidDoubleMeshProcessor() },
            { VertexType.LmStatic, new RigidDoubleMeshProcessor() },
            { VertexType.Blend1, new Blend1MeshProcessor() },
            { VertexType.Blend2, new Blend2MeshProcessor() },
            { VertexType.Blend3, new Blend3MeshProcessor() },
            { VertexType.Blend4, new Blend4MeshProcessor() }
        };
    }

    public void Export(Model sourceFile, string texPath, string outputPath)
    {
        var scene = new SceneBuilder();
        var defaultMaterial = MaterialProcessor.CreateDefaultMaterial();
        NodeBuilder[]? boneNodes = null;
        Matrix4x4[]? inverseBindMatrices = null;
        List<NodeBuilder>? generatedJoints = null;
        (NodeBuilder, Matrix4x4)[]? sharedBoneSkin = null;
        string? syntheticRootName = null;

        var materialBuilders = MaterialProcessor.ProcessMaterials(sourceFile, new MaterialProcessorOptions
        {
            TexturesFolder = texPath,
            EmbedTextures = _options.EmbedTextures,
            WarningHandler = message => Console.Error.WriteLine(message)
        });

        if (sourceFile.bones is { Length: > 0 })
        {
            boneNodes = new NodeBuilder[sourceFile.bones.Length];
            inverseBindMatrices = new Matrix4x4[sourceFile.bones.Length];
            Matrix4x4[] bindWorldMatrices = new Matrix4x4[sourceFile.bones.Length];

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                var inverseBindPose = SanitizeAffineTerms(NormalizeInverseBindPose(sourceFile.bones[i].matrix));
                inverseBindMatrices[i] = inverseBindPose;
                if (!Matrix4x4.Invert(inverseBindPose, out bindWorldMatrices[i]))
                {
                    throw new InvalidOperationException($"Bone {i} has a non-invertible inverse bind pose.");
                }

                boneNodes[i] = new NodeBuilder($"bone_{i}");
            }

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                byte parentIndex = sourceFile.bones[i].parent;
                Matrix4x4 localBindMatrix = bindWorldMatrices[i];

                if (parentIndex != 255 && parentIndex < bindWorldMatrices.Length)
                {
                    if (!Matrix4x4.Invert(bindWorldMatrices[parentIndex], out var inverseParentWorld))
                    {
                        throw new InvalidOperationException($"Bone {i} parent {parentIndex} has a non-invertible bind matrix.");
                    }

                    localBindMatrix = bindWorldMatrices[i] * inverseParentWorld;
                }

                localBindMatrix = SanitizeAffineTerms(localBindMatrix);
                var transposedLocalBindMatrix = SanitizeAffineTerms(Matrix4x4.Transpose(localBindMatrix));

                if (Matrix4x4.Decompose(localBindMatrix, out var scale, out var rotation, out var translation))
                {
                    boneNodes[i].LocalTransform = new AffineTransform(scale, rotation, translation);
                }
                else if (Matrix4x4.Decompose(transposedLocalBindMatrix, out scale, out rotation, out translation))
                {
                    boneNodes[i].LocalTransform = new AffineTransform(scale, rotation, translation);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Bone {i} local bind matrix is not decomposable. local={FormatMatrix(localBindMatrix)} transpose={FormatMatrix(transposedLocalBindMatrix)}");
                }
            }

            var rootBones = new List<NodeBuilder>();
            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                byte parentIndex = sourceFile.bones[i].parent;
                if (parentIndex == 255 || parentIndex >= boneNodes.Length)
                {
                    scene.AddNode(boneNodes[i]);
                    rootBones.Add(boneNodes[i]);
                }
                else
                {
                    boneNodes[parentIndex].AddNode(boneNodes[i]);
                }
            }

            NodeBuilder? syntheticRootJoint = null;
            if (rootBones.Count > 1)
            {
                syntheticRootJoint = new NodeBuilder("bone_root");
                syntheticRootName = syntheticRootJoint.Name;
                scene.AddNode(syntheticRootJoint);

                foreach (var rootBone in rootBones)
                {
                    syntheticRootJoint.AddNode(rootBone);
                }
            }

            if (boneNodes.Length > 0)
            {
                int skinJointCount = boneNodes.Length;
                sharedBoneSkin = new (NodeBuilder, Matrix4x4)[skinJointCount];
                for (int i = 0; i < boneNodes.Length; i++)
                {
                    sharedBoneSkin[i] = (boneNodes[i], inverseBindMatrices[i]);
                }
            }
        }

        foreach (var mesh in sourceFile.meshes)
        {
            var indexProcessor = _indexProcessors[(FaceType)mesh.Header.face_type];
            var meshProcessor = _meshProcessors[(VertexType)mesh.Header.vertex_type];

            MaterialBuilder matForMesh = defaultMaterial;
            if (mesh.Header.material_ref >= 0 && materialBuilders.TryGetValue(mesh.Header.material_ref, out var mb))
            {
                matForMesh = mb;
            }

            var meshBuilder = meshProcessor.Process(indexProcessor, matForMesh, mesh);
            (NodeBuilder, Matrix4x4)[]? skin = null;

            if (sharedBoneSkin != null)
            {
                skin = sharedBoneSkin;
            }
            else
            {
                bool isSkinnedMesh =
                    mesh.Header.vertex_type == (byte)VertexType.Blend1 ||
                    mesh.Header.vertex_type == (byte)VertexType.Blend2 ||
                    mesh.Header.vertex_type == (byte)VertexType.Blend3 ||
                    mesh.Header.vertex_type == (byte)VertexType.Blend4;

                if (isSkinnedMesh && mesh.BoneIndices != null && mesh.BoneIndices.Length > 0)
                {
                    generatedJoints ??= new List<NodeBuilder>();

                    int maxGlobalBoneId = 0;
                    foreach (var b in mesh.BoneIndices)
                    {
                        if (b > maxGlobalBoneId)
                        {
                            maxGlobalBoneId = b;
                        }
                    }

                    while (generatedJoints.Count <= maxGlobalBoneId)
                    {
                        int nextIndex = generatedJoints.Count;
                        NodeBuilder next;
                        if (nextIndex == 0)
                        {
                            next = new NodeBuilder("joint_000");
                            scene.AddNode(next);
                        }
                        else
                        {
                            next = generatedJoints[0].CreateNode($"joint_{nextIndex:D3}");
                        }

                        generatedJoints.Add(next);
                    }

                    skin = new (NodeBuilder, Matrix4x4)[maxGlobalBoneId + 1];
                    for (int i = 0; i <= maxGlobalBoneId; i++)
                    {
                        skin[i] = (generatedJoints[i], Matrix4x4.Identity);
                    }
                }
            }

            if (skin != null)
            {
                var joints = skin.Select(static s => s.Item1).ToArray();
                if (joints.Length == 0 || !NodeBuilder.IsValidArmature(joints))
                {
                    if (_options.ExportDiagnostics)
                    {
                        Console.Error.WriteLine(
                            $"[GLB.SKIN][WARN] Invalid armature for mesh '{mesh.GetDisplayName()}' ({joints.Length} joints). Falling back to rigid mesh export.");
                    }

                    skin = null;
                }
            }

            meshProcessor.AddToScene(scene, meshBuilder, skin);
        }

        var model = scene.ToGltf2();

        AssignMeshNodeNames(model);
        CanonicalizeEquivalentSkins(model);

        foreach (var skin in model.LogicalSkins)
        {
            if (!string.IsNullOrWhiteSpace(syntheticRootName))
            {
                var explicitRoot = model.LogicalNodes.FirstOrDefault(n => n.Name == syntheticRootName);
                if (explicitRoot != null)
                {
                    skin.Skeleton = explicitRoot;
                    continue;
                }
            }

            if (skin.Skeleton == null && skin.JointsCount > 0)
            {
                var rootJoint = skin.Joints[0];
                while (rootJoint.VisualParent != null && skin.Joints.Contains(rootJoint.VisualParent))
                {
                    rootJoint = rootJoint.VisualParent;
                }

                skin.Skeleton = rootJoint;
            }
        }

        if (sourceFile.Animations.Any() && boneNodes != null)
        {
            for (int clipIndex = 0; clipIndex < sourceFile.Animations.Length; clipIndex++)
            {
                string animName = string.IsNullOrWhiteSpace(sourceFile.Animations[clipIndex].Name)
                    ? $"animation_{clipIndex}"
                    : sourceFile.Animations[clipIndex].Name;
                var animation = model.CreateAnimation(animName);

                for (int boneIndex = 0; boneIndex < boneNodes.Length; boneIndex++)
                {
                    var targetNodeBuilder = boneNodes[boneIndex];
                    var targetNode = model.LogicalNodes.FirstOrDefault(n => n.Name == targetNodeBuilder.Name);
                    if (targetNode == null)
                    {
                        continue;
                    }

                    var translationKeys = new Dictionary<float, Vector3>();
                    var rotationKeys = new Dictionary<float, Quaternion>();
                    var scaleKeys = new Dictionary<float, Vector3>();

                    for (int keyIndex = 0; keyIndex < sourceFile.Animations[clipIndex].Header.keyframe_count; keyIndex++)
                    {
                        float time = sourceFile.Animations[clipIndex].Keyframes[keyIndex].time / 1000.0f;
                        if (!float.IsFinite(time))
                        {
                            continue;
                        }

                        ushort transformIndex = sourceFile.Animations[clipIndex].BoneTransformIndices[keyIndex, boneIndex];
                        if (transformIndex >= sourceFile.AllAnimationTransforms.Length)
                        {
                            continue;
                        }

                        var transform = sourceFile.AllAnimationTransforms[transformIndex];
                        if (!IsFinite(transform.pos) || !IsFinite(transform.quat) || !IsFinite(transform.scale))
                        {
                            continue;
                        }

                        translationKeys[time] = transform.pos;
                        rotationKeys[time] = NormalizeOrIdentity(transform.quat);
                        scaleKeys[time] = NormalizeScaleOrOne(transform.scale);
                    }

                    if (translationKeys.Count > 1)
                    {
                        animation.CreateTranslationChannel(targetNode, SortKeys(translationKeys), true);
                    }

                    if (rotationKeys.Count > 1)
                    {
                        animation.CreateRotationChannel(targetNode, SortKeys(rotationKeys), true);
                    }

                    if (scaleKeys.Count > 1)
                    {
                        animation.CreateScaleChannel(targetNode, SortKeys(scaleKeys), true);
                    }
                }
            }
        }

        model.SaveGLB(outputPath);
        GlbConformanceDiagnostics.ValidateAndReport(outputPath, _options.ExportDiagnostics);
    }

    private static void AssignMeshNodeNames(ModelRoot model)
    {
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in model.LogicalNodes)
        {
            if (!string.IsNullOrWhiteSpace(node.Name))
            {
                usedNames.Add(node.Name);
            }
        }

        for (int i = 0; i < model.LogicalNodes.Count; i++)
        {
            var node = model.LogicalNodes[i];
            if (node.Mesh == null || !string.IsNullOrWhiteSpace(node.Name))
            {
                continue;
            }

            var baseName = string.IsNullOrWhiteSpace(node.Mesh.Name) ? "MeshNode" : node.Mesh.Name;
            var candidate = baseName;
            int suffix = 1;
            while (!usedNames.Add(candidate))
            {
                candidate = $"{baseName}_{suffix++}";
            }

            node.Name = candidate;
        }
    }

    private static void CanonicalizeEquivalentSkins(ModelRoot model)
    {
        if (model.LogicalSkins.Count <= 1)
        {
            return;
        }

        var canonical = model.LogicalSkins[0];
        for (int i = 0; i < model.LogicalNodes.Count; i++)
        {
            var node = model.LogicalNodes[i];
            if (node.Skin == null || ReferenceEquals(node.Skin, canonical))
            {
                continue;
            }

            if (AreEquivalentSkins(canonical, node.Skin))
            {
                node.Skin = canonical;
            }
        }
    }

    private static bool AreEquivalentSkins(Skin a, Skin b)
    {
        if (a.JointsCount != b.JointsCount)
        {
            return false;
        }

        for (int i = 0; i < a.JointsCount; i++)
        {
            if (!ReferenceEquals(a.Joints[i], b.Joints[i]))
            {
                return false;
            }
        }

        if (!ReferenceEquals(a.Skeleton, b.Skeleton))
        {
            return false;
        }

        return true;
    }


    private static Quaternion NormalizeOrIdentity(Quaternion value)
    {
        float lenSq = value.LengthSquared();
        if (!float.IsFinite(lenSq) || lenSq < 1e-8f)
        {
            return Quaternion.Identity;
        }

        return Quaternion.Normalize(value);
    }

    private static Vector3 NormalizeScaleOrOne(Vector3 value)
    {
        if (!IsFinite(value))
        {
            return Vector3.One;
        }

        if (MathF.Abs(value.X) < 1e-8f && MathF.Abs(value.Y) < 1e-8f && MathF.Abs(value.Z) < 1e-8f)
        {
            return Vector3.One;
        }

        return value;
    }

    private static SortedDictionary<float, TValue> SortKeys<TValue>(Dictionary<float, TValue> source)
    {
        var sorted = new SortedDictionary<float, TValue>();
        foreach (var keyValue in source)
        {
            sorted[keyValue.Key] = keyValue.Value;
        }

        return sorted;
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }

    private static bool IsFinite(Quaternion value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z) && float.IsFinite(value.W);
    }

    private static Matrix4x4 NormalizeInverseBindPose(Matrix4x4 value)
    {
        if (IsAffineMatrix(value))
        {
            return value;
        }

        var transposed = Matrix4x4.Transpose(value);
        return IsAffineMatrix(transposed) ? transposed : value;
    }

    private static bool IsAffineMatrix(Matrix4x4 value)
    {
        const float epsilon = 1e-4f;
        return float.IsFinite(value.M11)
            && float.IsFinite(value.M12)
            && float.IsFinite(value.M13)
            && float.IsFinite(value.M14)
            && float.IsFinite(value.M21)
            && float.IsFinite(value.M22)
            && float.IsFinite(value.M23)
            && float.IsFinite(value.M24)
            && float.IsFinite(value.M31)
            && float.IsFinite(value.M32)
            && float.IsFinite(value.M33)
            && float.IsFinite(value.M34)
            && float.IsFinite(value.M41)
            && float.IsFinite(value.M42)
            && float.IsFinite(value.M43)
            && float.IsFinite(value.M44)
            && MathF.Abs(value.M14) <= epsilon
            && MathF.Abs(value.M24) <= epsilon
            && MathF.Abs(value.M34) <= epsilon
            && MathF.Abs(value.M44 - 1.0f) <= epsilon;
    }

    private static string FormatMatrix(Matrix4x4 value)
    {
        return $"[{value.M11}, {value.M12}, {value.M13}, {value.M14}; {value.M21}, {value.M22}, {value.M23}, {value.M24}; {value.M31}, {value.M32}, {value.M33}, {value.M34}; {value.M41}, {value.M42}, {value.M43}, {value.M44}]";
    }

    private static Matrix4x4 SanitizeAffineTerms(Matrix4x4 value)
    {
        const float epsilon = 1e-4f;

        if (MathF.Abs(value.M14) <= epsilon)
        {
            value.M14 = 0.0f;
        }

        if (MathF.Abs(value.M24) <= epsilon)
        {
            value.M24 = 0.0f;
        }

        if (MathF.Abs(value.M34) <= epsilon)
        {
            value.M34 = 0.0f;
        }

        if (MathF.Abs(value.M44 - 1.0f) <= epsilon)
        {
            value.M44 = 1.0f;
        }

        return value;
    }
}
