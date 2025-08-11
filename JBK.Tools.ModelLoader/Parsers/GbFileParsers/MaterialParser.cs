using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Materials;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class MaterialParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        model.materialData = new MaterialKey[model.header.MaterialCount];
        for (int i = 0; i < model.header.MaterialCount; i++)
        {
            model.materialData[i].szTexture = reader.ReadUInt32();
            model.materialData[i].mapoption = reader.ReadUInt16();
            model.materialData[i].szoption = reader.ReadUInt32();
            model.materialData[i].m_power = reader.ReadSingle();
            model.materialData[i].m_frame = reader.ReadUInt32();
        }
    }
}
