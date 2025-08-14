using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Transforms;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class GlbExporter : IExporter
{
    Dictionary<FaceType, IIndexProcessor> _IndexProcessors;
    Dictionary<VertexType, IMeshProcessor> _MeshProcessors;

    public GlbExporter()
    {
        _IndexProcessors = new Dictionary<FaceType, IIndexProcessor>
        {
            { FaceType.TriangleList, new ListIndexProcessor() },
            { FaceType.TriangleStrip, new StripIndexProcessor() }
        };

        _MeshProcessors = new Dictionary<VertexType, IMeshProcessor>
        {
            { VertexType.Rigid, new RigidMeshProcessor() },
            { VertexType.RigidDouble, new RigidDoubleMeshProcessor() },
            { VertexType.Blend1, new Blend1MeshProcessor() },
            { VertexType.Blend2, new Blend2MeshProcessor() },
            { VertexType.Blend3, new Blend3MeshProcessor() },
            { VertexType.Blend4, new Blend4MeshProcessor() }
        };
    }

    public void Export(Model sourceFile, string texPath, string outputPath)
    {
        var scene = new SceneBuilder();
        var defaultMaterial = CreateDefaultMaterial();
        NodeBuilder[]? boneNodes = null;
        Matrix4x4[]? inverseBindMatrices = null;

        var materialBuilders = BuildMaterials(sourceFile, texPath);


        // Bones
        if (sourceFile.header.BoneCount > 0)
        {
            var armature = new NodeBuilder("Armature");
            scene.AddNode(armature);

            boneNodes = new NodeBuilder[sourceFile.bones.Length];
            inverseBindMatrices = new Matrix4x4[sourceFile.bones.Length];

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                var columnMajorInverseBindPose = sourceFile.bones[i].matrix;
                inverseBindMatrices[i] = columnMajorInverseBindPose;
                Matrix4x4.Invert(columnMajorInverseBindPose, out var columnMajorLocalTransform);
                var node = new NodeBuilder($"bone_{i}");
                if (Matrix4x4.Decompose(columnMajorLocalTransform, out var scale, out var rotation, out var translation))
                {
                    node.LocalTransform = new AffineTransform(translation, rotation, scale);
                }
                else
                {
                    node.LocalMatrix = columnMajorLocalTransform;
                }
                boneNodes[i] = node;
            }

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                byte parentIndex = sourceFile.bones[i].parent;
                if (parentIndex == 255 || parentIndex >= boneNodes.Length)
                {
                    armature.AddNode(boneNodes[i]);
                }
                else
                {
                    boneNodes[parentIndex].AddNode(boneNodes[i]);
                }
            }

            const float boneTipLength = 0.1f;
            foreach (var boneNode in boneNodes)
            {
                if (!boneNode.VisualChildren.Any())
                {
                    var tipNode = new NodeBuilder($"{boneNode.Name}_tip");
                    tipNode.LocalTransform = new AffineTransform(Quaternion.Identity, Vector3.UnitY * boneTipLength);

                    boneNode.AddNode(tipNode);
                }
            }
        }

        // Meshes
        foreach (var mesh in sourceFile.meshes)
        {
            var indexProcessor = _IndexProcessors[(FaceType)mesh.Header.face_type];
            var meshProcessor = _MeshProcessors[(VertexType)mesh.Header.vertex_type];

            MaterialBuilder matForMesh = defaultMaterial;
            if (mesh.Header.material_ref >= 0 && materialBuilders.TryGetValue(mesh.Header.material_ref, out var mb))
            {
                matForMesh = mb;
            }

            var meshBuilder = meshProcessor.Process(indexProcessor, matForMesh, mesh);

            (NodeBuilder, Matrix4x4)[]? skin = null;

            if (boneNodes != null && boneNodes.Any())
            {
                skin = new (NodeBuilder, Matrix4x4)[boneNodes.Length];
                for (int i = 0; i < boneNodes.Length; i++)
                    skin[i] = (boneNodes[i], inverseBindMatrices[i]);
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
                    // Reuse one dummy armature across meshes in the file
                    var dummyArmature = new NodeBuilder("Armature_Dummy");
                    scene.AddNode(dummyArmature);

                    int maxGlobalBoneId = 0;
                    foreach (var b in mesh.BoneIndices) if (b > maxGlobalBoneId) maxGlobalBoneId = b;

                    // ensure we have nodes up to maxGlobalBoneId
                    var dummyJoints = new List<NodeBuilder>();
                    while (dummyJoints.Count <= maxGlobalBoneId)
                    {
                        var next = dummyArmature.CreateNode($"joint_{dummyJoints.Count:D3}");
                        dummyJoints.Add(next);
                    }

                    // build the skin tuple array up to max used id
                    skin = new (NodeBuilder, Matrix4x4)[maxGlobalBoneId + 1];
                    for (int i = 0; i <= maxGlobalBoneId; i++)
                        skin[i] = (dummyJoints[i], Matrix4x4.Identity);
                }
            }

            meshProcessor.AddToScene(scene, meshBuilder, skin);
        }

        var model = scene.ToGltf2();

        // Animations
        if (sourceFile.Animations.Any() && boneNodes != null)
        {
            for (int i = 0; i < sourceFile.Animations.Length; i++)
            {
                string animName = string.IsNullOrWhiteSpace(sourceFile.Animations[i].Name) ? $"animation_{i}" : sourceFile.Animations[i].Name;
                var animation = model.CreateAnimation(animName);

                for (int boneIndex = 0; boneIndex < sourceFile.header.BoneCount; boneIndex++)
                {
                    var targetNodeBuilder = boneNodes[boneIndex];
                    var targetNode = model.LogicalNodes.FirstOrDefault(n => n.Name == targetNodeBuilder.Name);
                    if (targetNode == null) continue;

                    var translationKeys = new Dictionary<float, Vector3>();
                    var rotationKeys = new Dictionary<float, Quaternion>();
                    var scaleKeys = new Dictionary<float, Vector3>();

                    for (int keyIndex = 0; keyIndex < sourceFile.Animations[i].Header.keyframe_count; keyIndex++)
                    {
                        float time = sourceFile.Animations[i].Keyframes[keyIndex].time / 1000.0f;
                        ushort transformIndex = sourceFile.Animations[i].BoneTransformIndices[keyIndex, boneIndex];

                        if (transformIndex < sourceFile.AllAnimationTransforms.Length)
                        {
                            var transform = sourceFile.AllAnimationTransforms[transformIndex];
                            translationKeys[time] = transform.pos;
                            rotationKeys[time] = transform.quat;
                            scaleKeys[time] = transform.scale.LengthSquared() > 0 ? transform.scale : Vector3.One;
                        }
                    }

                    if (translationKeys.Count > 1)
                    {
                        animation.CreateTranslationChannel(targetNode, translationKeys, true);
                    }
                    if (rotationKeys.Count > 1)
                    {
                        animation.CreateRotationChannel(targetNode, rotationKeys, true);
                    }
                    if (scaleKeys.Count > 1)
                    {
                        animation.CreateScaleChannel(targetNode, scaleKeys, true);
                    }
                }
            }
        }


        model.SaveGLB(outputPath);
    }

    private static MaterialBuilder CreateDefaultMaterial()
    {
        var material = MaterialBuilder.CreateDefault();
        material.WithMetallicRoughnessShader()
               .WithBaseColor(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        return material;
    }

    private static Vector4 ArgbToVector4(uint argb, float opacityOverride = -1f)
    {
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);

        float fa = a / 255f;
        float fr = r / 255f;
        float fg = g / 255f;
        float fb = b / 255f;

        if (opacityOverride >= 0f) fa = opacityOverride;

        return new Vector4(fr, fg, fb, fa);
    }

    private static string[] SplitNullSeparatedStrings(string s)
    {
        if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
        return s.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string ResolveTexturePath(string textureName, string textureFolder)
    {
        if (string.IsNullOrEmpty(textureName)) return null;
        if (File.Exists(textureName)) return textureName;

        if (!string.IsNullOrEmpty(textureFolder))
        {
            var candidate = Path.Combine(textureFolder, textureName);
            if (File.Exists(candidate)) return candidate;
        }

        var candidate2 = Path.Combine(AppContext.BaseDirectory, textureName);
        if (File.Exists(candidate2)) return candidate2;

        return string.Empty;
    }

    private static Dictionary<int, MaterialBuilder> BuildMaterials(Model sourceFile, string texturesFolder = null)
    {
        var result = new Dictionary<int, MaterialBuilder>();
        var builders = new Dictionary<uint, MaterialBuilder>();

        if (sourceFile.materialData == null) return result;

        for (int i = 0; i < sourceFile.materialData.Length; i++)
        {
            string texPacked = string.Empty;
            var mk = sourceFile.materialData[i];

            if (mk.szTexture != 0 && builders.ContainsKey(mk.szTexture))
            {
                result.Add(i, builders[mk.szTexture]);
                continue; // already processed this texture
            }

            // default name
            var matName = $"mat_{i}";

            if (mk.szTexture != 0)
            {
                texPacked = sourceFile.GetString(mk.szTexture);
                matName = Path.GetFileNameWithoutExtension(texPacked);
            }

            MaterialBuilder mb = new MaterialBuilder(matName).WithDoubleSide(true).WithMetallicRoughnessShader();
            builders.Add(mk.szTexture, mb);

            if (sourceFile.materialFrames != null && mk.m_frame < sourceFile.materialFrames.Length)
            {
                var mf = sourceFile.materialFrames[mk.m_frame];
                var baseColor = ArgbToVector4(mf.m_diffuse, mf.m_opacity);
                mb = mb.WithChannelParam(KnownChannel.BaseColor, baseColor);
            }

            // read textures (szTexture is an offset into the string table)
            if (mk.szTexture != 0)
            {
                var parts = SplitNullSeparatedStrings(texPacked);

                if (parts.Length >= 1)
                {
                    var baseName = parts[0];
                    var path = ResolveTexturePath(baseName, texturesFolder);

                    var tmpPath = ResolveTexturePath(System.IO.Path.ChangeExtension(baseName, ".png"), texturesFolder);
                    if (File.Exists(tmpPath))
                    {
                        path = tmpPath;
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        // base color / albedo texture
                        mb = mb.WithChannelImage(KnownChannel.BaseColor, path);
                    }
                }

                if (parts.Length >= 2)
                {
                    var secondName = parts[1];
                    var path2 = ResolveTexturePath(secondName, texturesFolder);
                    if (!string.IsNullOrEmpty(path2))
                    {
                        mb = mb.WithChannelImage(KnownChannel.Occlusion, path2);
                    }
                }
            }

            result.Add(i, mb);
        }

        return result;
    }
}
