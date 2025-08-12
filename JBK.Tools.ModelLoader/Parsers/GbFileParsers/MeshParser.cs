using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using JBK.Tools.ModelLoader.Parsers.VertexParsers;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class MeshParser
{
    private static Dictionary<VertexType, VertexParser> _VertexParsers = new Dictionary<VertexType, VertexParser>
    {
        { VertexType.Rigid, new RigidVertexParser() },
        { VertexType.Blend1, new Blend1VertexParser() },
        { VertexType.Blend2, new Blend2VertexParser() },
        { VertexType.Blend3, new Blend3VertexParser() },
        { VertexType.Blend4, new Blend4VertexParser() },
        { VertexType.RigidDouble, new RigidDoubleVertexParser() }
    };

    public static void Parse(Model model, BinaryReader reader)
    {
        model.meshes = new Mesh[model.header.MeshCount];
        for (int i = 0; i < model.header.MeshCount; i++)
        {
            model.meshes[i] = new Mesh();
            model.meshes[i].Header.name = reader.ReadUInt32();
            model.meshes[i].Header.material_ref = reader.ReadInt32();

            byte rawVertexType = reader.ReadByte();
            // older files used a slightly different numbering
            if (model.header.Version < 11 && rawVertexType > 0) rawVertexType -= 1;
            model.meshes[i].Header.vertex_type = rawVertexType;
            if (!Enum.IsDefined(typeof(VertexType), (int)rawVertexType))
                throw new InvalidDataException($"Unknown vertex type {rawVertexType}");
            VertexParser parser = _VertexParsers[(VertexType)rawVertexType];


            model.meshes[i].Header.face_type = reader.ReadByte();
            model.meshes[i].Header.vertex_count = reader.ReadUInt16();
            model.meshes[i].Header.index_count = reader.ReadUInt16();
            model.meshes[i].Header.bone_index_count = reader.ReadByte();
            model.meshes[i].BoneIndices = reader.ReadBytes(model.meshes[i].Header.bone_index_count);
            model.meshes[i].VertexBuffer = reader.ReadBytes(model.meshes[i].Header.vertex_count * parser.GetVertexSize());
            model.meshes[i].Vertecies = parser.GetVertexData(model.meshes[i].VertexBuffer).Cast<object>().ToArray();
            model.meshes[i].Indices = new ushort[model.meshes[i].Header.index_count];
            for (int j = 0; j < model.meshes[i].Indices.Length; j++)
            {
                model.meshes[i].Indices[j] = reader.ReadUInt16();
            }
        }
    }
}