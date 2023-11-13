using System.Numerics;

namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBAnimationKeyFrameTransformation
    {
        private GBHeader _header;

        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Scale { get; private set; }

        public GBAnimationKeyFrameTransformation(GBHeader header)
        {
            _header = header;
        }

        public static GBAnimationKeyFrameTransformation Get(BinaryReader reader, GBHeader gbheader)
        {
            GBAnimationKeyFrameTransformation gBAnimationKeyFrameTransformation = new GBAnimationKeyFrameTransformation(gbheader);
            gBAnimationKeyFrameTransformation.Translation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            gBAnimationKeyFrameTransformation.Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            gBAnimationKeyFrameTransformation.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            return gBAnimationKeyFrameTransformation;
        }
    }
}
