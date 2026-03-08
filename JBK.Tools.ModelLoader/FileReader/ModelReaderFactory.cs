namespace JBK.Tools.ModelLoader.FileReader;

public static class ModelReaderFactory
{
    public static IModelFormatReader Create(byte version, BinaryReader reader) => version switch
    {
        8 => new ModelReaderV8(reader),
        9 => new ModelReaderV9(reader),
        10 => new ModelReaderV10(reader),
        11 => new ModelReaderV11(reader),
        12 => new ModelReaderV12(reader),
        _ => throw new NotSupportedException($"Model version {version} not supported")
    };
}
