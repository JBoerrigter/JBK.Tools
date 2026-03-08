using JBK.Tools.ModelLoader.Diagnostics;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Animations;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class AnimationParser
{
    public static void Parse(Model model, BinaryReader reader)
    {
        model.Animations = new AnimationData[model.header.AnimFileCount];

        for (int clipIndex = 0; clipIndex < model.header.AnimFileCount; clipIndex++)
        {
            long clipStart = reader.BaseStream.Position;

            var clipHeader = new AnimationHeader
            {
                szoption = reader.ReadUInt32(),
                keyframe_count = reader.ReadUInt16()
            };

            long keyframesStart = reader.BaseStream.Position;
            var keyframes = new Keyframe[clipHeader.keyframe_count];
            for (int keyIndex = 0; keyIndex < clipHeader.keyframe_count; keyIndex++)
            {
                keyframes[keyIndex].time = reader.ReadUInt16();
                keyframes[keyIndex].option = reader.ReadUInt32();
            }

            long indexMapStart = reader.BaseStream.Position;
            var boneTransformIndices = new ushort[clipHeader.keyframe_count, model.header.BoneCount];
            for (int keyIndex = 0; keyIndex < clipHeader.keyframe_count; keyIndex++)
            {
                for (int boneIndex = 0; boneIndex < model.header.BoneCount; boneIndex++)
                {
                    long indexOffset = reader.BaseStream.Position;
                    ushort transformIndex = reader.ReadUInt16();
                    boneTransformIndices[keyIndex, boneIndex] = transformIndex;

                    if (transformIndex >= model.header.AnimCount)
                    {
                        GbTrace.Warn(
                            $"Animation transform index out of range at 0x{indexOffset:X}: clip={clipIndex} key={keyIndex} bone={boneIndex} index={transformIndex} poolCount={model.header.AnimCount}");
                    }
                }
            }

            model.Animations[clipIndex] = new AnimationData
            {
                Header = clipHeader,
                Keyframes = keyframes,
                BoneTransformIndices = boneTransformIndices,
                Name = string.Empty
            };

            if (GbTrace.TraceEnabled)
            {
                GbTrace.Chunk($"AnimClip[{clipIndex}].Header", clipStart, keyframesStart - clipStart, keyframesStart);
                GbTrace.Chunk(
                    $"AnimClip[{clipIndex}].Keyframes",
                    keyframesStart,
                    indexMapStart - keyframesStart,
                    indexMapStart);
                GbTrace.Chunk(
                    $"AnimClip[{clipIndex}].IndexMap",
                    indexMapStart,
                    reader.BaseStream.Position - indexMapStart,
                    reader.BaseStream.Position);
            }
        }

        uint totalTransforms = model.header.AnimCount;
        long poolStart = reader.BaseStream.Position;
        model.AllAnimationTransforms = new Animation[totalTransforms];
        for (int transformIndex = 0; transformIndex < totalTransforms; transformIndex++)
        {
            model.AllAnimationTransforms[transformIndex].pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[transformIndex].quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            model.AllAnimationTransforms[transformIndex].scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        if (GbTrace.TraceEnabled)
        {
            GbTrace.Chunk("AnimPool", poolStart, reader.BaseStream.Position - poolStart, reader.BaseStream.Position);
        }

        GbTrace.Animation(
            $"parsed clips={model.Animations.Length} pool={model.AllAnimationTransforms.Length} bones={model.header.BoneCount} keyframesTotal={model.header.KeyframeCount} timeUnit=milliseconds");
    }

    public static void ResolveStringDataAndTrace(Model model)
    {
        if (model.Animations == null || model.Animations.Length == 0)
        {
            model.animationNames = Array.Empty<string>();
            model.animationNameOffsets = Array.Empty<uint>();
            return;
        }

        model.animationNames = new string[model.Animations.Length];
        model.animationNameOffsets = new uint[model.Animations.Length];

        for (int clipIndex = 0; clipIndex < model.Animations.Length; clipIndex++)
        {
            var clip = model.Animations[clipIndex];
            clip.Name = ResolveString(model, clip.Header.szoption);
            model.Animations[clipIndex] = clip;
            model.animationNames[clipIndex] = clip.Name;
            model.animationNameOffsets[clipIndex] = clip.Header.szoption;
        }

        if (!GbTrace.AnimationTraceEnabled)
        {
            return;
        }

        for (int clipIndex = 0; clipIndex < model.Animations.Length; clipIndex++)
        {
            var clip = model.Animations[clipIndex];
            ushort keyCount = clip.Header.keyframe_count;
            int durationMs = keyCount > 0 ? clip.Keyframes[keyCount - 1].time : 0;
            float inferredFps = (keyCount > 1 && durationMs > 0)
                ? (keyCount - 1) * 1000.0f / durationMs
                : 0.0f;

            string clipName = string.IsNullOrWhiteSpace(clip.Name) ? $"clip_{clipIndex}" : clip.Name;
            GbTrace.Animation(
                $"clip[{clipIndex}] name='{clipName}' optionOffset=0x{clip.Header.szoption:X} keys={keyCount} durationMs={durationMs} inferredFps={inferredFps:F3}");
            GbTrace.Animation(
                $"clip[{clipIndex}] tracks={model.header.BoneCount} mapping=boneIndex channelMask=TRS");

            for (int boneIndex = 0; boneIndex < model.header.BoneCount; boneIndex++)
            {
                int validSamples = 0;
                int invalidSamples = 0;
                ushort minIndex = ushort.MaxValue;
                ushort maxIndex = 0;

                int translationUnique = 0;
                int rotationUnique = 0;
                int scaleUnique = 0;
                HashSet<string> uniqueTranslations = new(StringComparer.Ordinal);
                HashSet<string> uniqueRotations = new(StringComparer.Ordinal);
                HashSet<string> uniqueScales = new(StringComparer.Ordinal);

                for (int keyIndex = 0; keyIndex < keyCount; keyIndex++)
                {
                    ushort transformIndex = clip.BoneTransformIndices[keyIndex, boneIndex];
                    if (transformIndex < minIndex) minIndex = transformIndex;
                    if (transformIndex > maxIndex) maxIndex = transformIndex;

                    if (transformIndex >= model.AllAnimationTransforms.Length)
                    {
                        invalidSamples++;
                        continue;
                    }

                    validSamples++;
                    var sample = model.AllAnimationTransforms[transformIndex];
                    uniqueTranslations.Add($"{sample.pos.X:F5}|{sample.pos.Y:F5}|{sample.pos.Z:F5}");
                    uniqueRotations.Add($"{sample.quat.X:F6}|{sample.quat.Y:F6}|{sample.quat.Z:F6}|{sample.quat.W:F6}");
                    uniqueScales.Add($"{sample.scale.X:F5}|{sample.scale.Y:F5}|{sample.scale.Z:F5}");
                }

                translationUnique = uniqueTranslations.Count;
                rotationUnique = uniqueRotations.Count;
                scaleUnique = uniqueScales.Count;

                int startTime = keyCount > 0 ? clip.Keyframes[0].time : 0;
                int endTime = keyCount > 0 ? clip.Keyframes[keyCount - 1].time : 0;
                string indexRange = keyCount > 0 ? $"{minIndex}..{maxIndex}" : "n/a";

                GbTrace.Animation(
                    $"clip[{clipIndex}] track[{boneIndex}] target=bone_{boneIndex} mask=TRS keys={keyCount} valid={validSamples} invalid={invalidSamples} time={startTime}..{endTime} indexRange={indexRange} unique(T/R/S)={translationUnique}/{rotationUnique}/{scaleUnique}");
            }
        }
    }

    public static void RunSanityChecks(Model model)
    {
        if (model.AllAnimationTransforms == null || model.Animations == null)
        {
            return;
        }

        int nanTransformCount = 0;
        int hugeTranslationCount = 0;
        int invalidQuaternionCount = 0;

        for (int i = 0; i < model.AllAnimationTransforms.Length; i++)
        {
            var sample = model.AllAnimationTransforms[i];
            if (!IsFinite(sample.pos) || !IsFinite(sample.quat) || !IsFinite(sample.scale))
            {
                nanTransformCount++;
                continue;
            }

            if (sample.pos.LengthSquared() > 50000.0f * 50000.0f)
            {
                hugeTranslationCount++;
            }

            float qLenSq = sample.quat.LengthSquared();
            if (qLenSq < 1e-8f || qLenSq > 4.0f)
            {
                invalidQuaternionCount++;
            }
        }

        if (nanTransformCount > 0)
        {
            GbTrace.Warn($"Animation sanity: NaN/Inf transforms={nanTransformCount}");
        }

        if (hugeTranslationCount > 0)
        {
            GbTrace.Warn($"Animation sanity: unusually large translations={hugeTranslationCount}");
        }

        if (invalidQuaternionCount > 0)
        {
            GbTrace.Warn($"Animation sanity: abnormal quaternion lengths={invalidQuaternionCount}");
        }

        int outOfRangeIndexCount = 0;
        int emptyTrackCount = 0;

        for (int clipIndex = 0; clipIndex < model.Animations.Length; clipIndex++)
        {
            var clip = model.Animations[clipIndex];
            for (int boneIndex = 0; boneIndex < model.header.BoneCount; boneIndex++)
            {
                int validForTrack = 0;
                for (int keyIndex = 0; keyIndex < clip.Header.keyframe_count; keyIndex++)
                {
                    ushort index = clip.BoneTransformIndices[keyIndex, boneIndex];
                    if (index >= model.AllAnimationTransforms.Length)
                    {
                        outOfRangeIndexCount++;
                    }
                    else
                    {
                        validForTrack++;
                    }
                }

                if (clip.Header.keyframe_count > 0 && validForTrack == 0)
                {
                    emptyTrackCount++;
                    GbTrace.Warn(
                        $"Animation sanity: empty track clip={clipIndex} bone={boneIndex} keys={clip.Header.keyframe_count}");
                }
            }
        }

        if (outOfRangeIndexCount > 0)
        {
            GbTrace.Warn($"Animation sanity: out-of-range transform references={outOfRangeIndexCount}");
        }

        if (emptyTrackCount > 0)
        {
            GbTrace.Warn($"Animation sanity: empty tracks={emptyTrackCount}");
        }
    }

    private static string ResolveString(Model model, uint offset)
    {
        if (model.stringTable == null || model.stringTable.Length == 0)
        {
            return string.Empty;
        }

        if (offset >= model.stringTable.Length)
        {
            return string.Empty;
        }

        return model.GetString(offset);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }

    private static bool IsFinite(Quaternion value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z) && float.IsFinite(value.W);
    }
}
