using JBK.Tools.ModelLoader.Diagnostics;
using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using JBK.Tools.ModelLoader.Parsers.VertexParsers;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class MeshParser
{
    private static readonly Dictionary<VertexType, VertexParser> s_vertexParsers = new()
    {
        { VertexType.Rigid, new RigidVertexParser() },
        { VertexType.Blend1, new Blend1VertexParser() },
        { VertexType.Blend2, new Blend2VertexParser() },
        { VertexType.Blend3, new Blend3VertexParser() },
        { VertexType.Blend4, new Blend4VertexParser() },
        // v9/v10 lightmap-static and v11+ rigid-double share the same vertex layout.
        { VertexType.LmStatic, new RigidDoubleVertexParser() },
        { VertexType.RigidDouble, new RigidDoubleVertexParser() }
    };

    private static VertexType MapVertexType(byte fileVertexType, byte version)
    {
        // Original C++ enums (ModelHeader.h):
        // - v8/v9/v10: VT_STATIC(0), VT_RIGID(1), VT_BLEND1(2), VT_BLEND2(3), VT_BLEND3(4), VT_BLEND4(5)
        // - v9/v10 additionally: VT_LM_STATIC(6)
        // - v11+: VT_RIGID(0)..VT_BLEND4(4), VT_RIGID_DOUBLE(5)

        if (version < 11)
        {
            // Treat VT_STATIC like VT_RIGID for now (export path expects a bone-indexed mesh anyway).
            if (fileVertexType == 0) return VertexType.Rigid;

            if (version >= 9 && fileVertexType == 6) return VertexType.LmStatic;

            // Shift down by one because the file had VT_STATIC at index 0.
            byte normalized = (byte)(fileVertexType - 1);
            return normalized switch
            {
                0 => VertexType.Rigid,
                1 => VertexType.Blend1,
                2 => VertexType.Blend2,
                3 => VertexType.Blend3,
                4 => VertexType.Blend4,
                _ => throw new InvalidDataException($"Unknown vertex type {fileVertexType} for model version {version}")
            };
        }

        // v11+ mapping is 1:1, except we normalize VT_RIGID_DOUBLE(5) to our VertexType.RigidDouble(6)
        if (fileVertexType == 5) return VertexType.RigidDouble;

        return fileVertexType switch
        {
            0 => VertexType.Rigid,
            1 => VertexType.Blend1,
            2 => VertexType.Blend2,
            3 => VertexType.Blend3,
            4 => VertexType.Blend4,
            _ => throw new InvalidDataException($"Unknown vertex type {fileVertexType} for model version {version}")
        };
    }

    public static void Parse(Model model, BinaryReader reader)
    {
        model.meshes = new Mesh[model.header.MeshCount];
        for (int i = 0; i < model.header.MeshCount; i++)
        {
            long meshStart = reader.BaseStream.Position;

            model.meshes[i] = new Mesh();
            model.meshes[i].Header.name = reader.ReadUInt32();
            model.meshes[i].Header.material_ref = reader.ReadInt32();

            byte fileVertexType = reader.ReadByte();
            VertexType normalizedVertexType = MapVertexType(fileVertexType, model.header.Version);
            model.meshes[i].Header.vertex_type = (byte)normalizedVertexType;

            VertexParser parser = s_vertexParsers[normalizedVertexType];
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

            if (GbTrace.TraceEnabled)
            {
                GbTrace.Chunk($"Mesh[{i}]", meshStart, reader.BaseStream.Position - meshStart, reader.BaseStream.Position);
            }
        }
    }
}
