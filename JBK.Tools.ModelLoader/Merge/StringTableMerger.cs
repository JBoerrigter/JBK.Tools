namespace JBK.Tools.ModelLoader.Merge;

public static class StringTableMerger
{
    public static void Merge(MergeContext mergeContext)
    {
        var targetStrings = mergeContext.Target.stringTable ?? Array.Empty<byte>();
        var sourceStrings = mergeContext.Source.stringTable ?? Array.Empty<byte>();

        mergeContext.StringOffset = targetStrings.Length;
        if (sourceStrings.Length == 0)
        {
            return;
        }

        var merged = new byte[targetStrings.Length + sourceStrings.Length];
        if (targetStrings.Length > 0)
        {
            Array.Copy(targetStrings, merged, targetStrings.Length);
        }

        Array.Copy(sourceStrings, 0, merged, targetStrings.Length, sourceStrings.Length);
        mergeContext.Target.stringTable = merged;
    }

    public static uint RemapStringOffset(FileReader.Model source, uint offset, int baseOffset)
    {
        if (baseOffset == 0 || source.stringTable == null || source.stringTable.Length == 0)
        {
            return offset;
        }

        if (offset >= source.stringTable.Length)
        {
            return offset;
        }

        if (string.IsNullOrEmpty(source.GetString(offset)))
        {
            return offset;
        }

        return checked(offset + (uint)baseOffset);
    }

    public static uint RemapBlobOffset(uint offset, int baseOffset)
    {
        if (offset == 0 || baseOffset == 0)
        {
            return offset;
        }

        return checked(offset + (uint)baseOffset);
    }
}
