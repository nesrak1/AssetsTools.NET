/*
 ***********************************************************************************************************************
 *
 *  Copyright (c) 2007-2025 Advanced Micro Devices, Inc. All Rights Reserved.
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 *
 **********************************************************************************************************************/

using System;
using System.IO;

namespace AssetsTools.NET.Texture
{
    // PS5 mip layout. Small levels share a tile at the start of the buffer.
    // Image sizes round down, while storage sizes round up.
    // Based on AMD PAL Gfx10Lib::ComputeSurfaceInfoMacroTiled (MIT).
    // https://github.com/GPUOpen-Drivers/pal/blob/c5e800072a32f68b6ccc4422936d96167c6e0728/src/core/imported/addrlib/src/gfx10/gfx10addrlib.cpp
    public sealed class Ps5MipChain
    {
        public Ps5GfxLayout[] Levels { get; }
        public int[] Offsets { get; }
        public int[] TailX { get; }
        public int[] TailY { get; }
        public int FirstTailMip { get; }
        public int TileMode { get; }
        public int TiledSize { get; }
        public int LinearSize { get; }
        private readonly Ps5GfxLayout[] storage;

        public Ps5MipChain(int width, int height, TextureFormat format, int mipCount, int tileMode = Ps5GfxLayout.TileMode4KB)
        {
            int maxMips = 1;
            for (int size = Math.Max(width, height); size > 1; size >>= 1)
                maxMips++;
            if (width <= 0 || height <= 0 || mipCount < 1 || mipCount > maxMips)
                throw new ArgumentOutOfRangeException(nameof(mipCount), "Invalid PS5 dimensions or mip count.");
            TileMode = tileMode;
            Levels = new Ps5GfxLayout[mipCount];
            storage = new Ps5GfxLayout[mipCount];
            Offsets = new int[mipCount];
            TailX = new int[mipCount];
            TailY = new int[mipCount];
            var top = new Ps5GfxLayout(width, height, format, tileMode);
            int block = top.PixelBlockSize;
            FirstTailMip = mipCount;
            int maxTail = tileMode == Ps5GfxLayout.TileMode64KB ? 12 : 8;
            checked
            {
                for (int i = 0; i < mipCount; i++)
                {
                    Levels[i] = new Ps5GfxLayout(Math.Max(1, width >> i), Math.Max(1, height >> i), format, tileMode);
                    int divisor = 1 << i;
                    int ew = (int)(((long)top.ElementsWide + divisor - 1) / divisor);
                    int eh = (int)(((long)top.ElementsHigh + divisor - 1) / divisor);
                    storage[i] = new Ps5GfxLayout(ew * block, eh * block, format, tileMode);
                    LinearSize += Levels[i].LinearSize;
                    if (mipCount > 1 && tileMode != Ps5GfxLayout.TileMode256B && FirstTailMip == mipCount
                        && ew <= top.BlockWidth / 2 && eh <= top.BlockHeight && mipCount - i <= maxTail)
                        FirstTailMip = i;
                }
                int offset = FirstTailMip < mipCount ? top.BlockSize : 0;
                // AMD stores the smaller allocations before the larger levels.
                for (int i = FirstTailMip - 1; i >= 0; i--)
                {
                    Offsets[i] = offset;
                    offset += storage[i].TiledSize;
                }
                TiledSize = offset;
                var micro = new Ps5GfxLayout(width, height, format, Ps5GfxLayout.TileMode256B);
                for (int i = FirstTailMip; i < mipCount; i++)
                {
                    int m = maxTail - 1 - (i - FirstTailMip);
                    int tailOffset = m > 6 ? 16 << m : m << 8;
                    int tx = 0, ty = 0;
                    for (int bit = 0; bit < 6; bit++)
                    {
                        tx |= ((tailOffset >> (9 + 2 * bit)) & 1) << bit;
                        ty |= ((tailOffset >> (8 + 2 * bit)) & 1) << bit;
                    }
                    TailX[i] = tx * micro.BlockWidth;
                    TailY[i] = ty * micro.BlockHeight;
                }
            }
        }

        // Prefer 4 KB tiles, as used by the Unity samples tested so far.
        // Other tile sizes are accepted only when there is one size match.
        public static Ps5MipChain ForUnity(int width, int height, TextureFormat format, int mipCount, int storedSize)
        {
            var standard = new Ps5MipChain(width, height, format, mipCount);
            if (standard.TiledSize == storedSize)
                return standard;
            Ps5MipChain match = null;
            foreach (int mode in new[] { Ps5GfxLayout.TileMode256B, Ps5GfxLayout.TileMode64KB })
            {
                var candidate = new Ps5MipChain(width, height, format, mipCount, mode);
                if (candidate.TiledSize != storedSize)
                    continue;
                if (match != null)
                    throw new NotSupportedException("Ambiguous PS5 layout outside the Unity 4KB standard profile.");
                match = candidate;
            }
            return match ?? throw new InvalidDataException("PS5 payload size does not match a supported complete standard mip layout.");
        }

        public int GetElementOffset(int mip, int x, int y)
        {
            if ((uint)mip >= Levels.Length || (uint)x >= Levels[mip].ElementsWide || (uint)y >= Levels[mip].ElementsHigh)
                throw new ArgumentOutOfRangeException(nameof(mip));
            return checked(Offsets[mip] + storage[mip].TiledOffset(x + TailX[mip], y + TailY[mip]));
        }

        public byte[] DeswizzleLevel(ReadOnlySpan<byte> tiled, int mip)
        {
            if (tiled.Length != TiledSize)
                throw new InvalidDataException("Incomplete PS5 mip chain.");
            var level = Levels[mip];
            var result = new byte[level.LinearSize];
            for (int y = 0; y < level.ElementsHigh; y++)
                for (int x = 0; x < level.ElementsWide; x++)
                    tiled.Slice(GetElementOffset(mip, x, y), level.BytesPerElement).CopyTo(
                        result.AsSpan((y * level.ElementsWide + x) * level.BytesPerElement));
            return result;
        }

        public byte[][] Deswizzle(byte[] tiled)
        {
            var result = new byte[Levels.Length][];
            for (int i = 0; i < result.Length; i++)
                result[i] = DeswizzleLevel(tiled, i);
            return result;
        }

        public byte[][] SplitLinear(byte[] linear)
        {
            if (linear == null || linear.Length != LinearSize)
                throw new InvalidDataException("Incorrect linear PS5 mip size.");
            var result = new byte[Levels.Length][];
            int offset = 0;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = linear.AsSpan(offset, Levels[i].LinearSize).ToArray();
                offset += result[i].Length;
            }
            return result;
        }

        public byte[] Swizzle(byte[][] mips, byte[] original = null)
        {
            if (mips == null || mips.Length != Levels.Length)
                throw new InvalidDataException("Incorrect PS5 mip count.");
            if (original != null && original.Length != TiledSize)
                throw new InvalidDataException("Incorrect PS5 padding buffer size.");
            var result = original == null ? new byte[TiledSize] : (byte[])original.Clone();
            for (int i = 0; i < Levels.Length; i++)
            {
                var level = Levels[i];
                if (mips[i] == null || mips[i].Length != level.LinearSize)
                    throw new InvalidDataException("Incorrect PS5 encoded mip size.");
                for (int y = 0; y < level.ElementsHigh; y++)
                    for (int x = 0; x < level.ElementsWide; x++)
                        mips[i].AsSpan((y * level.ElementsWide + x) * level.BytesPerElement, level.BytesPerElement)
                            .CopyTo(result.AsSpan(GetElementOffset(i, x, y)));
            }
            return result;
        }
    }
}
