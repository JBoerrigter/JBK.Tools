using JBK.Tools.ModelLoader.FileReader;

namespace JBK.Tools.ModelLoader.Merge;

/// <summary>
/// Holds state for a single merge operation.
/// </summary>
public sealed class MergeContext
{
    public Model Target { get; }
    public Model Source { get; private set; }

    public MergeContext(Model target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public void SetSource(Model source) => Source = source;
}
