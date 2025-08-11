using JBK.Tools.ModelLoader.FileReader;

namespace JBK.Tools.ModelLoader.Export;

public interface IExporter
{
    void Export(Model source, string texPath, string outputPath);
}
