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

        BoneParser.Parse(model, reader);
        MaterialParser.Parse(model, reader);
        MeshParser.Parse(model, reader);
        AnimationParser.Parse(model, reader);
        CollisionParser.Parse(model, reader);
        StringTableParser.Parse(model, reader);
        MaterialFramesParser.Parse(model);

        return model;
    }
}