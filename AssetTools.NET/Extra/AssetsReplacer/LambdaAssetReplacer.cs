using System;

namespace AssetsTools.NET.Extra
{
    /// <summary>
    /// Replacer that deserializes the specified asset, lets a delegate
    /// modify the object graph, and serializes the result
    /// </summary>
    public class LambdaAssetReplacer : SerializingAssetReplacer
    {
        private readonly Action<AssetTypeValueField> modify;

        public LambdaAssetReplacer(AssetsManager manager, AssetsFileInstance assetsFile, AssetFileInfoEx asset, Action<AssetTypeValueField> modify)
            : this(manager, assetsFile.file, asset, modify)
        {
        }

        public LambdaAssetReplacer(AssetsManager manager, AssetsFile assetsFile, AssetFileInfoEx asset, Action<AssetTypeValueField> modify)
            : base(manager, assetsFile, asset)
        {
            this.modify = modify;
        }

        protected override void Modify(AssetTypeValueField baseField)
        {
            modify(baseField);
        }
    }
}
