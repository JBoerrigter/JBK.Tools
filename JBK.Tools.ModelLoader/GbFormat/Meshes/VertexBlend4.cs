using System.Numerics;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat.Meshes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend4
{
    public Vector3 Position;
    public float BlendWeight0;
    public float BlendWeight1;
    public float BlendWeight2;
    public uint BoneIndices;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public float GetBlendWeight3()
    {
        float weight = 1.0f;
        float calculatedWeight = BlendWeight0 + BlendWeight1 + BlendWeight2;
        calculatedWeight = Math.Clamp(calculatedWeight, 0.0f, 1.0f);
        return weight - calculatedWeight;
    }
}