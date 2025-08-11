namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

public struct MeshHeader
{
    public uint name;
    public int material_ref;
    public byte vertex_type;
    public byte face_type;
    public ushort vertex_count;
    public ushort index_count;
    public byte bone_index_count;
}
