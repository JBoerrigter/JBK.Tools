namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBAnimationBone
    {
        private ushort _keyFrameTransformationIndex;

        public static GBAnimationBone Get(BinaryReader reader)
        {
            GBAnimationBone gBAnimationBone = new GBAnimationBone();
            gBAnimationBone._keyFrameTransformationIndex = reader.ReadUInt16();
            return gBAnimationBone;
        }
    }
}
