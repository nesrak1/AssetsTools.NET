using System;

namespace AssetsTools.NET
{
    public class BundleReplacerFromMemory : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly bool hasSerializedData;
        private readonly byte[] buffer;
        private readonly long size;
        private readonly int bundleListIndex;

        public BundleReplacerFromMemory(string oldName, string newName, bool hasSerializedData, byte[] buffer, long size, int bundleListIndex = -1)
        {
            this.oldName = oldName;
            if (newName == null)
                this.newName = oldName;
            else
                this.newName = newName;
            this.hasSerializedData = hasSerializedData;
            this.buffer = buffer;
            if (size == -1)
                this.size = buffer.Length;
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
            writer.Write(buffer);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)BundleReplacerType.BundleEntryModifierFromMemory); // replacer type
            writer.Write((byte)1); // replacer from memory version

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
