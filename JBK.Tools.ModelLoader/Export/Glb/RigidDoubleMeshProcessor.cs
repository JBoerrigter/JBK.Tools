using JBK.Tools.ModelFileFormat;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class RigidDoubleMeshProcessor : IMeshProcessor
{
    public IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, Mesh mesh)
    {
        var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture2, VertexEmpty>($"Mesh_{mesh.Header.name}");
        var primitive = meshBuilder.UsePrimitive(material);

        var vertices = GetVertexBuilders(mesh.Vertecies.OfType<VertexRigidDouble>().ToArray());
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
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
    }

    public IVertexBuilder[] GetVertexBuilders(VertexRigidDouble[] vertices)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            var uv0 = vertices[i].TexCoord0;
            var uv1 = vertices[i].TexCoord1;
            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture2, VertexEmpty>(
                new VertexPositionNormal(vertices[i].Position, vertices[i].Normal),
                new VertexTexture2(new Vector2(uv0.X, uv0.Y), new Vector2(uv1.X, uv1.Y)),
                new VertexEmpty());
        }
        return vertexBuilders;
    }
}
