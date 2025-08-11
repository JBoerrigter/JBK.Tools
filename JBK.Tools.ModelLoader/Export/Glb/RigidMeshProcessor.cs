using JBK.Tools.ModelFileFormat;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class RigidMeshProcessor : IMeshProcessor
{
    public IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, Mesh mesh)
    {
        var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>($"Mesh_{mesh.Header.name}");
        var primitive = meshBuilder.UsePrimitive(material);

        var vertices = GetVertexBuilders(mesh.Vertecies.OfType<VertexRigid>().ToArray());
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

    private IVertexBuilder[] GetVertexBuilders(VertexRigid[] vertices)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>(
                new VertexPositionNormal(vertices[i].Position, vertices[i].Normal),
                new VertexTexture1(vertices[i].TexCoord),
                new VertexEmpty());
        }
        return vertexBuilders;
    }
}
