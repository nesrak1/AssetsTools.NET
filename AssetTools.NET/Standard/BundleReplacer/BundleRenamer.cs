using System;

namespace AssetsTools.NET
{
    public class BundleRenamer : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly int bundleListIndex;
        private readonly bool hasSerializedData; // unused, only for compatability with c++ at/uabe

        public BundleRenamer(string oldName, string newName, int bundleListIndex = -1, bool hasSerializedData = false)
        {
            this.oldName = oldName;
            if (newName == null)
                this.newName = oldName;
            else
                this.newName = newName;

            this.bundleListIndex = bundleListIndex;
            this.hasSerializedData = hasSerializedData;
        }
        public override BundleReplacementType GetReplacementType()
        {
            return BundleReplacementType.Rename;
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
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)BundleReplacerType.BundleEntryRenamer); // replacer type
            writer.Write((byte)1); // renamer version
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
            return writer.Position;
        }
        public override bool HasSerializedData()
        {
            return hasSerializedData;
        }
    }
}
