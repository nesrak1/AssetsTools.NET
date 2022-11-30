using AssetsTools.NET.Extra;

namespace AssetsTools.NET.Texture
{
    public abstract class Texture2DAssetReplacer : SerializingAssetReplacer
    {
        protected Texture2DAssetReplacer(AssetsManager manager, AssetsFileInstance assetsFile, AssetFileInfo asset)
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
            GetNewTextureData(out byte[] bgra, out int width, out int height);
            SetTextureData(texture, bgra, width, height);
            texture.WriteTo(baseField);
        }

        /// <summary>
        /// Retrieves the new image to patch into the texture, in the format BGRA32
        /// </summary>
        protected abstract void GetNewTextureData(out byte[] bgra, out int width, out int height);

        /// <summary>
        /// Encodes the BGRA32 data returned by <see cref="GetNewTextureData"/> and stores the result
        /// in the texture. Can be overriden in case the texture uses a format that AssetsTools.NET
        /// doesn't support yet; in this case, encode the image yourself and pass it to
        /// <see cref="TextureFile.SetTextureDataRaw"/>.
        /// </summary>
        protected virtual void SetTextureData(TextureFile texture, byte[] bgra, int width, int height)
        {
            texture.SetTextureData(bgra, width, height);
        }
    }
}
