using JBK.Tools.ModelFileFormat;
using JBK.Tools.ModelLoader.Export;
using JBK.Tools.ModelLoader.Export.Glb;

Console.WriteLine("=== GB to GLB Converter ===");
Console.WriteLine();
Console.WriteLine("Enter directory to convert: ");
//string? inputPath = Console.ReadLine()?.Trim();
string? inputPath = @"C:\Users\Jascha\Desktop\t";
if (string.IsNullOrEmpty(inputPath)) return;

string outputPath = Path.Combine(inputPath, "Converted");
Directory.CreateDirectory(outputPath);

string outputFile;
ModelFileFormat? fileFormat;
IExporter exporter = new GlbExporter();

foreach (string fileName in Directory.GetFiles(inputPath, "*.gb"))
{
    fileFormat = null;
    try
    {
        fileFormat = new ModelFileFormat();
        fileFormat.Read(fileName);

        outputFile = Path.GetFileName(fileName);
        outputFile = Path.ChangeExtension(outputFile, ".glb");
        outputFile = Path.Combine(outputPath, outputFile);

        exporter.Export(
            source: fileFormat,
            texPath: Path.Combine(inputPath, "tex"),
            outputPath: outputFile);
    }
    catch (Exception ex)
    {
        if (fileFormat != null)
        {
            Console.WriteLine("Header: {0} - Exception: {1}", fileFormat.header.version, ex.Message);
        }
        else
        {
            Console.WriteLine(ex.Message);
        }
    }
}

//void Decode(byte key, byte[] output, byte[] input, int length)
//{
//    for (int i = 0; i < length; i++)
//    {
//        output[i] = JBK.Tools.ModelLoader.Decode.DecodeTable[key, input[i]];
//    }
//}

//byte key = 4;
//byte[] decodedName = new byte[64];
//Decode(key, decodedName, fileFormat.header.name, fileFormat.header.name.Length);

//Console.WriteLine("Header Information:");
//Console.WriteLine($"Version: {fileFormat.header.version}");
//Console.WriteLine($"Bone Count: {fileFormat.header.bone_count}");
//Console.WriteLine($"Flags: {fileFormat.header.flags}");
//Console.WriteLine($"Mesh Count: {fileFormat.header.mesh_count}");
//Console.WriteLine($"CRC: {fileFormat.header.crc}");
//Console.WriteLine($"Name: {System.Text.Encoding.UTF8.GetString(decodedName).TrimEnd('\0')}");
//Console.WriteLine($"String Offset: {fileFormat.header.szoption}");
//Console.WriteLine("Vertex Count: " + string.Join(", ", fileFormat.header.vertex_count));
//Console.WriteLine($"Index Count: {fileFormat.header.index_count}");
//Console.WriteLine($"Bone Index Count: {fileFormat.header.bone_index_count}");
//Console.WriteLine($"Keyframe Count: {fileFormat.header.keyframe_count}");
//Console.WriteLine($"Reserved0: {fileFormat.header.reserved0}");
//Console.WriteLine($"String Size: {fileFormat.header.string_size}");
//Console.WriteLine($"Collision Size: {fileFormat.header.cls_size}");
//Console.WriteLine($"Animation Count: {fileFormat.header.anim_count}");
//Console.WriteLine($"Animation File Count: {fileFormat.header.anim_file_count}");
//Console.WriteLine($"Reserved1: {fileFormat.header.reserved1}");
//Console.WriteLine($"Material Count: {fileFormat.header.material_count}");
//Console.WriteLine($"Material Frame Count: {fileFormat.header.material_frame_count}");
//Console.WriteLine($"Minimum: {string.Join(", ", fileFormat.header.minimum)}");
//Console.WriteLine($"Maximum: {string.Join(", ", fileFormat.header.maximum)}");
////Console.WriteLine($"Reserved2: {fileFormat.header.reserved2}");

//var materialKey = fileFormat.materialData[mesh.Header.material_ref];
//fileFormat.materialData materialKey.m_frame
//var matSource = new GltfMaterialSource
//{
//    Name = "Material_X",
//    DiffuseColor = materialKey.m_,
//    EmissiveColor = materialKey.emissive,
//    SpecularColor = materialKey.specular,
//    AmbientColor = materialKey.ambient,
//    Power = materialKey.power,
//    TextureRef = materialFrame.texture_ref,
//    TextureRef2 = materialFrame.texture_ref2,
//    Flags = materialFrame.flags
//};

//int matIndex = writer.AddMaterial(matSource.ToGltfMaterial());
//exportMesh.MaterialIndex = matIndex;