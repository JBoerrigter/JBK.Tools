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
        var defaultMaterial = MaterialProcessor.CreateDefaultMaterial();
        NodeBuilder[]? boneNodes = null;
        Matrix4x4[]? inverseBindMatrices = null;

        var materialBuilders = MaterialProcessor.ProcessMaterials(sourceFile, texPath);

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

    
}
