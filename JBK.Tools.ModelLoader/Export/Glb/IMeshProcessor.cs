using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public interface IMeshProcessor
{
    IMeshBuilder<MaterialBuilder> Process(IIndexProcessor indexProcessor, MaterialBuilder material, ModelFileFormat.Mesh mesh);
    void AddToScene(SceneBuilder scene, IMeshBuilder<MaterialBuilder> mesh, (NodeBuilder, Matrix4x4)[]? skin);
}