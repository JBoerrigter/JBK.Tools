namespace JBK.Tools.MapExtractor.Maps;

public class TextureMap : IMap
{
	private byte[,] _map = new byte[256, 256];

	public byte[,] Map => _map;

	public TextureMap(BinaryReader reader)
	{
		ReadMap(reader);
	}

	public void ReadMap(BinaryReader reader)
	{
		for (int num = 255; num >= 0; num--)
		{
			for (int i = 0; i <= 255; i++)
			{
				_map[i, num] = reader.ReadByte();
			}
		}
	}

	public virtual Bitmap GetImage()
	{
		Bitmap bitmap = new Bitmap(256, 256);
		for (int num = 255; num >= 0; num--)
		{
			for (int i = 0; i <= 255; i++)
			{
				bitmap.SetPixel(i, num, Color.FromArgb(_map[i, num], _map[i, num], _map[i, num]));
			}
		}
		return bitmap;
	}
}
