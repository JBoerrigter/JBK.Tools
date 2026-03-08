using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Collisions;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class CollisionParser
{
    private const int CollisionNodeSize = sizeof(ushort) + sizeof(byte) * 6 + sizeof(ushort) * 2;
    private const int LegacyBoundsHeaderSize = sizeof(ushort) * 2 + sizeof(float) * 3 * 2;
    private const int ReservedHeaderSize = sizeof(ushort) * 2 + sizeof(uint) * 6;

    public static void Parse(Model model, BinaryReader reader)
    {
        if (model.header.ClsSize == 0)
        {
            model.collisionNodes = Array.Empty<CollisionNode>();
            return;
        }

        long collisionStart = reader.BaseStream.Position;
        long collisionEnd = collisionStart + model.header.ClsSize;

        CollisionHeader collisionHeader = new()
        {
            vertex_count = reader.ReadUInt16(),
            face_count = reader.ReadUInt16(),
            reserved = Array.Empty<uint>()
        };

        if (model.header.Version >= 11)
        {
            collisionHeader.reserved = new uint[6];
            for (int i = 0; i < collisionHeader.reserved.Length; i++)
            {
                collisionHeader.reserved[i] = reader.ReadUInt32();
            }
        }
        else
        {
            collisionHeader.HasBounds = true;
            collisionHeader.minimum = ReadVector3(reader);
            collisionHeader.maximum = ReadVector3(reader);
        }

        model.collisionHeader = collisionHeader;

        int headerSize = model.header.Version >= 11 ? ReservedHeaderSize : LegacyBoundsHeaderSize;
        long remainingBytes = collisionEnd - reader.BaseStream.Position;
        if (remainingBytes < 0)
        {
            throw new InvalidDataException($"Collision section over-read for model version {model.header.Version}.");
        }

        int nodeCount = (int)(remainingBytes / CollisionNodeSize);
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

        long parsedEnd = collisionStart + headerSize + (long)nodeCount * CollisionNodeSize;
        if (parsedEnd < collisionEnd)
        {
            reader.BaseStream.Seek(collisionEnd, SeekOrigin.Begin);
        }
    }

    private static Vector3 ReadVector3(BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
