namespace JBK.Tools.MapExtractor.Maps;

public class ObjectMap : TextureMap
{
	public ObjectMap(BinaryReader reader)
		: base(reader)
	{
	}

	public override Bitmap GetImage()
	{
		Bitmap bitmap = new Bitmap(256, 256);
		for (int num = 255; num >= 0; num--)
		{
			for (int i = 0; i <= 255; i++)
			{
				bitmap.SetPixel(i, num, Color.FromArgb(base.Map[i, num], base.Map[i, num], base.Map[i, num]));
			}
		}
		return bitmap;
	}
}
