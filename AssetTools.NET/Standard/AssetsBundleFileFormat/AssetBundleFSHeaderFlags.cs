namespace AssetsTools.NET
{
    public enum AssetBundleFSHeaderFlags
    {
        None = 0x00,
        LZMACompressed = 0x01,
        LZ4Compressed = 0x02,
        LZ4HCCompressed = 0x03,
        CompressionMask = 0x3f,
        HasDirectoryInfo = 0x40,
        BlockAndDirAtEnd = 0x80,
        OldWebPluginCompatibility = 0x100,
        BlockInfoNeedPaddingAtStart = 0x200
    }
}
