using System;
using System.IO;

namespace AssetsTools.NET.Texture
{
    // PS4 Morton swizzler with padded mip levels.
    public sealed class Ps4Swizzle : ISwizzler
    {
        private readonly int width, height;
        private readonly Ps4MortonLayout layout;
        private readonly byte[] original;
        private readonly TextureFormat format;
        private readonly int mipCount;

        public Ps4Swizzle(int width, int height, TextureFormat format, byte[] original = null, int mipCount = 1)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            layout = new Ps4MortonLayout(width, height, format);
            this.original = original;
            this.mipCount = mipCount;
        }

        public bool CanBeSwizzled() => true;

        public byte[] PreprocessDeswizzle(byte[] rawData, out int decodedWidth, out int decodedHeight)
        {
            decodedWidth = checked(layout.BlocksWide * layout.BlockWidth);
            decodedHeight = checked(layout.BlocksHigh * layout.BlockHeight);
            if (rawData == null)
                throw new InvalidDataException("PS4 texture data is missing.");
            var chain = Ps4MipChain.ForUnity(width, height, format, mipCount, rawData.Length);
            return chain.Levels[0].Deswizzle(rawData.AsSpan(0, chain.Levels[0].TiledSize));
        }

        public byte[] PostprocessDeswizzle(byte[] rawData)
        {
            if (rawData == null)
                return null;
            int stride = checked(layout.BlocksWide * layout.BlockWidth * 4);
            var result = new byte[checked(width * height * 4)];
            for (int y = 0; y < height; y++)
                Buffer.BlockCopy(rawData, y * stride, result, y * width * 4, width * 4);
            return result;
        }

        public byte[] ProcessSwizzle(byte[][] mips, out int[] mipOffsets)
        {
            var chain = original == null
                ? new Ps4MipChain(width, height, format, mips.Length)
                : Ps4MipChain.ForUnity(width, height, format, mips.Length, original.Length);
            mipOffsets = (int[])chain.Offsets.Clone();
            return chain.Swizzle(mips, original);
        }

        // PS4 keeps its existing platform data.
        public byte[] MakePlatformBlob(int[] mipOffsets, uint completeImageSize) => Array.Empty<byte>();
    }
}
