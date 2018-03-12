namespace AssetsTools.NET
{
    public struct AssetsBundleBlockInfo06
    {
        public uint decompressedSize;
        public uint compressedSize;
        public ushort flags; //(flags & 0x40) : is streamed; (flags & 0x3F) :  compression info;
        public byte GetCompressionType() { return (byte)(flags & 0x3F); }
        //example flags (LZMA, streamed) : 0x41
        //example flags (LZ4, not streamed) : 0x03
    }
}
