namespace AssetsTools.NET
{
    public class AssetsBundleOffsetPair
    {
        public uint compressed;
        public uint uncompressed;

        public void Read(AssetsFileReader reader)
        {
            compressed = reader.ReadUInt32();
            uncompressed = reader.ReadUInt32();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(compressed);
            writer.Write(uncompressed);
        }

        public AssetsBundleOffsetPair Clone()
        {
            return new AssetsBundleOffsetPair
                   {
                       compressed = compressed,
                       uncompressed = uncompressed
                   };
        }
    }
}
