namespace AssetsTools.NET
{
    public struct PreloadList
    {
        public uint len;
        public AssetPPtr[] items;

        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint format, bool bigEndian)
        {
            len = reader.ReadUInt32();
            items = new AssetPPtr[len];
            for (int i = 0; i < len; i++)
            {
                items[i] = new AssetPPtr(reader.ReadUInt32(), reader.ReadUInt64());
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint format)
        {
            writer.Write(len);
            for (int i = 0; i < len; i++)
            {
                writer.Write(items[i].fileID);
                writer.Write(items[i].pathID);
            }
            return writer.Position;
        }
    }
}
