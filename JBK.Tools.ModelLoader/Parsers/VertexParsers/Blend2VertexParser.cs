using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class Blend2VertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexBlend2>();
    public override VertexBlend2[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexBlend2>(data);
}
