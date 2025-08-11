using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class Blend1VertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexBlend1>();
    public override VertexBlend1[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexBlend1>(data);
}
