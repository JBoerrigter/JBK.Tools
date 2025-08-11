using System.Numerics;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexRigidDouble
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord0;
    public Vector2 TexCoord1;
}
