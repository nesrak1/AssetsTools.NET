using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class PreloadList
    {
        public int len;
        public List<AssetPPtr> items;

        public void Read(AssetsFileReader reader)
        {
            len = reader.ReadInt32();
            items = new List<AssetPPtr>();
            for (int i = 0; i < len; i++)
            {
                items.Add(new AssetPPtr(reader.ReadInt32(), reader.ReadInt64()));
            }
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(len);
            for (int i = 0; i < len; i++)
            {
                writer.Write(items[i].fileID);
                writer.Write(items[i].pathID);
            }
        }
    }
}
