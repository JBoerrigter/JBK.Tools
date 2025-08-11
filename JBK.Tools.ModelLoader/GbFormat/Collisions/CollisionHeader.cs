namespace JBK.Tools.ModelLoader.GbFormat.Collisions;

public struct CollisionHeader
{
    public ushort vertex_count;
    public ushort face_count;
    public uint[] reserved; // 6 elements
}