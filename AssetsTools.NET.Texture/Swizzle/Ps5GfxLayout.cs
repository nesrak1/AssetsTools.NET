using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Texture
{
    // PS5 standard 2D tiles: 256B_S, 4KB_S and 64KB_S.
    // Compressed blocks are moved as a whole.
    public sealed class Ps5GfxLayout
    {
        public const int TileMode256B = 1;
        public const int TileMode4KB = 5;
        public const int TileMode64KB = 9;

        public int Width { get; }
        public int Height { get; }
        public TextureFormat Format { get; }
        public int TileMode { get; }

        // Pixel width and height of one stored element.
        public int PixelBlockSize { get; }
        public int BytesPerElement { get; }

        public int ElementsWide { get; }
        public int ElementsHigh { get; }
        public int BlockWidth { get; }
        public int BlockHeight { get; }
        public int BlockSize { get; }
        public int PaddedElementsWide { get; }
        public int PaddedElementsHigh { get; }
        public int LinearSize { get; }
        public int TiledSize { get; }

        private readonly int blocksPerRow;

        public Ps5GfxLayout(int width, int height, TextureFormat format, int tileMode)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "PS5 texture dimensions must be positive.");
            if (!TryGetElementInfo(format, out int pixelBlockSize, out int bytesPerElement))
                throw new NotSupportedException($"Unsupported PS5 texture format: {format}.");
            if (tileMode != TileMode256B && tileMode != TileMode4KB && tileMode != TileMode64KB)
                throw new NotSupportedException($"Unsupported PS5 Gen5 standard tile mode: {tileMode}.");

            Width = width;
            Height = height;
            Format = format;
            TileMode = tileMode;
            PixelBlockSize = pixelBlockSize;
            BytesPerElement = bytesPerElement;

            checked
            {
                ElementsWide = DivRoundUp(width, pixelBlockSize);
                ElementsHigh = DivRoundUp(height, pixelBlockSize);

                GetBlockLayout(bytesPerElement, tileMode, out int blockWidth, out int blockHeight, out int blockSize);
                BlockWidth = blockWidth;
                BlockHeight = blockHeight;
                BlockSize = blockSize;

                PaddedElementsWide = AlignUp(ElementsWide, BlockWidth);
                PaddedElementsHigh = AlignUp(ElementsHigh, BlockHeight);
                blocksPerRow = PaddedElementsWide / BlockWidth;

                LinearSize = ElementsWide * ElementsHigh * BytesPerElement;
                TiledSize = PaddedElementsWide * PaddedElementsHigh * BytesPerElement;
            }
        }

        public static bool IsSupportedFormat(TextureFormat format)
            => TryGetElementInfo(format, out _, out _);

        // Observed Unity PS5 preprocessing stores RGB24 as four-byte RGBA elements.
        public static TextureFormat GetStorageFormat(TextureFormat format)
            => format == TextureFormat.RGB24 ? TextureFormat.RGBA32 : format;

        public static bool TryGetElementInfo(TextureFormat format, out int pixelBlockSize, out int bytesPerElement)
            => ConsoleTextureElements.TryGetInfo(GetStorageFormat(format), out pixelBlockSize, out bytesPerElement);

        // Find a tile mode by size for a single mip. Reject ambiguous sizes.
        public static int InferTileMode(int width, int height, TextureFormat format, int storedSize)
        {
            if (storedSize <= 0)
                throw new InvalidDataException("PS5 texture has no stored payload size.");

            var matches = new List<int>(3);
            foreach (int mode in new[] { TileMode4KB, TileMode64KB, TileMode256B })
            {
                var layout = new Ps5GfxLayout(width, height, format, mode);
                if (layout.TiledSize == storedSize)
                    matches.Add(mode);
            }

            if (matches.Count == 1)
                return matches[0];

            if (matches.Count > 1)
            {
                throw new NotSupportedException(
                    "PS5 payload size matches multiple standard tile modes. The tile mode must be specified.");
            }

            var l1 = new Ps5GfxLayout(width, height, format, TileMode256B);
            var l5 = new Ps5GfxLayout(width, height, format, TileMode4KB);
            var l9 = new Ps5GfxLayout(width, height, format, TileMode64KB);
            throw new InvalidDataException(
                $"PS5 payload size {storedSize} does not match a supported one-mip standard layout " +
                $"(256B_S={l1.TiledSize}, 4KB_S={l5.TiledSize}, 64KB_S={l9.TiledSize}).");
        }

        public byte[] Deswizzle(ReadOnlySpan<byte> tiled)
        {
            if (tiled.Length != TiledSize)
                throw new InvalidDataException(
                    $"PS5 tiled data size mismatch: expected {TiledSize}, got {tiled.Length}.");

            byte[] linear = new byte[LinearSize];
            for (int y = 0; y < ElementsHigh; y++)
            {
                for (int x = 0; x < ElementsWide; x++)
                {
                    int src = TiledOffset(x, y);
                    int dst = checked((y * ElementsWide + x) * BytesPerElement);
                    tiled.Slice(src, BytesPerElement).CopyTo(linear.AsSpan(dst, BytesPerElement));
                }
            }
            return linear;
        }

        // Replace image data, keeping padding when the original buffer is supplied.
        public byte[] Swizzle(ReadOnlySpan<byte> linear, ReadOnlySpan<byte> originalTiled = default)
        {
            if (linear.Length != LinearSize)
                throw new InvalidDataException(
                    $"PS5 linear data size mismatch: expected {LinearSize}, got {linear.Length}.");
            if (!originalTiled.IsEmpty && originalTiled.Length != TiledSize)
                throw new InvalidDataException(
                    $"PS5 original padding buffer size mismatch: expected {TiledSize}, got {originalTiled.Length}.");

            byte[] tiled = originalTiled.IsEmpty ? new byte[TiledSize] : originalTiled.ToArray();
            for (int y = 0; y < ElementsHigh; y++)
            {
                for (int x = 0; x < ElementsWide; x++)
                {
                    int src = checked((y * ElementsWide + x) * BytesPerElement);
                    int dst = TiledOffset(x, y);
                    linear.Slice(src, BytesPerElement).CopyTo(tiled.AsSpan(dst, BytesPerElement));
                }
            }
            return tiled;
        }

        internal int TiledOffset(int x, int y)
        {
            int blockX = x / BlockWidth;
            int blockY = y / BlockHeight;
            int localX = x % BlockWidth;
            int localY = y % BlockHeight;

            int offsetInBlock = TileMode == TileMode64KB
                ? Thin64KOffsetInBlock(localX, localY, BytesPerElement)
                : Thin4KOffsetInBlock(localX, localY, BytesPerElement);

            if (TileMode == TileMode256B)
                offsetInBlock &= 0xFF;

            int blockIndex = checked(blockY * blocksPerRow + blockX);
            return checked(blockIndex * BlockSize + offsetInBlock);
        }

        private static void GetBlockLayout(int bpe, int tileMode, out int blockWidth, out int blockHeight, out int blockSize)
        {
            int i = bpe switch
            {
                1 => 0,
                2 => 1,
                4 => 2,
                8 => 3,
                16 => 4,
                _ => throw new NotSupportedException($"Unsupported PS5 element size: {bpe}.")
            };

            if (tileMode == TileMode256B)
            {
                int[] widths = { 16, 16, 8, 8, 4 };
                int[] heights = { 16, 8, 8, 4, 4 };
                blockWidth = widths[i];
                blockHeight = heights[i];
                blockSize = 0x100;
                return;
            }

            if (tileMode == TileMode4KB)
            {
                int[] widths = { 64, 64, 32, 32, 16 };
                int[] heights = { 64, 32, 32, 16, 16 };
                blockWidth = widths[i];
                blockHeight = heights[i];
                blockSize = 0x1000;
                return;
            }

            int[] widths64 = { 256, 256, 128, 128, 64 };
            int[] heights64 = { 256, 128, 128, 64, 64 };
            blockWidth = widths64[i];
            blockHeight = heights64[i];
            blockSize = 0x10000;
        }

        // Gen5 4KB standard thin-2D byte offset within a tile.
        private static int Thin4KOffsetInBlock(int x, int y, int bpe)
        {
            int o = 0;
            switch (bpe)
            {
                case 1:
                    o ^= (y << 4) & 0x1F0;
                    o ^= (y << 5) & 0x400;
                    o ^= x & 0xF;
                    o ^= (x << 5) & 0x200;
                    o ^= (x << 6) & 0x800;
                    break;
                case 2:
                    o ^= (y << 4) & 0x70;
                    o ^= (y << 5) & 0x100;
                    o ^= (y << 6) & 0x400;
                    o ^= (x << 1) & 0xE;
                    o ^= (x << 4) & 0x80;
                    o ^= (x << 5) & 0x200;
                    o ^= (x << 6) & 0x800;
                    break;
                case 4:
                    o ^= (y << 4) & 0x70;
                    o ^= (y << 5) & 0x100;
                    o ^= (y << 6) & 0x400;
                    o ^= (x << 2) & 0xC;
                    o ^= (x << 5) & 0x80;
                    o ^= (x << 6) & 0x200;
                    o ^= (x << 7) & 0x800;
                    break;
                case 8:
                    o ^= (y << 4) & 0x30;
                    o ^= (y << 6) & 0x100;
                    o ^= (y << 7) & 0x400;
                    o ^= (x << 3) & 0x8;
                    o ^= (x << 5) & 0xC0;
                    o ^= (x << 6) & 0x200;
                    o ^= (x << 7) & 0x800;
                    break;
                case 16:
                    o ^= (y << 4) & 0x30;
                    o ^= (y << 6) & 0x100;
                    o ^= (y << 7) & 0x400;
                    o ^= (x << 6) & 0xC0;
                    o ^= (x << 7) & 0x200;
                    o ^= (x << 8) & 0x800;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported PS5 element size: {bpe}.");
            }
            return o;
        }

        // Gen5 64KB standard thin-2D byte offset within a tile.
        private static int Thin64KOffsetInBlock(int x, int y, int bpe)
        {
            int o = 0;
            switch (bpe)
            {
                case 1:
                    o ^= x & 0xF;
                    o ^= (x << 5) & 0x200;
                    o ^= (x << 6) & 0x800;
                    o ^= (x << 7) & 0x2000;
                    o ^= (x << 8) & 0x8000;
                    o ^= (y << 4) & 0x1F0;
                    o ^= (y << 5) & 0x400;
                    o ^= (y << 6) & 0x1000;
                    o ^= (y << 7) & 0x4000;
                    break;
                case 2:
                    o ^= (x << 1) & 0xE;
                    o ^= (x << 4) & 0x80;
                    o ^= (x << 5) & 0x200;
                    o ^= (x << 6) & 0x800;
                    o ^= (x << 7) & 0x2000;
                    o ^= (x << 8) & 0x8000;
                    o ^= (y << 4) & 0x70;
                    o ^= (y << 5) & 0x100;
                    o ^= (y << 6) & 0x400;
                    o ^= (y << 7) & 0x1000;
                    o ^= (y << 8) & 0x4000;
                    break;
                case 4:
                    o ^= (x << 2) & 0xC;
                    o ^= (x << 5) & 0x80;
                    o ^= (x << 6) & 0x200;
                    o ^= (x << 7) & 0x800;
                    o ^= (x << 8) & 0x2000;
                    o ^= (x << 9) & 0x8000;
                    o ^= (y << 4) & 0x70;
                    o ^= (y << 5) & 0x100;
                    o ^= (y << 6) & 0x400;
                    o ^= (y << 7) & 0x1000;
                    o ^= (y << 8) & 0x4000;
                    break;
                case 8:
                    o ^= (x << 3) & 0x8;
                    o ^= (x << 5) & 0xC0;
                    o ^= (x << 6) & 0x200;
                    o ^= (x << 7) & 0x800;
                    o ^= (x << 8) & 0x2000;
                    o ^= (x << 9) & 0x8000;
                    o ^= (y << 4) & 0x30;
                    o ^= (y << 6) & 0x100;
                    o ^= (y << 7) & 0x400;
                    o ^= (y << 8) & 0x1000;
                    o ^= (y << 9) & 0x4000;
                    break;
                case 16:
                    o ^= (x << 6) & 0xC0;
                    o ^= (x << 7) & 0x200;
                    o ^= (x << 8) & 0x800;
                    o ^= (x << 9) & 0x2000;
                    o ^= (x << 10) & 0x8000;
                    o ^= (y << 4) & 0x30;
                    o ^= (y << 6) & 0x100;
                    o ^= (y << 7) & 0x400;
                    o ^= (y << 8) & 0x1000;
                    o ^= (y << 9) & 0x4000;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported PS5 element size: {bpe}.");
            }
            return o;
        }

        private static int DivRoundUp(int value, int divisor) => value / divisor + (value % divisor == 0 ? 0 : 1);
        private static int AlignUp(int value, int alignment) => checked(DivRoundUp(value, alignment) * alignment);
    }
}
