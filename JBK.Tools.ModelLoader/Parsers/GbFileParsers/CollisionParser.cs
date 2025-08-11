using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Collisions;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class CollisionParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        if (model.header.ClsSize == 0)
            return; // No collision data to parse

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
}
