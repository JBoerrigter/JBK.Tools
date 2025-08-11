using JBK.Tools.ModelLoader.GbFormat.Meshes;

namespace JBK.Tools.ModelFileFormat;

public class Mesh
{
    public MeshHeader Header;
    public byte[] BoneIndices;
    public byte[] VertexBuffer;

    public object[] Vertecies;
    public ushort[] Indices;
}
