using JBK.Tools.ModelLoader.GbFormat.Bones;
using System.Globalization;

namespace JBK.Tools.ModelLoader.Merge;

public static class BoneMerger
{
    public static void Merge(MergeContext mergeContext)
    {
        var targetBones = mergeContext.Target.bones ?? Array.Empty<Bone>();
        var sourceBones = mergeContext.Source.bones ?? Array.Empty<Bone>();
        int requiredSourceBoneSlots = GetRequiredSourceBoneSlots(mergeContext.Source);

        if (mergeContext.Options.ResolveBonesToTarget)
        {
            if (targetBones.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Canonical merge requires target bones before processing '{Label(mergeContext)}'.");
            }

            if (sourceBones.Length == 0)
            {
                if (requiredSourceBoneSlots > targetBones.Length)
                {
                    throw new InvalidOperationException(
                        $"Source '{Label(mergeContext)}' requires {requiredSourceBoneSlots} bones, but canonical skeleton has {targetBones.Length}.");
                }

                mergeContext.SourceToTargetBoneMap = BuildIdentityMap(0, requiredSourceBoneSlots);
                return;
            }

            mergeContext.SourceToTargetBoneMap = BuildSourceToTargetMap(sourceBones, targetBones, Label(mergeContext));

            if (requiredSourceBoneSlots > mergeContext.SourceToTargetBoneMap.Length)
            {
                throw new InvalidOperationException(
                    $"Source '{Label(mergeContext)}' references {requiredSourceBoneSlots} bone slots but only {mergeContext.SourceToTargetBoneMap.Length} could be resolved.");
            }
            return;
        }

        if (sourceBones.Length == 0)
        {
            mergeContext.SourceToTargetBoneMap = Array.Empty<int>();
            return;
        }

        int oldCount = targetBones.Length;
        int addCount = sourceBones.Length;

        Bone[] newBones = new Bone[oldCount + addCount];
        if (oldCount > 0)
        {
            Array.Copy(targetBones, 0, newBones, 0, oldCount);
        }

        Array.Copy(sourceBones, 0, newBones, oldCount, addCount);

        mergeContext.Target.bones = newBones;
        mergeContext.Target.header.BoneCount = (byte)newBones.Length;
        mergeContext.SourceToTargetBoneMap = BuildIdentityMap(oldCount, addCount);
    }

    private static int[] BuildSourceToTargetMap(Bone[] sourceBones, Bone[] targetBones, string label)
    {
        if (sourceBones.Length <= targetBones.Length && IsIdentityMap(sourceBones, targetBones))
        {
            var identity = new int[sourceBones.Length];
            for (int i = 0; i < sourceBones.Length; i++)
            {
                identity[i] = i;
            }

            return identity;
        }

        var targetIndexBySignature = new Dictionary<string, Queue<int>>(StringComparer.Ordinal);
        for (int targetIndex = 0; targetIndex < targetBones.Length; targetIndex++)
        {
            var signature = CreateMatrixSignature(targetBones[targetIndex]);
            if (!targetIndexBySignature.TryGetValue(signature, out var queue))
            {
                queue = new Queue<int>();
                targetIndexBySignature[signature] = queue;
            }

            queue.Enqueue(targetIndex);
        }

        var map = new int[sourceBones.Length];
        Array.Fill(map, -1);

        for (int sourceIndex = 0; sourceIndex < sourceBones.Length; sourceIndex++)
        {
            var signature = CreateMatrixSignature(sourceBones[sourceIndex]);
            if (!targetIndexBySignature.TryGetValue(signature, out var queue) || queue.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve source bone {sourceIndex} from '{label}' to canonical skeleton.");
            }

            map[sourceIndex] = queue.Dequeue();
        }

        for (int sourceIndex = 0; sourceIndex < sourceBones.Length; sourceIndex++)
        {
            int mappedTarget = map[sourceIndex];
            byte sourceParent = sourceBones[sourceIndex].parent;
            if (sourceParent == byte.MaxValue)
            {
                continue;
            }

            if (sourceParent >= map.Length)
            {
                throw new InvalidOperationException(
                    $"Source bone {sourceIndex} from '{label}' has invalid parent index {sourceParent}.");
            }

            int mappedParent = map[sourceParent];
            byte canonicalParent = targetBones[mappedTarget].parent;
            if (canonicalParent != byte.MaxValue && canonicalParent != mappedParent)
            {
                throw new InvalidOperationException(
                    $"Bone hierarchy mismatch while resolving '{label}': source bone {sourceIndex} parent {sourceParent}->{mappedParent}, canonical parent {canonicalParent}.");
            }
        }

        return map;
    }

    private static bool IsIdentityMap(Bone[] sourceBones, Bone[] targetBones)
    {
        for (int i = 0; i < sourceBones.Length; i++)
        {
            if (!MatrixEquals(sourceBones[i], targetBones[i]))
            {
                return false;
            }

            if (sourceBones[i].parent != targetBones[i].parent)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatrixEquals(in Bone a, in Bone b)
    {
        const float epsilon = 1e-4f;
        return NearlyEqual(a.matrix.M11, b.matrix.M11, epsilon)
            && NearlyEqual(a.matrix.M12, b.matrix.M12, epsilon)
            && NearlyEqual(a.matrix.M13, b.matrix.M13, epsilon)
            && NearlyEqual(a.matrix.M14, b.matrix.M14, epsilon)
            && NearlyEqual(a.matrix.M21, b.matrix.M21, epsilon)
            && NearlyEqual(a.matrix.M22, b.matrix.M22, epsilon)
            && NearlyEqual(a.matrix.M23, b.matrix.M23, epsilon)
            && NearlyEqual(a.matrix.M24, b.matrix.M24, epsilon)
            && NearlyEqual(a.matrix.M31, b.matrix.M31, epsilon)
            && NearlyEqual(a.matrix.M32, b.matrix.M32, epsilon)
            && NearlyEqual(a.matrix.M33, b.matrix.M33, epsilon)
            && NearlyEqual(a.matrix.M34, b.matrix.M34, epsilon)
            && NearlyEqual(a.matrix.M41, b.matrix.M41, epsilon)
            && NearlyEqual(a.matrix.M42, b.matrix.M42, epsilon)
            && NearlyEqual(a.matrix.M43, b.matrix.M43, epsilon)
            && NearlyEqual(a.matrix.M44, b.matrix.M44, epsilon);
    }

    private static bool NearlyEqual(float a, float b, float epsilon)
    {
        return MathF.Abs(a - b) <= epsilon;
    }

    private static string CreateMatrixSignature(in Bone bone)
    {
        const float scale = 10000.0f;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Round(bone.matrix.M11, scale)}|{Round(bone.matrix.M12, scale)}|{Round(bone.matrix.M13, scale)}|{Round(bone.matrix.M14, scale)}|" +
            $"{Round(bone.matrix.M21, scale)}|{Round(bone.matrix.M22, scale)}|{Round(bone.matrix.M23, scale)}|{Round(bone.matrix.M24, scale)}|" +
            $"{Round(bone.matrix.M31, scale)}|{Round(bone.matrix.M32, scale)}|{Round(bone.matrix.M33, scale)}|{Round(bone.matrix.M34, scale)}|" +
            $"{Round(bone.matrix.M41, scale)}|{Round(bone.matrix.M42, scale)}|{Round(bone.matrix.M43, scale)}|{Round(bone.matrix.M44, scale)}");
    }

    private static int Round(float value, float scale)
    {
        return (int)MathF.Round(value * scale, MidpointRounding.AwayFromZero);
    }

    private static int[] BuildIdentityMap(int oldCount, int addCount)
    {
        var map = new int[addCount];
        for (int i = 0; i < addCount; i++)
        {
            map[i] = oldCount + i;
        }

        return map;
    }

    private static int GetRequiredSourceBoneSlots(JBK.Tools.ModelLoader.FileReader.Model source)
    {
        int required = source.header.BoneCount;

        if (source.meshes != null)
        {
            for (int meshIndex = 0; meshIndex < source.meshes.Length; meshIndex++)
            {
                var palette = source.meshes[meshIndex].BoneIndices;
                if (palette == null || palette.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < palette.Length; i++)
                {
                    int slot = palette[i] + 1;
                    if (slot > required)
                    {
                        required = slot;
                    }
                }
            }
        }

        if (source.Animations != null)
        {
            for (int i = 0; i < source.Animations.Length; i++)
            {
                int slot = source.Animations[i].BoneTransformIndices.GetLength(1);
                if (slot > required)
                {
                    required = slot;
                }
            }
        }

        return required;
    }

    private static string Label(MergeContext mergeContext)
    {
        return string.IsNullOrWhiteSpace(mergeContext.Options.SourceLabel)
            ? "<source>"
            : mergeContext.Options.SourceLabel;
    }
}
