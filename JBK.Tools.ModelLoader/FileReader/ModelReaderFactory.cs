namespace JBK.Tools.ModelLoader.FileReader;

public static class ModelReaderFactory
{
    public static IModelFormatReader Create(byte version, BinaryReader reader) => version switch
    {
        8 => new ModelReaderV8(reader),
        12 => new ModelReaderV12(reader),
        _ => throw new NotSupportedException($"Model version {version} not supported")
    };
}