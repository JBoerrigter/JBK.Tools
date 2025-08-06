namespace JBK.Tools.ModelLoader.Export.Glb;

public class ListIndexProcessor : IIndexProcessor
{
    public int[] Process(ushort[] indices)
    {
        return indices.Select(i => (int)i).ToArray();
    }
}
