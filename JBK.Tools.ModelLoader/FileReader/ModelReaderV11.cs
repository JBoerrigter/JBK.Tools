using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.Parsers;

namespace JBK.Tools.ModelLoader.FileReader;

internal class ModelReaderV11 : IModelFormatReader
{
    private readonly BinaryReader _BinaryReader;
    public ModelReaderV11(BinaryReader binaryReader) => _BinaryReader = binaryReader;

    public NormalizedHeader ReadHeader()
    {
        var header = HeaderV11.ReadFrom(_BinaryReader);
        var normalized = header.ToNormalized();
        return normalized;
    }

    public Model ReadModel()
    {
        var header = ReadHeader();
        return SharedModelParser.Parse(header, _BinaryReader);
    }
}
