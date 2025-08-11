using JBK.Tools.ModelLoader.GbFormat;
using JBK.Tools.ModelLoader.GbFormat.Animations;
using JBK.Tools.ModelLoader.GbFormat.Bones;
using JBK.Tools.ModelLoader.GbFormat.Collisions;
using JBK.Tools.ModelLoader.GbFormat.Materials;
using JBK.Tools.ModelLoader.GbFormat.Meshes;
using System.Text;

namespace JBK.Tools.ModelLoader.FileReader;

public class Model
{
    public NormalizedHeader header;
    public Bone[] bones;
    public MaterialKey[] materialData;
    public MaterialFrame[] materialFrames;
    public Mesh[] meshes;
    public byte[] stringTable;

    public AnimationData[] Animations;
    public Animation[] AllAnimationTransforms; // All unique transforms for all animations

    public CollisionHeader? collisionHeader;
    public CollisionNode[] collisionNodes;
    public uint[] animationNameOffsets;
    public string[] animationNames;
    public MaterialFrame[][] materialFramesByMaterial; // [materialIndex][frameIndex]

    public string GetString(uint offset)
    {
        int i = (int)offset;
        List<byte> bytes = new();

        while (i < stringTable.Length && stringTable[i] != 0)
            bytes.Add(stringTable[i++]);

        return Encoding.ASCII.GetString(bytes.ToArray());
    }
}
