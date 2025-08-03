namespace JBK.Tools.ModelLoader.Enums;

// Material flags
[Flags]
public enum MaterialFlags
{
    Texture = 0x10000,
    Volume = 0x20000,
    Billboard = 0x40000,
    BillboardY = 0x80000,
    Skin = 0x100000
}
