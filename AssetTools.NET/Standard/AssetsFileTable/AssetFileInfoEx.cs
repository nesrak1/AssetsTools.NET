namespace AssetsTools.NET
{
    public class AssetFileInfoEx : AssetFileInfo
    {
        public uint curFileType;
        public ulong absoluteFilePos;
        public string name; //-blank unless the first field in that type (so gameobjects would have blank names)
    }
}
