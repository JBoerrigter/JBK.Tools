using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat;

public struct HeaderV12
{
    const byte HeaderVersion = 12;

    public byte version;
    public byte bone_count;
    public byte flags;
    public byte mesh_count;
    public uint crc;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] name;

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

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] minimum;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] maximum;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] Reserved2;

    public static HeaderV12 ReadFrom(BinaryReader reader)
    {
        HeaderV12 header = new()
        {
            version = reader.ReadByte()
        };

        if (header.version != HeaderVersion)
        {
            throw new NotSupportedException($"Version {header.version} currently not supported.");
        }

        header.bone_count = reader.ReadByte();
        header.flags = reader.ReadByte();
        header.mesh_count = reader.ReadByte();
        header.crc = reader.ReadUInt32();
        header.name = reader.ReadBytes(64);
        header.szoption = reader.ReadUInt32();
        header.vertex_count = new ushort[12];
        for (int i = 0; i < header.vertex_count.Length; i++)
        {
            header.vertex_count[i] = reader.ReadUInt16();
        }
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
        header.minimum = new float[3];
        for (int i = 0; i < header.minimum.Length; i++)
        {
            header.minimum[i] = reader.ReadSingle();
        }
        header.maximum = new float[3];
        for (int i = 0; i < header.maximum.Length; i++)
        {
            header.maximum[i] = reader.ReadSingle();
        }
        header.Reserved2 = new uint[4];
        for (int i = 0; i < 4; i++)
        {
            header.Reserved2[i] = reader.ReadUInt32();
        }
        return header;
    }

    public NormalizedHeader ToNormalized()
    {
        NormalizedHeader normalized = new()
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
            MaterialFrameCount = material_frame_count
        };
        return normalized;
    }
}
