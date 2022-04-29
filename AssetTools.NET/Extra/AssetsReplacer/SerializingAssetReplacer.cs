using System;

namespace AssetsTools.NET.Extra
{
    /// <summary>
    /// Abstract replacer that deserializes the specified asset, lets the deriving class
    /// modify the object graph, and serializes the result
    /// </summary>
    public abstract class SerializingAssetReplacer : AssetsReplacer
    {
        private readonly AssetsManager manager;
        private readonly AssetsFile assetsFile;
        private readonly AssetFileInfo asset;

        protected SerializingAssetReplacer(AssetsManager manager, AssetsFileInstance assetsFile, AssetFileInfo asset)
            : this(manager, assetsFile.file, asset)
        {
        }

        protected SerializingAssetReplacer(AssetsManager manager, AssetsFile assetsFile, AssetFileInfo asset)
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
            return asset.PathId;
        }

        public override int GetClassID()
        {
            return asset.TypeId;
        }

        public override ushort GetMonoScriptID()
        {
            return AssetHelper.GetScriptIndex(assetsFile, asset);
        }

        public override long Write(AssetsFileWriter writer)
        {
            AssetTypeValueField baseField = manager.GetBaseField(assetsFile, asset);
            Modify(baseField);
            baseField.Write(writer);
            return writer.Position;
        }

        protected abstract void Modify(AssetTypeValueField baseField);

        public override long WriteReplacer(AssetsFileWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
