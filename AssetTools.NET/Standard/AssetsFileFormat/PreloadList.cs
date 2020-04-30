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
                int fileId = reader.ReadInt32();
                reader.Align();
                long pathId = reader.ReadInt64();
                reader.Align();
                items.Add(new AssetPPtr(fileId, pathId));
            }
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(len);
            for (int i = 0; i < len; i++)
            {
                writer.Write(items[i].fileID);
                writer.Align();
                writer.Write(items[i].pathID);
                writer.Align();
            }
        }
    }
}
