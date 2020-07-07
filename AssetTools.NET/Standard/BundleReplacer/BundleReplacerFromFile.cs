using System;
using System.IO;

namespace AssetsTools.NET
{
    public class BundleReplacerFromFile : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly bool hasSerializedData;
        private readonly FileStream stream;
        private readonly long offset;
        private readonly long size;
        private readonly int bundleListIndex;

        public BundleReplacerFromFile(string oldName, string newName, bool hasSerializedData, FileStream stream, long offset, long size, int bundleListIndex = -1)
        {
            this.oldName = oldName;
            if (newName == null)
                this.newName = oldName;
            else
                this.newName = newName;
            this.hasSerializedData = hasSerializedData;
            this.stream = stream;
            this.offset = offset;
            if (size == -1)
                this.size = stream.Length;
            else
                this.size = size;
            this.bundleListIndex = bundleListIndex;
        }
        public override BundleReplacementType GetReplacementType()
        {
            return BundleReplacementType.AddOrModify;
        }
        public override int GetBundleListIndex()
        {
            return bundleListIndex;
        }
        public override string GetOriginalEntryName()
        {
            return oldName;
        }
        public override string GetEntryName()
        {
            return newName;
        }
        public override long GetSize()
        {
            return size;
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
            byte[] assetData = new byte[size];
            stream.Read(assetData, (int)offset, (int)size);
            writer.Write(assetData);
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
