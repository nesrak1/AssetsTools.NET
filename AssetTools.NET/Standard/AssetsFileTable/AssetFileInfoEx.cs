using System;

namespace AssetsTools.NET
{
    public class AssetFileInfoEx : AssetFileInfo
    {
        public uint curFileType;
        public long absoluteFilePos;

        //recommend GetAssetNameFast if at possible
        public bool ReadName(AssetsFile file, out string str)
        {
            str = string.Empty;
            AssetsFileReader reader = file.reader;
            if (AssetsFileExtra.HasName(curFileType))
            {
                reader.Position = absoluteFilePos;
                int length = Math.Min(reader.ReadInt32(), 99);
                str = reader.ReadStringLength(length);
                return true;
            }
            return false;
        }
    }
}
