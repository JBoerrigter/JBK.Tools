namespace JBK.Tools.ModelLoader.GbFormat;

public struct HeaderV8
{
    const byte HeaderVersion = 8;

    public byte version;
    public byte bone_count;
    public byte flags;
    public byte mesh_count;

    // v8 doesn't include crc/name
    public uint szoption;

    // v8 had 6 entries
    public ushort[] vertex_count; // length 6
    public ushort index_count;
    public ushort bone_index_count;
    public ushort keyframe_count;

    // v8 used smaller string fields
    public ushort string_count;
    public ushort string_size; // 16-bit in v8
    public ushort cls_size;    // 16-bit in v8

    public ushort anim_count;
    public byte anim_file_count;
    public ushort material_count;
    public ushort material_frame_count;

    public static HeaderV8 ReadFrom(BinaryReader reader)
    {
        var header = new HeaderV8
        {
            version = reader.ReadByte()
        };

        if (header.version != HeaderVersion)
            throw new NotSupportedException($"HeaderV8 reader expected version {HeaderVersion} but got {header.version}");

        header.bone_count = reader.ReadByte();
        header.flags = reader.ReadByte();
        header.mesh_count = reader.ReadByte();

        header.szoption = reader.ReadUInt32();

        header.vertex_count = new ushort[6];
        for (int i = 0; i < 6; i++) header.vertex_count[i] = reader.ReadUInt16();

        header.index_count = reader.ReadUInt16();
        header.bone_index_count = reader.ReadUInt16();
        header.keyframe_count = reader.ReadUInt16();

        header.string_count = reader.ReadUInt16();
        header.string_size = reader.ReadUInt16();
        header.cls_size = reader.ReadUInt16();

        header.anim_count = reader.ReadUInt16();
        header.anim_file_count = reader.ReadByte();
        header.material_count = reader.ReadUInt16();
        header.material_frame_count = reader.ReadUInt16();

        return header;
    }

    public NormalizedHeader ToNormalized()
    {
        var vertexCounts = new ushort[12];
        for (int i = 0; i < 6; i++) vertexCounts[i] = vertex_count[i];
        // remaining entries left 0

        return new NormalizedHeader
        {
            Version = version,
            BoneCount = bone_count,
            Flags = flags,
            MeshCount = mesh_count,
            SzOption = szoption,
            VertexCounts = vertexCounts,
            IndexCount = index_count,
            BoneIndexCount = bone_index_count,
            KeyframeCount = keyframe_count,
            // promote the 16-bit values to 32-bit fields
            StringSize = (uint)string_size,
            ClsSize = (uint)cls_size,
            AnimCount = anim_count,
            AnimFileCount = anim_file_count,
            MaterialCount = material_count,
            MaterialFrameCount = material_frame_count
        };
    }
}
