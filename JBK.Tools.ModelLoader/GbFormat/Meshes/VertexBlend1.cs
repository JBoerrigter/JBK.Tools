using System.Numerics;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend1
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public uint BoneIndices;     // DWORD indices (packed as 4 bytes, usually only 1 used)
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t
}