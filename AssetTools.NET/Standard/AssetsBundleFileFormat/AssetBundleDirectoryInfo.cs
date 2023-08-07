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
        /// 0x01: Entry is a directory. Unknown usage.
        /// 0x02: Entry is deleted. Unknown usage.
        /// 0x04: Entry is serialized file. Assets files should enable this, and other files like .resS or .resource(s) should disable this.
        /// </summary>
        public uint Flags;
        /// <summary>
        /// Name of this entry.
        /// </summary>
        public string Name;

        /// <summary>
        /// Replacer which can be set by the user.
        /// You can use <see cref="SetNewData(byte[])"/> or <see cref="SetNewData(AssetsFile)"/>
        /// for convenience.
        /// </summary>
        public IContentReplacer Replacer { get; set; }
        /// <summary>
        /// Replacer type such as modified or removed.
        /// </summary>
        public ContentReplacerType ReplacerType => Replacer != null ? Replacer.GetReplacerType() : ContentReplacerType.None;
        /// <summary>
        /// Is the replacer non-null and does the replacer has a preview?
        /// </summary>
        public bool IsReplacerPreviewable => Replacer != null && Replacer.HasPreview();

        /// <summary>
        /// Sets the bytes used when the AssetBundleFile is written.
        /// </summary>
        public void SetNewData(byte[] newBytes)
        {
            Replacer = new ContentReplacerFromBuffer(newBytes);
        }

        /// <summary>
        /// Sets the assets file to use when the AssetBundleFile is written.
        /// </summary>
        public void SetNewData(AssetsFile assetsFile)
        {
            Replacer = new ContentReplacerFromAssets(assetsFile);
        }

        /// <summary>
        /// Set the asset to be removed when the AssetBundleFile is written.
        /// </summary>
        public void SetRemoved()
        {
            Replacer = new ContentRemover();
        }

        /// <summary>
        /// Creates a new directory info.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <param name="isSerialized">Is the file serialized (i.e. is it an assets file)?</param>
        /// <returns>The new directory info</returns>
        public static AssetBundleDirectoryInfo Create(string name, bool isSerialized)
        {
            return new AssetBundleDirectoryInfo()
            {
                Offset = -1,
                DecompressedSize = 0,
                Flags = isSerialized ? 0x04u : 0x00u,
                Name = name
            };
        }
    }
}
