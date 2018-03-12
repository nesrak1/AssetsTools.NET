namespace AssetsTools.NET
{
    public struct AssetsBundleEntry
    {
        public uint offset;
        public uint length;
        public byte name;
        ///public uint GetAbsolutePos(AssetsBundleHeader03 pHeader);//, DWORD listIndex);
        ///public uint GetAbsolutePos(AssetsBundleFile pFile);//, DWORD listIndex);
    }
}
