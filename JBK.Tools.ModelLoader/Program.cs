
using JBK.Tools.ModelFileFormat;
using JBK.Tools.ModelLoader;

string fileName;
fileName = @"D:\ObjTest\[ae]w_mill_house.gb";
fileName = @"D:\ObjTest\[a]b_ship01.gb";
//fileName = @"D:\ObjTest\P001_b2.gb"; 

ModelFileFormat fileFormat = new ModelFileFormat();
fileFormat.Read(fileName);


string exportPath = "C:\\Users\\Jascha\\Desktop\\test.glb";
GlbExporter.ExportToGlb(fileFormat, exportPath);



return;

//void Decode(byte key, byte[] output, byte[] input, int length)
//{
//    for (int i = 0; i < length; i++)
//    {
//        output[i] = JBK.Tools.ModelLoader.Decode.DecodeTable[key, input[i]];
//    }
//}

//List<ushort> ConvertTriangleStripToList(ushort[] strip)
//{
//    var list = new List<ushort>();
//    for (int i = 0; i + 2 < strip.Length; i++)
//    {
//        ushort i0 = strip[i];
//        ushort i1 = strip[i + 1];
//        ushort i2 = strip[i + 2];

//        // Skip degenerate triangles
//        if (i0 == i1 || i1 == i2 || i0 == i2)
//            continue;

//        if (i % 2 == 0)
//            list.AddRange(new[] { i0, i1, i2 });
//        else
//            list.AddRange(new[] { i1, i0, i2 });
//    }
//    return list;
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

//var writer = new GltfWriter();
//foreach (Mesh mesh in fileFormat.meshes)
//{
//    var exportMesh = new GltfMesh { Name = System.Text.Encoding.UTF8.GetString(decodedName).TrimEnd('\0') };

//    switch ((VertexType)mesh.Header.vertex_type)
//    {
//        case VertexType.Rigid:
//            IEnumerable<VertexRigid> rigids = mesh.Vertecies.OfType<VertexRigid>();
//            exportMesh.Positions.AddRange(rigids.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(rigids.Select(r => r.TexCoord));
//            exportMesh.Normals.AddRange(rigids.Select(r => r.Normal));

//            writer.AddMesh(exportMesh);
//            break;
//        case VertexType.RigidDouble:
//            IEnumerable<VertexRigidDouble> dr = mesh.Vertecies.OfType<VertexRigidDouble>();
//            exportMesh.Positions.AddRange(dr.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(dr.Select(r => r.TexCoord0));
//            exportMesh.Normals.AddRange(dr.Select(r => r.Normal));
//            exportMesh.UVs1.AddRange(dr.Select(r => r.TexCoord1));

//            writer.AddMesh(exportMesh);
//            break;
//        case VertexType.Blend1:
//            IEnumerable<VertexBlend3> b1 = mesh.Vertecies.OfType<VertexBlend3>();
//            exportMesh.Positions.AddRange(b1.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(b1.Select(r => r.TexCoord));
//            exportMesh.Normals.AddRange(b1.Select(r => r.Normal));

//            writer.AddMesh(exportMesh);
//            break;
//        case VertexType.Blend2:
//            IEnumerable<VertexBlend2> b2 = mesh.Vertecies.OfType<VertexBlend2>();
//            exportMesh.Positions.AddRange(b2.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(b2.Select(r => r.TexCoord));
//            exportMesh.Normals.AddRange(b2.Select(r => r.Normal));

//            writer.AddMesh(exportMesh);
//            break;
//        case VertexType.Blend3:
//            IEnumerable<VertexBlend3> b3 = mesh.Vertecies.OfType<VertexBlend3>();
//            exportMesh.Positions.AddRange(b3.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(b3.Select(r => r.TexCoord));
//            exportMesh.Normals.AddRange(b3.Select(r => r.Normal));

//            writer.AddMesh(exportMesh);
//            break;
//        case VertexType.Blend4:
//            IEnumerable<VertexBlend4> b4 = mesh.Vertecies.OfType<VertexBlend4>();
//            exportMesh.Positions.AddRange(b4.Select(r => r.Position));
//            if (mesh.Header.face_type == 0) // FT_LIST
//                exportMesh.Indices.AddRange(mesh.Indices);
//            else if (mesh.Header.face_type == 1) // FT_STRIP
//                exportMesh.Indices.AddRange(ConvertTriangleStripToList(mesh.Indices));
//            else
//                throw new Exception("Unsupported face type: " + mesh.Header.face_type);
//            exportMesh.UVs.AddRange(b4.Select(r => r.TexCoord));
//            exportMesh.Normals.AddRange(b4.Select(r => r.Normal));

//            writer.AddMesh(exportMesh);
//            break;
//    }

//    var materialKey = fileFormat.materialData[mesh.Header.material_ref];


//    //fileFormat.materialData materialKey.m_frame
//    //var matSource = new GltfMaterialSource
//    //{
//    //    Name = "Material_X",
//    //    DiffuseColor = materialKey.m_,
//    //    EmissiveColor = materialKey.emissive,
//    //    SpecularColor = materialKey.specular,
//    //    AmbientColor = materialKey.ambient,
//    //    Power = materialKey.power,
//    //    TextureRef = materialFrame.texture_ref,
//    //    TextureRef2 = materialFrame.texture_ref2,
//    //    Flags = materialFrame.flags
//    //};

//    //int matIndex = writer.AddMaterial(matSource.ToGltfMaterial());
//    //exportMesh.MaterialIndex = matIndex;



//}

//writer.Save("test.gltf");



//Console.ReadLine();

