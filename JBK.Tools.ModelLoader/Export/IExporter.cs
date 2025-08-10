namespace JBK.Tools.ModelLoader.Export;

public interface IExporter
{
    void Export(ModelFileFormat.ModelFileFormat source, string texPath, string outputPath);
}
