using JBK.Tools.ModelLoader.Enums;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Transforms;
using System.Numerics;
using System.Text;

namespace JBK.Tools.ModelLoader.Export.Glb;

public class GlbExporter : IExporter
{
    Dictionary<FaceType, IIndexProcessor> _IndexProcessors;
    Dictionary<VertexType, IMeshProcessor> _MeshProcessors;

    public GlbExporter()
    {
        _IndexProcessors = new Dictionary<FaceType, IIndexProcessor>
        {
            { FaceType.TriangleList, new ListIndexProcessor() },
            { FaceType.TriangleStrip, new StripIndexProcessor() }
        };

        _MeshProcessors = new Dictionary<VertexType, IMeshProcessor>
        {
            { VertexType.Rigid, new RigidMeshProcessor() },
            { VertexType.RigidDouble, new RigidDoubleMeshProcessor() },
            { VertexType.Blend1, new Blend1MeshProcessor() },
            { VertexType.Blend2, new Blend2MeshProcessor() },
            { VertexType.Blend3, new Blend3MeshProcessor() },
            { VertexType.Blend4, new Blend4MeshProcessor() }
        };
    }

    public void Export(ModelFileFormat.ModelFileFormat sourceFile, string texPath, string outputPath)
    {
        var scene = new SceneBuilder();
        var defaultMaterial = CreateDefaultMaterial();
        NodeBuilder[]? boneNodes = null;
        Matrix4x4[]? inverseBindMatrices = null;

        var materialBuilders = BuildMaterials(sourceFile, texPath /* pass null or a configured path */);


        // Bones
        if (sourceFile.header.bone_count > 0)
        {
            var armature = new NodeBuilder("Armature");
            scene.AddNode(armature);

            boneNodes = new NodeBuilder[sourceFile.bones.Length];
            inverseBindMatrices = new Matrix4x4[sourceFile.bones.Length];

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                var columnMajorInverseBindPose = sourceFile.bones[i].matrix;
                inverseBindMatrices[i] = columnMajorInverseBindPose;
                Matrix4x4.Invert(columnMajorInverseBindPose, out var columnMajorLocalTransform);
                var node = new NodeBuilder($"bone_{i}");
                if (Matrix4x4.Decompose(columnMajorLocalTransform, out var scale, out var rotation, out var translation))
                {
                    node.LocalTransform = new AffineTransform(translation, rotation, scale);
                }
                else
                {
                    node.LocalMatrix = columnMajorLocalTransform;
                }
                boneNodes[i] = node;
            }

            for (int i = 0; i < sourceFile.bones.Length; i++)
            {
                byte parentIndex = sourceFile.bones[i].parent;
                if (parentIndex == 255 || parentIndex >= boneNodes.Length)
                {
                    armature.AddNode(boneNodes[i]);
                }
                else
                {
                    boneNodes[parentIndex].AddNode(boneNodes[i]);
                }
            }

            const float boneTipLength = 0.1f;
            foreach (var boneNode in boneNodes)
            {
                if (!boneNode.VisualChildren.Any())
                {
                    var tipNode = new NodeBuilder($"{boneNode.Name}_tip");
                    tipNode.LocalTransform = new AffineTransform(Quaternion.Identity, Vector3.UnitY * boneTipLength);

                    boneNode.AddNode(tipNode);
                }
            }
        }

        // Meshes
        foreach (var mesh in sourceFile.meshes)
        {
            var indexProcessor = _IndexProcessors[(FaceType)mesh.Header.face_type];
            var meshProcessor = _MeshProcessors[(VertexType)mesh.Header.vertex_type];

            MaterialBuilder matForMesh = defaultMaterial;
            if (mesh.Header.material_ref >= 0 && materialBuilders.TryGetValue(mesh.Header.material_ref, out var mb))
            {
                matForMesh = mb;
            }

            var meshBuilder = meshProcessor.Process(indexProcessor, matForMesh, mesh);

            (NodeBuilder, Matrix4x4)[]? skin = null;
            if (boneNodes != null)
            {
                skin = new (NodeBuilder, Matrix4x4)[boneNodes.Length];
                for (int i = 0; i < boneNodes.Length; i++)
                {
                    skin[i] = (boneNodes[i], inverseBindMatrices[i]);
                }
            }

            meshProcessor.AddToScene(scene, meshBuilder, skin);
        }

        var model = scene.ToGltf2();

        // fix Textures
        //foreach (var mat in model.LogicalMaterials)
        //{
        //    // Access the low-level schema and set texture texCoord where needed
        //    var schemaMat = mat; // model.LogicalMaterials contains Schema2.Material
        //    if (schemaMat.PbrMetallicRoughness?.BaseColorTexture != null)
        //        schemaMat.PbrMetallicRoughness.BaseColorTexture.TexCoord = 0;

        //    if (schemaMat.EmissiveTexture != null)
        //        schemaMat.EmissiveTexture.TexCoord = 0; // or 1 if you used second UV set

        //    if (schemaMat.OcclusionTexture != null)
        //        schemaMat.OcclusionTexture.TexCoord = 0; // change to 1 for secondary UVs
        //}


        // Animations
        if (sourceFile.Animations.Any() && boneNodes != null)
        {
            foreach (var animData in sourceFile.Animations)
            {
                string animName = string.IsNullOrWhiteSpace(animData.Name) ? $"animation_{animData.Header.szoption}" : animData.Name;
                var animation = model.CreateAnimation(animName);

                // For each bone, create its animation channels
                for (int boneIndex = 0; boneIndex < sourceFile.header.bone_count; boneIndex++)
                {
                    var targetNodeBuilder = boneNodes[boneIndex];
                    var targetNode = model.LogicalNodes.FirstOrDefault(n => n.Name == targetNodeBuilder.Name);
                    if (targetNode == null) continue;

                    var translationKeys = new Dictionary<float, Vector3>();
                    var rotationKeys = new Dictionary<float, Quaternion>();
                    var scaleKeys = new Dictionary<float, Vector3>();

                    for (int keyIndex = 0; keyIndex < animData.Header.keyframe_count; keyIndex++)
                    {
                        float time = animData.Keyframes[keyIndex].time / 1000.0f;
                        ushort transformIndex = animData.BoneTransformIndices[keyIndex, boneIndex];

                        if (transformIndex < sourceFile.AllAnimationTransforms.Length)
                        {
                            var transform = sourceFile.AllAnimationTransforms[transformIndex];
                            translationKeys[time] = transform.pos;
                            rotationKeys[time] = transform.quat;
                            scaleKeys[time] = transform.scale.LengthSquared() > 0 ? transform.scale : Vector3.One;
                        }
                    }

                    if (translationKeys.Count > 1)
                    {
                        animation.CreateTranslationChannel(targetNode, translationKeys, true);
                    }
                    if (rotationKeys.Count > 1)
                    {
                        animation.CreateRotationChannel(targetNode, rotationKeys, true);
                    }
                    if (scaleKeys.Count > 1)
                    {
                        animation.CreateScaleChannel(targetNode, scaleKeys, true);
                    }
                }
            }
        }


        model.SaveGLB(outputPath);
    }

    private static MaterialBuilder CreateDefaultMaterial()
    {
        var material = MaterialBuilder.CreateDefault();
        material.WithMetallicRoughnessShader()
               .WithBaseColor(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        return material;
    }

    private static Vector4 DecodeArgbToVector4(uint argb)
    {
        var a = (byte)((argb >> 24) & 0xFF);
        var r = (byte)((argb >> 16) & 0xFF);
        var g = (byte)((argb >> 8) & 0xFF);
        var b = (byte)(argb & 0xFF);
        return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    private static string? ReadSecondString(byte[] stringTable, uint offset)
    {
        if (offset >= stringTable.Length) return null;
        int i = (int)offset;
        // skip first string
        while (i < stringTable.Length && stringTable[i] != 0) i++;
        i++; // move to byte after first null
        if (i >= stringTable.Length) return null;
        int start = i;
        while (i < stringTable.Length && stringTable[i] != 0) i++;
        if (i <= start) return null;
        return Encoding.ASCII.GetString(stringTable, start, i - start);
    }

    // --- helper functions to add to your exporter class/file ---

    private static Vector4 ArgbToVector4(uint argb, float opacityOverride = -1f)
    {
        // input stored as 0xAARRGGBB (your comment says ARGB)
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);

        float fa = a / 255f;
        float fr = r / 255f;
        float fg = g / 255f;
        float fb = b / 255f;

        if (opacityOverride >= 0f) fa = opacityOverride;

        // MaterialBuilder expects baseColor in linear RGBA space. For now we send sRGB-like values
        return new Vector4(fr, fg, fb, fa);
    }

    private static string[] SplitNullSeparatedStrings(string s)
    {
        if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
        // the texture field in the original format often holds multiple null-terminated strings packed together
        return s.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string ResolveTexturePath(string textureName, string textureFolder)
    {
        if (string.IsNullOrEmpty(textureName)) return null;
        // textureName can already be an absolute path; try it first
        if (File.Exists(textureName)) return textureName;

        // if it's just a filename, try textureFolder
        if (!string.IsNullOrEmpty(textureFolder))
        {
            var candidate = Path.Combine(textureFolder, textureName);
            if (File.Exists(candidate)) return candidate;
        }

        // also try same directory as the running app
        var candidate2 = Path.Combine(AppContext.BaseDirectory, textureName);
        if (File.Exists(candidate2)) return candidate2;

        return null; // not found
    }

    // Build MaterialBuilder objects from your sourceFile structures
    private static Dictionary<int, MaterialBuilder> BuildMaterials(ModelFileFormat.ModelFileFormat sourceFile, string texturesFolder = null)
    {
        var result = new Dictionary<int, MaterialBuilder>();

        if (sourceFile.materialData == null) return result;

        for (int i = 0; i < sourceFile.materialData.Length; i++)
        {
            var mk = sourceFile.materialData[i];
            // Create a readable name
            var matName = $"mat_{i}";

            // create builder and set base color from materialFrame (if present)
            MaterialBuilder mb = new MaterialBuilder(matName).WithDoubleSide(true).WithMetallicRoughnessShader();

            // map materialFrames (if exist)
            if (sourceFile.materialFrames != null && mk.m_frame < sourceFile.materialFrames.Length)
            {
                var mf = sourceFile.materialFrames[mk.m_frame];
                var baseColor = ArgbToVector4(mf.m_diffuse, mf.m_opacity);
                mb = mb.WithChannelParam(KnownChannel.BaseColor, baseColor);
                // optionally set specular/emissive heuristics from mf.m_specular or mf.mapoption if you need
            }

            // read textures (szTexture is an offset into the string table)
            if (mk.szTexture != 0)
            {
                string texPacked = sourceFile.GetString(mk.szTexture); // your model class has GetString(uint)
                var parts = SplitNullSeparatedStrings(texPacked);

                if (parts.Length >= 1)
                {
                    var baseName = parts[0];
                    var path = ResolveTexturePath(baseName, texturesFolder);

                    var tmpPath = System.IO.Path.ChangeExtension(path, ".png"); // ensure .png extension if needed
                    if (File.Exists(tmpPath))
                    {
                        path = tmpPath; // use .png if it exists
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        // base color / albedo texture
                        mb = mb.WithChannelImage(KnownChannel.BaseColor, path);
                    }
                }

                if (parts.Length >= 2)
                {
                    // many exporters store a second texture after a \\0 — interpret it as detail/occlusion/skin depending on your source.
                    var secondName = parts[1];
                    var path2 = ResolveTexturePath(secondName, texturesFolder);
                    if (!string.IsNullOrEmpty(path2))
                    {
                        // guess mapping — use Occlusion or Emissive depending on your original game's meaning.
                        // I'll attach it to Occlusion as an example:
                        mb = mb.WithChannelImage(KnownChannel.Occlusion, path2);
                    }
                }
            }

            result.Add(i, mb);
        }

        return result;
    }
}
