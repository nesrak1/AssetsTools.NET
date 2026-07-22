namespace AssetsTools.NET
{
    public struct ClassPackageCommonStringEntry
    {
        public ushort Offset;
        public ushort StringIndex;

        public void Read(AssetsFileReader reader)
        {
            Offset = reader.ReadUInt16();
            StringIndex = reader.ReadUInt16();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(Offset);
            writer.Write(StringIndex);
        }

        public static long GetSize()
        {
            return sizeof(ushort) + sizeof(ushort);
        }
    }
}
