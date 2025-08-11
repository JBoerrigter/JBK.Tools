using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Collections;
using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class MeshParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        model.meshes = new Mesh[model.header.MeshCount];
        for (int i = 0; i < model.header.MeshCount; i++)
        {
            model.meshes[i] = new Mesh();
            model.meshes[i].Header.name = reader.ReadUInt32();
            model.meshes[i].Header.material_ref = reader.ReadInt32();
            model.meshes[i].Header.vertex_type = reader.ReadByte();
            model.meshes[i].Header.face_type = reader.ReadByte();
            model.meshes[i].Header.vertex_count = reader.ReadUInt16();
            model.meshes[i].Header.index_count = reader.ReadUInt16();
            model.meshes[i].Header.bone_index_count = reader.ReadByte();
            model.meshes[i].BoneIndices = reader.ReadBytes(model.meshes[i].Header.bone_index_count);
            model.meshes[i].VertexBuffer = reader.ReadBytes(model.meshes[i].Header.vertex_count * GetVertexSize((VertexType)model.meshes[i].Header.vertex_type));
            model.meshes[i].Vertecies = GetVertexData((VertexType)model.meshes[i].Header.vertex_type, model.meshes[i].VertexBuffer).Cast<object>().ToArray();
            model.meshes[i].Indices = new ushort[model.meshes[i].Header.index_count];
            for (int j = 0; j < model.meshes[i].Indices.Length; j++)
            {
                model.meshes[i].Indices[j] = reader.ReadUInt16();
            }
        }
    }

    private static int GetVertexSize(VertexType vertexType)
    {
        return vertexType switch
        {
            VertexType.Rigid => Marshal.SizeOf<VertexRigid>(),
            VertexType.Blend1 => Marshal.SizeOf<VertexBlend1>(),
            VertexType.Blend2 => Marshal.SizeOf<VertexBlend2>(),
            VertexType.Blend3 => Marshal.SizeOf<VertexBlend3>(),
            VertexType.Blend4 => Marshal.SizeOf<VertexBlend4>(),
            VertexType.RigidDouble => Marshal.SizeOf<VertexRigidDouble>(),
            _ => throw new NotSupportedException($"Vertex type {vertexType} is not supported.")
        };
    }

    private static IEnumerable GetVertexData(VertexType vertexType, byte[] buffer)
    {
        return vertexType switch
        {
            VertexType.Rigid => ReadVertexBuffer<VertexRigid>(buffer),
            VertexType.RigidDouble => ReadVertexBuffer<VertexRigidDouble>(buffer),
            VertexType.Blend1 => ReadVertexBuffer<VertexBlend1>(buffer),
            VertexType.Blend2 => ReadVertexBuffer<VertexBlend2>(buffer),
            VertexType.Blend3 => ReadVertexBuffer<VertexBlend3>(buffer),
            VertexType.Blend4 => ReadVertexBuffer<VertexBlend4>(buffer),
            _ => throw new NotSupportedException($"Unsupported vertex type: {vertexType}"),
        };
    }

    private static T[] ReadVertexBuffer<T>(byte[] data) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        int count = data.Length / size;
        T[] result = new T[count];

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        nint basePtr = handle.AddrOfPinnedObject();

        for (int i = 0; i < count; i++)
        {
            result[i] = Marshal.PtrToStructure<T>(basePtr + i * size);
        }

        handle.Free();
        return result;
    }
}
