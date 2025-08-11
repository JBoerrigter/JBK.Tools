namespace JBK.Tools.ModelLoader.Parsers;

public class MaterialData
{
    public MaterialFlags MapFlags { get; set; }
    public List<(uint time, float value)> VolumeAnim { get; } = new();
    public string? DiffuseTexture { get; set; }
    public string? LightmapTexture { get; set; }
    public float AlphaRef { get; set; }
    public float SpecularPower { get; set; }
    public float[] DiffuseColor { get; } = new float[4];
    public float[] AmbientColor { get; } = new float[4];
    public float[] SpecularColor { get; } = new float[4];
    public float[] EmissiveColor { get; } = new float[4];
    public float[] UVScale { get; } = { 1f, 1f };
    public float[] UVScroll { get; } = { 0f, 0f };
}

[Flags]
public enum MaterialFlags : uint
{
    None = 0,
    Billboard = 1 << 0,
    BillboardY = 1 << 1,
    TwoSided = 1 << 2,
    AlphaTest = 1 << 3,
    AlphaBlend = 1 << 4,
    ZDisable = 1 << 5,
    Wireframe = 1 << 6,
}

public static class MaterialOptionApplier
{
    private static readonly Dictionary<string, Action<MaterialData, SList>> Handlers =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["*Billboard"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom val && val.Value.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        md.MapFlags |= MaterialFlags.BillboardY;
                    else
                        md.MapFlags |= MaterialFlags.Billboard;
                },
                ["volume"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SList pairs)
                    {
                        foreach (var p in pairs.Items)
                        {
                            if (p is SList pair && pair.Items.Count >= 2
                                && pair.Items[0] is SAtom tAtom
                                && pair.Items[1] is SAtom vAtom)
                            {
                                if (!tAtom.TryAsUInt(out uint time))
                                {
                                    if (!uint.TryParse(tAtom.Value, out time)) continue;
                                }
                                vAtom.TryAsFloat(out float val);
                                md.VolumeAnim.Add((time, val));
                            }
                        }
                    }
                },
                ["diffuse"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom dt)
                        md.DiffuseTexture = dt.Value;
                },
                ["lightmap"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom lt)
                        md.LightmapTexture = lt.Value;
                },
                ["twosided"] = (md, args) => md.MapFlags |= MaterialFlags.TwoSided,
                ["alphatest"] = (md, args) => md.MapFlags |= MaterialFlags.AlphaTest,
                ["alphablend"] = (md, args) => md.MapFlags |= MaterialFlags.AlphaBlend,
                ["zdisable"] = (md, args) => md.MapFlags |= MaterialFlags.ZDisable,
                ["wireframe"] = (md, args) => md.MapFlags |= MaterialFlags.Wireframe,
                ["alpharef"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom val && val.TryAsFloat(out float f))
                        md.AlphaRef = f;
                },
                ["specularpower"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom val && val.TryAsFloat(out float f))
                        md.SpecularPower = f;
                },
                ["diffusecolor"] = (md, args) =>
                {
                    for (int i = 0; i < 4 && i + 1 < args.Items.Count; i++)
                        if (args.Items[i + 1] is SAtom val && val.TryAsFloat(out float f))
                            md.DiffuseColor[i] = f;
                },
                ["ambientcolor"] = (md, args) =>
                {
                    for (int i = 0; i < 4 && i + 1 < args.Items.Count; i++)
                        if (args.Items[i + 1] is SAtom val && val.TryAsFloat(out float f))
                            md.AmbientColor[i] = f;
                },
                ["specularcolor"] = (md, args) =>
                {
                    for (int i = 0; i < 4 && i + 1 < args.Items.Count; i++)
                        if (args.Items[i + 1] is SAtom val && val.TryAsFloat(out float f))
                            md.SpecularColor[i] = f;
                },
                ["emissivecolor"] = (md, args) =>
                {
                    for (int i = 0; i < 4 && i + 1 < args.Items.Count; i++)
                        if (args.Items[i + 1] is SAtom val && val.TryAsFloat(out float f))
                            md.EmissiveColor[i] = f;
                },
                ["uvscale"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom sx && sx.TryAsFloat(out float fx))
                        md.UVScale[0] = fx;
                    if (args.Items.Count > 2 && args.Items[2] is SAtom sy && sy.TryAsFloat(out float fy))
                        md.UVScale[1] = fy;
                },
                ["uvscroll"] = (md, args) =>
                {
                    if (args.Items.Count > 1 && args.Items[1] is SAtom sx && sx.TryAsFloat(out float fx))
                        md.UVScroll[0] = fx;
                    if (args.Items.Count > 2 && args.Items[2] is SAtom sy && sy.TryAsFloat(out float fy))
                        md.UVScroll[1] = fy;
                },
            };

    private static void ApplyOptionsFromString(MaterialData md, string optionString)
    {
        if (string.IsNullOrWhiteSpace(optionString)) return;
        var parser = new SExprParser(optionString);
        var root = parser.Parse();
        if (root is not SList rootList) return;

        foreach (var child in rootList.Items)
        {
            if (child is SList optList && optList.Items.Count > 0 && optList.Items[0] is SAtom nameAtom)
            {
                var name = nameAtom.Value;
                if (Handlers.TryGetValue(name, out var handler))
                    handler(md, optList);
                else
                    break; // Unknown option
            }
        }
    }
}