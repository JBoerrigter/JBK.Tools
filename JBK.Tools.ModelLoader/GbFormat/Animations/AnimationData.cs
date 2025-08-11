namespace JBK.Tools.ModelLoader.GbFormat.Animations;

public struct AnimationData
{
    public AnimationHeader Header { get; set; }
    public Keyframe[] Keyframes { get; set; }

    // Stores [keyframeIndex, boneIndex] -> transformIndex
    public ushort[,] BoneTransformIndices { get; set; }
    public string Name { get; set; }
}