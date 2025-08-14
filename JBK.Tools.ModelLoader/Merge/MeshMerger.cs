using JBK.Tools.ModelLoader.GbFormat.Meshes;

namespace JBK.Tools.ModelLoader.Merge
{
    public static class MeshMerger
    {
        public static void Merge(MergeContext mergeContext)
        {
            if (mergeContext.Source.meshes == null || mergeContext.Source.meshes.Length == 0) return;
            int oldCount = mergeContext.Target.meshes?.Length ?? 0;
            int addCount = mergeContext.Source.meshes.Length;
            Mesh[] newMeshes = new Mesh[oldCount + addCount];
            if (oldCount > 0) Array.Copy(mergeContext.Target.meshes, 0, newMeshes, 0, oldCount);
            Array.Copy(mergeContext.Source.meshes, 0, newMeshes, oldCount, addCount);
            mergeContext.Target.meshes = newMeshes;
            mergeContext.Target.header.MeshCount = (byte)newMeshes.Length;
        }
    }
}
