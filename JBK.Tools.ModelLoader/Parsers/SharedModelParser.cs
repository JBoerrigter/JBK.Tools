using JBK.Tools.ModelLoader.Diagnostics;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.Parsers.GbFileParsers;

namespace JBK.Tools.ModelLoader.Parsers;

public class SharedModelParser
{
    public static Model Parse(NormalizedHeader header, BinaryReader reader)
    {
        Model model = new()
        {
            header = header
        };

        long sectionStart = reader.BaseStream.Position;
        BoneParser.Parse(model, reader);
        GbTrace.Chunk("Bones", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        sectionStart = reader.BaseStream.Position;
        MaterialParser.Parse(model, reader);
        GbTrace.Chunk("Materials", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        sectionStart = reader.BaseStream.Position;
        MeshParser.Parse(model, reader);
        GbTrace.Chunk("Meshes", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        sectionStart = reader.BaseStream.Position;
        AnimationParser.Parse(model, reader);
        GbTrace.Chunk("Animations", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        sectionStart = reader.BaseStream.Position;
        CollisionParser.Parse(model, reader);
        GbTrace.Chunk("Collision", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        sectionStart = reader.BaseStream.Position;
        StringTableParser.Parse(model, reader);
        GbTrace.Chunk("StringTable", sectionStart, reader.BaseStream.Position - sectionStart, reader.BaseStream.Position);

        AnimationParser.ResolveStringDataAndTrace(model);
        AnimationParser.RunSanityChecks(model);
        MaterialFramesParser.Parse(model);

        return model;
    }
}
