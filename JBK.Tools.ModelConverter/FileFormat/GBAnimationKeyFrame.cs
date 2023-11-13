namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBAnimationKeyFrame
    {
        private ushort _duration;
        private uint _animationIdentifier;

        public static GBAnimationKeyFrame Get(BinaryReader reader)
        {
            GBAnimationKeyFrame gBAnimationKeyFrame = new GBAnimationKeyFrame();
            gBAnimationKeyFrame._duration = reader.ReadUInt16();
            gBAnimationKeyFrame._animationIdentifier = reader.ReadUInt32();
            return gBAnimationKeyFrame;
        }
    }
}
