namespace JBK.Tools.ModelLoader.GbFormat.Collisions;

public struct CollisionNode
{
    public ushort flag;
    public byte x_min;
    public byte y_min;
    public byte z_min;
    public byte x_max;
    public byte y_max;
    public byte z_max;
    public ushort left;
    public ushort right;
}