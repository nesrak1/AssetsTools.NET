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
        public string[] pUnityVersions;
        
        public uint stringTableLen;
        public uint stringTablePos;
        public ulong Read(AssetsFileReader reader, ulong filePos)
        {
            reader.bigEndian = false;
            header = reader.ReadStringLength(4);
            if (header != "cldb") return reader.Position;
            fileVersion = reader.ReadByte();
            switch (fileVersion)
            {
                case 1:
                    break;
                case 3:
                    compressionType = reader.ReadByte();
                    if (compressionType != 0) return reader.Position;
                    compressedSize = reader.ReadUInt32();
                    uncompressedSize = reader.ReadUInt32();
                    break;
                default:
                    return reader.Position;
            }
            unityVersionCount = reader.ReadByte();
            pUnityVersions = new string[unityVersionCount];
            for (int i = 0; i < unityVersionCount; i++)
            {
                pUnityVersions[i] = reader.ReadCountString();
            }
            stringTableLen = reader.ReadUInt32();
            stringTablePos = reader.ReadUInt32();
            return reader.Position;
        }
        public ulong Write(AssetsFileWriter writer, ulong filePos)
        {
            writer.bigEndian = false;
            writer.Write(Encoding.ASCII.GetBytes(header));
            writer.Write(fileVersion);
            switch(fileVersion)
            {
                case 1:
                    break;
                case 3:
                    writer.Write(compressionType);
                    if (compressionType != 0) return writer.Position;
                    writer.Write(compressedSize);
                    writer.Write(uncompressedSize);
                    break;
                default:
                    return writer.Position;
            }
            writer.Write(unityVersionCount);
            for (int i = 0; i < unityVersionCount; i++)
            {
                writer.WriteCountString(pUnityVersions[i]);
            }
            writer.Write(stringTableLen);
            writer.Write(stringTablePos);
            return writer.Position;
        }
    }
}
