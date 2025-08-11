using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.GbFormat.Materials;
using System.Numerics;
using System.Text;

namespace JBK.Tools.ModelLoader.Parsers.GbFileParsers;

public static class MaterialFramesParser
{
    public static void Parse(Model model)
    {
        if (model.materialData == null || model.stringTable == null || model.header.MaterialFrameCount == 0)
        {
            model.materialFramesByMaterial = Array.Empty<MaterialFrame[]>();
            return;
        }

        uint framesPerMaterial = model.header.MaterialFrameCount;
        int perFrameSize = /* 3 DWORD colors */ 4 * 3 + /* opacity */ 4 + /* vec2 */ 4 * 2 + /* vec3 */ 4 * 3;
        model.materialFramesByMaterial = new MaterialFrame[model.materialData.Length][];

        using var ms = new MemoryStream(model.stringTable);
        using var br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);

        for (int matIndex = 0; matIndex < model.materialData.Length; matIndex++)
        {
            uint frameOffset = model.materialData[matIndex].m_frame;
            if (frameOffset == 0 || frameOffset >= model.stringTable.Length)
            {
                // No frames or invalid offset — leave empty
                model.materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
                continue;
            }

            long start = frameOffset;
            long needed = framesPerMaterial * perFrameSize;
            if (start + needed > model.stringTable.Length)
            {
                model.materialFramesByMaterial[matIndex] = Array.Empty<MaterialFrame>();
                continue;
            }

            ms.Position = start;
            var frames = new MaterialFrame[framesPerMaterial];
            for (int f = 0; f < framesPerMaterial; f++)
            {
                frames[f].m_ambient = br.ReadUInt32();
                frames[f].m_diffuse = br.ReadUInt32();
                frames[f].m_specular = br.ReadUInt32();
                frames[f].m_opacity = br.ReadSingle();
                float ox = br.ReadSingle();
                float oy = br.ReadSingle();
                frames[f].m_offset = new Vector2(ox, oy);
                float ax = br.ReadSingle();
                float ay = br.ReadSingle();
                float az = br.ReadSingle();
                frames[f].m_angle = new Vector3(ax, ay, az);
            }

            model.materialFramesByMaterial[matIndex] = frames;
        }
    }
}
