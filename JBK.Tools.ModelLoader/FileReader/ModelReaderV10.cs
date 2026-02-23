using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.Parsers;

namespace JBK.Tools.ModelLoader.FileReader;

internal class ModelReaderV10 : IModelFormatReader
{
    private readonly BinaryReader _BinaryReader;
    public ModelReaderV10(BinaryReader binaryReader) => _BinaryReader = binaryReader;

    public NormalizedHeader ReadHeader()
    {
        var header = HeaderV10.ReadFrom(_BinaryReader);
        return header.ToNormalized();
    }

    public Model ReadModel()
    {
        var header = ReadHeader();
        return SharedModelParser.Parse(header, _BinaryReader);
    }
}
