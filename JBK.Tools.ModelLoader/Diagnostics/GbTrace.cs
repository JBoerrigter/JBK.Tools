namespace JBK.Tools.ModelLoader.Diagnostics;

internal static class GbTrace
{
    private static readonly bool s_traceEnabled = ReadFlag("GB_TRACE") || ReadFlag("GB_TRACE_GB");
    private static readonly bool s_animationTraceEnabled = s_traceEnabled || ReadFlag("GB_TRACE_ANIM");

    public static bool TraceEnabled => s_traceEnabled;
    public static bool AnimationTraceEnabled => s_animationTraceEnabled;

    public static void Chunk(string chunkType, long startOffset, long size, long nextOffset)
    {
        if (!s_traceEnabled)
        {
            return;
        }

        Console.WriteLine(
            $"[GB.CHUNK] type={chunkType} start=0x{startOffset:X} size=0x{size:X} next=0x{nextOffset:X}");
    }

    public static void Animation(string message)
    {
        if (!s_animationTraceEnabled)
        {
            return;
        }

        Console.WriteLine($"[GB.ANIM] {message}");
    }

    public static void Warn(string message)
    {
        if (!s_traceEnabled && !s_animationTraceEnabled)
        {
            return;
        }

        Console.WriteLine($"[GB.WARN] {message}");
    }

    private static bool ReadFlag(string envVarName)
    {
        var raw = Environment.GetEnvironmentVariable(envVarName);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        raw = raw.Trim();
        return raw.Equals("1", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("true", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("on", StringComparison.OrdinalIgnoreCase);
    }
}
