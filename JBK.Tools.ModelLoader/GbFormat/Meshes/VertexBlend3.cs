using System.Numerics;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend3
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public float BlendWeight0;   // FLOAT blend[0]
    public float BlendWeight1;   // FLOAT blend[1]
    public uint BoneIndices;     // DWORD indices
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t

    public float GetBlendWeight2()
    {
        float weight = 1.0f;
        float calculatedWeight = BlendWeight0 + BlendWeight1;
        calculatedWeight = Math.Clamp(calculatedWeight, 0.0f, 1.0f);
        return weight - calculatedWeight;
    }
}