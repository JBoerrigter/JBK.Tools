using ImageMagick;
using JBK.Tools.ModelLoader.Merge;
using JBK.Tools.ModelLoader.Export.Glb;
using JBK.Tools.ModelLoader.FileReader;
using System.Buffers.Binary;
using System.Collections;
using System.Text.Json;

namespace JBK.Tools.ModelLoader.Tests
{
    public class GbFileLoaderTests
    {
        private static string GetPath(params string[] parts)
        {
            return Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, .. parts]));
        }

        private static void AssertHeaderMatchesModel(Model model)
        {
            Assert.Equal(model.meshes?.Length ?? 0, model.header.MeshCount);
            Assert.Equal(model.bones?.Length ?? 0, model.header.BoneCount);
            Assert.Equal((uint)(model.materialData?.Length ?? 0), model.header.MaterialCount);
            Assert.Equal((uint)(model.stringTable?.Length ?? 0), model.header.StringSize);
            Assert.Equal((uint)(model.meshes?.Sum(m => m.Indices?.Length ?? m.Header.index_count) ?? 0), model.header.IndexCount);
            Assert.Equal((uint)(model.meshes?.Sum(m => m.BoneIndices?.Length ?? m.Header.bone_index_count) ?? 0), model.header.BoneIndexCount);
            Assert.Equal((uint)(model.Animations?.Sum(a => a.Header.keyframe_count) ?? 0), model.header.KeyframeCount);
            Assert.Equal((uint)(model.AllAnimationTransforms?.Length ?? 0), model.header.AnimCount);
            Assert.Equal(model.Animations?.Length ?? 0, model.header.AnimFileCount);

            var expectedVertexCounts = new ushort[12];
            if (model.meshes != null)
            {
                foreach (var mesh in model.meshes)
                {
                    expectedVertexCounts[mesh.Header.vertex_type] += (ushort)(mesh.Vertecies?.Length ?? mesh.Header.vertex_count);
                }
            }

            Assert.Equal(expectedVertexCounts, model.header.VertexCounts);

            uint expectedClsSize = model.collisionHeader == null
                ? 0u
                : (uint)(sizeof(ushort) * 2 + sizeof(float) * 3 * 2 + ((model.collisionNodes?.Length ?? 0) * System.Runtime.InteropServices.Marshal.SizeOf<JBK.Tools.ModelLoader.GbFormat.Collisions.CollisionNode>()));
            Assert.Equal(expectedClsSize, model.header.ClsSize);
        }

        [Fact]
        public void GbFileLoader_ShouldParse_Header_v12()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));

            Assert.Equal(12, model.header.Version);
            Assert.Equal(1, model.header.MeshCount);
            Assert.Equal(2511u, model.header.IndexCount);
            Assert.Equal(19528u, model.header.ClsSize);
            Assert.Equal(71u, model.header.StringSize);
            Assert.Equal(13u, model.header.SzOption);
            Assert.Equal(1, model.header.Flags);
            Assert.Equal(2, model.header.BoneCount);
            Assert.Equal(1u, model.header.BoneIndexCount);
            Assert.Equal(1u, model.header.MaterialCount);
            Assert.Equal(1u, model.header.MaterialFrameCount);
            Assert.Equal(8u, model.header.AnimCount);
            Assert.Equal(1, model.header.AnimFileCount);
        }

        [Fact]
        public void GbFileLoader_ShouldParse_Header_v8()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v8.gb"));

            Assert.Equal(8, model.header.Version);
            Assert.Equal(1, model.header.MeshCount);
            Assert.Equal(1946u, model.header.IndexCount);
            Assert.Equal(0u, model.header.ClsSize);
            Assert.Equal(51u, model.header.StringSize);
            Assert.Equal(0u, model.header.SzOption);
            Assert.Equal(0, model.header.Flags);
            Assert.Equal(28, model.header.BoneCount);
            Assert.Equal(22u, model.header.BoneIndexCount);
            Assert.Equal(1u, model.header.MaterialCount);
            Assert.Equal(1u, model.header.MaterialFrameCount);
            Assert.Equal(0u, model.header.AnimCount);
            Assert.Equal(0, model.header.AnimFileCount);
        }

        [Fact]
        public void GbFileLoader_ShouldParse_Bones_v8()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v8_bone.gb"));

            Assert.NotNull(model);
            Assert.Equal(28, model.bones.Length);
            Assert.Equal(255, model.bones[0].parent);
        }

        [Fact]
        public void GbFileLoader_ShouldParse_Animation_v8()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v8_animation_1.gb"));

            Assert.NotNull(model);
            Assert.Equal(1, model.header.AnimFileCount);
            Assert.Single(model.Animations);
            Assert.Equal(4u, model.header.KeyframeCount);
            Assert.Equal(78u, model.header.AnimCount);
            Assert.Equal((int)model.header.AnimCount, model.AllAnimationTransforms.Length);
            Assert.Equal(model.Animations[0].Header.keyframe_count, model.Animations[0].Keyframes.Length);
            Assert.Equal(model.header.BoneCount, model.Animations[0].BoneTransformIndices.GetLength(1));
            Assert.Equal(model.Animations.Length, model.animationNames.Length);
            Assert.Equal(model.Animations[0].Name, model.animationNames[0]);
        }

        [Fact]
        public void GbFileLoader_ShouldMerge_v8()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v8.gb"));
            model = GbFileLoader.Append(model, GetPath("TestFiles", "v8_bone.gb"));

            Assert.NotNull(model);
        }

        [Fact]
        public void GbFileLoader_ShouldReject_MismatchedThirdPartyCanonicalBones_ByDefault()
        {
            var canonicalModel = GbFileLoader.LoadFromFile(GetPath("..", "..", "..", "..", "TestAssets", "T1272_Bone.gb"));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                GbFileLoader.Append(
                    canonicalModel,
                    GetPath("..", "..", "..", "..", "TestAssets", "M1272_B1.gb"),
                    new MergeOptions
                    {
                        ResolveBonesToTarget = true,
                        SourceLabel = "M1272_B1.gb"
                    }));

            Assert.Contains("Cannot resolve source bone 0", exception.Message);
        }

        [Fact]
        public void GbFileLoader_ShouldMerge_ThirdPartyCanonicalBones_WhenBoneOrderIsAssumed()
        {
            var canonicalPath = GetPath("..", "..", "..", "..", "TestAssets", "T1272_Bone.gb");
            var modelPath = GetPath("..", "..", "..", "..", "TestAssets", "M1272_B1.gb");

            var canonicalModel = GbFileLoader.LoadFromFile(canonicalPath);
            var merged = GbFileLoader.Append(
                canonicalModel,
                modelPath,
                new MergeOptions
                {
                    ResolveBonesToTarget = true,
                    AssumeMatchingBoneOrder = true,
                    SourceLabel = "M1272_B1.gb"
                });

            Assert.Equal(77, merged.bones.Length);
            Assert.Single(merged.meshes);
            Assert.Equal(74, merged.meshes[0].BoneIndices.Length);
            Assert.Equal(76, merged.meshes[0].BoneIndices.Max());
            AssertHeaderMatchesModel(merged);
        }

        [Fact]
        public void GbFileLoader_ShouldPreserve_MaterialTextureNames_WhenMerging_WithCanonicalBones()
        {
            var canonicalPath = GetPath("..", "..", "..", "..", "TestAssets", "T1272_Bone.gb");
            var modelPath = GetPath("..", "..", "..", "..", "TestAssets", "M1272_B1.gb");

            var source = GbFileLoader.LoadFromFile(modelPath);
            int sourceMaterialRef = source.meshes[0].Header.material_ref;
            string expectedTextureName = source.GetString(source.materialData[sourceMaterialRef].szTexture);
            string expectedMaterialName = Path.GetFileNameWithoutExtension(expectedTextureName);

            var canonicalModel = GbFileLoader.LoadFromFile(canonicalPath);
            var merged = GbFileLoader.Append(
                canonicalModel,
                modelPath,
                new MergeOptions
                {
                    ResolveBonesToTarget = true,
                    AssumeMatchingBoneOrder = true,
                    SourceLabel = "M1272_B1.gb"
                });

            int mergedMaterialRef = merged.meshes[0].Header.material_ref;
            string actualTextureName = merged.GetString(merged.materialData[mergedMaterialRef].szTexture);
            var materials = MaterialProcessor.ProcessMaterials(merged);

            Assert.Equal(expectedTextureName, actualTextureName);
            Assert.True(materials.TryGetValue(mergedMaterialRef, out var material));
            Assert.Equal(expectedMaterialName, material.Name);
        }

        [Fact]
        public void MaterialProcessor_ShouldUse_TextureBaseName_ForGlbMaterialName()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            int materialRef = model.meshes[0].Header.material_ref;
            string textureName = model.GetString(model.materialData[materialRef].szTexture);

            var materials = MaterialProcessor.ProcessMaterials(model);

            Assert.True(materials.TryGetValue(materialRef, out var material));
            Assert.Equal(Path.GetFileNameWithoutExtension(textureName), material.Name);
        }

        [Fact]
        public void MaterialProcessor_ShouldKeepCurrentBehavior_WhenTextureEmbeddingDisabled()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            int materialRef = model.meshes[0].Header.material_ref;

            string tempDir = CreateTempDirectory();
            try
            {
                string textureName = model.GetString(model.materialData[materialRef].szTexture);
                string pngPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(textureName) + ".png");
                WritePngTexture(pngPath);

                var legacyMaterials = MaterialProcessor.ProcessMaterials(model);
                var materials = MaterialProcessor.ProcessMaterials(model, new MaterialProcessorOptions
                {
                    TexturesFolder = tempDir,
                    EmbedTextures = false
                });

                Assert.True(legacyMaterials.TryGetValue(materialRef, out var legacyMaterial));
                Assert.True(materials.TryGetValue(materialRef, out var material));
                Assert.Equal(legacyMaterial.Name, material.Name);
                Assert.False(MaterialBuilderHasBoundTexture(material));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void GlbExporter_ShouldEmbed_PngTexture_WhenEnabled()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            string tempDir = CreateTempDirectory();

            try
            {
                string textureDir = Path.Combine(tempDir, "tex");
                Directory.CreateDirectory(textureDir);

                int materialRef = model.meshes[0].Header.material_ref;
                string textureName = model.GetString(model.materialData[materialRef].szTexture);
                string pngPath = Path.Combine(textureDir, Path.GetFileNameWithoutExtension(textureName) + ".png");
                WritePngTexture(pngPath);

                string outputPath = Path.Combine(tempDir, "png-embedded.glb");
                new GlbExporter(new GlbExporterOptions { EmbedTextures = true }).Export(model, textureDir, outputPath);

                using var glb = ReadGlb(outputPath);
                Assert.Equal(1, GetOptionalArrayLength(glb.Json.RootElement, "images"));
                Assert.Equal(1, GetOptionalArrayLength(glb.Json.RootElement, "textures"));
                Assert.True(ContainsSequence(glb.Binary, PngSignature));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void GlbExporter_ShouldEmbed_DdsTexture_AsPng_WhenEnabled()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            string tempDir = CreateTempDirectory();

            try
            {
                string textureDir = Path.Combine(tempDir, "tex");
                Directory.CreateDirectory(textureDir);

                int materialRef = model.meshes[0].Header.material_ref;
                string textureName = model.GetString(model.materialData[materialRef].szTexture);
                string ddsPath = Path.Combine(textureDir, textureName);
                WriteDdsTexture(ddsPath);

                string outputPath = Path.Combine(tempDir, "dds-embedded.glb");
                new GlbExporter(new GlbExporterOptions { EmbedTextures = true }).Export(model, textureDir, outputPath);

                using var glb = ReadGlb(outputPath);
                Assert.Equal(1, GetOptionalArrayLength(glb.Json.RootElement, "images"));
                Assert.Equal(1, GetOptionalArrayLength(glb.Json.RootElement, "textures"));
                Assert.True(ContainsSequence(glb.Binary, PngSignature));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void GlbExporter_ShouldWarnAndFallback_WhenEmbeddedTextureIsMissing()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            string tempDir = CreateTempDirectory();
            var error = new StringWriter();
            TextWriter originalError = Console.Error;

            try
            {
                string outputPath = Path.Combine(tempDir, "missing-texture.glb");

                Console.SetError(error);
                new GlbExporter(new GlbExporterOptions { EmbedTextures = true }).Export(model, tempDir, outputPath);

                using var glb = ReadGlb(outputPath);
                Assert.Equal(0, GetOptionalArrayLength(glb.Json.RootElement, "images"));
                Assert.Equal(0, GetOptionalArrayLength(glb.Json.RootElement, "textures"));
                Assert.Contains("Using fallback material", error.ToString());
            }
            finally
            {
                Console.SetError(originalError);
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void GbFileLoader_ShouldRecompute_HeaderAggregates_AfterAppend()
        {
            var path = GetPath("TestFiles", "v12.gb");

            var merged = GbFileLoader.LoadFromFile(path);
            merged = GbFileLoader.Append(merged, path);

            Assert.Equal(2, merged.meshes.Length);
            Assert.Equal(4, merged.bones.Length);
            AssertHeaderMatchesModel(merged);
        }

        [Fact]
        public void GbFileLoader_ShouldCopy_CollisionData_WhenTargetHasNone()
        {
            var target = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v8.gb"));
            var source = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            Assert.Null(target.collisionHeader);

            var merged = GbFileLoader.Append(target, GetPath("TestFiles", "v12.gb"));

            Assert.NotNull(merged.collisionHeader);
            Assert.Equal(source.collisionNodes.Length, merged.collisionNodes.Length);
            Assert.Equal(source.collisionHeader!.Value.vertex_count, merged.collisionHeader!.Value.vertex_count);
            AssertHeaderMatchesModel(merged);
        }

        [Fact]
        public void GbFileLoader_ShouldRemap_MeshNameOffsets_WhenAppending()
        {
            var path = GetPath("TestFiles", "v12.gb");
            var source = GbFileLoader.LoadFromFile(path);
            string expectedMeshName = source.GetString(source.meshes[0].Header.name);

            var merged = GbFileLoader.LoadFromFile(path);
            merged = GbFileLoader.Append(merged, path);

            string actualMeshName = merged.GetString(merged.meshes[1].Header.name);

            Assert.False(string.IsNullOrWhiteSpace(expectedMeshName));
            Assert.Equal(expectedMeshName, actualMeshName);
            Assert.Equal(expectedMeshName, merged.meshes[1].Name);
        }

        [Fact]
        public void AnimationMerger_ShouldRemap_AnimationNameOffsets_WhenSourceStringsAreAppended()
        {
            var target = new Model
            {
                header = new JBK.Tools.ModelLoader.GbFormat.NormalizedHeader
                {
                    BoneCount = 1
                },
                stringTable = [.. System.Text.Encoding.ASCII.GetBytes("base\0")],
                Animations = Array.Empty<JBK.Tools.ModelLoader.GbFormat.Animations.AnimationData>(),
                AllAnimationTransforms = Array.Empty<JBK.Tools.ModelLoader.GbFormat.Animations.Animation>()
            };

            var source = new Model
            {
                header = new JBK.Tools.ModelLoader.GbFormat.NormalizedHeader
                {
                    BoneCount = 1,
                    AnimFileCount = 1
                },
                stringTable = [.. System.Text.Encoding.ASCII.GetBytes("clip\0")],
                Animations =
                [
                    new JBK.Tools.ModelLoader.GbFormat.Animations.AnimationData
                    {
                        Header = new JBK.Tools.ModelLoader.GbFormat.Animations.AnimationHeader
                        {
                            szoption = 0,
                            keyframe_count = 0
                        },
                        Keyframes = Array.Empty<JBK.Tools.ModelLoader.GbFormat.Animations.Keyframe>(),
                        BoneTransformIndices = new ushort[0, 1],
                        Name = "clip"
                    }
                ],
                AllAnimationTransforms = Array.Empty<JBK.Tools.ModelLoader.GbFormat.Animations.Animation>(),
                animationNameOffsets = [0]
            };

            var mergeContext = new MergeContext(target);
            mergeContext.SetSource(source);

            StringTableMerger.Merge(mergeContext);
            AnimationMerger.Merge(mergeContext);

            Assert.Single(target.Animations);
            Assert.Equal("clip", target.GetString(target.Animations[0].Header.szoption));
            Assert.Single(target.animationNameOffsets);
            Assert.Equal("clip", target.GetString(target.animationNameOffsets[0]));
        }

        [Fact]
        public void GbFileLoader_ShouldResolve_MeshNames_FromStringTable()
        {
            var model = GbFileLoader.LoadFromFile(GetPath("TestFiles", "v12.gb"));
            string expectedMeshName = model.GetString(model.meshes[0].Header.name);

            Assert.False(string.IsNullOrWhiteSpace(expectedMeshName));
            Assert.Equal(expectedMeshName, model.meshes[0].Name);
            Assert.Equal($"Mesh_{expectedMeshName}", model.meshes[0].GetBuilderName());
        }

        private static bool MaterialBuilderHasBoundTexture(object materialBuilder)
        {
            var channelsProperty = materialBuilder.GetType().GetProperty("Channels");
            if (channelsProperty?.GetValue(materialBuilder) is not IEnumerable channels)
            {
                return false;
            }

            foreach (var channel in channels)
            {
                if (channel == null)
                {
                    continue;
                }

                var channelType = channel.GetType();
                var key = channelType.GetProperty("Key")?.GetValue(channel)?.ToString();
                if (!string.Equals(key, "BaseColor", StringComparison.Ordinal))
                {
                    continue;
                }

                var parameters = channelType.GetProperty("Parameters")?.GetValue(channel) as IDictionary;
                if (parameters == null)
                {
                    return false;
                }

                foreach (DictionaryEntry entry in parameters)
                {
                    if (entry.Value?.GetType().Name.Contains("Texture", StringComparison.Ordinal) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static GlbContents ReadGlb(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length >= 20);

            Assert.Equal(0x46546C67u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(0, 4)));
            Assert.Equal(2u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4, 4)));

            int offset = 12;
            int jsonLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
            uint jsonType = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
            Assert.Equal(0x4E4F534Au, jsonType);
            offset += 8;

            string jsonText = System.Text.Encoding.UTF8.GetString(bytes, offset, jsonLength).TrimEnd('\0', ' ');
            offset += jsonLength;

            byte[] binaryChunk = Array.Empty<byte>();
            if (offset + 8 <= bytes.Length)
            {
                int binaryLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
                uint binaryType = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4, 4));
                Assert.Equal(0x004E4942u, binaryType);
                offset += 8;

                binaryChunk = new byte[binaryLength];
                Array.Copy(bytes, offset, binaryChunk, 0, binaryLength);
            }

            return new GlbContents(JsonDocument.Parse(jsonText), binaryChunk);
        }

        private static int GetOptionalArrayLength(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Array
                ? property.GetArrayLength()
                : 0;
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "JBK.Tools.ModelLoader.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void WritePngTexture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var image = new MagickImage(MagickColors.White, 1, 1);
            image.Format = MagickFormat.Png;
            image.Write(path);
        }

        private static void WriteDdsTexture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var image = new MagickImage(MagickColors.White, 1, 1);
            image.Format = MagickFormat.Dds;
            image.Write(path);
        }

        private static bool ContainsSequence(byte[] buffer, byte[] sequence)
        {
            if (sequence.Length == 0 || buffer.Length < sequence.Length)
            {
                return false;
            }

            for (int i = 0; i <= buffer.Length - sequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (buffer[i + j] != sequence[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed record GlbContents(JsonDocument Json, byte[] Binary) : IDisposable
        {
            public void Dispose() => Json.Dispose();
        }

        private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    }
}
