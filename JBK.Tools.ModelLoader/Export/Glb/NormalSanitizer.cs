using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

internal static class NormalSanitizer
{
    public static Vector3 NormalizeOrFallback(Vector3 value)
    {
        if (!IsFinite(value))
        {
            return Vector3.UnitY;
        }

        float lengthSquared = value.LengthSquared();
        if (lengthSquared < 1e-8f)
        {
            return Vector3.UnitY;
        }

        if (MathF.Abs(lengthSquared - 1f) < 1e-4f)
        {
            return value;
        }

        return Vector3.Normalize(value);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }
}
