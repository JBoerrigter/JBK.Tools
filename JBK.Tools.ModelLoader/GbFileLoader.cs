using JBK.Tools.ModelLoader.FileReader;
using System.Text;

namespace JBK.Tools.ModelLoader;

public static class GbFileLoader
{
    public static Model LoadFromFile(string path)
    {
        using var fileStream = File.OpenRead(path);
        using var binaryReader = new BinaryReader(fileStream, Encoding.UTF8, leaveOpen: true);
        // read first byte to detect version, then rewind to allow reader to read header from start
        byte version = binaryReader.ReadByte();
        fileStream.Seek(0, SeekOrigin.Begin);
        var modelFormatReader = ModelReaderFactory.Create(version, binaryReader);
        return modelFormatReader.ReadModel();
    }
}
