namespace AssetsTools.NET
{
    public class AssetBundleDirectoryInfo
    {
        /// <summary>
        /// Offset from bundle's data start (header.GetFileDataOffset()).
        /// </summary>
        public long Offset;
        /// <summary>
        /// Decompressed size of this entry.
        /// </summary>
        public long DecompressedSize;
        /// <summary>
        /// Flags of this entry. <br/>
        /// 0x04: File is serialized. Assets files should enable this, and other files like .resS or .resource(s) should disable this.
        /// </summary>
        public uint Flags;
        /// <summary>
        /// Name of this entry.
        /// </summary>
        public string Name;
    }
}
