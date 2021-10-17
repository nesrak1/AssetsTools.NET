namespace AssetsTools.NET.Extra
{
    public abstract class Texture2DAssetReplacer : SerializingAssetReplacer
    {
        protected Texture2DAssetReplacer(AssetsManager manager, AssetsFile assetsFile, AssetFileInfoEx asset)
            : base(manager, assetsFile, asset)
        {
        }

        public override ushort GetMonoScriptID()
        {
            return 0xFFFF;
        }

        protected override void Modify(AssetTypeValueField baseField)
        {
            TextureFile texture = TextureFile.ReadTextureFile(baseField);
            SetTextureData(texture, GetBgra());
            TextureFile.WriteTextureFile(texture, baseField);
        }

        /// <summary>
        /// Retrieves the new image to patch into the texture, in the format BGRA32
        /// </summary>
        protected abstract byte[] GetBgra();

        /// <summary>
        /// Encodes the BGRA32 data and stores the result in the texture. Can be overriden in case the texture uses
        /// a format that AssetsTools.NET doesn't support yet; in this case, write the encoded data directly to
        /// <see cref="TextureFile.pictureData"/> instead of calling <see cref="TextureFile.SetTextureData"/>.
        /// </summary>
        protected virtual void SetTextureData(TextureFile texture, byte[] bgra)
        {
            texture.SetTextureData(bgra);
        }
    }
}
