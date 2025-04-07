﻿namespace AssetsTools.NET
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

        /// <summary>
        /// Read the <see cref="AssetsFileHeader"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            reader.BigEndian = true;
            MetadataSize = reader.ReadUInt32();
            FileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();
            Endianness = reader.ReadBoolean();
            reader.Position += 3; // unused bytes

            if (Version >= 22)
            {
                MetadataSize = reader.ReadUInt32();
                FileSize = reader.ReadInt64();
                DataOffset = reader.ReadInt64();
                reader.Position += 8; // unused bytes
            }

            reader.BigEndian = Endianness;
        }

        /// <summary>
        /// Write the <see cref="AssetsFileHeader"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.BigEndian = true;
            if (Version >= 22)
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

            if (Version >= 22)
            {
                writer.Write((uint)MetadataSize);
                writer.Write(FileSize);
                writer.Write(DataOffset);
                writer.Write(new byte[8]);
            }

            writer.BigEndian = Endianness;
        }

        /// <summary>
        /// Get the size of this header.
        /// </summary>
        public long GetSize()
        {
            long size = 20;
            if (Version >= 22)
                size += 28;

            return size;
        }
    }
}
