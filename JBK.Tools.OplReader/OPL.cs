﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JBK.Tools.OplReader
{
    public class OPL
    {
        private OPLHeader _header = new OPLHeader();
        private List<OPLItem> _items = new List<OPLItem>();

        public OPLHeader Header
        {
            get { return _header; }
        }

        public List<OPLItem> Items
        {
            get { return _items; }
        }

        public OPL(BinaryReader reader)
        {
            _header.CRC32 = reader.ReadInt32();
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            _header.LowVersion = reader.ReadInt32();
            _header.HighVersion = reader.ReadInt32();
            reader.BaseStream.Seek(16, SeekOrigin.Current);
            _header.Unknown1 = reader.ReadInt32();
            _header.ObjectCount = reader.ReadInt32();

            for (int i = 0; i < _header.ObjectCount; i++)
            {
                OPLItem item = new OPLItem();
                item.PathLength = reader.ReadInt32();
                item.PathBytes = reader.ReadBytes(item.PathLength);
                item.Position = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );
                item.Rotation = new Quaternion(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );
                item.Scale = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );

                _items.Add(item);
            }
        }
    }

    public class OPLHeader
    {
        public int CRC32;
        public int LowVersion;
        public int HighVersion;
        public int Unknown1;
        public int ObjectCount;
    }

    public class OPLItem
    {
        public int PathLength;
        public byte[] PathBytes;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public string RelativePath
        {
            get { return ASCIIEncoding.ASCII.GetString(PathBytes); }
            set
            {
                PathBytes = ASCIIEncoding.ASCII.GetBytes(value);
                PathLength = PathBytes.Length;
            }
        }

        public override string ToString()
        {
            return Path.GetFileName(RelativePath);
        }
    }
}
