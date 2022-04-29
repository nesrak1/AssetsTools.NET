namespace AssetsTools.NET
{
    public class AssetBundleBlockInfo
    {
        public uint DecompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public ushort Flags { get; set; }
        public byte GetCompressionType() { return (byte)(Flags & 0x3F); }
    }
}
