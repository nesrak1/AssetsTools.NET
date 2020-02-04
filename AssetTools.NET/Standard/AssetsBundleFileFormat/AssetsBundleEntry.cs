namespace AssetsTools.NET
{
    public struct AssetsBundleEntry
    {
        public uint offset;
        public uint length;
        public byte name;
        ///public uint GetAbsolutePos(AssetsBundleHeader03 header);//, DWORD listIndex);
        ///public uint GetAbsolutePos(AssetsBundleFile file);//, DWORD listIndex);
    }
}
