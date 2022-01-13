namespace AssetsTools.NET
{
    public class AssetsList
    {
        public AssetsBundleEntry[] entries;

        public void Read(AssetsFileReader reader)
        {
            uint count = reader.ReadUInt32();
            entries = new AssetsBundleEntry[count];
            for (int i = 0; i < count; i++)
            {
                entries[i] = new AssetsBundleEntry();
                entries[i].Read(reader);
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            if (entries == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(entries.Length);
            foreach (AssetsBundleEntry entry in entries)
            {
                entry.Write(writer);
            }
        }
    }
}
