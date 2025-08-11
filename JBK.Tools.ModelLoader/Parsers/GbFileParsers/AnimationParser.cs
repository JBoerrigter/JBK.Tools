using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Animations;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class AnimationParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        model.Animations = new AnimationData[model.header.AnimFileCount];
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
                    model.Animations[i].BoneTransformIndices[j, k] = reader.ReadUInt16();
                }
            }
        }

        uint totalTransforms = model.header.AnimCount;
        model.AllAnimationTransforms = new Animation[totalTransforms];
        for (int i = 0; i < totalTransforms; i++)
        {
            model.AllAnimationTransforms[i].pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[i].quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[i].scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
