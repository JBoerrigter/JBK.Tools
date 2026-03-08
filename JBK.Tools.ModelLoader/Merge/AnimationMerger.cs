using JBK.Tools.ModelLoader.GbFormat.Animations;

namespace JBK.Tools.ModelLoader.Merge;

public static class AnimationMerger
{
    private const ushort NONE = 0xFFFF;

    public static void Merge(MergeContext mergeContext)
    {
        var target = mergeContext.Target;
        var source = mergeContext.Source;

        if (source.Animations == null || source.Animations.Length == 0)
        {
            return;
        }

        int transformOffset = target.AllAnimationTransforms?.Length ?? 0;
        int addTransforms = source.AllAnimationTransforms?.Length ?? 0;

        if (addTransforms > 0)
        {
            var newTransforms = new Animation[transformOffset + addTransforms];
            if (transformOffset > 0)
            {
                Array.Copy(target.AllAnimationTransforms, 0, newTransforms, 0, transformOffset);
            }

            Array.Copy(source.AllAnimationTransforms, 0, newTransforms, transformOffset, addTransforms);
            target.AllAnimationTransforms = newTransforms;
        }

        int oldAnimCount = target.Animations?.Length ?? 0;
        var newAnims = new AnimationData[oldAnimCount + source.Animations.Length];
        if (oldAnimCount > 0)
        {
            Array.Copy(target.Animations!, 0, newAnims, 0, oldAnimCount);
        }

        int[]? boneMap = mergeContext.Options.ResolveBonesToTarget
            ? mergeContext.SourceToTargetBoneMap
            : null;
        int stringOffset = mergeContext.StringOffset;

        int targetBoneCount = target.header.BoneCount;

        for (int i = 0; i < source.Animations.Length; i++)
        {
            var sourceAnimation = source.Animations[i];
            var destinationHeader = sourceAnimation.Header;
            destinationHeader.szoption = StringTableMerger.RemapStringOffset(source, destinationHeader.szoption, stringOffset);
            var destinationAnimation = new AnimationData
            {
                Header = destinationHeader,
                Keyframes = sourceAnimation.Keyframes,
                Name = sourceAnimation.Name
            };

            var src = sourceAnimation.BoneTransformIndices;
            int keyframes = sourceAnimation.Header.keyframe_count;
            int sourceBones = src.GetLength(1);

            ushort[,] dst;
            if (boneMap != null)
            {
                dst = new ushort[keyframes, targetBoneCount];
                for (int k = 0; k < keyframes; k++)
                {
                    for (int b = 0; b < targetBoneCount; b++)
                    {
                        dst[k, b] = NONE;
                    }
                }

                for (int sourceBone = 0; sourceBone < sourceBones; sourceBone++)
                {
                    if (sourceBone >= boneMap.Length || boneMap[sourceBone] < 0)
                    {
                        throw new InvalidOperationException(
                            $"Animation '{sourceAnimation.Name}' in '{Label(mergeContext)}' references unresolved source bone {sourceBone}.");
                    }

                    int mappedTargetBone = boneMap[sourceBone];
                    if (mappedTargetBone >= targetBoneCount)
                    {
                        throw new InvalidOperationException(
                            $"Animation '{sourceAnimation.Name}' in '{Label(mergeContext)}' maps source bone {sourceBone} to {mappedTargetBone}, outside target bone count {targetBoneCount}.");
                    }

                    for (int k = 0; k < keyframes; k++)
                    {
                        ushort ix = src[k, sourceBone];
                        dst[k, mappedTargetBone] = ix == NONE ? NONE : (ushort)(ix + transformOffset);
                    }
                }
            }
            else
            {
                dst = new ushort[keyframes, sourceBones];
                for (int k = 0; k < keyframes; k++)
                {
                    for (int b = 0; b < sourceBones; b++)
                    {
                        ushort ix = src[k, b];
                        dst[k, b] = ix == NONE ? NONE : (ushort)(ix + transformOffset);
                    }
                }
            }

            destinationAnimation.BoneTransformIndices = dst;
            newAnims[oldAnimCount + i] = destinationAnimation;
        }

        target.Animations = newAnims;

        if (source.animationNames != null && source.animationNames.Length > 0)
        {
            var old = target.animationNames ?? Array.Empty<string>();
            var merged = new string[old.Length + source.animationNames.Length];
            if (old.Length > 0)
            {
                Array.Copy(old, merged, old.Length);
            }

            Array.Copy(source.animationNames, 0, merged, old.Length, source.animationNames.Length);
            target.animationNames = merged;
        }

        if (source.animationNameOffsets != null && source.animationNameOffsets.Length > 0)
        {
            var old = target.animationNameOffsets ?? Array.Empty<uint>();
            var merged = new uint[old.Length + source.animationNameOffsets.Length];
            if (old.Length > 0)
            {
                Array.Copy(old, merged, old.Length);
            }

            for (int i = 0; i < source.animationNameOffsets.Length; i++)
            {
                merged[old.Length + i] = StringTableMerger.RemapStringOffset(source, source.animationNameOffsets[i], stringOffset);
            }

            target.animationNameOffsets = merged;
        }

        target.header.AnimFileCount = (byte)(target.Animations?.Length ?? 0);
        target.header.AnimCount = (uint)(target.AllAnimationTransforms?.Length ?? 0);
        target.header.KeyframeCount = (uint)(target.Animations?.Sum(a => a.Header.keyframe_count) ?? 0);
    }

    private static string Label(MergeContext mergeContext)
    {
        return string.IsNullOrWhiteSpace(mergeContext.Options.SourceLabel)
            ? "<source>"
            : mergeContext.Options.SourceLabel;
    }
}
