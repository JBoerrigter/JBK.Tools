using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.Parsers;

namespace JBK.Tools.ModelLoader.FileReader;

internal class ModelReaderV9 : IModelFormatReader
{
    private readonly BinaryReader _binaryReader;

    public ModelReaderV9(BinaryReader binaryReader) => _binaryReader = binaryReader;

    public NormalizedHeader ReadHeader()
    {
        var header = HeaderV9.ReadFrom(_binaryReader);
        return header.ToNormalized();
    }

    public Model ReadModel()
    {
        var header = ReadHeader();
        return SharedModelParser.Parse(header, _binaryReader);
    }
}
