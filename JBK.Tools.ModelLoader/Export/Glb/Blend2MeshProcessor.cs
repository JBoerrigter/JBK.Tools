using JBK.Tools.ModelLoader.GbFormat.Meshes;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class Blend2MeshProcessor : IMeshProcessor
{
    public IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, Mesh mesh)
    {
        var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>($"Mesh_{mesh.Header.name}");
        var primitive = meshBuilder.UsePrimitive(material);

        var vertices = GetVertexBuilders(mesh.BoneIndices, mesh.Vertecies.OfType<VertexBlend2>().ToArray());
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
        scene.AddSkinnedMesh(mesh, skin);
    }

    public IVertexBuilder[] GetVertexBuilders(byte[] palette, VertexBlend2[] vertices)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            var idx0 = (int)vertices[i].BoneIndices & 0xFF;
            var idx1 = (int)(vertices[i].BoneIndices >> 8) & 0xFF;
            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(
                new VertexPositionNormal(vertices[i].Position, vertices[i].Normal),
                new VertexTexture1(vertices[i].TexCoord),
                new VertexJoints4(
                    (palette[idx0], vertices[i].BlendWeight0),
                    (palette[idx1], 1f - vertices[i].BlendWeight0)
                ));
        }
        return vertexBuilders;
    }
}
