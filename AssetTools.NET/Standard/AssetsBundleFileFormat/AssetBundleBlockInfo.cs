namespace AssetsTools.NET
{
    public class AssetBundleBlockInfo
    {
        /// <summary>
        /// Decompressed size of this block.
        /// </summary>
        public uint DecompressedSize { get; set; }
        /// <summary>
        /// Compressed size of this block. If uncompressed, this is the same as DecompressedSize.
        /// </summary>
        public uint CompressedSize { get; set; }
        /// <summary>
        /// Flags of this block.
        /// </summary>
        public ushort Flags { get; set; }
        public byte GetCompressionType() { return (byte)(Flags & 0x3F); }
    }
}
