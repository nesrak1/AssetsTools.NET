namespace AssetsTools.NET.Extra
{
    public abstract class SerializingAssetReplacer : AssetsReplacer
    {
        private readonly AssetsManager manager;
        private readonly AssetsFile assetsFile;
        private readonly AssetFileInfoEx asset;

        protected SerializingAssetReplacer(AssetsManager manager, AssetsFileInstance assetsFile, AssetFileInfoEx asset)
            : this(manager, assetsFile.file, asset)
        {
        }

        protected SerializingAssetReplacer(AssetsManager manager, AssetsFile assetsFile, AssetFileInfoEx asset)
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
            return AssetHelper.GetScriptIndex(assetsFile, asset);
        }

        public override long Write(AssetsFileWriter writer)
        {
            AssetTypeValueField baseField = manager.GetTypeInstance(assetsFile, asset).GetBaseField();
            Modify(baseField);
            baseField.Write(writer);
            return writer.Position;
        }

        protected abstract void Modify(AssetTypeValueField baseField);
    }
}
