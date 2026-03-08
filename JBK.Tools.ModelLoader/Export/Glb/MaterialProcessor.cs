using JBK.Tools.ModelLoader.FileReader;
using SharpGLTF.Materials;
using System.Numerics;
using System.Text;

namespace JBK.Tools.ModelLoader.Export.Glb
{
    public class MaterialProcessor
    {
        public static Dictionary<int, MaterialBuilder> ProcessMaterials(Model sourceFile, string texturesFolder = "")
        {
            var result = new Dictionary<int, MaterialBuilder>();
            var cache = new Dictionary<string, MaterialBuilder>(StringComparer.Ordinal);

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

                builder = builder.WithChannelParam(KnownChannel.BaseColor, Vector4.One);
                builder = builder.WithMetallicRoughness(0f, 1f);

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
    }
}
