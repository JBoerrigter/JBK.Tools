using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.GbFormat.Animations;
using JBK.Tools.ModelLoader.GbFormat.Bones;
using JBK.Tools.ModelLoader.GbFormat.Collisions;
using JBK.Tools.ModelLoader.GbFormat.Materials;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace JBK.Tools.ModelFileFormat;

//// NormalizedHeader.cs
//public record NormalizedHeader
//{
//    public byte Version { get; init; }
//    public byte BoneCount { get; init; }
//    public byte Flags { get; init; }
//    public byte MeshCount { get; init; }
//    public uint SzOption { get; init; }
//    public int[] VertexCounts { get; init; } = new int[12]; // canonical 12 slots
//    public uint IndexCount { get; init; }
//    public uint BoneIndexCount { get; init; }
//    public uint KeyframeCount { get; init; }
//    public uint StringSize { get; init; }    // always 32-bit in canonical model
//    public uint ClsSize { get; init; }       // canonical
//    public uint AnimCount { get; init; }
//    public byte AnimFileCount { get; init; }
//    public uint MaterialCount { get; init; }
//    public uint MaterialFrameCount { get; init; }
//}

//// HeaderV8.cs -- reads v8 fields and converts to NormalizedHeader
//public record HeaderV8
//{
//    public byte Version;
//    public byte BoneCount;
//    public byte Flags;
//    public byte MeshCount;
//    public uint SzOption;
//    public ushort[] VertexCount = new ushort[6]; // v8 had 6 entries
//    public ushort IndexCount;
//    public ushort BoneIndexCount;
//    public ushort KeyframeCount;
//    public ushort StringCount;
//    public ushort StringSize; // v8 uses 16-bit
//    public ushort ClsSize;    // v8 uses 16-bit
//    public ushort AnimCount;
//    public byte AnimFileCount;
//    public ushort MaterialCount;
//    public ushort MaterialFrameCount;

//    public static HeaderV8 ReadFrom(BinaryReader br)
//    {
//        var h = new HeaderV8();
//        h.Version = br.ReadByte();
//        h.BoneCount = br.ReadByte();
//        h.Flags = br.ReadByte();
//        h.MeshCount = br.ReadByte();
//        h.SzOption = br.ReadUInt32();
//        for (int i = 0; i < 6; i++) h.VertexCount[i] = br.ReadUInt16();
//        h.IndexCount = br.ReadUInt16();
//        h.BoneIndexCount = br.ReadUInt16();
//        h.KeyframeCount = br.ReadUInt16();
//        h.StringCount = br.ReadUInt16();
//        h.StringSize = br.ReadUInt16();
//        h.ClsSize = br.ReadUInt16();
//        h.AnimCount = br.ReadUInt16();
//        h.AnimFileCount = br.ReadByte();
//        h.MaterialCount = br.ReadUInt16();
//        h.MaterialFrameCount = br.ReadUInt16();
//        return h;
//    }

//    public NormalizedHeader ToNormalized()
//    {
//        var n = new NormalizedHeader
//        {
//            Version = Version,
//            BoneCount = BoneCount,
//            Flags = Flags,
//            MeshCount = MeshCount,
//            SzOption = SzOption,
//            IndexCount = IndexCount,
//            BoneIndexCount = BoneIndexCount,
//            KeyframeCount = KeyframeCount,
//            StringSize = StringSize,   // promote ushort -> uint implicitly
//            ClsSize = ClsSize,
//            AnimCount = AnimCount,
//            AnimFileCount = AnimFileCount,
//            MaterialCount = MaterialCount,
//            MaterialFrameCount = MaterialFrameCount
//        };
//        for (int i = 0; i < 6; i++) n.VertexCounts[i] = VertexCount[i];
//        // remaining VertexCounts already zero-initialized
//        return n;
//    }
//}

//// IModelFormatReader.cs
//public interface IModelFormatReader
//{
//    NormalizedHeader ReadHeader();
//    Model ReadModel(); // Model is your in-memory result type
//}

//// V8ModelReader.cs
//public class V8ModelReader : IModelFormatReader
//{
//    private readonly BinaryReader _BinaryReader;
//    public V8ModelReader(BinaryReader binaryReader) => _BinaryReader = binaryReader;

//    public NormalizedHeader ReadHeader()
//    {
//        var h8 = HeaderV8.ReadFrom(_BinaryReader);
//        var normalized = h8.ToNormalized();
//        // optional: validate normalized values here (simple sanity checks)
//        ValidateHeader(normalized);
//        return normalized;
//    }

//    public Model ReadModel()
//    {
//        var header = ReadHeader();
//        // call into shared parsing code that consumes the rest using header
//        return SharedModelParser.Parse(header, _BinaryReader);
//    }

//    private void ValidateHeader(NormalizedHeader h)
//    {
//        if (h.StringSize > 100_000_000) // arbitrary sanity bound
//            throw new ModelFormatException("string table size unreasonable");
//    }
//}

//// ReaderFactory + Loader orchestration
//public static class ModelReaderFactory
//{
//    public static IModelFormatReader Create(byte version, BinaryReader br) => version switch
//    {
//        8 => new V8ModelReader(br),
//        12 => new V12ModelReader(br),
//        >= 9 => new LegacyModelReader(br),
//        _ => throw new NotSupportedException($"Model version {version} not supported")
//    };
//}

//public static class ModelLoader
//{
//    public static Model LoadFromFile(string path)
//    {
//        using var fs = File.OpenRead(path);
//        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);
//        // read first byte to detect version, then rewind to allow reader to read header from start
//        byte version = br.ReadByte();
//        fs.Seek(0, SeekOrigin.Begin);
//        var reader = ModelReaderFactory.Create(version, br);
//        return reader.ReadModel();
//    }
//}



public class ModelFileFormat
{
    const byte GB_HEADER_VERSION = 12;

    public Header header;
    public Bone[] bones;
    public MaterialKey[] materialData;
    public MaterialFrame[] materialFrames;
    public Mesh[] meshes;
    public byte[] stringTable;

    public AnimationData[] Animations;
    public Animation[] AllAnimationTransforms; // All unique transforms for all animations

    public CollisionHeader? collisionHeader;
    public CollisionNode[] collisionNodes;
    public uint[] animationNameOffsets;
    public string[] animationNames;
    public MaterialFrame[][] materialFramesByMaterial; // [materialIndex][frameIndex]

    public void Read(string fileName)
    {
        using FileStream file = new(fileName, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(file);

        ReadHeader(reader);
        ReadBones(reader);
        ReadMaterials(reader);
        ReadMeshes(reader);
        ReadAnimations(reader);

        if (header.cls_size > 0)
            ReadCollisionData(reader);     // collision header + nodes

        ReadStringTable(reader);           // string_size
        ParseMaterialFramesFromStringTable();
    }
    
    private void ReadHeader(BinaryReader reader)
    {
        header.version = reader.ReadByte();

        if (header.version != GB_HEADER_VERSION)
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
    }

    private void ReadBones(BinaryReader reader)
    {
        bones = new Bone[header.bone_count];
        for (int i = 0; i < header.bone_count; i++)
        {
            bones[i].matrix.M11 = reader.ReadSingle();
            bones[i].matrix.M12 = reader.ReadSingle();
            bones[i].matrix.M13 = reader.ReadSingle();
            bones[i].matrix.M14 = reader.ReadSingle();
            bones[i].matrix.M21 = reader.ReadSingle();
            bones[i].matrix.M22 = reader.ReadSingle();
            bones[i].matrix.M23 = reader.ReadSingle();
            bones[i].matrix.M24 = reader.ReadSingle();
            bones[i].matrix.M31 = reader.ReadSingle();
            bones[i].matrix.M32 = reader.ReadSingle();
            bones[i].matrix.M33 = reader.ReadSingle();
            bones[i].matrix.M34 = reader.ReadSingle();
            bones[i].matrix.M41 = reader.ReadSingle();
            bones[i].matrix.M42 = reader.ReadSingle();
            bones[i].matrix.M43 = reader.ReadSingle();
            bones[i].matrix.M44 = reader.ReadSingle();
            bones[i].parent = reader.ReadByte();
        }
    }

    private void ReadMaterials(BinaryReader reader)
    {
        materialData = new MaterialKey[header.material_count];
        for (int i = 0; i < header.material_count; i++)
        {
            materialData[i].szTexture = reader.ReadUInt32();
            materialData[i].mapoption = reader.ReadUInt16();
            materialData[i].szoption = reader.ReadUInt32();
            materialData[i].m_power = reader.ReadSingle();
            materialData[i].m_frame = reader.ReadUInt32();
        }
    }

    private void ReadMeshes(BinaryReader reader)
    {
        meshes = new Mesh[header.mesh_count];
        for (int i = 0; i < header.mesh_count; i++)
        {
            meshes[i] = new Mesh();
            meshes[i].Header.name = reader.ReadUInt32();
            meshes[i].Header.material_ref = reader.ReadInt32();
            meshes[i].Header.vertex_type = reader.ReadByte();
            meshes[i].Header.face_type = reader.ReadByte();
            meshes[i].Header.vertex_count = reader.ReadUInt16();
            meshes[i].Header.index_count = reader.ReadUInt16();
            meshes[i].Header.bone_index_count = reader.ReadByte();
            meshes[i].BoneIndices = reader.ReadBytes(meshes[i].Header.bone_index_count);
            meshes[i].VertexBuffer = reader.ReadBytes(meshes[i].Header.vertex_count * GetVertexSize((VertexType)meshes[i].Header.vertex_type));
            meshes[i].Vertecies = GetVertexData((VertexType)meshes[i].Header.vertex_type, meshes[i].VertexBuffer).Cast<object>().ToArray();
            meshes[i].Indices = new ushort[meshes[i].Header.index_count];
            for (int j = 0; j < meshes[i].Indices.Length; j++)
            {
                meshes[i].Indices[j] = reader.ReadUInt16();
            }
        }
    }

    private void ReadAnimations(BinaryReader reader)
    {
        Animations = new AnimationData[header.anim_file_count];
        // Read animation headers, keyframes, and the index map
        for (int i = 0; i < header.anim_file_count; i++)
        {
            var animHeader = new AnimationHeader
            {
                szoption = reader.ReadUInt32(),
                keyframe_count = reader.ReadUInt16()
            };
            Animations[i].Header = animHeader;

            var frames = new Keyframe[animHeader.keyframe_count];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                frames[j].time = reader.ReadUInt16();
                frames[j].option = reader.ReadUInt32();
            }
            Animations[i].Keyframes = frames;

            Animations[i].BoneTransformIndices = new ushort[animHeader.keyframe_count, header.bone_count];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                for (int k = 0; k < header.bone_count; k++)
                {
                    // This is the critical part that was missing
                    Animations[i].BoneTransformIndices[j, k] = reader.ReadUInt16();
                }
            }
        }

        // Now, read the global array of all unique animation transforms
        // The header.anim_count stores the total number of these transforms
        int totalTransforms = header.anim_count;
        AllAnimationTransforms = new Animation[totalTransforms];
        for (int i = 0; i < totalTransforms; i++)
        {
            AllAnimationTransforms[i].pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            AllAnimationTransforms[i].quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            AllAnimationTransforms[i].scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }

    private void ReadAnimationNameOffsets(BinaryReader reader)
    {
        animationNameOffsets = new uint[header.anim_file_count];
        animationNames = new string[header.anim_file_count];

        for (int i = 0; i < header.anim_file_count; i++)
        {
            animationNameOffsets[i] = reader.ReadUInt32();
            animationNames[i] = GetString(animationNameOffsets[i]);

            if (i < Animations.Length)
            {
                Animations[i].Name = animationNames[i];
            }
        }
    }


    public string GetString(uint offset)
    {
        int i = (int)offset;
        List<byte> bytes = new();

        while (i < stringTable.Length && stringTable[i] != 0)
            bytes.Add(stringTable[i++]);

        return Encoding.ASCII.GetString(bytes.ToArray());
    }


    private void ReadCollisionData(BinaryReader reader)
    {
        collisionHeader = new CollisionHeader
        {
            vertex_count = reader.ReadUInt16(),
            face_count = reader.ReadUInt16(),
            reserved = new uint[6]
        };
        for (int i = 0; i < 6; i++)
            collisionHeader.Value.reserved[i] = reader.ReadUInt32();

        // Calculate node count or read based on cls_size
        int nodeCount = (int)((header.cls_size - 4 - (6 * 4)) / 12);
        collisionNodes = new CollisionNode[nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            collisionNodes[i].flag = reader.ReadUInt16();
            collisionNodes[i].x_min = reader.ReadByte();
            collisionNodes[i].y_min = reader.ReadByte();
            collisionNodes[i].z_min = reader.ReadByte();
            collisionNodes[i].x_max = reader.ReadByte();
            collisionNodes[i].y_max = reader.ReadByte();
            collisionNodes[i].z_max = reader.ReadByte();
            collisionNodes[i].left = reader.ReadUInt16();
            collisionNodes[i].right = reader.ReadUInt16();
        }
    }

    private void ReadStringTable(BinaryReader reader)
    {
        stringTable = reader.ReadBytes((int)header.string_size);
    }

    private void ParseMaterialFramesFromStringTable()
    {
        if (materialData == null || stringTable == null || header.material_frame_count == 0)
        {
            materialFramesByMaterial = Array.Empty<MaterialFrame[]>();
            return;
        }

        int framesPerMaterial = header.material_frame_count;
        int perFrameSize = (/* 3 DWORD colors */ 4 * 3) + /* opacity */ 4 + /* vec2 */ (4 * 2) + /* vec3 */ (4 * 3);
        materialFramesByMaterial = new MaterialFrame[materialData.Length][];

        using var ms = new MemoryStream(stringTable);
        using var br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);

        for (int matIndex = 0; matIndex < materialData.Length; matIndex++)
        {
            uint frameOffset = materialData[matIndex].m_frame;
            if (frameOffset == 0 || frameOffset >= stringTable.Length)
            {
                // No frames or invalid offset — leave empty
                materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
                continue;
            }

            long start = frameOffset;
            long needed = (long)framesPerMaterial * perFrameSize;
            if (start + needed > stringTable.Length)
            {
                materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
                continue;
            }

            ms.Position = start;
            var frames = new MaterialFrame[framesPerMaterial];
            for (int f = 0; f < framesPerMaterial; f++)
            {
                frames[f].m_ambient = br.ReadUInt32();
                frames[f].m_diffuse = br.ReadUInt32();
                frames[f].m_specular = br.ReadUInt32();
                frames[f].m_opacity = br.ReadSingle();
                float ox = br.ReadSingle();
                float oy = br.ReadSingle();
                frames[f].m_offset = new Vector2(ox, oy);
                float ax = br.ReadSingle();
                float ay = br.ReadSingle();
                float az = br.ReadSingle();
                frames[f].m_angle = new Vector3(ax, ay, az);
            }

            materialFramesByMaterial[matIndex] = frames;
        }
    }

    int GetVertexSize(VertexType vertexType)
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

    IEnumerable GetVertexData(VertexType vertexType, byte[] buffer)
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

    T[] ReadVertexBuffer<T>(byte[] data) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        int count = data.Length / size;
        T[] result = new T[count];

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        IntPtr basePtr = handle.AddrOfPinnedObject();

        for (int i = 0; i < count; i++)
        {
            result[i] = Marshal.PtrToStructure<T>(basePtr + i * size);
        }

        handle.Free();
        return result;
    }

}
