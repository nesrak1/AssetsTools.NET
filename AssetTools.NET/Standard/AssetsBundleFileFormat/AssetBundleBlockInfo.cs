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
        /// Flags of this block. <br/>
        /// First 6 bits (0x3f mask): Compression mode. 0 for uncompressed, 1 for LZMA, 2/3 for LZ4/LZ4HC. <br/>
        /// 0x40: Streamed (will be read in blocks) <br/>
        /// </summary>
        public ushort Flags { get; set; }
        public byte GetCompressionType() { return (byte)(Flags & 0x3F); }
    }
}
