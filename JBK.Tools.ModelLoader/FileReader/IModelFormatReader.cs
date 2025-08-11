using JBK.Tools.ModelLoader.GbFormat;

namespace JBK.Tools.ModelLoader.FileReader;

public interface IModelFormatReader
{
    NormalizedHeader ReadHeader();
    Model ReadModel();
}
