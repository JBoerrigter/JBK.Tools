namespace JBK.Tools.ModelLoader.Export.Glb;

internal static class SkinWeightSanitizer
{
    public static (int Joint, float Weight)[] Normalize(params (int Joint, float Weight)[] rawBindings)
    {
        var sanitized = new List<(int Joint, float Weight)>(rawBindings.Length);
        float sum = 0f;

        foreach (var binding in rawBindings)
        {
            if (binding.Joint < 0)
            {
                continue;
            }

            float weight = binding.Weight;
            if (!float.IsFinite(weight))
            {
                continue;
            }

            if (weight <= 0f)
            {
                continue;
            }

            sanitized.Add((binding.Joint, weight));
            sum += weight;
        }

        if (sanitized.Count == 0 || sum <= 1e-8f)
        {
            return new[] { (0, 1f) };
        }

        var normalized = new (int Joint, float Weight)[sanitized.Count];
        for (int i = 0; i < sanitized.Count; i++)
        {
            normalized[i] = (sanitized[i].Joint, sanitized[i].Weight / sum);
        }

        return normalized;
    }

    public static int MapPaletteBone(byte[] palette, int localIndex)
    {
        if (palette == null || palette.Length == 0)
        {
            return 0;
        }

        if ((uint)localIndex >= (uint)palette.Length)
        {
            return palette[0];
        }

        return palette[localIndex];
    }
}
