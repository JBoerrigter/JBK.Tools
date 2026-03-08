namespace JBK.Tools.ModelLoader.Export.Glb;

public sealed class MaterialProcessorOptions
{
    public string TexturesFolder { get; init; } = string.Empty;
    public bool EmbedTextures { get; init; }
    public Action<string> WarningHandler { get; init; } = static _ => { };
}
