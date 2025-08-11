using JBK.Tools.ModelLoader.FileReader;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class StringTableParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        model.stringTable = reader.ReadBytes((int)model.header.StringSize);
    }
}
