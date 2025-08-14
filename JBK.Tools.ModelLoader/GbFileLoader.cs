using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.Merge;
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

    /// <summary>
    /// Loads a .gb from <paramref name="path"/> and merges it into <paramref name="targetModel"/>.
    /// Use it to add meshes (body parts), bones (skeleton), or animations (motions).
    /// </summary>
    public static Model Append(Model targetModel, string path)
    {
        using var fileStream = File.OpenRead(path);
        using var binaryReader = new BinaryReader(fileStream);

        byte version = binaryReader.ReadByte();
        binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

        var reader = ModelReaderFactory.Create(version, binaryReader);
        var sourceModel = reader.ReadModel();

        var mergeContext = new MergeContext(targetModel);
        mergeContext.SetSource(sourceModel);

        BoneMerger.Merge(mergeContext);
        MeshMerger.Merge(mergeContext);
        AnimationMerger.Merge(mergeContext);

        return targetModel;
    }
}
