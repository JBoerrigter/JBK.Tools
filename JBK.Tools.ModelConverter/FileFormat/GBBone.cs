namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBBone
    {
        public byte Link { get; private set; }
        public GBHeader GBHeader { get; init; }
        public float[,] Transformation { get; init; }

        public GBBone(GBHeader gbheader)
        {
            GBHeader = gbheader;
            Transformation = new float[2, 4];
        }

        public static GBBone Get(BinaryReader reader, GBHeader gbheader)
        {
            GBBone gBBone = new GBBone(gbheader);
            gBBone.Transformation[0, 0] = reader.ReadSingle();
            gBBone.Transformation[0, 1] = reader.ReadSingle();
            gBBone.Transformation[0, 2] = reader.ReadSingle();
            gBBone.Transformation[0, 3] = reader.ReadSingle();
            gBBone.Transformation[1, 0] = reader.ReadSingle();
            gBBone.Transformation[1, 1] = reader.ReadSingle();
            gBBone.Transformation[1, 2] = reader.ReadSingle();
            gBBone.Transformation[1, 3] = reader.ReadSingle();
            gBBone.Link = reader.ReadByte();
            return gBBone;
        }
    }
}