using System;
using System.Text;

namespace AssetsTools.NET
{
    public struct ClassDatabaseFileHeader
    {
        public string header;
        public byte fileVersion;

        public byte compressionType; //0 - none, 1 - lz4, only supports none right now
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
                throw new NotImplementedException("header not detected. is this a cldb file?");
            fileVersion = reader.ReadByte();
            switch (fileVersion)
            {
                case 1:
                    break;
                case 3:
                    compressionType = reader.ReadByte();
                    if (compressionType != 0)
                        throw new NotImplementedException("compressed cldb reading not supported");
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
            switch(fileVersion)
            {
                case 1:
                    break;
                case 3:
                    if (compressionType != 0)
                        throw new NotImplementedException("compressed cldb writing not supported");
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
