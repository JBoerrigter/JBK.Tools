
using JBK.Tools.ModelConverter;
using JBK.Tools.ModelConverter.Exporter;


Console.WriteLine("Choose the directory with the .gb files you want to convert:");
string? directory = Console.ReadLine();

Console.WriteLine("What type of export do you want?");
Console.WriteLine("1: .obj");
Console.WriteLine("0. None");
string? choice = Console.ReadLine();

if (choice == "1")
{
    DirectoryInfo dirInfo = new(directory);
    List<string> files = dirInfo.GetFiles()
        .Where(file => file.Extension.Equals(".gb", StringComparison.OrdinalIgnoreCase))
        .Select(file => file.FullName)
        .ToList();

    DirectoryInfo outputDir = new DirectoryInfo(Environment.CurrentDirectory);
    outputDir = outputDir.CreateSubdirectory("ConvertedModels");

    foreach (var fileName in files)
    {
        try
        {
            using FileStream stream = new(fileName, FileMode.Open);
            using BinaryReader reader = new(stream);

            GeoBinary gb = new(reader);

            if (gb is null)
            {
                Console.WriteLine("Can't convert {0}!", fileName);
                continue;
            }

            string outputFile = Path.GetFileNameWithoutExtension(fileName);

            for (int i = 0; i < gb.Meshes.Count; i++)
            {
                outputFile = Path.Combine(outputDir.FullName, outputFile + "_" + i.ToString() + ".obj");
                OBJExporter.ExportToObj(outputFile, gb.Meshes[i].Vertices, gb.Meshes[i].UVs, gb.Meshes[i].Normals, gb.Meshes[i].Faces);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}: {1}", fileName, ex.Message);
        }
    }
}