using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class Blend3VertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexBlend3>();
    public override VertexBlend3[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexBlend3>(data);
}
