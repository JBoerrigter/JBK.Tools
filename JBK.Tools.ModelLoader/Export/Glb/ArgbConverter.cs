using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb;

public static class ArgbConverter
{
    public static Vector4 ToVector4(uint argb, float opacityOverride = -1f)
    {
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);

        float fa = a / 255f;
        float fr = r / 255f;
        float fg = g / 255f;
        float fb = b / 255f;

        if (opacityOverride >= 0f) fa = opacityOverride;

        return new Vector4(fr, fg, fb, fa);
    }
}
