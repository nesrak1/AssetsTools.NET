using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class BundleReplacerFromAssets : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly List<AssetsReplacer> replacers;
        private readonly uint fileId;
        private readonly int bundleListIndex;
        private AssetsFile assetsFile;
        private ClassDatabaseFile typeMeta;

        public BundleReplacerFromAssets(string oldName, string newName, AssetsFile assetsFile, List<AssetsReplacer> replacers, uint fileId, int bundleListIndex = -1)
        {
            this.oldName = oldName;
            if (newName == null)
                this.newName = oldName;
            else
                this.newName = newName;
            this.assetsFile = assetsFile;
            this.replacers = replacers;
            this.fileId = fileId;
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
            return -1; //todo
        }
        public override bool Init(AssetsFileReader entryReader, long entryPos, long entrySize, ClassDatabaseFile typeMeta = null)
        {
            if (assetsFile != null)
                return false;

            this.typeMeta = typeMeta;

            entryReader.Position = entryPos;
            //memorystream for alignment issue
            MemoryStream ms = new MemoryStream();
            AssetsFileReader r = new AssetsFileReader(ms);
            AssetsFileWriter w = new AssetsFileWriter(ms);
            {
                w.Write(entryReader.ReadBytes((int)entrySize));
                ms.Position = 0;
                assetsFile = new AssetsFile(r);
            }
            return true;
        }
        public override void Uninit()
        {
            assetsFile.Close();
            return;
        }
        public override long Write(AssetsFileWriter writer)
        {
            //memorystream for alignment issue
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                assetsFile.Write(w, -1, replacers, fileId, typeMeta);
                writer.Write(ms.ToArray());
            }
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)4); //replacer type
            writer.Write((byte)0); //file type (0 bundle, 1 assets)
            writer.WriteCountStringInt16(oldName);
            writer.WriteCountStringInt16(newName);
            writer.Write((byte)1); //probably hasSerializedData
            writer.Write((long)replacers.Count);
            foreach (AssetsReplacer replacer in replacers)
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
