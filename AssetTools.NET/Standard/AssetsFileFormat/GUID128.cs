namespace AssetsTools.NET
{
    // todo: very similar to hash128?
    public struct GUID128
    {
        public ulong mostSignificant;
        public ulong leastSignificant;
        public void Read(AssetsFileReader reader)
        {
            mostSignificant = reader.ReadUInt64();
            leastSignificant = reader.ReadUInt64();
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(mostSignificant);
            writer.Write(leastSignificant);
        }
    }
}
