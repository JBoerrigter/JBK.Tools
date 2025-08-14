using JBK.Tools.ModelLoader.GbFormat.Bones;

namespace JBK.Tools.ModelLoader.Merge;

public static class BoneMerger
{
    public static void Merge(MergeContext mergeContext)
    {
        if (mergeContext.Source.bones == null ||
            mergeContext.Source.bones.Length == 0) return;

        int oldCount = mergeContext.Target.bones?.Length ?? 0;
        int addCount = mergeContext.Source.bones.Length;

        Bone[] newBones = new Bone[oldCount + addCount];
        if (oldCount > 0) Array.Copy(mergeContext.Target.bones, 0, newBones, 0, oldCount);

        Array.Copy(mergeContext.Source.bones, 0, newBones, oldCount, addCount);

        mergeContext.Target.bones = newBones;
        mergeContext.Target.header.BoneCount = (byte)newBones.Length;
    }
}
