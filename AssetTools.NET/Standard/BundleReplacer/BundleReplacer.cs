using System;

namespace AssetsTools.NET
{
    public abstract class BundleReplacer
    {
        public abstract BundleReplacementType GetReplacementType();

        public abstract string GetOriginalEntryName();
        public abstract string GetEntryName();

        public abstract long Write(AssetsFileWriter writer);

        #region Unused methods kept for backwards compatibility

        public virtual bool Init(AssetsFileReader entryReader, long entryPos, long entrySize, ClassDatabaseFile typeMeta = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Uninit()
        {
            throw new NotImplementedException();
        }

        public virtual int GetBundleListIndex()
        {
            throw new NotImplementedException();
        }

        //doc says this value isn't reliable, most likely referring to the FromAssets replacer?
        //anyway, that makes this basically useless if we don't know all values so this just goes unused
        public virtual long GetSize()
        {
            throw new NotImplementedException();
        }

        public virtual long WriteReplacer(AssetsFileWriter writer)
        {
            return writer.Position;
        }

        public virtual bool HasSerializedData()
        {
            return false;
        }

        public static BundleReplacer ReadBundleReplacer(AssetsFileReader reader)
        {
            return null;
        }

        #endregion
    }
}
