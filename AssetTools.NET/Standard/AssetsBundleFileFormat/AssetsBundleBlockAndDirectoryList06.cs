namespace AssetsTools.NET
{
    public struct AssetBundleBlockAndDirectoryList06
    {
        public ulong checksumLow;
        public ulong checksumHigh;
        public uint blockCount;
        public AssetBundleBlockInfo06[] blockInf;
        public uint directoryCount;
        public AssetBundleDirectoryInfo06[] dirInf;

        ///void Free();
        public void Read(long filePos, AssetsFileReader reader)
        {
            reader.Position = filePos;
            checksumLow = reader.ReadUInt64();
            checksumHigh = reader.ReadUInt64();
            blockCount = reader.ReadUInt32();
            blockInf = new AssetBundleBlockInfo06[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                blockInf[i].decompressedSize = reader.ReadUInt32();
                blockInf[i].compressedSize = reader.ReadUInt32();
                blockInf[i].flags = reader.ReadUInt16();
            }
            directoryCount = reader.ReadUInt32();
            dirInf = new AssetBundleDirectoryInfo06[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                dirInf[i].offset = reader.ReadInt64();
                dirInf[i].decompressedSize = reader.ReadInt64();
                dirInf[i].flags = reader.ReadUInt32();
                dirInf[i].name = reader.ReadNullTerminated();
            }
        }
        //Write doesn't compress
        public void Write(AssetsFileWriter writer)
        {
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
        }
    }
}
