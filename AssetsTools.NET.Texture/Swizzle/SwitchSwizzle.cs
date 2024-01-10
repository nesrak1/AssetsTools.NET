using System;
using System.Drawing;

namespace AssetsTools.NET.Texture
{
    // how to use:
    // 1. get raw texture block dimensions (region of pixels that fits in 16 bytes)
    //      use GetTextureFormatBlockSize
    // 2. get gob block height from platform blob (if available) or block size (if making new texture)
    //      use GetBlockHeightByPlatformBlob or GetBlockHeightByBlockSize
    //      (note: swizzle starts at a certain unity version, but I can't figure out which one)
    // 3. get padded texture size based on texture2d's size
    //      use GetPaddedTextureSize
    // 4. unswizzle using padded image size, raw texture block size, and gob block height
    //      use Unswizzle
    // 5. crop image to original texture2d size
    public class SwitchSwizzle
    {
        private const int GOB_X_TEXEL_COUNT = 4;
        private const int GOB_Y_TEXEL_COUNT = 8;
        private const int TEXEL_BYTE_SIZE = 16;
        // referring to block here as a compressed texture block, not a gob one
        private const int BLOCKS_IN_GOB = GOB_X_TEXEL_COUNT * GOB_Y_TEXEL_COUNT;
        private static readonly int[] GOB_X_POSES = new int[]
        {
            0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3
        };
        private static readonly int[] GOB_Y_POSES = new int[]
        {
            0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7
        };

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
            switch (textureFormat)
            {
                case TextureFormat.Alpha8: return new Size(16, 1);
                case TextureFormat.ARGB4444: return new Size(8, 1);
                case TextureFormat.RGBA32: return new Size(4, 1);
                case TextureFormat.ARGB32: return new Size(4, 1);
                case TextureFormat.ARGBFloat: return new Size(1, 1);
                case TextureFormat.RGB565: return new Size(8, 1);
                case TextureFormat.R16: return new Size(8, 1);
                case TextureFormat.DXT1: return new Size(8, 4);
                case TextureFormat.DXT5: return new Size(4, 4);
                case TextureFormat.RGBA4444: return new Size(8, 1);
                case TextureFormat.BGRA32: return new Size(4, 1);
                case TextureFormat.BC6H: return new Size(4, 4);
                case TextureFormat.BC7: return new Size(4, 4);
                case TextureFormat.BC4: return new Size(8, 4);
                case TextureFormat.BC5: return new Size(4, 4);
                case TextureFormat.ASTC_RGB_4x4: return new Size(4, 4);
                case TextureFormat.ASTC_RGB_5x5: return new Size(5, 5);
                case TextureFormat.ASTC_RGB_6x6: return new Size(6, 6);
                case TextureFormat.ASTC_RGB_8x8: return new Size(8, 8);
                case TextureFormat.ASTC_RGB_10x10: return new Size(10, 10);
                case TextureFormat.ASTC_RGB_12x12: return new Size(12, 12);
                case TextureFormat.ASTC_RGBA_4x4: return new Size(4, 4);
                case TextureFormat.ASTC_RGBA_5x5: return new Size(5, 5);
                case TextureFormat.ASTC_RGBA_6x6: return new Size(6, 6);
                case TextureFormat.ASTC_RGBA_8x8: return new Size(8, 8);
                case TextureFormat.ASTC_RGBA_10x10: return new Size(10, 10);
                case TextureFormat.ASTC_RGBA_12x12: return new Size(12, 12);
                case TextureFormat.RG16: return new Size(8, 1);
                case TextureFormat.R8: return new Size(16, 1);
                default: throw new NotImplementedException();
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
            // apparently there is another value to worry about, but seeing as it's
            // always 0 and I have nothing else to test against, this will probably
            // work fine for now
            return 1 << BitConverter.ToInt32(platformBlob, 8);
        }

        public static TextureFormat GetCorrectedSwitchTextureFormat(TextureFormat format)
        {
            // in older versions of unity, rgb24 has a platformblob which shouldn't
            // be possible. it turns out in this case, the image is just rgba32.
            if (format == TextureFormat.RGB24)
            {
                return TextureFormat.RGBA32;
            }
            else if (format == TextureFormat.BGR24)
            {
                return TextureFormat.BGRA32;
            }
            return format;
        }
    }
}
