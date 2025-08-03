namespace JBK.Tools.ModelLoader.Enums;

[Flags]
public enum ModelFileFlags
{
    Shader = 1,
    Script = 2,
    NoCache = 0x10000,
    CRC = 0x20000
}
