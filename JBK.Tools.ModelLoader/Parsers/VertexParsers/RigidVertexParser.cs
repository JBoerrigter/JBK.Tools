using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class RigidVertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexRigid>();
    public override VertexRigid[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexRigid>(data);
}
