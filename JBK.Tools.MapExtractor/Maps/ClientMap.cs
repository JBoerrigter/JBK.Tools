namespace JBK.Tools.MapExtractor.Maps;

public class ClientMap
{
    private const int HEADER_LENGTH = 52;
    private byte[] _header;

    public int X { get; set; }
    public int Y { get; set; }
    public HeightMap HeightMap { get; private set; }
    public ColorMap ColorMap { get; private set; }
    public ObjectMap ObjectMap { get; private set; }
    public TextureMap[] TextureMaps { get; private set; }

    public ClientMap(string fileName)
    {
        _header = new byte[HEADER_LENGTH];
        TextureMaps  = new TextureMap[7];
        using FileStream input = new(Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read);
        using BinaryReader binaryReader = new(input);
        for (int i = 0; i < HEADER_LENGTH; i++)
        {
            _header[i] = binaryReader.ReadByte();
        }
        X = _header[8];
        Y = _header[12];
        for (int j = 0; j < 7; j++)
        {
            if (_header[37 + j] != byte.MaxValue)
            {
                TextureMaps[6 - j] = new TextureMap(binaryReader);
            }
        }
        HeightMap = new HeightMap(binaryReader);
        ColorMap = new ColorMap(binaryReader);
        ObjectMap = new ObjectMap(binaryReader);
    }
}
