using JBK.Tools.ModelLoader.Enums;
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

    public void Export(ModelFileFormat.ModelFileFormat sourceFile, string outputPath)
    {
        var scene = new SceneBuilder();
        var defaultMaterial = CreateDefaultMaterial();
        NodeBuilder[]? boneNodes = null;
        Matrix4x4[]? inverseBindMatrices = null;

        // Bones
        if (sourceFile.header.bone_count > 0)
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
            var meshBuilder = meshProcessor.Process(indexProcessor, defaultMaterial, mesh);

            (NodeBuilder, Matrix4x4)[]? skin = null;
            if (boneNodes != null)
            {
                skin = new (NodeBuilder, Matrix4x4)[boneNodes.Length];
                for (int i = 0; i < boneNodes.Length; i++)
                {
                    skin[i] = (boneNodes[i], inverseBindMatrices[i]);
                }
            }

            meshProcessor.AddToScene(scene, meshBuilder, skin);
        }

        var model = scene.ToGltf2();

        // Animations
        if (sourceFile.Animations.Any() && boneNodes != null)
        {
            foreach (var animData in sourceFile.Animations)
            {
                string animName = string.IsNullOrWhiteSpace(animData.Name) ? $"animation_{animData.Header.szoption}" : animData.Name;
                var animation = model.CreateAnimation(animName);

                // For each bone, create its animation channels
                for (int boneIndex = 0; boneIndex < sourceFile.header.bone_count; boneIndex++)
                {
                    var targetNodeBuilder = boneNodes[boneIndex];
                    var targetNode = model.LogicalNodes.FirstOrDefault(n => n.Name == targetNodeBuilder.Name);
                    if (targetNode == null) continue;

                    var translationKeys = new Dictionary<float, Vector3>();
                    var rotationKeys = new Dictionary<float, Quaternion>();
                    var scaleKeys = new Dictionary<float, Vector3>();

                    for (int keyIndex = 0; keyIndex < animData.Header.keyframe_count; keyIndex++)
                    {
                        float time = animData.Keyframes[keyIndex].time / 1000.0f;
                        ushort transformIndex = animData.BoneTransformIndices[keyIndex, boneIndex];

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
}
