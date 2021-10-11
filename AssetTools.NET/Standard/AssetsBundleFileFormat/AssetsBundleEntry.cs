namespace AssetsTools.NET
{
    public class AssetsBundleEntry
    {
        public uint offset;
        public uint length;
        public string name;

        public void Read(AssetsFileReader reader)
        {
            name = reader.ReadNullTerminated();
            offset = reader.ReadUInt32();
            length = reader.ReadUInt32();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(name);
            writer.Write(offset);
            writer.Write(length);
        }
    }
}
