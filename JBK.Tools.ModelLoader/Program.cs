using JBK.Tools.ModelLoader;
using JBK.Tools.ModelLoader.Export.Glb;
using JBK.Tools.ModelLoader.FileReader;


string meshFile = @"C:\Users\Jascha\Desktop\Sample Assets\Meshes\house_01.gb";
var mHouse = GbFileLoader.LoadFromFile(meshFile);
GlbExporter exporter = new GlbExporter();
string texPath = @"C:\Users\Jascha\Desktop\Sample Assets\Meshes\tex";
string exportPath = @"C:\Users\Jascha\Desktop\Sample Assets\Meshes\house_01.glb";
exporter.Export(mHouse, texPath, exportPath);
return;
/*
string meshFile1 = @"C:\Users\Jascha\Desktop\t\M001_B1.gb";
string meshFile2 = @"C:\Users\Jascha\Desktop\t\M001_H1.gb";
string boneFile = @"C:\Users\Jascha\Desktop\t\T001_Bone.gb";
string animationFile1 = @"C:\Users\Jascha\Desktop\t\T001_0_A_01.gb";
string animationFile2 = @"C:\Users\Jascha\Desktop\t\T001_0_D_01.gb";
string animationFile3 = @"C:\Users\Jascha\Desktop\t\T001_1_A_01.gb";
string testFile = @"C:\Users\Jascha\Desktop\t\w_mill.gb";
*/
//Model model = GbFileLoader.LoadFromFile(testFile);

//string texPath = @"C:\Users\Jascha\Desktop\t\tex";

//var m1 = GbFileLoader.LoadFromFile(@"C:\Users\Jascha\Desktop\Neuer Ordner\M001_B1.gb");
//Console.WriteLine("bla");

//return;

string folder = @"C:\Users\Jascha\Desktop\Sample Assets\Meshes";
DirectoryInfo dir = new DirectoryInfo(folder);

Model model = null;
foreach(var file in dir.GetFiles("*.gb"))
{
    try
    {
        if (model is null)
        {
            model = GbFileLoader.LoadFromFile(file.FullName);
        }else
        {
            GbFileLoader.Append(model, file.FullName);
        }
            
    }
    catch(NotSupportedException ex)
    {
        Console.WriteLine($"Can not process {file.Name}");
        Console.WriteLine(ex.Message);
    }
}
/*
if (model is not null)
{
GlbExporter exporter = new GlbExporter();
string texPath = dir.FullName + @"\tex";
string exportPath = dir.FullName + @"\test.glb";
exporter.Export(model, texPath, exportPath);
Console.WriteLine("Test");
}
*/

//string meshFile1 = @"C:\Games\Naraeha Reignited\data\UI\Quest\Daily_Quest\UI-quest01-1.gb";




//model = GbFileLoader.Append(model, meshFile2);
//model = GbFileLoader.Append(model, boneFile);
//model = GbFileLoader.Append(model, animationFile1);
//model = GbFileLoader.Append(model, animationFile2);
//model = GbFileLoader.Append(model, animationFile3);

//string outputPath = @"C:\Users\Jascha\Desktop\t\Converted\M001.glb";
//IExporter exporter = new GlbExporter();

//exporter.Export(
//            source: model,
//  //          texPath: texPath,
//            outputPath: outputPath);

/*
Console.WriteLine("=== GB to GLB Converter ===");
Console.WriteLine();
Console.WriteLine("Enter directory to convert: ");
string? inputPath = Console.ReadLine()?.Trim();
//inputPath = @"C:\Users\Jascha\Desktop\t";
if (string.IsNullOrEmpty(inputPath)) return;

string outputPath = Path.Combine(inputPath, "Converted");
Directory.CreateDirectory(outputPath);

string outputFile;
IExporter exporter = new GlbExporter();

foreach (string fileName in Directory.GetFiles(inputPath, "*.gb"))
{
    try
    {
        Model model = GbFileLoader.LoadFromFile(fileName);

        outputFile = Path.GetFileName(fileName);
        outputFile = Path.ChangeExtension(outputFile, ".glb");
        outputFile = Path.Combine(outputPath, outputFile);

        exporter.Export(
            source: model,
            texPath: Path.Combine(inputPath, "tex"),
            outputPath: outputFile);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
*/
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