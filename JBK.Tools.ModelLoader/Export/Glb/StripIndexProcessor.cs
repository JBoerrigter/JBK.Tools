namespace JBK.Tools.ModelLoader.Export.Glb;

public class StripIndexProcessor : IIndexProcessor
{
    public int[] Process(ushort[] indices)
    {
        var triList = new List<int>();
        for (int i = 0; i < indices.Length - 2; i++)
        {
            if (i % 2 == 0)
            {
                triList.Add(indices[i]);
                triList.Add(indices[i + 1]);
                triList.Add(indices[i + 2]);
            }
            else
            {
                triList.Add(indices[i + 1]);
                triList.Add(indices[i]);
                triList.Add(indices[i + 2]);
            }
        }
        return triList.ToArray();
    }
}
