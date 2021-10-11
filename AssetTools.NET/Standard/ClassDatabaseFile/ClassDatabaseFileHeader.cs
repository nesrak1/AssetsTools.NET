using System;
using System.Text;

namespace AssetsTools.NET
{
    public struct ClassDatabaseFileHeader
    {
        public string header;
        public byte fileVersion;

        public byte flags;
        public byte compressionType;
        public uint compressedSize, uncompressedSize;

        public byte unityVersionCount;
        public string[] unityVersions;
        
        public uint stringTableLen;
        public uint stringTablePos;
        public void Read(AssetsFileReader reader)
        {
            reader.bigEndian = false;
            header = reader.ReadStringLength(4);
            if (header != "cldb")
                throw new Exception("header not detected. is this a cldb file?");
            fileVersion = reader.ReadByte();
            flags = 0;
            if (fileVersion == 4)
                flags = reader.ReadByte();
            switch (fileVersion)
            {
                case 1:
                    compressionType = 0;
                    break;
                case 2:
                    compressionType = reader.ReadByte();
                    break;
                case 3:
                case 4:
                    compressionType = reader.ReadByte();
                    compressedSize = reader.ReadUInt32();
                    uncompressedSize = reader.ReadUInt32();
                    break;
                default:
                    return;
            }
            unityVersionCount = reader.ReadByte();
            unityVersions = new string[unityVersionCount];
            for (int i = 0; i < unityVersionCount; i++)
            {
                unityVersions[i] = reader.ReadCountString();
            }
            stringTableLen = reader.ReadUInt32();
            stringTablePos = reader.ReadUInt32();
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.bigEndian = false;
            writer.Write(Encoding.ASCII.GetBytes(header));
            writer.Write(fileVersion);
            if (fileVersion == 4)
                writer.Write(flags);
            switch (fileVersion)
            {
                case 1:
                    break;
                case 2:
                    writer.Write(compressionType);
                    break;
                case 3:
                case 4:
                    writer.Write(compressionType);
                    writer.Write(compressedSize);
                    writer.Write(uncompressedSize);
                    break;
                default:
                    return;
            }
            writer.Write(unityVersionCount);
            for (int i = 0; i < unityVersionCount; i++)
            {
                writer.WriteCountString(unityVersions[i]);
            }
            writer.Write(stringTableLen);
            writer.Write(stringTablePos);
        }
    }
}
