using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat;

public struct Header
{
    public byte version;
    public byte bone_count;
    public byte flags;
    public byte mesh_count;
    public uint crc;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] name;

    public uint szoption;
    public ushort[] vertex_count; // 12 elements
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
    public float[] minimum; // 3 elements
    public float[] maximum; // 3 elements

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] Reserved2;         // DWORD reserved2[4];
}
