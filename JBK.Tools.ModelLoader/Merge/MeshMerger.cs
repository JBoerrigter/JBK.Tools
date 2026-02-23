using JBK.Tools.ModelLoader.GbFormat.Meshes;

namespace JBK.Tools.ModelLoader.Merge
{
    public static class MeshMerger
    {
        public static void Merge(MergeContext mergeContext)
        {
            if (mergeContext.Source.meshes == null || mergeContext.Source.meshes.Length == 0)
            {
                return;
            }

            var sourceMeshes = mergeContext.Source.meshes;
            if (mergeContext.Options.ResolveBonesToTarget)
            {
                RemapMeshBoneIndices(mergeContext, sourceMeshes);
            }

            int oldCount = mergeContext.Target.meshes?.Length ?? 0;
            int addCount = sourceMeshes.Length;
            Mesh[] newMeshes = new Mesh[oldCount + addCount];
            if (oldCount > 0)
            {
                Array.Copy(mergeContext.Target.meshes, 0, newMeshes, 0, oldCount);
            }

            Array.Copy(sourceMeshes, 0, newMeshes, oldCount, addCount);
            mergeContext.Target.meshes = newMeshes;
            mergeContext.Target.header.MeshCount = (byte)newMeshes.Length;
        }

        private static void RemapMeshBoneIndices(MergeContext mergeContext, Mesh[] meshes)
        {
            var map = mergeContext.SourceToTargetBoneMap;
            if (map == null)
            {
                throw new InvalidOperationException(
                    $"Bone remap map missing for mesh merge from '{Label(mergeContext)}'.");
            }

            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                var palette = meshes[meshIndex].BoneIndices;
                if (palette == null || palette.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < palette.Length; i++)
                {
                    int sourceBoneIndex = palette[i];
                    if (sourceBoneIndex >= map.Length || map[sourceBoneIndex] < 0)
                    {
                        throw new InvalidOperationException(
                            $"Mesh {meshIndex} in '{Label(mergeContext)}' references unresolved source bone {sourceBoneIndex}.");
                    }

                    int targetBoneIndex = map[sourceBoneIndex];
                    if (targetBoneIndex > byte.MaxValue)
                    {
                        throw new InvalidOperationException(
                            $"Mesh {meshIndex} in '{Label(mergeContext)}' maps source bone {sourceBoneIndex} to {targetBoneIndex}, exceeds byte range.");
                    }

                    palette[i] = (byte)targetBoneIndex;
                }
            }
        }

        private static string Label(MergeContext mergeContext)
        {
            return string.IsNullOrWhiteSpace(mergeContext.Options.SourceLabel)
                ? "<source>"
                : mergeContext.Options.SourceLabel;
        }
    }
}
