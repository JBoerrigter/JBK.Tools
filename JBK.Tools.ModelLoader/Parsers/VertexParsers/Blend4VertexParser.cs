using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class Blend4VertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexBlend4>();
    public override VertexBlend4[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexBlend4>(data);
}
