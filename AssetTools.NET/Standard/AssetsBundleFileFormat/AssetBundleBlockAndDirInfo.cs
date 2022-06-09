using System;

namespace AssetsTools.NET
{
    public class AssetBundleBlockAndDirInfo
    {
        /// <summary>
        /// Hash of this entry.
        /// </summary>
        public Hash128 Hash { get; set; }
        /// <summary>
        /// List of blocks in this bundle.
        /// </summary>
        public AssetBundleBlockInfo[] BlockInfos { get; set; }
        /// <summary>
        /// List of file infos in this bundle.
        /// </summary>
        public AssetBundleDirectoryInfo[] DirectoryInfos { get; set; }

        public void Read(AssetsFileReader reader)
        {
            Hash = new Hash128(reader.ReadBytes(16));

            int blockCount = reader.ReadInt32();
            BlockInfos = new AssetBundleBlockInfo[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                BlockInfos[i] = new AssetBundleBlockInfo();
                BlockInfos[i].DecompressedSize = reader.ReadUInt32();
                BlockInfos[i].CompressedSize = reader.ReadUInt32();
                BlockInfos[i].Flags = reader.ReadUInt16();
            }

            int directoryCount = reader.ReadInt32();
            DirectoryInfos = new AssetBundleDirectoryInfo[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                DirectoryInfos[i] = new AssetBundleDirectoryInfo();
                DirectoryInfos[i].Offset = reader.ReadInt64();
                DirectoryInfos[i].DecompressedSize = reader.ReadInt64();
                DirectoryInfos[i].Flags = reader.ReadUInt32();
                DirectoryInfos[i].Name = reader.ReadNullTerminated();
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            if (Hash.data == null)
            {
                writer.Write((ulong)0);
                writer.Write((ulong)0);
            }
            else
            {
                writer.Write(Hash.data);
            }

            int blockCount = BlockInfos.Length;
            writer.Write(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                writer.Write(BlockInfos[i].DecompressedSize);
                writer.Write(BlockInfos[i].CompressedSize);
                writer.Write(BlockInfos[i].Flags);
            }

            int directoryCount = DirectoryInfos.Length;
            writer.Write(directoryCount);
            for (int i = 0; i < directoryCount; i++)
            {
                writer.Write(DirectoryInfos[i].Offset);
                writer.Write(DirectoryInfos[i].DecompressedSize);
                writer.Write(DirectoryInfos[i].Flags);
                writer.WriteNullTerminated(DirectoryInfos[i].Name);
            }
        }
    }
}
