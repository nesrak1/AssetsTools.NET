namespace AssetsTools.NET
{
    public class AssetBundleBlockInfo06
    {
        public uint decompressedSize;
        public uint compressedSize;
        public ushort flags;
        public byte GetCompressionType() { return (byte)(flags & 0x3F); }
    }
}
