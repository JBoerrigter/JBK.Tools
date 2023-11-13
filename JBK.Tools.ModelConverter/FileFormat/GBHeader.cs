using System.Numerics;
using System.Text;

namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBHeader
    {
        public uint Checksum = 3735944941u;

        public uint CollisionLength { get; private set; }
        public byte Version { get; private set; }
        public byte Bones { get; private set; }
        public byte BoneId { get; private set; }
        public byte Materials { get; private set; }
        public ushort StartFrame { get; private set; }
        public ushort Meshes { get; private set; }
        public ushort Indexes { get; private set; }
        public ushort BoneInfluences { get; private set; }
        public uint KeyFrames { get; private set; }
        public ushort DescriptorLength { get; private set; }
        public ushort Unknown1 { get; private set; }
        public ushort Transformations { get; private set; }
        public ushort Animations { get; private set; }
        public string Filename { get; private set; }
        public uint FilenameLength { get; private set; }
        public List<ushort> Vertices { get; private set; }
        public Vector3 BoundingBoxMin { get; private set; }
        public Vector3 BoundingBoxMax { get; private set; }
        public Vector3 BoundingSphereCenter { get; private set; }

        public float BoundingSphereRadius { get; private set; }

        public GBHeader()
        {
            Vertices = new List<ushort>();
            BoundingSphereRadius = 0f;
        }

        public static GBHeader Get(BinaryReader reader)
        {
            GBHeader gBHeader = new GBHeader();
            gBHeader.Version = reader.ReadByte();
            gBHeader.Bones = reader.ReadByte();
            gBHeader.BoneId = reader.ReadByte();
            gBHeader.Materials = reader.ReadByte();
            if (gBHeader.Version > 9)
            {
                gBHeader.Checksum = reader.ReadUInt32();
            }
            if (gBHeader.Version > 11)
            {
                gBHeader.Filename = Encoding.UTF8.GetString(reader.ReadBytes(64));
            }
            gBHeader.FilenameLength = reader.ReadUInt32();
            for (int i = 0; i < 6; i++)
            {
                gBHeader.Vertices.Add(reader.ReadUInt16());
            }
            if (gBHeader.Version > 8)
            {
                for (int i = 0; i < 6; i++)
                {
                    gBHeader.Vertices.Add(reader.ReadUInt16());
                }
            }
            gBHeader.Indexes = reader.ReadUInt16();
            gBHeader.BoneInfluences = reader.ReadUInt16();
            gBHeader.KeyFrames = reader.ReadUInt32();
            gBHeader.DescriptorLength = reader.ReadUInt16();
            gBHeader.Unknown1 = reader.ReadUInt16();
            if (gBHeader.Version > 8)
            {
                gBHeader.CollisionLength = reader.ReadUInt32();
            }
            gBHeader.Transformations = reader.ReadUInt16();
            if (gBHeader.Version == 8)
            {
                gBHeader.Animations = reader.ReadByte();
            }
            else
            {
                gBHeader.Animations = reader.ReadUInt16();
            }
            gBHeader.Meshes = reader.ReadUInt16();
            gBHeader.StartFrame = reader.ReadUInt16();
            if (gBHeader.Version > 10)
            {
                gBHeader.BoundingBoxMin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                gBHeader.BoundingBoxMax = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }
            if (gBHeader.Version > 8)
            {
                gBHeader.BoundingSphereCenter = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                gBHeader.BoundingSphereRadius = reader.ReadSingle();
            }
            return gBHeader;
        }
    }
}
