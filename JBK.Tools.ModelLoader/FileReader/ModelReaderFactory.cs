namespace JBK.Tools.ModelLoader.FileReader;

public static class ModelReaderFactory
{
    public static IModelFormatReader Create(byte version, BinaryReader br) => version switch
    {
        //8 => new V8ModelReader(br),
        12 => new ModelReaderV12(br),
        _ => throw new NotSupportedException($"Model version {version} not supported")
    };
}