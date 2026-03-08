using System.Numerics;

namespace JBK.Tools.ModelLoader.GbFormat.Collisions;

public struct CollisionHeader
{
    public ushort vertex_count;
    public ushort face_count;
    public bool HasBounds;
    public Vector3 minimum;
    public Vector3 maximum;
    public uint[] reserved;
}
