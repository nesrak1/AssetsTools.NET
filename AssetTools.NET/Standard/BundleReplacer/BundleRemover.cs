using System;

namespace AssetsTools.NET
{
    public class BundleRemover : BundleReplacer
    {
        private readonly string name;
        private readonly bool hasSerializedData;
        private readonly int bundleListIndex;
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
        public override bool Init(AssetBundleFile bundleFile, AssetsFileReader entryReader, long entryPos, long entrySize)
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
            throw new NotImplementedException("not implemented");
        }
        public override bool HasSerializedData()
        {
            return hasSerializedData;
        }
    }
}
