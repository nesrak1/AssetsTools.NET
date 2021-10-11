namespace AssetsTools.NET
{
    public class BundleRemover : BundleReplacer
    {
        private readonly string name;

        public BundleRemover(string name, bool hasSerializedData = true, int bundleListIndex = -1)
        {
            this.name = name;
        }

        public override BundleReplacementType GetReplacementType()
        {
            return BundleReplacementType.Remove;
        }

        public override string GetOriginalEntryName()
        {
            return name;
        }

        public override string GetEntryName()
        {
            return name;
        }

        public override long Write(AssetsFileWriter writer)
        {
            return writer.Position;
        }

        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)0); //replacer type
            writer.Write((byte)0); //file type (0 bundle, 1 assets)
            writer.WriteCountStringInt16(name);
            return writer.Position;
        }

        public override bool HasSerializedData()
        {
            return true;
        }
    }
}
