using System.Numerics;

namespace JBK.Tools.ModelLoader.GbFormat.Materials;

public struct MaterialFrame
{
    public uint m_ambient; // ARGB packed color
    public uint m_diffuse; // ARGB packed color
    public uint m_specular; // ARGB packed color
    public float m_opacity;
    public Vector2 m_offset;
    public Vector3 m_angle;
}