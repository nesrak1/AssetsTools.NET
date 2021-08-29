using AssetsTools.NET.Extra;
using System;
using System.IO;

namespace AssetsTools.NET
{
    public class BundleReplacerFromStream : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly bool hasSerializedData;
        private readonly Stream stream;
        private readonly long offset;
        private readonly long size;
        private readonly int bundleListIndex;

        public BundleReplacerFromStream(string oldName, string newName, bool hasSerializedData, Stream stream, long offset, long size, int bundleListIndex = -1)
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
            writer.BaseStream.Position = offset;
            stream.CopyToCompat(writer.BaseStream, size);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)2); //replacer type
            writer.Write((byte)0); //file type (0 bundle, 1 assets)
            writer.WriteCountStringInt16(oldName);
            writer.WriteCountStringInt16(newName);
            writer.Write(hasSerializedData);
            writer.Write(GetSize());
            Write(writer);

            return writer.Position;
        }
        public override bool HasSerializedData()
        {
            return hasSerializedData;
        }
    }
}
