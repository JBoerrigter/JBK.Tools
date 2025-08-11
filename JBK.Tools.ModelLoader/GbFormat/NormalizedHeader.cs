using System.Runtime.InteropServices;

namespace JBK.Tools.ModelLoader.GbFormat;

public struct NormalizedHeader
{
    public byte Version;
    public byte BoneCount;
    public byte Flags;
    public byte MeshCount;
    public uint SzOption;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public ushort[] VertexCounts;
    
    public uint IndexCount;
    public uint BoneIndexCount;
    public uint KeyframeCount;
    public uint StringSize;
    public uint ClsSize;
    public uint AnimCount;
    public byte AnimFileCount;
    public uint MaterialCount;
    public uint MaterialFrameCount;
}