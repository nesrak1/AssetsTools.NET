namespace AssetsTools.NET.Extra
{
    public abstract class FieldBasedAssetReplacer : AssetsReplacer
    {
        private readonly AssetsManager manager;
        private readonly AssetsFile assetsFile;
        private readonly AssetFileInfoEx asset;

        protected FieldBasedAssetReplacer(AssetsManager manager, AssetsFile assetsFile, AssetFileInfoEx asset)
        {
            this.manager = manager;
            this.assetsFile = assetsFile;
            this.asset = asset;
        }

        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AddOrModify;
        }

        public override long GetPathID()
        {
            return asset.index;
        }

        public override int GetClassID()
        {
            return (int)asset.curFileType;
        }

        public override ushort GetMonoScriptID()
        {
            return asset.scriptIndex;
        }

        public override long Write(AssetsFileWriter writer)
        {
            AssetTypeValueField baseField = manager.GetTypeInstance(assetsFile, asset).GetBaseField();
            ModifyField(baseField);
            baseField.Write(writer);
            return writer.Position;
        }

        protected abstract void ModifyField(AssetTypeValueField baseField);
    }
}
