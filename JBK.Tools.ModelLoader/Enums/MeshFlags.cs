namespace JBK.Tools.ModelLoader.Enums;

[Flags]
public enum MeshFlags
{
    VSBlend = 1,
    FixedBlend = 2,
    Software = 4,
    Effect = 8,
    BlendOpacity = 0x10,
    Lightmap = 0x20
}
