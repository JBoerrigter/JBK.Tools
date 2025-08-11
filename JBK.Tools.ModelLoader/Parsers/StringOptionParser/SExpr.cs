using System.Globalization;

namespace JBK.Tools.ModelLoader.Parsers;

public abstract record SExpr;

public sealed record SAtom(string Value) : SExpr
{
    public bool TryAsInt(out int v)
    {
        v = 0;
        if (string.IsNullOrEmpty(Value)) return false;
        var s = Value;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v);

        // octal if starts with 0 and length>1 (mimic old C-style octal)
        if (s.Length > 1 && s.StartsWith("0") && s.All(c => c >= '0' && c <= '7'))
        {
            try { v = Convert.ToInt32(s, 8); return true; } catch { return false; }
        }

        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
    }

    public bool TryAsUInt(out uint v)
    {
        v = 0;
        if (string.IsNullOrEmpty(Value)) return false;
        var s = Value;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return uint.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v);

        if (s.Length > 1 && s.StartsWith("0") && s.All(c => c >= '0' && c <= '7'))
        {
            try { v = Convert.ToUInt32(s, 8); return true; } catch { return false; }
        }

        return uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
    }

    public bool TryAsFloat(out float f)
    {
        return float.TryParse(Value, NumberStyles.Float | NumberStyles.AllowExponent,
            CultureInfo.InvariantCulture, out f);
    }

    public override string ToString() => Value;
}

public sealed record SList(List<SExpr> Items) : SExpr
{
    public int Length => Items.Count;
    public SExpr PopFront()
    {
        if (Items.Count == 0) throw new InvalidOperationException("list empty");
        var r = Items[0];
        Items.RemoveAt(0);
        return r;
    }
    public override string ToString() => "(" + string.Join(' ', Items) + ")";
}