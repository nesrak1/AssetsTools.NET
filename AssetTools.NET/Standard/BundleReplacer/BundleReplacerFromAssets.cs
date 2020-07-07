using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class BundleReplacerFromAssets : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;
        private readonly AssetsFile assetsFile;
        private readonly List<AssetsReplacer> replacers;
        private readonly uint fileId;
        private readonly int bundleListIndex;

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
            //todo, we have to write to ms because AssetsFile.Write
            //rewrites back at 0 which is the bundle header, not
            //the top of the assets file
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                assetsFile.Write(w, 0, replacers, fileId);
                data = ms.ToArray();
            }
            writer.Write(data);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            throw new NotImplementedException("not implemented");
        }
        public override bool HasSerializedData()
        {
            return true;
        }
    }
}
