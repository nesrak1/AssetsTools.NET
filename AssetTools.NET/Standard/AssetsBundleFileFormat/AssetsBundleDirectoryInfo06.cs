namespace AssetsTools.NET
{
    public class AssetBundleDirectoryInfo06
    {
        public long offset;
        public long decompressedSize;
        public uint flags;
        public string name;
        public long GetAbsolutePos(AssetBundleHeader06 header)
        {
            return header.GetFileDataOffset() + offset;
        }
        public long GetAbsolutePos(AssetBundleFile file)
        {
            return file.bundleHeader6.GetFileDataOffset() + offset;
        }
    }
}
