namespace AssetsTools.NET.Texture
{
    public interface ISwizzler
    {
        // this may change the format, width, or height, but doesn't have to.
        public byte[] PreprocessDeswizzle(byte[] rawData, out TextureFormat format, out int width, out int height);
        // this MUST crop the image back to the image's original width and height
        public byte[] PostprocessDeswizzle(byte[] rawData);
    }
}
