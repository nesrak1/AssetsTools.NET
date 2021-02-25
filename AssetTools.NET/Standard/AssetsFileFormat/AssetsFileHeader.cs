using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileHeader
    {
        public uint metadataSize;
        public long fileSize;
        public uint format;
        public long firstFileOffset;
        public uint endianness;
        public byte[] unknown;
        public uint unknown1;
        public uint unknown2;

        public int GetSizeBytes()
        {
            if (format < 9)
                return 0x10;
            else
                return 0x14;
        }
        public void Read(AssetsFileReader reader)
        {
            metadataSize = reader.ReadUInt32();
            fileSize = reader.ReadUInt32();
            format = reader.ReadUInt32();
            firstFileOffset = reader.ReadUInt32();
            endianness = reader.ReadByte(); //todo "fileSize - metadataSize" for v<9 but I have no files to test on
            unknown = reader.ReadBytes(3);
            reader.Align();
            if (format >= 0x16)
            {
                metadataSize = reader.ReadUInt32();
                fileSize = reader.ReadInt64();
                firstFileOffset = reader.ReadInt64();
            }
            reader.bigEndian = endianness == 1;
            if (format >= 0x16)
            {
                unknown1 = reader.ReadUInt32(); //seen as 0x00 everywhere
                unknown2 = reader.ReadUInt32(); //seen as 0x1b in bundles and 0x00 everywhere else
            }
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.bigEndian = true;
            if (format >= 0x16)
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write(format);
                writer.Write(0);
            }
            else
            {
                writer.Write(metadataSize);
                writer.Write((uint)fileSize);
                writer.Write(format);
                writer.Write((uint)firstFileOffset);
            }

            writer.Write((byte)endianness);
            writer.Write(unknown);
            writer.Align();
            if (format >= 0x16)
            {
                writer.Write(metadataSize);
                writer.Write(fileSize);
                writer.Write(firstFileOffset);
            }
            writer.bigEndian = endianness == 1;
            if (format >= 0x16)
            {
                writer.Write(unknown1);
                writer.Write(unknown2);
            }
        }
    }
}
