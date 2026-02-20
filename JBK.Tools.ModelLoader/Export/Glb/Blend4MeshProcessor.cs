using JBK.Tools.ModelLoader.GbFormat.Meshes;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class Blend4MeshProcessor : IMeshProcessor
{
    public IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, Mesh mesh)
    {
        var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>($"Mesh_{mesh.Header.name}");
        var primitive = meshBuilder.UsePrimitive(material);

        var vertices = GetVertexBuilders(mesh.BoneIndices, mesh.Vertecies.OfType<VertexBlend4>().ToArray());
        var indices = indexProcessor.Process(mesh.Indices);
        for (int i = 0; i < indices.Length - 2; i += 3)
        {
            primitive.AddTriangle(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]);
        }

        return meshBuilder;
    }

    public void AddToScene(SceneBuilder scene, IMeshBuilder<MaterialBuilder> mesh, (NodeBuilder, Matrix4x4)[]? skin)
    {
        if (skin != null && skin.Length > 0)
        {
            scene.AddSkinnedMesh(mesh, skin);
            return;
        }

        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
    }

    public IVertexBuilder[] GetVertexBuilders(byte[] palette, VertexBlend4[] vertices)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            var idx0 = (int)vertices[i].BoneIndices & 0xFF;
            var idx1 = (int)(vertices[i].BoneIndices >> 8) & 0xFF;
            var idx2 = (int)(vertices[i].BoneIndices >> 16) & 0xFF;
            var idx3 = (int)(vertices[i].BoneIndices >> 24) & 0xFF;
            int joint0 = SkinWeightSanitizer.MapPaletteBone(palette, idx0);
            int joint1 = SkinWeightSanitizer.MapPaletteBone(palette, idx1);
            int joint2 = SkinWeightSanitizer.MapPaletteBone(palette, idx2);
            int joint3 = SkinWeightSanitizer.MapPaletteBone(palette, idx3);
            var bindings = SkinWeightSanitizer.Normalize(
                (joint0, vertices[i].BlendWeight0),
                (joint1, vertices[i].BlendWeight1),
                (joint2, vertices[i].BlendWeight2),
                (joint3, vertices[i].GetBlendWeight3()));

            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                new VertexPositionNormal(vertices[i].Position, vertices[i].Normal),
                new VertexTexture1(vertices[i].TexCoord),
                new VertexJoints4(bindings));
        }

        return vertexBuilders;
    }
}
