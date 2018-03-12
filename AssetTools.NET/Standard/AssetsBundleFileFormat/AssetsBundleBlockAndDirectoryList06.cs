namespace AssetsTools.NET
{
    public struct AssetsBundleBlockAndDirectoryList06
    {
        public ulong checksumLow;
        public ulong checksumHigh;
        public uint blockCount;
        public AssetsBundleBlockInfo06[] blockInf;
        public uint directoryCount;
        public AssetsBundleDirectoryInfo06[] dirInf;

        ///void Free();
        public bool Read(ulong filePos, AssetsFileReader reader/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            reader.Position = filePos;
            checksumLow = reader.ReadUInt64();
            checksumHigh = reader.ReadUInt64();
            blockCount = reader.ReadUInt32();
            blockInf = new AssetsBundleBlockInfo06[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                blockInf[i].decompressedSize = reader.ReadUInt32();
                blockInf[i].compressedSize = reader.ReadUInt32();
                blockInf[i].flags = reader.ReadUInt16();
            }
            directoryCount = reader.ReadUInt32();
            dirInf = new AssetsBundleDirectoryInfo06[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                dirInf[i].offset = reader.ReadUInt64();
                dirInf[i].decompressedSize = reader.ReadUInt64();
                dirInf[i].flags = reader.ReadUInt32();
                dirInf[i].name = reader.ReadNullTerminated();
            }
            return true;
        }
        //Write doesn't compress
        public bool Write(AssetsFileWriter writer, ulong curFilePos/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            writer.Position = curFilePos;
            writer.Write(checksumHigh);
            writer.Write(checksumLow);
            writer.Write(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                writer.Write(blockInf[i].decompressedSize);
                writer.Write(blockInf[i].compressedSize);
                writer.Write(blockInf[i].flags);
            }
            writer.Write(directoryCount);
            for (int i = 0; i < directoryCount; i++)
            {
                writer.Write(dirInf[i].offset);
                writer.Write(dirInf[i].decompressedSize);
                writer.Write(dirInf[i].flags);
                writer.WriteNullTerminated(dirInf[i].name);
            }
            return true;
        }
    }
}
