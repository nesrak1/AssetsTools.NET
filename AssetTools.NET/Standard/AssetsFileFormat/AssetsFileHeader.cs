using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public struct AssetsFileHeader
    {
        public uint metadataSize;                   //0x00
        public uint fileSize;                       //0x04 //big-endian
        public uint format;                         //0x08
        public uint offs_firstFile;                 //0x0C //big-endian
        //0 == little-endian (default, haven't seen anything else); 1 == big-endian, in theory
        public uint endianness;                     //0x10, for format < 9 at (fileSize - metadataSize), right before TypeTree
        public byte[] unknown;                      //0x11, for format >= 9

        ///public uint GetSizeBytes();
        public ulong Read(ulong absFilePos, AssetsFileReader reader)
        {
            metadataSize = reader.ReadUInt32();
            fileSize = reader.ReadUInt32();
            format = reader.ReadUInt32();
            offs_firstFile = reader.ReadUInt32();
            endianness = reader.ReadByte();
            reader.bigEndian = endianness == 1 ? true : false;
            unknown = reader.ReadBytes(3);
            return reader.Position;
        }
        //does NOT write the endianness byte for format < 9!
        public ulong Write(ulong pos, AssetsFileWriter writer)
        {
            writer.bigEndian = true;
            writer.Write(metadataSize);
            writer.Write(fileSize);
            writer.Write(format);
            writer.Write(offs_firstFile);
            writer.Write((byte)endianness);
            writer.bigEndian = endianness == 1 ? true : false;
            writer.Write(unknown);
            return writer.Position;
        }
    }
}
