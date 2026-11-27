using System;
using System.IO;

namespace AssetsTools.NET.Texture
{
    // PS5 standard 256-byte, 4 KB and 64 KB tiles, including mipmaps.
    public sealed class Ps5Swizzle : ISwizzler
    {
        private readonly int width;
        private readonly int height;
        private readonly TextureFormat format;
        private readonly byte[] original;
        private readonly int storedSizeHint;
        private readonly int mipCount;

        public Ps5Swizzle(int width, int height, TextureFormat format, byte[] original = null, int storedSizeHint = 0, int mipCount = 1)
        {
            this.mipCount = mipCount;
            this.width = width;
            this.height = height;
            this.format = format;
            this.original = original;
            this.storedSizeHint = storedSizeHint > 0 ? storedSizeHint : original?.Length ?? 0;
        }

        public bool CanBeSwizzled() => Ps5GfxLayout.IsSupportedFormat(format);

        public byte[] PreprocessDeswizzle(byte[] rawData, out int decodedWidth, out int decodedHeight)
        {
            if (rawData == null || rawData.Length == 0)
                throw new InvalidDataException("PS5 texture data is missing.");

            var chain = Ps5MipChain.ForUnity(width, height, format, mipCount, rawData.Length);
            var layout = chain.Levels[0];

            decodedWidth = checked(layout.ElementsWide * layout.PixelBlockSize);
            decodedHeight = checked(layout.ElementsHigh * layout.PixelBlockSize);
            return chain.DeswizzleLevel(rawData, 0);
        }

        public byte[] PostprocessDeswizzle(byte[] rawData)
        {
            if (rawData == null)
                return null;

            if (!Ps5GfxLayout.TryGetElementInfo(format, out int pixelBlockSize, out _))
                throw new NotSupportedException($"Unsupported PS5 texture format: {format}.");

            int decodedWidth = checked(((width + pixelBlockSize - 1) / pixelBlockSize) * pixelBlockSize);
            int decodedHeight = checked(((height + pixelBlockSize - 1) / pixelBlockSize) * pixelBlockSize);

            int srcStride = checked(decodedWidth * 4);
            int dstStride = checked(width * 4);
            if (rawData.Length < checked(srcStride * decodedHeight))
                throw new InvalidDataException("Decoded PS5 texture buffer is smaller than expected.");

            if (decodedWidth == width && decodedHeight == height)
                return rawData;

            byte[] result = new byte[checked(width * height * 4)];
            for (int y = 0; y < height; y++)
                Buffer.BlockCopy(rawData, y * srcStride, result, y * dstStride, dstStride);
            return result;
        }

        public byte[] ProcessSwizzle(byte[][] mips, out int[] mipOffsets)
        {
            if (mips == null || mips.Length != mipCount)
                throw new InvalidDataException("PS5 import must retain the original mip count.");
            int sizeHint = original != null && original.Length > 0 ? original.Length : storedSizeHint;
            var chain = Ps5MipChain.ForUnity(width, height, format, mipCount, sizeHint);
            mipOffsets = (int[])chain.Offsets.Clone();
            return chain.Swizzle(mips, original);

        }

        // PS5 keeps its existing platform data.
        public byte[] MakePlatformBlob(int[] mipOffsets, uint completeImageSize) => Array.Empty<byte>();
    }
}
