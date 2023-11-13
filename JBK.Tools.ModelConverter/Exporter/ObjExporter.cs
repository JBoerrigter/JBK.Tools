using System.Globalization;
using System.Numerics;

namespace JBK.Tools.ModelConverter.Exporter;

public class OBJExporter
{
    public static void ExportToObj(string filePath, List<Vector3> vertices, List<Vector2> textureCoords, List<Vector3> normals, List<int[]> faces)
    {
        using StreamWriter writer = new(filePath);

        foreach (var vertex in vertices)
        {
            writer.WriteLine($"v {vertex.X.ToString(CultureInfo.InvariantCulture)} {vertex.Y.ToString(CultureInfo.InvariantCulture)} {vertex.Z.ToString(CultureInfo.InvariantCulture)}");
        }

        foreach (var texCoord in textureCoords)
        {
            writer.WriteLine($"vt {texCoord.X.ToString(CultureInfo.InvariantCulture)} {texCoord.Y.ToString(CultureInfo.InvariantCulture)}");
        }

        foreach (var normal in normals)
        {
            writer.WriteLine($"vn {normal.X.ToString(CultureInfo.InvariantCulture)} {normal.Y.ToString(CultureInfo.InvariantCulture)} {normal.Z.ToString(CultureInfo.InvariantCulture)}");
        }

        foreach (var face in faces)
        {
            writer.Write("f");
            foreach (var index in face)
            {
                // OBJ indices are 1-based, so add 1 to each index
                writer.Write($" {index + 1}/{index + 1}/{index + 1}");
            }
            writer.WriteLine();
        }
    }
}
