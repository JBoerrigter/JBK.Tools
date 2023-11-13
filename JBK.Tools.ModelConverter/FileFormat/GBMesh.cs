using System.Numerics;

namespace JBK.Tools.ModelConverter.FileFormat
{
    internal class GBMesh
    {
        internal enum Type
        {
            List = 0,
            Strip = 1
        }

        private GBHeader _header;

        public uint NameIndex { get; private set; }
        public uint MaterialIndex { get; private set; }
        public byte VertexType { get; private set; }
        public Type TriangleType { get; private set; }
        public ushort VertexCount { get; private set; }
        public ushort FaceIndexCount { get; private set; }
        public byte BoneIndexCount { get; private set; }

        public List<Vector3> Vertices { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<Vector2> UVs { get; private set; }
        public List<int[]> Faces { get; private set; }
        public List<int> BoneIndices { get; private set; }
        public List<List<float>> VertexWeights { get; private set; }
        public List<byte[]> VertexBoneIndices { get; private set; }

        public static GBMesh Get(BinaryReader reader, GBHeader header)
        {
            GBMesh mesh = new(header);
            mesh.ReadFromFile(reader);
            return mesh;
        }

        public GBMesh(GBHeader header)
        {
            _header = header;
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
            Faces = new List<int[]>();
            BoneIndices = new List<int>();
            VertexWeights = new List<List<float>>();
            VertexBoneIndices = new List<byte[]>();
        }

        private void ReadFromFile(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadBoneIndices(reader);
            ReadVertices(reader);
            ReadFaceIndices(reader);
        }

        private void ReadHeader(BinaryReader reader)
        {
            NameIndex = reader.ReadUInt32();
            MaterialIndex = reader.ReadUInt32();
            VertexType = reader.ReadByte();
            TriangleType = ((reader.ReadByte() == 0) ? Type.List : Type.Strip);
            VertexCount = reader.ReadUInt16();
            FaceIndexCount = reader.ReadUInt16();
            BoneIndexCount = reader.ReadByte();

            // Skip remaining bytes to complete the 15-byte structure
            // reader.BaseStream.Seek(1, SeekOrigin.Current);
        }

        private void ReadBoneIndices(BinaryReader reader)
        {
            for (int i = 0; i < BoneIndexCount; i++)
            {
                int boneIndex = reader.ReadByte();
                BoneIndices.Add(boneIndex);

                // todo: Additional logic to handle bone groups:
                // CreateOrUpdateBoneGroup(boneIndex);
            }
        }

        private void ReadVertices(BinaryReader reader)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                // Read vertex position
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                Vertices.Add(new Vector3(x, y, z));

                // Handle Vertex Weights and Bone Indices
                List<float> weights = new List<float>();
                byte[] boneIndices = new byte[4];
                if (VertexType > 0 && VertexType < 6)
                {
                    int vertexWeightCount = (_header.Version > 10) ? VertexType - 1 : VertexType - 2;
                    if (BoneIndexCount == 0)
                        vertexWeightCount -= 3;

                    if (vertexWeightCount > 0)
                    {
                        for (int j = 0; j < vertexWeightCount; j++)
                        {
                            weights.Add(reader.ReadSingle());
                        }

                        float weightSum = weights.Sum();
                        if (1.0f - weightSum > 0)
                        {
                            weights.Add(1.0f - weightSum);
                        }

                        boneIndices = reader.ReadBytes(4);
                    }
                }
                VertexWeights.Add(weights);
                VertexBoneIndices.Add(boneIndices);

                // Read Normal
                float nx = reader.ReadSingle();
                float ny = reader.ReadSingle();
                float nz = reader.ReadSingle();
                Normals.Add(new Vector3(nx, ny, nz));

                // Read UV Coordinates
                float u = reader.ReadSingle();
                float v = reader.ReadSingle();
                UVs.Add(new Vector2(u, -v));

                // Handle Additional Vertex Data for vertexFormat > 5
                if (VertexType > 5)
                {
                    reader.BaseStream.Seek(((VertexType % 6) * 4) + 8, SeekOrigin.Current);
                }
            }
        }

        private void ReadFaceIndices(BinaryReader reader)
        {
            switch (TriangleType)
            {
                case Type.List:
                    ReadTriangleList(reader);
                    break;
                case Type.Strip:
                    ReadTriangleStrip(reader);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported primitive type.");
            }
        }

        private void ReadTriangleList(BinaryReader reader)
        {
            for (int i = 0; i < FaceIndexCount; i += 3)
            {
                int[] face = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    face[j] = reader.ReadUInt16();
                }
                Faces.Add(face);
            }
        }

        private void ReadTriangleStrip(BinaryReader reader)
        {
            List<int> indices = new();
            for (int i = 0; i < FaceIndexCount; i++)
            {
                indices.Add(reader.ReadUInt16());
            }
            for (int i = 0; i < indices.Count - 2; i++)
            {
                int[] face = new int[3];
                if (i % 2 == 0)
                {
                    face[0] = indices[i];
                    face[1] = indices[i + 1];
                    face[2] = indices[i + 2];
                }
                else
                {
                    face[0] = indices[i + 2];
                    face[1] = indices[i + 1];
                    face[2] = indices[i];
                }
                if (!IsDegenerate(face))
                {
                    Faces.Add(face);
                }
            }
        }

        /// <summary>
        /// A triangle is degenerate if two or more of its vertices are the same
        /// </summary>
        private bool IsDegenerate(int[] face)
        {
            return face[0] == face[1] || face[1] == face[2] || face[0] == face[2];
        }
    }
}
