namespace JBK.Tools.MapExtractor.Maps;

public class ColorMap : IMap
{
	private int[,,] _map = new int[256, 256, 3];

	public int[,,] Map => _map;

	public ColorMap(BinaryReader reader)
	{
		ReadMap(reader);
	}

	public void ReadMap(BinaryReader reader)
	{
		for (int num = 255; num >= 0; num--)
		{
			for (int i = 0; i <= 255; i++)
			{
				_map[i, num, 2] = reader.ReadByte();
				_map[i, num, 1] = reader.ReadByte();
				_map[i, num, 0] = reader.ReadByte();
			}
		}
	}

	public Bitmap GetImage()
	{
		Bitmap bitmap = new Bitmap(256, 256);
		for (int num = 255; num >= 0; num--)
		{
			for (int i = 0; i <= 255; i++)
			{
				bitmap.SetPixel(i, num, Color.FromArgb(_map[i, num, 0], _map[i, num, 1], _map[i, num, 2]));
			}
		}
		return bitmap;
	}
}
