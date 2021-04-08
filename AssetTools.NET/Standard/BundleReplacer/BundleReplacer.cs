namespace AssetsTools.NET
{
    public abstract class BundleReplacer
    {
        public abstract BundleReplacementType GetReplacementType();

        public abstract int GetBundleListIndex();

        public abstract string GetOriginalEntryName();
        public abstract string GetEntryName();

        //doc says this value isn't reliable, most likely referring to the FromAssets replacer?
        //anyway, that makes this basically useless if we don't know all values so this just goes unused
        public abstract long GetSize();

        //despite sounding generic, this method only seems to work only on BundleReplacerFromAssets.
        //whenever you read in a BundleReplacerFromAssets, you might not initially have the AssetsFile.
        //in the case of the installer, you read the replacer from the emip first, then call Init
        //when you have loaded the bundle. except for you can't give the AssetsFile manually, you
        //must give it a reader and position and let it read the AssetsFile for you. strange, but
        //that's just how it works I guess. also the bundle argument seemed useless, so I removed it.
        public abstract bool Init(AssetsFileReader entryReader, long entryPos, long entrySize, ClassDatabaseFile typeMeta = null);
		public abstract void Uninit();
        
        public abstract long Write(AssetsFileWriter writer);

        public abstract long WriteReplacer(AssetsFileWriter writer);

        //todo, what does this affect
        public abstract bool HasSerializedData();

        public static BundleReplacer ReadBundleReplacer(AssetsFileReader reader)
        {
            return null;
        }
    }
}
