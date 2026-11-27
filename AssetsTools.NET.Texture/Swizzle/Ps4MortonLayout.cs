using System;
using System.IO;

namespace AssetsTools.NET.Texture
{
    // PS4 8x8 Morton tiles of pixels or compressed blocks.
    public sealed class Ps4MortonLayout
    {
        public int BlockWidth { get; }
        public int BlockHeight => BlockWidth;
        public int BlocksWide { get; }
        public int BlocksHigh { get; }
        public int BytesPerBlock { get; }
        public int LinearSize { get; }
        public int TiledSize { get; }
        private readonly int tilesWide;

        // Unity PS4 preprocessing expands RGB24 to four-byte RGBA storage.
        public static TextureFormat GetStorageFormat(TextureFormat format)
            => format == TextureFormat.RGB24 ? TextureFormat.RGBA32 : format;

        public Ps4MortonLayout(int width, int height, TextureFormat format)
            : this(width, height, format, width, height)
        {
        }

        internal Ps4MortonLayout(int width, int height, TextureFormat format, int storageWidth, int storageHeight)
        {
            format = GetStorageFormat(format);
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (storageWidth < width || storageHeight < height)
                throw new ArgumentOutOfRangeException(nameof(storageWidth));
            if (!ConsoleTextureElements.TryGetInfo(format, out int block, out int bytes))
                throw new NotSupportedException($"Unsupported PS4 Morton element format: {format}.");
            BlockWidth = block;
            BytesPerBlock = bytes;
            checked
            {
                BlocksWide = (width + BlockWidth - 1) / BlockWidth;
                BlocksHigh = (height + BlockWidth - 1) / BlockWidth;
                int storageBlocksWide = (storageWidth + BlockWidth - 1) / BlockWidth;
                int storageBlocksHigh = (storageHeight + BlockWidth - 1) / BlockWidth;
                tilesWide = (storageBlocksWide + 7) / 8;
                LinearSize = BlocksWide * BlocksHigh * BytesPerBlock;
                TiledSize = tilesWide * ((storageBlocksHigh + 7) / 8) * 64 * BytesPerBlock;
            }
        }

        private int TiledOffset(int x, int y)
        {
            int morton = (x & 1) | ((y & 1) << 1)
                | ((x & 2) << 1) | ((y & 2) << 2)
                | ((x & 4) << 2) | ((y & 4) << 3);
            return (((y / 8) * tilesWide + x / 8) * 64 + morton) * BytesPerBlock;
        }

        public byte[] Deswizzle(ReadOnlySpan<byte> tiled)
        {
            if (tiled.Length < TiledSize)
                throw new InvalidDataException(
                    $"Incomplete PS4 tiled data: need {TiledSize} bytes for the padded top level, got {tiled.Length}. " +
                    "The original texture data is required.");

            byte[] linear = new byte[LinearSize];
            for (int y = 0; y < BlocksHigh; y++)
                for (int x = 0; x < BlocksWide; x++)
                    tiled.Slice(TiledOffset(x, y), BytesPerBlock).CopyTo(
                        linear.AsSpan((y * BlocksWide + x) * BytesPerBlock, BytesPerBlock));
            return linear;
        }

        // Replace image data while keeping the original padding.
        public byte[] Swizzle(ReadOnlySpan<byte> linear, ReadOnlySpan<byte> originalTiled)
        {
            if (linear.Length != LinearSize || originalTiled.Length != TiledSize)
                throw new InvalidDataException("PS4 import requires one complete mip with the original padded size.");

            byte[] tiled = originalTiled.ToArray();
            for (int y = 0; y < BlocksHigh; y++)
                for (int x = 0; x < BlocksWide; x++)
                    linear.Slice((y * BlocksWide + x) * BytesPerBlock, BytesPerBlock).CopyTo(
                        tiled.AsSpan(TiledOffset(x, y), BytesPerBlock));
            return tiled;
        }
    }
}
