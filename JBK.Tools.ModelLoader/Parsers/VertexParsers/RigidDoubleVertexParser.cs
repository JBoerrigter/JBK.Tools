using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.VertexParsers;

public class RigidDoubleVertexParser : VertexParser
{
    public override int GetVertexSize() => Marshal.SizeOf<VertexRigidDouble>();
    public override VertexRigidDouble[] GetVertexData(byte[] data) => ReadVertexBuffer<VertexRigidDouble>(data);
}