using System;

namespace AssetsTools.NET
{
    public class BundleRemover : BundleReplacer
    {
        private readonly string name;
        private readonly bool hasSerializedData;
        private readonly int bundleListIndex;
        //apparently hasSerializedData was removed from the constructor
        public BundleRemover(string name, bool hasSerializedData, int bundleListIndex = -1)
        {
            this.name = name;
            this.hasSerializedData = hasSerializedData;
            this.bundleListIndex = bundleListIndex;
        }
        public override BundleReplacementType GetReplacementType()
        {
            return BundleReplacementType.Remove;
        }
        public override int GetBundleListIndex()
        {
            return bundleListIndex;
        }
        public override string GetOriginalEntryName()
        {
            return name;
        }
        public override string GetEntryName()
        {
            return name;
        }
        public override long GetSize()
        {
            return 0;
        }
        public override bool Init(AssetsFileReader entryReader, long entryPos, long entrySize, ClassDatabaseFile typeMeta = null)
        {
            return true;
        }
        public override void Uninit()
        {
            return;
        }
        public override long Write(AssetsFileWriter writer)
        {
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)0); //replacer type
            writer.Write((byte)0); //file type (0 bundle, 1 assets)
            writer.WriteCountStringInt16(name);
            return writer.Position;
        }
        public override bool HasSerializedData()
        {
            return hasSerializedData;
        }
    }
}
