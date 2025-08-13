using JBK.Tools.ModelLoader.Enums;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Bones;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class BoneParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        if ((model.header.Flags & (byte)ModelFlags.MODEL_BONE) == 0)
        {
            model.bones = Array.Empty<Bone>();
            return;
        }

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
}
