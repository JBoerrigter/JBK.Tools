namespace JBK.Tools.ModelLoader.Merge;

public static class HeaderMerger
{
    private const uint CollisionHeaderSize = sizeof(ushort) * 2 + sizeof(float) * 3 * 2;

    public static void Refresh(FileReader.Model model)
    {
        model.header.MeshCount = ToByte(model.meshes?.Length ?? 0, nameof(model.header.MeshCount));
        model.header.BoneCount = ToByte(model.bones?.Length ?? 0, nameof(model.header.BoneCount));
        model.header.MaterialCount = (uint)(model.materialData?.Length ?? 0);
        model.header.StringSize = (uint)(model.stringTable?.Length ?? 0);
        model.header.AnimFileCount = ToByte(model.Animations?.Length ?? 0, nameof(model.header.AnimFileCount));
        model.header.AnimCount = (uint)(model.AllAnimationTransforms?.Length ?? 0);
        model.header.KeyframeCount = (uint)(model.Animations?.Sum(a => a.Header.keyframe_count) ?? 0);
        model.header.IndexCount = SumIndexCount(model);
        model.header.BoneIndexCount = SumBoneIndexCount(model);
        model.header.VertexCounts = BuildVertexCounts(model);
        model.header.ClsSize = ComputeCollisionSize(model);
    }

    private static uint SumIndexCount(FileReader.Model model)
    {
        if (model.meshes == null || model.meshes.Length == 0)
        {
            return 0;
        }

        uint total = 0;
        foreach (var mesh in model.meshes)
        {
            total += (uint)(mesh.Indices?.Length ?? mesh.Header.index_count);
        }

        return total;
    }

    private static uint SumBoneIndexCount(FileReader.Model model)
    {
        if (model.meshes == null || model.meshes.Length == 0)
        {
            return 0;
        }

        uint total = 0;
        foreach (var mesh in model.meshes)
        {
            total += (uint)(mesh.BoneIndices?.Length ?? mesh.Header.bone_index_count);
        }

        return total;
    }

    private static ushort[] BuildVertexCounts(FileReader.Model model)
    {
        var counts = new uint[12];
        if (model.meshes == null || model.meshes.Length == 0)
        {
            return counts.Select(ToUShort).ToArray();
        }

        foreach (var mesh in model.meshes)
        {
            int slot = mesh.Header.vertex_type;
            if ((uint)slot >= counts.Length)
            {
                continue;
            }

            counts[slot] += (uint)(mesh.Vertecies?.Length ?? mesh.Header.vertex_count);
        }

        return counts.Select(ToUShort).ToArray();
    }

    private static uint ComputeCollisionSize(FileReader.Model model)
    {
        if (model.collisionHeader == null)
        {
            return 0;
        }

        int nodeCount = model.collisionNodes?.Length ?? 0;
        return CollisionHeaderSize + (uint)(nodeCount * System.Runtime.InteropServices.Marshal.SizeOf<GbFormat.Collisions.CollisionNode>());
    }

    private static byte ToByte(int value, string fieldName)
    {
        if ((uint)value > byte.MaxValue)
        {
            throw new InvalidOperationException($"{fieldName} exceeds byte range: {value}.");
        }

        return (byte)value;
    }

    private static ushort ToUShort(uint value)
    {
        if (value > ushort.MaxValue)
        {
            throw new InvalidOperationException($"Vertex count exceeds ushort range: {value}.");
        }

        return (ushort)value;
    }
}
