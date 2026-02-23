using JBK.Tools.ModelLoader.FileReader;

namespace JBK.Tools.ModelLoader.Merge;

/// <summary>
/// Holds state for a single merge operation.
/// </summary>
public sealed class MergeContext
{
    public Model Target { get; }
    public Model Source { get; private set; }
    public MergeOptions Options { get; }
    public int[] SourceToTargetBoneMap { get; set; }

    public MergeContext(Model target, MergeOptions? options = null)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Options = options ?? new MergeOptions();
    }

    public void SetSource(Model source) => Source = source;
}
