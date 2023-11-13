using JBK.Tools.ModelConverter.FileFormat;
using System.Text;

namespace JBK.Tools.ModelConverter
{
    internal class GeoBinary
    {
        string[] _textureTypes = new string[] { ".png", ".dds", ".gtx", ".bmp" };

        public GBHeader Header { get; private set; }
        public List<GBBone> Bones { get; init; }
        public List<GBMaterialHeader> Materials { get; init; }
        public List<GBMesh> Meshes { get; init; }
        public List<GBAnimationHeader> Animations { get; init; }
        public List<GBAnimationKeyFrameTransformation> Transformations { get; init; }
        public List<string> Textures { get; init; }

        public GeoBinary(BinaryReader reader)
        {
            // Initialization
            Bones = new List<GBBone>();
            Materials = new List<GBMaterialHeader>();
            Meshes = new List<GBMesh>();
            Animations = new List<GBAnimationHeader>();
            Transformations = new List<GBAnimationKeyFrameTransformation>();
            Textures = new List<string>();

            // Reading 
            Header = GBHeader.Get(reader);

            if (Header.BoneId > 0)
            {
                for (int i = 0; i < Header.Bones; i++)
                {
                    Bones.Add(GBBone.Get(reader, Header));
                }
            }

            for (int i = 0; i < Header.Materials; i++)
            {
                Materials.Add(GBMaterialHeader.Get(reader, Header));
            }

            for (int i = 0; i < Header.Meshes; i++)
            {
                Meshes.Add(GBMesh.Get(reader, Header));
            }

            for (int i = 0; i < Header.Animations; i++)
            {
                Animations.Add(GBAnimationHeader.Get(reader, Header));
            }

            for (int i = 0; i < Header.Transformations; i++)
            {
                Transformations.Add(GBAnimationKeyFrameTransformation.Get(reader, Header));
            }

            // Get the remaining bytes in the stream for processing
            long remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            byte[] array = new byte[remainingBytes];
            reader.Read(array, 0, (int)remainingBytes);

            // Get the descriptor part from the end of the array
            byte[] descriptorArray = array
                .Skip(array.Length - Header.DescriptorLength)
                .Take(Header.DescriptorLength)
                .ToArray();

            foreach (GBMaterialHeader material in Materials)
            {
                int lengthToRead = array.Length - (int)material.TextureFilenameOffset;
                byte[] textureBytes = new byte[lengthToRead];
                Array.Copy(array, material.TextureFilenameOffset, textureBytes, 0, lengthToRead);

                string textureFilename = Encoding.UTF8.GetString(textureBytes);
                char[] separator = new char[1]; // Split on null characters as in the original code
                foreach (string filename in textureFilename.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string extension = Path.GetExtension(filename).ToLower();
                    if (_textureTypes.Contains(extension))
                    {
                        Textures.Add(filename);
                    }
                }
            }
        }
    }
}