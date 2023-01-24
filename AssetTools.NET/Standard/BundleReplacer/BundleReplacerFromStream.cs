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

        public BundleReplacerFromStream(string oldName, string newName, bool hasSerializedData, Stream stream, long offset = 0, long size = -1, int bundleListIndex = -1)
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
            stream.Position = offset;
            stream.CopyToCompat(writer.BaseStream, size);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)BundleReplacerType.BundleEntryModifierFromStream); // replacer type
            writer.Write((byte)1); // replacer from stream version

            if (oldName != null)
            {
                writer.Write((byte)1);
                writer.WriteCountStringInt16(oldName);
            }
            else
            {
                writer.Write((byte)0);
            }

            if (newName != null)
            {
                writer.Write((byte)1);
                writer.WriteCountStringInt16(newName);
            }
            else
            {
                writer.Write((byte)0);
            }

            writer.Write(hasSerializedData);
            writer.Write(size);
            Write(writer);

            return writer.Position;
        }
        public override bool HasSerializedData()
        {
            return hasSerializedData;
        }
    }
}
