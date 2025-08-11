using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.GbFormat.Animations;
using JBK.Tools.ModelLoader.GbFormat.Bones;
using JBK.Tools.ModelLoader.GbFormat.Collisions;
using JBK.Tools.ModelLoader.GbFormat.Materials;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Collections;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace JBK.Tools.ModelLoader.Parsers;

public class SharedModelParser 
{
    public NormalizedHeader header;
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

    public static Model Parse(NormalizedHeader header, BinaryReader reader)
    {
        Model model = new Model
        {
            header = header
        };

        ReadBones(model, reader);
        ReadMaterials(model, reader);
        ReadMeshes(model, reader);
        ReadAnimations(model, reader);

        if (header.ClsSize > 0)
            ReadCollisionData(model, reader);

        ReadStringTable(model, reader);
        ParseMaterialFramesFromStringTable(model);

        return model;
    }

    private static void ReadBones(Model model, BinaryReader reader)
    {
        model.bones = new Bone[model.header.BoneCount];
        for (int i = 0; i < model.header.BoneCount; i++)
        {
            model.bones[i].matrix.M11 = reader.ReadSingle();
            model.bones[i].matrix.M12 = reader.ReadSingle();
            model.bones[i].matrix.M13 = reader.ReadSingle();
            model.bones[i].matrix.M14 = reader.ReadSingle();
            model.bones[i].matrix.M21 = reader.ReadSingle();
            model.bones[i].matrix.M22 = reader.ReadSingle();
            model.bones[i].matrix.M23 = reader.ReadSingle();
            model.bones[i].matrix.M24 = reader.ReadSingle();
            model.bones[i].matrix.M31 = reader.ReadSingle();
            model.bones[i].matrix.M32 = reader.ReadSingle();
            model.bones[i].matrix.M33 = reader.ReadSingle();
            model.bones[i].matrix.M34 = reader.ReadSingle();
            model.bones[i].matrix.M41 = reader.ReadSingle();
            model.bones[i].matrix.M42 = reader.ReadSingle();
            model.bones[i].matrix.M43 = reader.ReadSingle();
            model.bones[i].matrix.M44 = reader.ReadSingle();
            model.bones[i].parent = reader.ReadByte();
        }
    }

    private static void ReadMaterials(Model model, BinaryReader reader)
    {
        model.materialData = new MaterialKey[model.header.MaterialCount];
        for (int i = 0; i < model.header.MaterialCount; i++)
        {
            model.materialData[i].szTexture = reader.ReadUInt32();
            model.materialData[i].mapoption = reader.ReadUInt16();
            model.materialData[i].szoption = reader.ReadUInt32();
            model.materialData[i].m_power = reader.ReadSingle();
            model.materialData[i].m_frame = reader.ReadUInt32();
        }
    }

    private static void ReadMeshes(Model model, BinaryReader reader)
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

    private static void ReadAnimations(Model model, BinaryReader reader)
    {
        model.Animations = new AnimationData[model.header.AnimFileCount];
        // Read animation headers, keyframes, and the index map
        for (int i = 0; i < model.header.AnimFileCount; i++)
        {
            var animHeader = new AnimationHeader
            {
                szoption = reader.ReadUInt32(),
                keyframe_count = reader.ReadUInt16()
            };
            model.Animations[i].Header = animHeader;

            var frames = new Keyframe[animHeader.keyframe_count];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                frames[j].time = reader.ReadUInt16();
                frames[j].option = reader.ReadUInt32();
            }
            model.Animations[i].Keyframes = frames;

            model.Animations[i].BoneTransformIndices = new ushort[animHeader.keyframe_count, model.header.BoneCount];
            for (int j = 0; j < animHeader.keyframe_count; j++)
            {
                for (int k = 0; k < model.header.BoneCount; k++)
                {
                    // This is the critical part that was missing
                    model.Animations[i].BoneTransformIndices[j, k] = reader.ReadUInt16();
                }
            }
        }

        // Now, read the global array of all unique animation transforms
        // The header.anim_count stores the total number of these transforms
        uint totalTransforms = model.header.AnimCount;
        model.AllAnimationTransforms = new Animation[totalTransforms];
        for (int i = 0; i < totalTransforms; i++)
        {
            model.AllAnimationTransforms[i].pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[i].quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[i].scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
    
    private static void ReadCollisionData(Model model, BinaryReader reader)
    {
        model.collisionHeader = new CollisionHeader
        {
            vertex_count = reader.ReadUInt16(),
            face_count = reader.ReadUInt16(),
            reserved = new uint[6]
        };
        for (int i = 0; i < 6; i++)
            model.collisionHeader.Value.reserved[i] = reader.ReadUInt32();

        // Calculate node count or read based on cls_size
        int nodeCount = (int)((model.header.ClsSize - 4 - 6 * 4) / 12);
        model.collisionNodes = new CollisionNode[nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            model.collisionNodes[i].flag = reader.ReadUInt16();
            model.collisionNodes[i].x_min = reader.ReadByte();
            model.collisionNodes[i].y_min = reader.ReadByte();
            model.collisionNodes[i].z_min = reader.ReadByte();
            model.collisionNodes[i].x_max = reader.ReadByte();
            model.collisionNodes[i].y_max = reader.ReadByte();
            model.collisionNodes[i].z_max = reader.ReadByte();
            model.collisionNodes[i].left = reader.ReadUInt16();
            model.collisionNodes[i].right = reader.ReadUInt16();
        }
    }

    private static void ReadStringTable(Model model, BinaryReader reader)
    {
        model.stringTable = reader.ReadBytes((int)model.header.StringSize);
    }

    private static void ParseMaterialFramesFromStringTable(Model model)
    {
        if (model.materialData == null || model.stringTable == null || model.header.MaterialFrameCount == 0)
        {
            model.materialFramesByMaterial = Array.Empty<MaterialFrame[]>();
            return;
        }

        uint framesPerMaterial = model.header.MaterialFrameCount;
        int perFrameSize = /* 3 DWORD colors */ 4 * 3 + /* opacity */ 4 + /* vec2 */ 4 * 2 + /* vec3 */ 4 * 3;
        model.materialFramesByMaterial = new MaterialFrame[model.materialData.Length][];

        using var ms = new MemoryStream(model.stringTable);
        using var br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);

        for (int matIndex = 0; matIndex < model.materialData.Length; matIndex++)
        {
            uint frameOffset = model.materialData[matIndex].m_frame;
            if (frameOffset == 0 || frameOffset >= model.stringTable.Length)
            {
                // No frames or invalid offset — leave empty
                model.materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
                continue;
            }

            long start = frameOffset;
            long needed = framesPerMaterial * perFrameSize;
            if (start + needed > model.stringTable.Length)
            {
                model.materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
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

            model.materialFramesByMaterial[matIndex] = frames;
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