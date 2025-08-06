using JBK.Tools.ModelFileFormat;
using JBK.Tools.ModelLoader.Enums;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Transforms;
using System.Numerics;

namespace JBK.Tools.ModelLoader;

public class GlbExporter
{
    public static void ExportToGlb(ModelFileFormat.ModelFileFormat sourceFile, string outputPath)
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
            var vertices = ProcessVertices(mesh);
            var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>($"Mesh_{mesh.Header.name}");

            var primitive = meshBuilder.UsePrimitive(defaultMaterial);

            switch (mesh.Header.face_type)
            {
                case 0: // FT_LIST (triangle list)
                    ProcessTriangleList(primitive, vertices, mesh.Indices);
                    break;

                case 1: // FT_STRIP (triangle strip)
                    ProcessTriangleStrip(primitive, vertices, mesh.Indices);
                    break;

                default:
                    throw new NotSupportedException($"Face type {mesh.Header.face_type} is not supported");
            }

            if (mesh.Header.vertex_type > 0 && mesh.Header.vertex_type < 5 && boneNodes != null)
            {
                var skin = new (NodeBuilder, Matrix4x4)[boneNodes.Length];
                for (int i = 0; i < boneNodes.Length; i++)
                {
                    skin[i] = (boneNodes[i], inverseBindMatrices[i]);
                }
                scene.AddSkinnedMesh(meshBuilder, skin);
            }
            else
            {
                scene.AddRigidMesh(meshBuilder, Matrix4x4.Identity);
            }

        }

        var model = scene.ToGltf2();

        if (sourceFile.Animations.Any() && boneNodes != null)
        {
            foreach (var animData in sourceFile.Animations)
            {
                string animName = string.IsNullOrWhiteSpace(animData.Name) ? $"animation_{animData.Header.szoption}" : animData.Name;

                var animation = model.CreateAnimation(animName);

                // For each bone, create its animation channels
                for (int boneIndex = 0; boneIndex < sourceFile.header.bone_count; boneIndex++)
                {
                    // STEP 3: Find the final Node object that corresponds to our NodeBuilder blueprint.
                    // We use the name we assigned to the NodeBuilder during skeleton creation.
                    var targetNodeBuilder = boneNodes[boneIndex];
                    var targetNode = model.LogicalNodes.FirstOrDefault(n => n.Name == targetNodeBuilder.Name);

                    // If we can't find the target bone node, skip to the next one.
                    if (targetNode == null) continue;

                    var translationKeys = new Dictionary<float, Vector3>();
                    var rotationKeys = new Dictionary<float, Quaternion>();
                    var scaleKeys = new Dictionary<float, Vector3>();

                    // This logic for gathering keyframes remains the same.
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

                    // STEP 4: Use the extension methods on our Animation object to create the channels.
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

    private static void ProcessTriangleList(PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexJoints4> primitive,
        List<VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>> vertices, ushort[] indices)
    {
        // Triangle list: every 3 indices form a triangle
        for (int i = 0; i < indices.Length; i += 3)
        {
            if (i + 2 >= indices.Length) break;

            primitive.AddTriangle(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            );
        }
    }

    private static void ProcessTriangleStrip(PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexTexture1, VertexJoints4> primitive,
        List<VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>> vertices, ushort[] indices)
    {
        // Triangle strip: each new index forms a triangle with previous two indices
        // Note: This assumes counter-clockwise winding order
        for (int i = 2; i < indices.Length; i++)
        {
            if (i % 2 == 0) // Even-numbered triangles
            {
                primitive.AddTriangle(
                    vertices[indices[i - 2]],
                    vertices[indices[i - 1]],
                    vertices[indices[i]]
                );
            }
            else // Odd-numbered triangles (order reversed to maintain winding)
            {
                primitive.AddTriangle(
                    vertices[indices[i - 1]],
                    vertices[indices[i - 2]],
                    vertices[indices[i]]
                );
            }
        }
    }

    private static List<VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>> ProcessVertices(Mesh mesh)
    {
        var result = new List<VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>>();

        // Get the bone palette for this specific mesh
        var palette = mesh.BoneIndices;
        var vertexType = (VertexType)mesh.Header.vertex_type;
        var vertices = mesh.Vertecies;

        switch (vertexType)
        {
            case VertexType.Rigid:
            case VertexType.RigidDouble:
                foreach (VertexRigid v in vertices.OfType<VertexRigid>())
                {
                    result.Add(new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                        new VertexPositionNormal(v.Position, v.Normal),
                        new VertexTexture1(v.TexCoord),
                        new VertexJoints4()
                    ));
                }
                VertexRigid rigid = (VertexRigid)vertices[0];
                break;
            case VertexType.Blend1:
                foreach (VertexBlend1 v in vertices.OfType<VertexBlend1>())
                {
                    var idx0 = (int)v.BoneIndices & 0xFF;
                    result.Add(new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                        new VertexPositionNormal(v.Position, v.Normal),
                        new VertexTexture1(v.TexCoord),
                        new VertexJoints4((palette[idx0], 1f))));
                }
                break;
            case VertexType.Blend2:
                foreach (VertexBlend2 v in vertices.OfType<VertexBlend2>())
                {
                    var idx0 = (int)v.BoneIndices & 0xFF;
                    var idx1 = (int)(v.BoneIndices >> 8) & 0xFF;
                    result.Add(new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                        new VertexPositionNormal(v.Position, v.Normal),
                        new VertexTexture1(v.TexCoord),
                        new VertexJoints4(
                            (palette[idx0], v.BlendWeight0),
                            (palette[idx1], 1f - v.BlendWeight0)
                        )));
                }
                break;
            case VertexType.Blend3:
                foreach (VertexBlend3 v in vertices.OfType<VertexBlend3>())
                {
                    var idx0 = (int)v.BoneIndices & 0xFF;
                    var idx1 = (int)(v.BoneIndices >> 8) & 0xFF;
                    var idx2 = (int)(v.BoneIndices >> 16) & 0xFF;
                    result.Add(new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                        new VertexPositionNormal(v.Position, v.Normal),
                        new VertexTexture1(v.TexCoord),
                        new VertexJoints4(
                            (palette[idx0], v.BlendWeight0),
                            (palette[idx1], v.BlendWeight1),
                            (palette[idx2], v.GetBlendWeight2())
                        )));
                }
                break;
            case VertexType.Blend4:
                foreach (VertexBlend4 v in vertices.OfType<VertexBlend4>())
                {
                    var idx0 = (int)v.BoneIndices & 0xFF;
                    var idx1 = (int)(v.BoneIndices >> 8) & 0xFF;
                    var idx2 = (int)(v.BoneIndices >> 16) & 0xFF;
                    var idx3 = (int)(v.BoneIndices >> 24) & 0xFF;
                    result.Add(new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                        new VertexPositionNormal(v.Position, v.Normal),
                        new VertexTexture1(v.TexCoord),
                        new VertexJoints4(
                            (palette[idx0], v.BlendWeight0),
                            (palette[idx1], v.BlendWeight1),
                            (palette[idx2], v.BlendWeight2),
                            (palette[idx3], v.GetBlendWeight3())
                        )));
                }
                break;
            default:
                throw new NotSupportedException($"Vertex type {vertexType} is not supported");
        }

        return result;
    }

    private static MaterialBuilder CreateDefaultMaterial()
    {
        var material = MaterialBuilder.CreateDefault();
        material.WithMetallicRoughnessShader()
               .WithBaseColor(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        return material;
    }
}
