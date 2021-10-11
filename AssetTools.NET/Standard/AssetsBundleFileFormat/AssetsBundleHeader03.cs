using System.Linq;

namespace AssetsTools.NET
{
    public class AssetBundleHeader03
    {
        public string signature;
        public uint fileVersion;
        public string minPlayerVersion;
        public string fileEngineVersion;
        public uint minimumStreamedBytes;
        public uint bundleDataOffs;
        public uint numberOfAssetsToDownload;
        public AssetsBundleOffsetPair[] blockList;
        public uint fileSize2;
        public uint assetsListSize;
        public byte unknown3;

        public void Read(AssetsFileReader reader)
        {
            reader.bigEndian = true;
            signature = reader.ReadNullTerminated();
            fileVersion = reader.ReadUInt32();
            minPlayerVersion = reader.ReadNullTerminated();
            fileEngineVersion = reader.ReadNullTerminated();
            minimumStreamedBytes = reader.ReadUInt32();
            bundleDataOffs = reader.ReadUInt32();
            numberOfAssetsToDownload = reader.ReadUInt32();
            uint blockCount = reader.ReadUInt32();
            blockList = new AssetsBundleOffsetPair[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                blockList[i] = new AssetsBundleOffsetPair();
                blockList[i].Read(reader);
            }

            if (fileVersion >= 2)
                fileSize2 = reader.ReadUInt32();

            if (fileVersion >= 3)
                assetsListSize = reader.ReadUInt32();

            unknown3 = reader.ReadByte();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.bigEndian = true;
            writer.WriteNullTerminated(signature);
            writer.Write(fileVersion);
            writer.WriteNullTerminated(minPlayerVersion);
            writer.WriteNullTerminated(fileEngineVersion);
            writer.Write(minimumStreamedBytes);
            writer.Write(bundleDataOffs);
            writer.Write(numberOfAssetsToDownload);
            writer.Write(blockList.Length);
            foreach (AssetsBundleOffsetPair block in blockList)
            {
                block.Write(writer);
            }

            if (fileVersion >= 2)
                writer.Write(fileSize2);

            if (fileVersion >= 3)
                writer.Write(assetsListSize);

            writer.Write(unknown3);
        }

        public AssetBundleHeader03 Clone()
        {
            return new AssetBundleHeader03
                   {
                       signature = signature,
                       fileVersion = fileVersion,
                       minPlayerVersion = minPlayerVersion,
                       fileEngineVersion = fileEngineVersion,
                       minimumStreamedBytes = minimumStreamedBytes,
                       bundleDataOffs = bundleDataOffs,
                       numberOfAssetsToDownload = numberOfAssetsToDownload,
                       blockList = blockList.Select(b => b.Clone()).ToArray(),
                       fileSize2 = fileSize2,
                       assetsListSize = assetsListSize,
                       unknown3 = unknown3
                   };
        }
    }
}
