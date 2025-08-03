namespace JBK.Tools.ModelLoader.Enums;

[Flags]
public enum StaticModelFlags
{
    ForceAmbient = 1,
    BlendOpacity = 2,
    ReflectTransform = 4,
    Water = 8,
    ContainTransparent = 0x10,
    Blendable = 0x20
}