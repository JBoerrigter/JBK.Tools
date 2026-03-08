namespace JBK.Tools.ModelLoader.Merge;

public sealed class MergeOptions
{
    public bool ResolveBonesToTarget { get; set; }
    public bool AssumeMatchingBoneOrder { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
}
