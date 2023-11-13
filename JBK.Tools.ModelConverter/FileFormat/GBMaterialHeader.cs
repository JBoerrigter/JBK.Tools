using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBMaterialHeader
    {
        GBHeader _header;

        public uint TextureFilenameOffset { get; private set; }
        public ushort TextureMappingOption { get; private set; }
        public uint TextureFilenameLength { get; private set; }
        public uint TextureOverlayOffset { get; private set; }
        public uint MaterialOffset { get; private set; }

        public GBMaterialHeader(GBHeader header)
        {
            _header = header;
        }

        public static GBMaterialHeader Get(BinaryReader reader, GBHeader gbheader)
        {
            GBMaterialHeader gBMaterialHeader = new(gbheader);
            gBMaterialHeader.TextureFilenameOffset = reader.ReadUInt32();
            gBMaterialHeader.TextureMappingOption = reader.ReadUInt16();
            gBMaterialHeader.TextureFilenameLength = reader.ReadUInt32();
            gBMaterialHeader.TextureOverlayOffset = reader.ReadUInt32();
            gBMaterialHeader.MaterialOffset = reader.ReadUInt32();
            return gBMaterialHeader;
        }
    }
}
