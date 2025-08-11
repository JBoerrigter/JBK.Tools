namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

public class Mesh
{
    public MeshHeader Header;
    public byte[] BoneIndices;
    public byte[] VertexBuffer;

    public object[] Vertecies;
    public ushort[] Indices;
}
