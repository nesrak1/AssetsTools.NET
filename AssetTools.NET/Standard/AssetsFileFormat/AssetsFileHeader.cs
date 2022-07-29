using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileHeader
    {
        /// <summary>
        /// Size of the metadata block (not including this header).
        /// </summary>
        public long MetadataSize { get; set; }
        /// <summary>
        /// Size of the entire file.
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// Version of this file. This only affects the structure of the serialized file, not asset data.
        /// </summary>
        public uint Version { get; set; }
        /// <summary>
        /// Offset to the data of the first asset.
        /// </summary>
        public long DataOffset { get; set; }
        /// <summary>
        /// File endianness. Little endian is false and big endian is true.
        /// </summary>
        public bool Endianness { get; set; }

        public void Read(AssetsFileReader reader)
        {
            reader.BigEndian = true;
            MetadataSize = reader.ReadUInt32();
            FileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();
            Endianness = reader.ReadBoolean();
            reader.Position += 3; // unused bytes

            if (Version >= 0x16)
            {
                MetadataSize = reader.ReadUInt32();
                FileSize = reader.ReadInt64();
                DataOffset = reader.ReadInt64();
                reader.Position += 8; // unused bytes
            }

            reader.BigEndian = Endianness;
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.BigEndian = true;
            if (Version >= 0x16)
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write(Version);
                writer.Write(0);
            }
            else
            {
                writer.Write((uint)MetadataSize);
                writer.Write((uint)FileSize);
                writer.Write(Version);
                writer.Write((uint)DataOffset);
            }

            writer.Write(Endianness);
            writer.Write(new byte[3]);

            if (Version >= 0x16)
            {
                writer.Write((uint)MetadataSize);
                writer.Write(FileSize);
                writer.Write(DataOffset);
                writer.Write(new byte[8]);
            }

            writer.BigEndian = Endianness;
        }
    }
}
