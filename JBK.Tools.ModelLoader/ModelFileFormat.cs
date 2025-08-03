using JBK.Tools.ModelLoader.Enums;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace JBK.Tools.ModelFileFormat;

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

public struct Bone
{
    public Matrix4x4 matrix;
    public byte parent;
}

public struct MeshHeader
{
    public uint name;
    public int material_ref;
    public byte vertex_type;
    public byte face_type;
    public ushort vertex_count;
    public ushort index_count;
    public byte bone_index_count;
}

public struct AnimationHeader
{
    public uint szoption;
    public ushort keyframe_count;
}

public struct Keyframe
{
    public ushort time;
    public uint option;
}

public struct MaterialKey
{
    public uint szTexture;
    public ushort mapoption;
    public uint szoption;
    public float m_power;
    public uint m_frame;
}

public struct MaterialFrame
{
    public uint m_ambient; // ARGB packed color
    public uint m_diffuse; // ARGB packed color
    public uint m_specular; // ARGB packed color
    public float m_opacity;
    public Vector2 m_offset;
    public Vector3 m_angle;
}

public struct CollisionHeader
{
    public ushort vertex_count;
    public ushort face_count;
    public uint[] reserved; // 6 elements
}

public struct CollisionNode
{
    public ushort flag;
    public byte x_min;
    public byte y_min;
    public byte z_min;
    public byte x_max;
    public byte y_max;
    public byte z_max;
    public ushort left;
    public ushort right;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexRigid 
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend1
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public uint BoneIndices;     // DWORD indices (packed as 4 bytes, usually only 1 used)
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend2
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public float BlendWeight0;   // FLOAT blend[0]
    public uint BoneIndices;     // DWORD indices
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend3 
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public float BlendWeight0;   // FLOAT blend[0]
    public float BlendWeight1;   // FLOAT blend[1]
    public uint BoneIndices;     // DWORD indices
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t

    public float GetBlendWeight2()
    {
        float weight = 1.0f;
        float calculatedWeight = BlendWeight0 + BlendWeight1;
        calculatedWeight = Math.Clamp(calculatedWeight, 0.0f, 1.0f);
        return weight - calculatedWeight;
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexBlend4 
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public float BlendWeight0;   // FLOAT blend[0]
    public float BlendWeight1;   // FLOAT blend[1]
    public float BlendWeight2;   // FLOAT blend[2]
    public uint BoneIndices;     // DWORD indices
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord;     // D3DXVECTOR2 t

    public float GetBlendWeight3()
    {
        float weight = 1.0f;
        float calculatedWeight = BlendWeight0 + BlendWeight1 + BlendWeight2;
        calculatedWeight = Math.Clamp(calculatedWeight, 0.0f, 1.0f);
        return weight - calculatedWeight;
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexRigidDouble
{
    public Vector3 Position;     // D3DXVECTOR3 v
    public Vector3 Normal;       // D3DXVECTOR3 n
    public Vector2 TexCoord0;    // D3DXVECTOR2 t0
    public Vector2 TexCoord1;    // D3DXVECTOR2 t1 (e.g., lightmap UVs)
}


public struct Animation
{
    public Vector3 pos;
    public Quaternion quat;
    public Vector3 scale;
}

public class Mesh
{
    public MeshHeader Header;
    public byte[] BoneIndices;
    public byte[] VertexBuffer;

    public object[] Vertecies;
    public ushort[] Indices;


}

public class ModelFileFormat
{
    const byte GB_HEADER_VERSION = 12;

    public Header header;
    public Bone[] bones;
    public MaterialKey[] materialData;
    public MaterialFrame[] materialFrames;
    public Mesh[] meshes;
    public byte[] stringTable;

    public List<AnimationData> Animations = new();
    public Animation[] AllAnimationTransforms; // All unique transforms for all animations

    public CollisionHeader? collisionHeader;
    public CollisionNode[] collisionNodes;
    public uint[] animationNameOffsets;
    public string[] animationNames;

    public void Read(string fileName)
    {
        using FileStream file = new(fileName, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(file);

        ReadHeader(reader);
        ReadBones(reader);
        ReadMaterials(reader);
        ReadMeshes(reader);

        //animationHeaders = new AnimationHeader[header.anim_file_count];
        //for (int i = 0; i < header.anim_file_count; i++)
        //{
        //    animationHeaders[i].szoption = reader.ReadUInt32();
        //    animationHeaders[i].keyframe_count = reader.ReadUInt16();

        //    Keyframe[] frames = new Keyframe[animationHeaders[i].keyframe_count];
        //    for (int j = 0; j < animationHeaders[i].keyframe_count; j++)
        //    {
        //        frames[j].time = reader.ReadUInt16();
        //        frames[j].option = reader.ReadUInt32();
        //    }

        //    for (int j = 0; j < header.bone_count; j++)
        //    {
        //        // todo ka
        //        var ppAnimData = reader.ReadUInt16();
        //    }
        //}
        ReadAnimations(reader);

        if (header.cls_size > 0)
            ReadCollisionData(reader);     // collision header + nodes

        ReadStringTable(reader);           // string_size
        //ReadAnimationNameOffsets(reader); // Get names after string table is read
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
        if ((header.flags & (byte)ModelFlags.MODEL_BONE) == 1)
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

    private void ReadMaterialFrames(BinaryReader reader)
    {
        materialFrames = new MaterialFrame[header.material_frame_count];
        for (int i = 0; i < materialFrames.Length; i++)
        {
            materialFrames[i].m_ambient = reader.ReadUInt32();
            materialFrames[i].m_diffuse = reader.ReadUInt32();
            materialFrames[i].m_specular = reader.ReadUInt32();
            materialFrames[i].m_opacity = reader.ReadSingle();
            materialFrames[i].m_offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            materialFrames[i].m_angle = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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
        // Read animation headers, keyframes, and the index map
        for (int i = 0; i < header.anim_file_count; i++)
        {
            var animData = new AnimationData();

            var animHeader = new AnimationHeader
            {
                szoption = reader.ReadUInt32(),
                keyframe_count = reader.ReadUInt16()
            };
            animData.Header = animHeader;

            var frames = new Keyframe[animHeader.keyframe_count];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                frames[j].time = reader.ReadUInt16();
                frames[j].option = reader.ReadUInt32();
            }
            animData.Keyframes = frames;

            animData.BoneTransformIndices = new ushort[animHeader.keyframe_count, header.bone_count];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                for (int k = 0; k < header.bone_count; k++)
                {
                    // This is the critical part that was missing
                    animData.BoneTransformIndices[j, k] = reader.ReadUInt16();
                }
            }

            Animations.Add(animData);
        }

        // Now, read the global array of all unique animation transforms
        // The header.anim_count stores the total number of these transforms
        int totalTransforms = (int)header.anim_count;
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
        int count = (int)header.anim_file_count;

        // This part can remain if you need it, but we'll also assign names to our AnimationData
        animationNameOffsets = new uint[count];
        animationNames = new string[count];

        for (int i = 0; i < count; i++)
        {
            animationNameOffsets[i] = reader.ReadUInt32();
            animationNames[i] = GetString(animationNameOffsets[i]);

            // Assign the name to the corresponding animation
            if (i < Animations.Count)
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

public class AnimationData
{
    public AnimationHeader Header { get; set; }
    public Keyframe[] Keyframes { get; set; }

    // Stores [keyframeIndex, boneIndex] -> transformIndex
    public ushort[,] BoneTransformIndices { get; set; }
    public string Name { get; set; }
}