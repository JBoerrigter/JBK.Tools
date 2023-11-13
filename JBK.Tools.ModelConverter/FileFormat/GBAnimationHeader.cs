namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBAnimationHeader
    {
        GBHeader _header;

        public uint Identifier { get; private set; }
        public ushort KeyFrameCount { get; private set; }
        public List<GBAnimationBone> Bones { get; private set; }
        public List<GBAnimationKeyFrame> KeyFrames { get; private set; }

        public GBAnimationHeader(GBHeader header)
        {
            _header = header;

            KeyFrames = new List<GBAnimationKeyFrame>();
            Bones = new List<GBAnimationBone>();
        }

        public static GBAnimationHeader Get(BinaryReader reader, GBHeader gbheader)
        {
            GBAnimationHeader gBAnimationHeader = new GBAnimationHeader(gbheader);
            gBAnimationHeader.Identifier = reader.ReadUInt32();
            gBAnimationHeader.KeyFrameCount = reader.ReadUInt16();
            for (int i = 0; i < gBAnimationHeader.KeyFrameCount; i++)
            {
                gBAnimationHeader.KeyFrames.Add(GBAnimationKeyFrame.Get(reader));
            }
            for (int i = 0; i < gBAnimationHeader.KeyFrameCount; i++)
            {
                gBAnimationHeader.Bones.Add(GBAnimationBone.Get(reader));
            }
            return gBAnimationHeader;
        }
    }
}
