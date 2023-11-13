
using JBK.Tools.ModelConverter;
using JBK.Tools.ModelConverter.Exporter;
using System.Diagnostics;

Console.WriteLine("What type of export do you want?");
Console.WriteLine("1: .obj");
Console.WriteLine("0. None");
string? choice = Console.ReadLine();

string[] files = args;

if (Debugger.IsAttached)
{
    files = new string[] { /* test file here ( debugging ) */ };
}

if (choice == "1")
{
    foreach (var fileName in files)
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
            outputFile = Path.Combine(Environment.CurrentDirectory, outputFile + "_" + i.ToString() + ".obj");
            OBJExporter.ExportToObj(outputFile, gb.Meshes[i].Vertices, gb.Meshes[i].UVs, gb.Meshes[i].Normals, gb.Meshes[i].Faces);
        }
    }
}