using JBK.Tools.ModelLoader.FileReader;
using ImageMagick;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using System.Numerics;
using System.Text;

namespace JBK.Tools.ModelLoader.Export.Glb
{
    public class MaterialProcessor
    {
        public static Dictionary<int, MaterialBuilder> ProcessMaterials(Model sourceFile, string texturesFolder = "")
        {
            return ProcessMaterials(sourceFile, new MaterialProcessorOptions
            {
                TexturesFolder = texturesFolder
            });
        }

        public static Dictionary<int, MaterialBuilder> ProcessMaterials(Model sourceFile, MaterialProcessorOptions? options)
        {
            options ??= new MaterialProcessorOptions();

            var result = new Dictionary<int, MaterialBuilder>();
            var cache = new Dictionary<string, MaterialBuilder>(StringComparer.Ordinal);
            var textureCache = new Dictionary<string, MemoryImage>(StringComparer.OrdinalIgnoreCase);

            if (sourceFile.materialData == null) return result;

            for (int i = 0; i < sourceFile.materialData.Length; i++)
            {
                var mk = sourceFile.materialData[i];
                var originalFileName = mk.szTexture != 0 ? sourceFile.GetString(mk.szTexture) : "unknown";
                var key = BuildTextureKey(originalFileName);
                if (string.IsNullOrWhiteSpace(key))
                {
                    key = $"material_{i}";
                }

                if (cache.TryGetValue(key, out var existing))
                {
                    result.Add(i, existing);
                    continue;
                }

                var builder = new MaterialBuilder().WithDoubleSide(true).WithMetallicRoughnessShader();
                builder.Name = BuildMaterialName(originalFileName, key, i);

                builder = builder.WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, Vector4.One);
                builder = builder.WithMetallicRoughness(0f, 1f);
                ApplyTextureIfEnabled(builder, originalFileName, options, textureCache);

                cache[key] = builder;
                result.Add(i, builder);
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

        private static void ApplyTextureIfEnabled(
            MaterialBuilder builder,
            string originalFileName,
            MaterialProcessorOptions options,
            Dictionary<string, MemoryImage> textureCache)
        {
            if (!options.EmbedTextures || string.IsNullOrWhiteSpace(options.TexturesFolder))
            {
                return;
            }

            if (!TryResolveTextureImage(originalFileName, options, textureCache, out var image))
            {
                return;
            }

            builder.WithChannelImage(KnownChannel.BaseColor, image);
        }

        private static string BuildTextureKey(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName) ?? "unknown";
            name = name.ToLowerInvariant();

            var sb = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if ((c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_' || c == '-')
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }

        private static string BuildMaterialName(string originalFileName, string key, int materialIndex)
        {
            var name = Path.GetFileNameWithoutExtension(originalFileName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            return $"material_{materialIndex}";
        }

        private static bool TryResolveTextureImage(
            string originalFileName,
            MaterialProcessorOptions options,
            Dictionary<string, MemoryImage> textureCache,
            out MemoryImage image)
        {
            image = default;

            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                return false;
            }

            string? resolvedPath = ResolveTexturePath(options.TexturesFolder, originalFileName);
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                options.WarningHandler($"[GLB.MATERIAL][WARN] Texture '{originalFileName}' was not found under '{options.TexturesFolder}'. Using fallback material.");
                return false;
            }

            if (textureCache.TryGetValue(resolvedPath, out image))
            {
                return true;
            }

            try
            {
                image = LoadTextureAsPng(resolvedPath);
                textureCache[resolvedPath] = image;
                return true;
            }
            catch (Exception ex)
            {
                options.WarningHandler($"[GLB.MATERIAL][WARN] Texture '{resolvedPath}' could not be decoded ({ex.Message}). Using fallback material.");
                return false;
            }
        }

        private static MemoryImage LoadTextureAsPng(string texturePath)
        {
            using var image = new MagickImage(texturePath);
            image.Format = MagickFormat.Png;

            using var stream = new MemoryStream();
            image.Write(stream);
            return new MemoryImage(stream.ToArray());
        }

        private static string? ResolveTexturePath(string texturesFolder, string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(texturesFolder) || string.IsNullOrWhiteSpace(originalFileName))
            {
                return null;
            }

            string normalizedRelativePath = NormalizeRelativeTexturePath(originalFileName);
            var candidates = new List<string>();
            AddTextureCandidate(candidates, texturesFolder, normalizedRelativePath);

            string directory = Path.GetDirectoryName(normalizedRelativePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(normalizedRelativePath);
            foreach (string extension in s_textureExtensions)
            {
                string relativeCandidate = string.IsNullOrEmpty(directory)
                    ? baseName + extension
                    : Path.Combine(directory, baseName + extension);
                AddTextureCandidate(candidates, texturesFolder, relativeCandidate);
            }

            return candidates.FirstOrDefault(File.Exists);
        }

        private static string NormalizeRelativeTexturePath(string originalFileName)
        {
            string normalized = originalFileName.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            return normalized.TrimStart(Path.DirectorySeparatorChar);
        }

        private static void AddTextureCandidate(List<string> candidates, string texturesFolder, string relativePath)
        {
            string fullPath = Path.GetFullPath(Path.Combine(texturesFolder, relativePath));
            if (!fullPath.StartsWith(Path.GetFullPath(texturesFolder), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!candidates.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(fullPath);
            }
        }

        private static readonly string[] s_textureExtensions =
        [
            ".dds",
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".tga"
        ];
    }
}
