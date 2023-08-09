using System.Collections.Generic;

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
        /// Do not modify this array, it's needed to read the existing file correctly.
        /// </summary>
        public AssetBundleBlockInfo[] BlockInfos { get; set; }
        /// <summary>
        /// List of file infos in this bundle.
        /// You can add new infos or make changes to existing ones and they will be
        /// updated on write.
        /// </summary>
        public List<AssetBundleDirectoryInfo> DirectoryInfos { get; set; }

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
            DirectoryInfos = new List<AssetBundleDirectoryInfo>(directoryCount);
            for (int i = 0; i < directoryCount; i++)
            {
                AssetBundleDirectoryInfo dirInfo = new AssetBundleDirectoryInfo
                {
                    Offset = reader.ReadInt64(),
                    DecompressedSize = reader.ReadInt64(),
                    Flags = reader.ReadUInt32(),
                    Name = reader.ReadNullTerminated()
                };
                DirectoryInfos.Add(dirInfo);
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

            int directoryCount = DirectoryInfos.Count;
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
