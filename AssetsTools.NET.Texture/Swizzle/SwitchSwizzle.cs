using System;
using System.Drawing;

namespace AssetsTools.NET.Texture
{
    public class SwitchSwizzle : ISwizzler
    {
        private const int GOB_X_TEXEL_COUNT = 4;
        private const int GOB_Y_TEXEL_COUNT = 8;
        private const int TEXEL_BYTE_SIZE = 16;
        // referring to block here as a compressed texture block, not a gob one
        private const int BLOCKS_IN_GOB = GOB_X_TEXEL_COUNT * GOB_Y_TEXEL_COUNT;
        private static readonly int[] GOB_X_POSES =
        [
            0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3
        ];
        private static readonly int[] GOB_Y_POSES =
        [
            0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7
        ];

        /*
        sector (made of compressed texture blocks):
        A
        B
        
        gob (made of sectors):
        ABIJ
        CDKL
        EFMN
        GHOP

        gob blocks (example with gob block height 2):
        ACEGIK... from left to right of image
        BDFHJL...
        --------- start new row of blocks
        MOQSUW...
        NPRTVX...
        */

        private readonly Size originalSize;
        private readonly Size paddedSize;
        private readonly Size blockSize;
        private readonly int gobsPerBlock;
        private readonly TextureFormat realFormat;

        // deswizzle
        public SwitchSwizzle(byte[] platformBlob, int width, int height,
            ref TextureFormat format, out int paddedWidth, out int paddedHeight)
        {
            originalSize = new Size(width, height);
            realFormat = GetCorrectedSwitchTextureFormat(format);
            gobsPerBlock = GetBlockHeightByPlatformBlob(platformBlob);

            blockSize = GetTextureFormatBlockSize(realFormat);
            if (!blockSize.IsEmpty)
            {
                paddedSize = GetPaddedTextureSize(originalSize.Width, originalSize.Height, blockSize.Width, blockSize.Height, gobsPerBlock);
            }
            else
            {
                paddedSize = originalSize;
            }

            format = realFormat;
            paddedWidth = paddedSize.Width;
            paddedHeight = paddedSize.Height;
        }

        // swizzle
        public SwitchSwizzle(int width, int height,
            ref TextureFormat format, out int paddedWidth, out int paddedHeight)
        {
            originalSize = new Size(width, height);
            realFormat = GetCorrectedSwitchTextureFormat(format);

            blockSize = GetTextureFormatBlockSize(realFormat);
            if (!blockSize.IsEmpty)
            {
                gobsPerBlock = GetBlockHeightByBlockSize(blockSize, height);
                paddedSize = GetPaddedTextureSize(originalSize.Width, originalSize.Height, blockSize.Width, blockSize.Height, gobsPerBlock);
            }
            else
            {
                gobsPerBlock = 1;
                paddedSize = originalSize;
            }

            format = realFormat;
            paddedWidth = paddedSize.Width;
            paddedHeight = paddedSize.Height;
        }

        public byte[] PreprocessDeswizzle(byte[] rawData, out int width, out int height)
        {
            if (!CanBeSwizzled())
            {
                width = originalSize.Width;
                height = originalSize.Height;
                return rawData;
            }
            else
            {
                width = paddedSize.Width;
                height = paddedSize.Height;
                return Unswizzle(rawData, paddedSize, blockSize, gobsPerBlock);
            }
        }

        public byte[] PostprocessDeswizzle(byte[] rawData)
        {
            if (!CanBeSwizzled())
                return rawData;

            // same size crop handled by crop method
            return TextureOperations.CropFromTopLeft(rawData, paddedSize.Width, paddedSize.Height, originalSize.Width, originalSize.Height);
        }

        public byte[] ProcessSwizzle(byte[][] rawData, out int[] mipOffsets)
        {
            if (!CanBeSwizzled())
                return TextureOperations.FlattenMips(rawData, out mipOffsets);

            return SwizzleMips(rawData, originalSize.Width, originalSize.Height, realFormat, out mipOffsets);
        }

        // todo: review llm code **start**
        public static byte[] SwizzleMips(byte[][] mips, int baseWidth, int baseHeight, TextureFormat format, out int[] mipOffsets)
        {
            Size blockSize = GetTextureFormatBlockSize(format);
            mipOffsets = new int[mips.Length];

            int totalSize = 0;
            for (int mip = 0; mip < mips.Length; mip++)
            {
                int mipWidth = Math.Max(baseWidth >> mip, 1);
                int mipHeight = Math.Max(baseHeight >> mip, 1);
                int gobsPerBlock = GetBlockHeightByBlockSize(blockSize, mipHeight);
                Size paddedSize = GetPaddedTextureSize(mipWidth, mipHeight, blockSize.Width, blockSize.Height, gobsPerBlock);
                int paddedBlockCountX = CeilDivide(paddedSize.Width, blockSize.Width);
                int paddedBlockCountY = CeilDivide(paddedSize.Height, blockSize.Height);
                mipOffsets[mip] = totalSize;
                totalSize += paddedBlockCountX * paddedBlockCountY * TEXEL_BYTE_SIZE;
            }

            byte[] result = new byte[totalSize];
            int resultOffset = 0;

            for (int mip = 0; mip < mips.Length; mip++)
            {
                int mipWidth = Math.Max(baseWidth >> mip, 1);
                int mipHeight = Math.Max(baseHeight >> mip, 1);
                int gobsPerBlock = GetBlockHeightByBlockSize(blockSize, mipHeight);
                Size paddedSize = GetPaddedTextureSize(mipWidth, mipHeight, blockSize.Width, blockSize.Height, gobsPerBlock);

                byte[] paddedMip = PadMipToGobSize(mips[mip], mipHeight, paddedSize, blockSize);
                byte[] swizzledMip = Swizzle(paddedMip, paddedSize, blockSize, gobsPerBlock);

                Array.Copy(swizzledMip, 0, result, resultOffset, swizzledMip.Length);
                resultOffset += swizzledMip.Length;
            }

            return result;
        }

        private static byte[] PadMipToGobSize(byte[] mipData, int mipHeight, Size paddedSize, Size blockSize)
        {
            int srcBlockCountY = CeilDivide(mipHeight, blockSize.Height);
            int srcRowStride = mipData.Length / srcBlockCountY; // actual bytes per row of the source

            int dstBlockCountX = CeilDivide(paddedSize.Width, blockSize.Width);
            int dstBlockCountY = CeilDivide(paddedSize.Height, blockSize.Height);

            byte[] padded = new byte[dstBlockCountX * dstBlockCountY * TEXEL_BYTE_SIZE];

            for (int row = 0; row < srcBlockCountY; row++)
            {
                int srcOffset = row * srcRowStride;
                int dstOffset = row * dstBlockCountX * TEXEL_BYTE_SIZE;
                Array.Copy(mipData, srcOffset, padded, dstOffset, srcRowStride);
            }

            return padded;
        }
        // todo: review llm code **end**

        public byte[] MakePlatformBlob(int[] mipOffsets, uint completeImageSize)
        {
            int blockHeightIndex = gobsPerBlock switch
            {
                16 => 4,
                8 => 3,
                4 => 2,
                2 => 1,
                _ => 0
            };

            return SwitchPlatformBlob.MakePlatformBlob(blockHeightIndex, mipOffsets, completeImageSize);
        }

        public bool CanBeSwizzled()
        {
            return !blockSize.IsEmpty;
        }

        private static int CeilDivide(int a, int b)
        {
            return (a + b - 1) / b;
        }

        public static byte[] Unswizzle(byte[] data, Size imageSize, Size blockSize, int blockHeight)
        {
            byte[] newData = new byte[data.Length];

            int width = imageSize.Width;
            int height = imageSize.Height;

            int blockCountX = CeilDivide(width, blockSize.Width);
            int blockCountY = CeilDivide(height, blockSize.Height);

            int gobCountX = blockCountX / GOB_X_TEXEL_COUNT;
            int gobCountY = blockCountY / GOB_Y_TEXEL_COUNT;

            int srcPos = 0;
            for (int i = 0; i < gobCountY / blockHeight; i++)
            {
                for (int j = 0; j < gobCountX; j++)
                {
                    for (int k = 0; k < blockHeight; k++)
                    {
                        for (int l = 0; l < BLOCKS_IN_GOB; l++)
                        {
                            int gobX = GOB_X_POSES[l];
                            int gobY = GOB_Y_POSES[l];
                            int gobDstX = j * GOB_X_TEXEL_COUNT + gobX;
                            int gobDstY = (i * blockHeight + k) * GOB_Y_TEXEL_COUNT + gobY;
                            int gobDstLinPos = gobDstY * blockCountX * TEXEL_BYTE_SIZE + gobDstX * TEXEL_BYTE_SIZE;

                            Array.Copy(data, srcPos, newData, gobDstLinPos, TEXEL_BYTE_SIZE);

                            srcPos += TEXEL_BYTE_SIZE;
                        }
                    }
                }
            }

            return newData;
        }

        public static byte[] Swizzle(byte[] data, Size imageSize, Size blockSize, int blockHeight)
        {
            byte[] newData = new byte[data.Length];

            int width = imageSize.Width;
            int height = imageSize.Height;

            int blockCountX = CeilDivide(width, blockSize.Width);
            int blockCountY = CeilDivide(height, blockSize.Height);

            int gobCountX = blockCountX / GOB_X_TEXEL_COUNT;
            int gobCountY = blockCountY / GOB_Y_TEXEL_COUNT;

            int srcPos = 0;
            for (int i = 0; i < gobCountY / blockHeight; i++)
            {
                for (int j = 0; j < gobCountX; j++)
                {
                    for (int k = 0; k < blockHeight; k++)
                    {
                        for (int l = 0; l < BLOCKS_IN_GOB; l++)
                        {
                            int gobX = GOB_X_POSES[l];
                            int gobY = GOB_Y_POSES[l];
                            int gobDstX = j * GOB_X_TEXEL_COUNT + gobX;
                            int gobDstY = (i * blockHeight + k) * GOB_Y_TEXEL_COUNT + gobY;
                            int gobDstLinPos = gobDstY * blockCountX * TEXEL_BYTE_SIZE + gobDstX * TEXEL_BYTE_SIZE;

                            Array.Copy(data, gobDstLinPos, newData, srcPos, TEXEL_BYTE_SIZE);

                            srcPos += TEXEL_BYTE_SIZE;
                        }
                    }
                }
            }

            return newData;
        }

        // this should be the amount of pixels that can fit 16 bytes.
        // as far as I can tell, this is all supported texture types with swizzle enabled.
        // this will need to be updated in the future if more texture types are supported.
        public static Size GetTextureFormatBlockSize(TextureFormat textureFormat)
        {
            return textureFormat switch
            {
                TextureFormat.Alpha8 => new Size(16, 1),
                TextureFormat.ARGB4444 => new Size(8, 1),
                TextureFormat.RGBA32 => new Size(4, 1),
                TextureFormat.ARGB32 => new Size(4, 1),
                TextureFormat.ARGBFloat => new Size(1, 1),
                TextureFormat.RGB565 => new Size(8, 1),
                TextureFormat.R16 => new Size(8, 1),
                TextureFormat.DXT1 => new Size(8, 4),
                TextureFormat.DXT5 => new Size(4, 4),
                TextureFormat.RGBA4444 => new Size(8, 1),
                TextureFormat.BGRA32 => new Size(4, 1),
                TextureFormat.BC6H => new Size(4, 4),
                TextureFormat.BC7 => new Size(4, 4),
                TextureFormat.BC4 => new Size(8, 4),
                TextureFormat.BC5 => new Size(4, 4),
                TextureFormat.ASTC_RGB_4x4 => new Size(4, 4),
                TextureFormat.ASTC_RGB_5x5 => new Size(5, 5),
                TextureFormat.ASTC_RGB_6x6 => new Size(6, 6),
                TextureFormat.ASTC_RGB_8x8 => new Size(8, 8),
                TextureFormat.ASTC_RGB_10x10 => new Size(10, 10),
                TextureFormat.ASTC_RGB_12x12 => new Size(12, 12),
                TextureFormat.ASTC_RGBA_4x4 => new Size(4, 4),
                TextureFormat.ASTC_RGBA_5x5 => new Size(5, 5),
                TextureFormat.ASTC_RGBA_6x6 => new Size(6, 6),
                TextureFormat.ASTC_RGBA_8x8 => new Size(8, 8),
                TextureFormat.ASTC_RGBA_10x10 => new Size(10, 10),
                TextureFormat.ASTC_RGBA_12x12 => new Size(12, 12),
                TextureFormat.RGBA64 => new Size(2, 1),
                TextureFormat.RG16 => new Size(8, 1),
                TextureFormat.R8 => new Size(16, 1),
                _ => Size.Empty
            };
        }

        public static Size GetPaddedTextureSize(int width, int height, int blockWidth, int blockHeight, int gobsPerBlock)
        {
            width = CeilDivide(width, blockWidth * GOB_X_TEXEL_COUNT) * blockWidth * GOB_X_TEXEL_COUNT;
            height = CeilDivide(height, blockHeight * GOB_Y_TEXEL_COUNT * gobsPerBlock) * blockHeight * GOB_Y_TEXEL_COUNT * gobsPerBlock;
            return new Size(width, height);
        }

        // hope this is right :woozy:
        // src: https://github.com/ScanMountGoat/tegra_swizzle/blob/main/tegra_swizzle/src/blockheight.rs#L29
        public static int GetBlockHeightByBlockSize(Size blockSize, int imageHeight)
        {
            int blockHeight = CeilDivide(imageHeight, blockSize.Height);
            int heightAndHalf = blockHeight + blockHeight / 2;
            return heightAndHalf switch
            {
                >= 128 => 16,
                >= 64 => 8,
                >= 32 => 4,
                >= 16 => 2,
                _ => 1
            };
        }

        public static int GetBlockHeightByPlatformBlob(byte[] platformBlob)
        {
            return 1 << SwitchPlatformBlob.GetBlockHeightIndex(platformBlob);
        }

        public static TextureFormat GetCorrectedSwitchTextureFormat(TextureFormat format)
        {
            // in older versions of unity, there was (presumably) a mistake where
            // rgb24 has a platformblob, but rgb24 can't be swizzled. it turns out
            // in this case, the image was just encoded as rgba32.
            return format switch
            {
                TextureFormat.RGB24 => TextureFormat.RGBA32,
                TextureFormat.BGR24 => TextureFormat.BGRA32,
                _ => format
            };
        }

        public static bool IsSwitchSwizzled(byte[] platformBlob, SwizzleType swizzleType)
        {
            return swizzleType == SwizzleType.Switch && platformBlob != null && platformBlob.Length >= 12;
        }
    }
}
