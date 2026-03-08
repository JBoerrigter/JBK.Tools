using JBK.Tools.ModelLoader.GbFormat.Materials;

namespace JBK.Tools.ModelLoader.Merge;

public static class MaterialMerger
{
    public static void Merge(MergeContext mergeContext)
    {
        if (mergeContext.Source.materialData == null || mergeContext.Source.materialData.Length == 0)
        {
            return;
        }

        int targetMaterialCount = mergeContext.Target.materialData?.Length ?? 0;
        int sourceMaterialCount = mergeContext.Source.materialData.Length;
        int stringOffset = mergeContext.StringOffset;

        MergeMaterialKeys(mergeContext.Target, mergeContext.Source, targetMaterialCount, stringOffset);
        MergeMaterialFrames(mergeContext.Target, mergeContext.Source, targetMaterialCount, sourceMaterialCount);
        RemapMeshMaterialReferences(mergeContext.Source, targetMaterialCount);

        mergeContext.Target.header.MaterialCount = (uint)mergeContext.Target.materialData.Length;

        if (mergeContext.Target.header.MaterialFrameCount == 0)
        {
            mergeContext.Target.header.MaterialFrameCount = mergeContext.Source.header.MaterialFrameCount;
        }
    }

    private static void MergeMaterialKeys(
        FileReader.Model target,
        FileReader.Model source,
        int targetMaterialCount,
        int stringOffset)
    {
        var targetMaterials = target.materialData ?? Array.Empty<MaterialKey>();
        var merged = new MaterialKey[targetMaterialCount + source.materialData.Length];

        if (targetMaterialCount > 0)
        {
            Array.Copy(targetMaterials, merged, targetMaterialCount);
        }

        for (int i = 0; i < source.materialData.Length; i++)
        {
            var material = source.materialData[i];
            material.szTexture = StringTableMerger.RemapStringOffset(source, material.szTexture, stringOffset);
            material.szoption = StringTableMerger.RemapStringOffset(source, material.szoption, stringOffset);
            material.m_frame = StringTableMerger.RemapBlobOffset(material.m_frame, stringOffset);
            merged[targetMaterialCount + i] = material;
        }

        target.materialData = merged;
    }

    private static void MergeMaterialFrames(
        FileReader.Model target,
        FileReader.Model source,
        int targetMaterialCount,
        int sourceMaterialCount)
    {
        var targetFrames = target.materialFramesByMaterial ?? Array.Empty<MaterialFrame[]>();
        var sourceFrames = source.materialFramesByMaterial ?? Array.Empty<MaterialFrame[]>();
        var merged = new MaterialFrame[targetMaterialCount + sourceMaterialCount][];

        for (int i = 0; i < targetMaterialCount; i++)
        {
            merged[i] = i < targetFrames.Length ? targetFrames[i] : Array.Empty<MaterialFrame>();
        }

        for (int i = 0; i < sourceMaterialCount; i++)
        {
            merged[targetMaterialCount + i] = i < sourceFrames.Length ? sourceFrames[i] : Array.Empty<MaterialFrame>();
        }

        target.materialFramesByMaterial = merged;
    }

    private static void RemapMeshMaterialReferences(FileReader.Model source, int materialOffset)
    {
        if (materialOffset == 0 || source.meshes == null)
        {
            return;
        }

        for (int i = 0; i < source.meshes.Length; i++)
        {
            if (source.meshes[i].Header.material_ref >= 0)
            {
                source.meshes[i].Header.material_ref += materialOffset;
            }
        }
    }
}
