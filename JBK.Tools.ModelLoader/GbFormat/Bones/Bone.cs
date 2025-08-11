using System.Numerics;

namespace JBK.Tools.ModelLoader.GbFormat.Bones;

public struct Bone
{
    public Matrix4x4 matrix;
    public byte parent;
}
