using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat;

/// <summary>
/// GB_HEADER layout for file version 10.
/// 
/// Based on the original C++ struct in ModelHeader.h with GB_HEADER_VERSION 10:
/// - Has CRC
/// - No name[64] (added in v12)
/// - Has vertex_count[12] (added in v9)
/// - No minimum/maximum vectors (added in v11)
/// - Has reserved2[4] (present since v9)
/// </summary>
public struct HeaderV10
{
    private const byte HeaderVersion = 10;

    public byte version;
    public byte bone_count;
    public byte flags;
    public byte mesh_count;
    public uint crc;

    public uint szoption;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public ushort[] vertex_count;

    public ushort index_count;
    public ushort bone_index_count;
    public ushort keyframe_count;

    public ushort reserved0;
    public uint string_size;
    public uint cls_size;

    public ushort anim_count;
    public byte anim_file_count;
    public byte reserved1;

    public ushort material_count;
    public ushort material_frame_count;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] reserved2;

    public static HeaderV10 ReadFrom(BinaryReader reader)
    {
        HeaderV10 header = new()
        {
            version = reader.ReadByte()
        };

        if (header.version != HeaderVersion)
            throw new NotSupportedException($"HeaderV10 reader expected version {HeaderVersion} but got {header.version}");

        header.bone_count = reader.ReadByte();
        header.flags = reader.ReadByte();
        header.mesh_count = reader.ReadByte();
        header.crc = reader.ReadUInt32();
        header.szoption = reader.ReadUInt32();

        header.vertex_count = new ushort[12];
        for (int i = 0; i < header.vertex_count.Length; i++)
            header.vertex_count[i] = reader.ReadUInt16();

        header.index_count = reader.ReadUInt16();
        header.bone_index_count = reader.ReadUInt16();
        header.keyframe_count = reader.ReadUInt16();
        header.reserved0 = reader.ReadUInt16();
        header.string_size = reader.ReadUInt32();
        header.cls_size = reader.ReadUInt32();
        header.anim_count = reader.ReadUInt16();
        header.anim_file_count = reader.ReadByte();
        header.reserved1 = reader.ReadByte();
        header.material_count = reader.ReadUInt16();
        header.material_frame_count = reader.ReadUInt16();

        header.reserved2 = new uint[4];
        for (int i = 0; i < 4; i++)
            header.reserved2[i] = reader.ReadUInt32();

        return header;
    }

    public NormalizedHeader ToNormalized()
    {
        return new NormalizedHeader
        {
            Version = version,
            BoneCount = bone_count,
            Flags = flags,
            MeshCount = mesh_count,
            SzOption = szoption,
            VertexCounts = vertex_count,
            IndexCount = index_count,
            BoneIndexCount = bone_index_count,
            KeyframeCount = keyframe_count,
            StringSize = string_size,
            ClsSize = cls_size,
            AnimCount = anim_count,
            AnimFileCount = anim_file_count,
            MaterialCount = material_count,
            MaterialFrameCount = material_frame_count,
        };
    }
}
