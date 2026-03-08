using JBK.Tools.ModelLoader.GbFormat.Meshes;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class RigidDoubleMeshProcessor : IMeshProcessor
{
    private int _primaryBoneIndex = -1;
    private bool _usesSkinning;

    public IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, Mesh mesh)
    {
        _primaryBoneIndex = (mesh.BoneIndices != null && mesh.BoneIndices.Length > 0)
            ? mesh.BoneIndices[0]
            : -1;
        _usesSkinning = _primaryBoneIndex >= 0;

        if (_usesSkinning)
        {
            var meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture2, VertexJoints4>(mesh.GetBuilderName());
            var primitive = meshBuilder.UsePrimitive(material);

            int jointIndex = _primaryBoneIndex;
            var vertices = GetSkinnedVertexBuilders(mesh.Vertecies.OfType<VertexRigidDouble>().ToArray(), jointIndex);
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

        var rigidMeshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture2, VertexEmpty>(mesh.GetBuilderName());
        var rigidPrimitive = rigidMeshBuilder.UsePrimitive(material);
        var rigidVertices = GetRigidVertexBuilders(mesh.Vertecies.OfType<VertexRigidDouble>().ToArray());
        var rigidIndices = indexProcessor.Process(mesh.Indices);
        for (int i = 0; i < rigidIndices.Length - 2; i += 3)
        {
            rigidPrimitive.AddTriangle(
                rigidVertices[rigidIndices[i]],
                rigidVertices[rigidIndices[i + 1]],
                rigidVertices[rigidIndices[i + 2]]);
        }

        return rigidMeshBuilder;
    }

    public void AddToScene(SceneBuilder scene, IMeshBuilder<MaterialBuilder> mesh, (NodeBuilder, Matrix4x4)[]? skin)
    {
        if (_usesSkinning && skin != null && _primaryBoneIndex >= 0 && _primaryBoneIndex < skin.Length)
        {
            scene.AddSkinnedMesh(mesh, skin);
            return;
        }

        scene.AddRigidMesh(mesh, Matrix4x4.Identity);
    }

    private static IVertexBuilder[] GetSkinnedVertexBuilders(VertexRigidDouble[] vertices, int jointIndex)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        var bindings = SkinWeightSanitizer.Normalize((jointIndex, 1f));
        for (int i = 0; i < vertices.Length; i++)
        {
            var uv0 = vertices[i].TexCoord0;
            var uv1 = vertices[i].TexCoord1;
            var normal = NormalSanitizer.NormalizeOrFallback(vertices[i].Normal);
            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture2, VertexJoints4>(
                new VertexPositionNormal(vertices[i].Position, normal),
                new VertexTexture2(new Vector2(uv0.X, uv0.Y), new Vector2(uv1.X, uv1.Y)),
                new VertexJoints4(bindings));
        }

        return vertexBuilders;
    }

    private static IVertexBuilder[] GetRigidVertexBuilders(VertexRigidDouble[] vertices)
    {
        IVertexBuilder[] vertexBuilders = new IVertexBuilder[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            var uv0 = vertices[i].TexCoord0;
            var uv1 = vertices[i].TexCoord1;
            var normal = NormalSanitizer.NormalizeOrFallback(vertices[i].Normal);
            vertexBuilders[i] = new VertexBuilder<VertexPositionNormal, VertexTexture2, VertexEmpty>(
                new VertexPositionNormal(vertices[i].Position, normal),
                new VertexTexture2(new Vector2(uv0.X, uv0.Y), new Vector2(uv1.X, uv1.Y)),
                new VertexEmpty());
        }

        return vertexBuilders;
    }
}
