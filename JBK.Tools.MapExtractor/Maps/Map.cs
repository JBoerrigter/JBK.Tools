namespace JBK.Tools.MapExtractor.Maps;

public interface IMap
{
	void ReadMap(BinaryReader reader);
	Bitmap GetImage();
}
