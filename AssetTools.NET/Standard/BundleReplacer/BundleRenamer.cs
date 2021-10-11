namespace AssetsTools.NET
{
    public class BundleRenamer : BundleReplacer
    {
        private readonly string oldName;
        private readonly string newName;

        public BundleRenamer(string oldName, string newName, bool hasSerializedData = true, int bundleListIndex = -1)
        {
            this.oldName = oldName;
            if (newName == null)
                this.newName = oldName;
            else
                this.newName = newName;
        }

        public override BundleReplacementType GetReplacementType()
        {
            return BundleReplacementType.Rename;
        }

        public override string GetOriginalEntryName()
        {
            return oldName;
        }

        public override string GetEntryName()
        {
            return newName;
        }

        public override long Write(AssetsFileWriter writer)
        {
            return writer.Position;
        }

        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)1); //replacer type
            writer.Write((byte)0); //file type (0 bundle, 1 assets)
            writer.WriteCountStringInt16(oldName);
            writer.WriteCountStringInt16(newName);
            return writer.Position;
        }

        public override bool HasSerializedData()
        {
            return true;
        }
    }
}
