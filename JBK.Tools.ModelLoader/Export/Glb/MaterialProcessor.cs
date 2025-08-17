using JBK.Tools.ModelLoader.FileReader;
using SharpGLTF.Materials;
using System.Numerics;

namespace JBK.Tools.ModelLoader.Export.Glb
{
    public class MaterialProcessor
    {
        public static Dictionary<int, MaterialBuilder> ProcessMaterials(Model sourceFile, string texturesFolder = "")
        {
            var result = new Dictionary<int, MaterialBuilder>();
            var cache = new Dictionary<uint, MaterialBuilder>();

            if (sourceFile.materialData == null) return result;

            for (int i = 0; i < sourceFile.materialData.Length; i++)
            {
                string texPacked = string.Empty;
                var mk = sourceFile.materialData[i];

                if (mk.szTexture != 0 && cache.ContainsKey(mk.szTexture))
                {
                    result.Add(i, cache[mk.szTexture]);
                    continue; // already processed this texture
                }

                // default name
                var matName = $"mat_{i}";

                if (mk.szTexture != 0)
                {
                    texPacked = sourceFile.GetString(mk.szTexture);
                    matName = Path.GetFileNameWithoutExtension(texPacked);
                }

                MaterialBuilder mb = new MaterialBuilder(matName).WithDoubleSide(true).WithMetallicRoughnessShader();
                cache.Add(mk.szTexture, mb);

                if (sourceFile.materialFrames != null && mk.m_frame < sourceFile.materialFrames.Length)
                {
                    var mf = sourceFile.materialFrames[mk.m_frame];
                    var baseColor = ArgbConverter.ToVector4(mf.m_diffuse, mf.m_opacity);
                    mb = mb.WithChannelParam(KnownChannel.BaseColor, baseColor);
                }

                // read textures (szTexture is an offset into the string table)
                if (mk.szTexture != 0)
                {
                    var parts = SplitNullSeparatedStrings(texPacked);

                    if (parts.Length >= 1)
                    {
                        var baseName = parts[0];
                        var path = ResolveTexturePath(baseName, texturesFolder);

                        var tmpPath = ResolveTexturePath(System.IO.Path.ChangeExtension(baseName, ".png"), texturesFolder);
                        if (File.Exists(tmpPath))
                        {
                            path = tmpPath;
                        }

                        if (!string.IsNullOrEmpty(path))
                        {
                            // base color / albedo texture
                            mb = mb.WithChannelImage(KnownChannel.BaseColor, path);
                        }
                    }

                    if (parts.Length >= 2)
                    {
                        var secondName = parts[1];
                        var path2 = ResolveTexturePath(secondName, texturesFolder);
                        if (!string.IsNullOrEmpty(path2))
                        {
                            mb = mb.WithChannelImage(KnownChannel.Occlusion, path2);
                        }
                    }
                }

                result.Add(i, mb);
            }

            return result;
        }

        public static MaterialBuilder CreateDefaultMaterial()
        {
            var material = MaterialBuilder.CreateDefault();
            material.WithMetallicRoughnessShader()
                   .WithBaseColor(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            return material;
        }

        private static string[] SplitNullSeparatedStrings(string mergedString)
        {
            if (string.IsNullOrEmpty(mergedString)) return Array.Empty<string>();
            return mergedString.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string ResolveTexturePath(string textureName, string textureFolder)
        {
            if (string.IsNullOrEmpty(textureName)) return string.Empty;
            if (File.Exists(textureName)) return textureName;

            if (!string.IsNullOrEmpty(textureFolder))
            {
                var candidate = Path.Combine(textureFolder, textureName);
                if (File.Exists(candidate)) return candidate;
            }

            var candidate2 = Path.Combine(AppContext.BaseDirectory, textureName);
            if (File.Exists(candidate2)) return candidate2;

            return string.Empty;
        }
    }
}
