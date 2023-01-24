using System;

namespace AssetsTools.NET
{
    public class BundleRemover : BundleReplacer
    {
        private readonly string name;
        private readonly int bundleListIndex;

        public BundleRemover(string name, int bundleListIndex = -1)
        {
            this.name = name;
            this.bundleListIndex = bundleListIndex;
        }
        public BundleRemover(int index)
        {
            name = null;
            this.bundleListIndex = index;
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
            writer.Write((short)BundleReplacerType.BundleEntryRemover); // replacer type
            writer.Write((byte)1); // remover version
            if (name != null)
            {
                writer.Write((byte)1);
                writer.WriteCountStringInt16(name);
            }
            else
            {
                writer.Write((byte)0);
            }
            return writer.Position;
        }
        public override bool HasSerializedData()
        {
            return false; // doesn't matter
        }
    }
}
