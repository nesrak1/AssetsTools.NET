namespace AssetsTools.NET.Texture
{
    public interface ISwizzler
    {
        // this may change the format, width, or height, but doesn't have to.
        byte[] PreprocessDeswizzle(byte[] rawData, out int width, out int height);
        // this MUST crop the image back to the image's original width and height
        byte[] PostprocessDeswizzle(byte[] rawData);

        byte[] ProcessSwizzle(byte[][] rawData, out int[] mipOffsets);
        byte[] MakePlatformBlob(int[] mipOffsets, uint completeImageSize);

        bool CanBeSwizzled();
    }
}
