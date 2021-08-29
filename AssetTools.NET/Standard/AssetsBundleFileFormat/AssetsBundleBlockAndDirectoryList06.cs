namespace AssetsTools.NET
{
    public class AssetBundleBlockAndDirectoryList06
    {
        public ulong checksumLow;
        public ulong checksumHigh;
        public int blockCount;
        public AssetBundleBlockInfo06[] blockInf;
        public int directoryCount;
        public AssetBundleDirectoryInfo06[] dirInf;

        public void Read(long filePos, AssetsFileReader reader)
        {
            reader.Position = filePos;
            checksumLow = reader.ReadUInt64();
            checksumHigh = reader.ReadUInt64();
            blockCount = reader.ReadInt32();
            blockInf = new AssetBundleBlockInfo06[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                blockInf[i] = new AssetBundleBlockInfo06();
                blockInf[i].decompressedSize = reader.ReadUInt32();
                blockInf[i].compressedSize = reader.ReadUInt32();
                blockInf[i].flags = reader.ReadUInt16();
            }
            directoryCount = reader.ReadInt32();
            dirInf = new AssetBundleDirectoryInfo06[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                dirInf[i] = new AssetBundleDirectoryInfo06();
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
            blockCount = blockInf.Length;
            writer.Write(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                writer.Write(blockInf[i].decompressedSize);
                writer.Write(blockInf[i].compressedSize);
                writer.Write(blockInf[i].flags);
            }
            directoryCount = dirInf.Length;
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
