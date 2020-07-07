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

        public abstract bool Init(AssetBundleFile bundleFile, AssetsFileReader entryReader, long entryPos, long entrySize/*, ClassDatabaseFile typeMeta = null*/);
		public abstract void Uninit();
        
        public abstract long Write(AssetsFileWriter writer);

        public abstract long WriteReplacer(AssetsFileWriter writer);

        //todo, what does this affect
        public abstract bool HasSerializedData();
    }
}
