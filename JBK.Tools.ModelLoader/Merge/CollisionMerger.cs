using JBK.Tools.ModelLoader.GbFormat.Collisions;

namespace JBK.Tools.ModelLoader.Merge;

public static class CollisionMerger
{
    public static void Merge(MergeContext mergeContext)
    {
        if (mergeContext.Source.collisionHeader == null)
        {
            return;
        }

        if (mergeContext.Target.collisionHeader != null)
        {
            return;
        }

        mergeContext.Target.collisionHeader = CloneHeader(mergeContext.Source.collisionHeader.Value);
        mergeContext.Target.collisionNodes = CloneNodes(mergeContext.Source.collisionNodes);
    }

    private static CollisionHeader CloneHeader(CollisionHeader header)
    {
        return new CollisionHeader
        {
            vertex_count = header.vertex_count,
            face_count = header.face_count,
            HasBounds = header.HasBounds,
            minimum = header.minimum,
            maximum = header.maximum,
            reserved = header.reserved?.ToArray() ?? Array.Empty<uint>()
        };
    }

    private static CollisionNode[] CloneNodes(CollisionNode[]? nodes)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return Array.Empty<CollisionNode>();
        }

        var clone = new CollisionNode[nodes.Length];
        Array.Copy(nodes, clone, nodes.Length);
        return clone;
    }
}
