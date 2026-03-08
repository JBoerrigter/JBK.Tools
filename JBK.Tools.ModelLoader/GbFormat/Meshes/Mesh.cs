namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

public class Mesh
{
    public MeshHeader Header;
    public string Name { get; set; } = string.Empty;
    public byte[] BoneIndices;
    public byte[] VertexBuffer;

    public object[] Vertecies;
    public ushort[] Indices;

    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(Name)
            ? $"mesh_{Header.name}"
            : Name;
    }

    public string GetBuilderName()
    {
        return $"Mesh_{GetDisplayName()}";
    }
}
