using ImageMagick;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace JBK.Tools.MapExtractor.Maps;

public class HeightMap : IMap
{
    private int[,] _map = new int[257, 257];
    private byte _previewScale = 16;

    public int[,] Map => _map;

    public HeightMap(BinaryReader reader)
    {
        ReadMap(reader);
    }

    public void ReadMap(BinaryReader reader)
    {
        for (int num = 256; num >= 0; num--)
        {
            for (int i = 0; i <= 256; i++)
            {
                _map[i, num] = reader.ReadUInt16();
            }
        }
    }

    /// <summary>
    /// Used to preview since GDI+ does not support 16-Bit Images
    /// </summary>
    public Bitmap GetImage()
    {
        Bitmap bitmap = new Bitmap(257, 257);
        for (int num = 256; num >= 0; num--)
        {
            for (int i = 0; i <= 256; i++)
            {
                bitmap.SetPixel(i, num, Color.FromArgb(_map[i, num] / _previewScale, _map[i, num] / _previewScale, _map[i, num] / _previewScale));
            }
        }
        return bitmap;
    }

    /// <summary>
    /// Save with Magick.NET because GDI+ is not able to save 16-Bit Images
    /// </summary>
    public void Save16BitHeightmap(string filePath)
    {
        int width = _map.GetLength(0);
        int height = _map.GetLength(1);

        // Create a new 16-bit grayscale image
        using (MagickImage image = new MagickImage(MagickColors.Black, width, height))
        {
            image.Depth = 16; // Set image depth to 16-bit

            // Find the maximum height value
            int maxHeight = FindMaxHeight(_map);

            // Prepare to set pixel values
            using (IPixelCollection<ushort> pixels = image.GetPixels())
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        ushort normalizedHeight = (ushort)(_map[x, y] / (float)maxHeight * 65535);
                        pixels.SetPixel(x, y, new ushort[] { normalizedHeight, normalizedHeight, normalizedHeight });
                    }
                }
            }

            // Save the image as a 16-bit grayscale TIFF
            image.Format = MagickFormat.Tiff;
            image.Write(filePath);
        }
    }

    private int FindMaxHeight(int[,] data)
    {
        int max = 0;
        foreach (int value in data)
        {
            if (value > max)
                max = value;
        }
        return max;
    }
}
