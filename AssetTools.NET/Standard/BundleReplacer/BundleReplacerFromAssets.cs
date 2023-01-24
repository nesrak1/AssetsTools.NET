using System.Collections.Generic;
using System.IO;
using AssetsTools.NET.Extra;

namespace AssetsTools.NET
{
    public class BundleReplacerFromAssets : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly int bundleListIndex;
        private AssetsFile assetsFile;
        private readonly List<AssetsReplacer> assetReplacers;

        public BundleReplacerFromAssets(string oldName, string newName, AssetsFile assetsFile, List<AssetsReplacer> assetReplacers, uint fileId = 0, int bundleListIndex = -1)
        {
            this.oldName = oldName;
            this.newName = newName ?? oldName;
            this.assetsFile = assetsFile;
            this.assetReplacers = assetReplacers;
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
            if (assetsFile != null)
                return false;

            SegmentStream stream = new SegmentStream(entryReader.BaseStream, entryPos, entrySize);
            AssetsFileReader reader = new AssetsFileReader(stream);
            assetsFile = new AssetsFile();
            assetsFile.Read(reader);
            return true;
        }

        public override void Uninit()
        {
            assetsFile.Close();
        }

        public override long Write(AssetsFileWriter writer)
        {
            // Some parts of an assets file need to be aligned to a multiple of 4/8/16 bytes,
            // but for this to work correctly, the start of the file of course needs to be aligned too.
            // In a loose .assets file this is true by default, but inside a bundle file,
            // this might not be the case. Therefore wrap the bundle output stream in a SegmentStream
            // which will make it look like the start of the new assets file is at position 0
            SegmentStream alignedStream = new SegmentStream(writer.BaseStream, writer.Position);
            AssetsFileWriter alignedWriter = new AssetsFileWriter(alignedStream);
            assetsFile.Write(alignedWriter, -1, assetReplacers);
            writer.Position = writer.BaseStream.Length;
            return writer.Position;
        }

        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)BundleReplacerType.BundleEntryModifierFromAssets); // replacer type
            writer.Write((byte)1); // replacer from assets version
            
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

            writer.Write((byte)1); // always true, hasSerializedData
            writer.Write((long)assetReplacers.Count);
            foreach (AssetsReplacer replacer in assetReplacers)
            {
                replacer.WriteReplacer(writer);
            }

            return writer.Position;
        }

        public override bool HasSerializedData()
        {
            return true;
        }
    }
}
