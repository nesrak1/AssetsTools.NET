using StbImageWriteSharp;
using System;
using System.Drawing;
using System.IO;
using StbWriteColorComponents = StbImageWriteSharp.ColorComponents;

namespace AssetsTools.NET.Texture
{
    public static class TextureOperations
    {
        public static void SwapRBComponentsInplace(byte[] rgbaData)
        {
            Span<byte> ptr = rgbaData.AsSpan();
            for (int i = 0; i < rgbaData.Length; i += 4)
            {
                (ptr[i + 2], ptr[i]) = (ptr[i], ptr[i + 2]);
            }
        }

        public static byte[] SwapRBComponents(byte[] rgbaData)
        {
            var swappedData = new byte[rgbaData.Length];
            Array.Copy(rgbaData, swappedData, rgbaData.Length);
            SwapRBComponentsInplace(swappedData);
            return swappedData;
        }

        public static void FlipBGRA32VerticallyInplace(byte[] data, int width, int height)
        {
            int rowSize = width * 4;
            byte[] row = new byte[rowSize];
            for (int i = 0; i < height / 2; i++)
            {
                int topIndex = i * rowSize;
                int bottomIndex = (height - i - 1) * rowSize;
                Array.Copy(data, topIndex, row, 0, rowSize);
                Array.Copy(data, bottomIndex, data, topIndex, rowSize);
                Array.Copy(row, 0, data, bottomIndex, rowSize);
            }
        }

        public static byte[] FlipRGBA32Vertically(byte[] data, int width, int height)
        {
            var flippedData = new byte[data.Length];
            Array.Copy(data, flippedData, data.Length);
            FlipBGRA32VerticallyInplace(flippedData, width, height);
            return flippedData;
        }

        public static byte[] CropFromTopLeft(byte[] data, int width, int height, int cropToWidth, int cropToHeight)
        {
            if (cropToWidth > width || cropToHeight > height)
                throw new ArgumentException("Crop size is bigger than the original image size.");

            if (width == cropToWidth && height == cropToHeight)
                return data;

            int cropToSize = cropToWidth * cropToHeight * 4;
            byte[] cropped = new byte[cropToSize];
            for (int i = 0; i < cropToHeight; i++)
            {
                int srcIndex = i * width * 4;
                int dstIndex = i * cropToWidth * 4;
                Array.Copy(data, srcIndex, cropped, dstIndex, cropToWidth * 4);
            }
            return cropped;
        }

        public static byte[] FlattenMips(byte[][] mips, out int[] mipOffsets)
        {
            mipOffsets = new int[mips.Length];

            int totalSize = 0;
            for (int i = 0; i < mips.Length; i++)
            {
                mipOffsets[i] = totalSize;
                totalSize += mips[i].Length;
            }

            byte[] result = new byte[totalSize];
            int offset = 0;
            for (int i = 0; i < mips.Length; i++)
            {
                Buffer.BlockCopy(mips[i], 0, result, offset, mips[i].Length);
                offset += mips[i].Length;
            }

            return result;
        }

        public static bool WriteRawImage(
            byte[] data, int width, int height,
            Stream outputStream, ImageExportType exportType, int quality = 90)
        {
            ImageWriter imageWriter = new ImageWriter();
            switch (exportType)
            {
                case ImageExportType.Bmp:
                {
                    imageWriter.WriteBmp(data, width, height, StbWriteColorComponents.RedGreenBlueAlpha, outputStream);
                    return true;
                }
                case ImageExportType.Tga:
                {
                    imageWriter.WriteTga(data, width, height, StbWriteColorComponents.RedGreenBlueAlpha, outputStream);
                    return true;
                }
                case ImageExportType.Png:
                {
                    imageWriter.WritePng(data, width, height, StbWriteColorComponents.RedGreenBlueAlpha, outputStream);
                    return true;
                }
                case ImageExportType.Jpg:
                {
                    imageWriter.WriteJpg(data, width, height, StbWriteColorComponents.RedGreenBlueAlpha, outputStream, quality);
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        public static int GetMaxMipCount(int width, int height)
        {
#if NET6_0_OR_GREATER
            int widthMipCount = (int)Math.Log2(width) + 1;
            int heightMipCount = (int)Math.Log2(height) + 1;
#else
            int widthMipCount = (int)Math.Log(width, 2d) + 1;
            int heightMipCount = (int)Math.Log(height, 2d) + 1;
#endif
            // if the texture is 512x1024 for example, select the height (1024)
            // I guess the width would stay 1 while the height resizes down
            return Math.Max(widthMipCount, heightMipCount);
        }

        private static Size GetSizePaddedToMultiple(int width, int height, int mul)
        {
            var c = mul - 1;
            return new Size((width + c) & ~c, (height + c) & ~c);
        }

        private static int NextPo2(int val)
        {
            val--;
            val |= val >> 16;
            val |= val >> 8;
            val |= val >> 4;
            val |= val >> 2;
            val |= val >> 1;
            return val + 1;
        }

        private static Size GetSizePaddedToPo2(int width, int height)
        {
            int maxDim = Math.Max(width, height);
            int newDim = NextPo2(maxDim);
            return new Size(newDim, newDim);
        }

        public static Size GetPaddedTextureSize(TextureFormat textureFormat, int width, int height)
        {
            return textureFormat switch
            {
                TextureFormat.ARGB4444 => new Size(width, height),
                TextureFormat.RGB24 => new Size(width, height),
                TextureFormat.RGBA32 => new Size(width, height),
                TextureFormat.ARGB32 => new Size(width, height),
                TextureFormat.ARGBFloat => new Size(width, height),
                TextureFormat.RGB565 => new Size(width, height),
                TextureFormat.BGR24 => new Size(width, height),
                TextureFormat.R16 => new Size(width, height),
                TextureFormat.DXT1 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.DXT3 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.DXT5 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.RGBA4444 => new Size(width, height),
                TextureFormat.BGRA32 => new Size(width, height),
                TextureFormat.RHalf => new Size(width, height),
                TextureFormat.RGHalf => new Size(width, height),
                TextureFormat.RGBAHalf => new Size(width, height),
                TextureFormat.RFloat => new Size(width, height),
                TextureFormat.RGFloat => new Size(width, height),
                TextureFormat.RGBAFloat => new Size(width, height),
                TextureFormat.YUY2 => new Size(width, height),
                TextureFormat.RGB9e5Float => new Size(width, height),
                TextureFormat.RGBFloat => new Size(width, height),
                TextureFormat.BC6H => new Size(width, height),
                TextureFormat.BC7 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.BC4 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.BC5 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.DXT1Crunched => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.DXT5Crunched => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.PVRTC_RGB2 => GetSizePaddedToPo2(width, height),
                TextureFormat.PVRTC_RGBA2 => GetSizePaddedToPo2(width, height),
                TextureFormat.PVRTC_RGB4 => GetSizePaddedToPo2(width, height),
                TextureFormat.PVRTC_RGBA4 => GetSizePaddedToPo2(width, height),
                TextureFormat.ETC_RGB4 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ATC_RGB4 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ATC_RGBA8 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.BGRA32Old => new Size(width, height),
                TextureFormat.EAC_R => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.EAC_R_SIGNED => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.EAC_RG => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.EAC_RG_SIGNED => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ETC2_RGB4 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ETC2_RGBA1 => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ETC2_RGBA8 => GetSizePaddedToMultiple(width, height, 4),
                // astc supports non-multiple textures without any extra work
                TextureFormat.ASTC_RGB_4x4 => new Size(width, height),
                TextureFormat.ASTC_RGB_5x5 => new Size(width, height),
                TextureFormat.ASTC_RGB_6x6 => new Size(width, height),
                TextureFormat.ASTC_RGB_8x8 => new Size(width, height),
                TextureFormat.ASTC_RGB_10x10 => new Size(width, height),
                TextureFormat.ASTC_RGB_12x12 => new Size(width, height),
                TextureFormat.ASTC_RGBA_4x4 => new Size(width, height),
                TextureFormat.ASTC_RGBA_5x5 => new Size(width, height),
                TextureFormat.ASTC_RGBA_6x6 => new Size(width, height),
                TextureFormat.ASTC_RGBA_8x8 => new Size(width, height),
                TextureFormat.ASTC_RGBA_10x10 => new Size(width, height),
                TextureFormat.ASTC_RGBA_12x12 => new Size(width, height),
                TextureFormat.ETC_RGB4_3DS => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ETC_RGBA8_3DS => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.RG16 => new Size(width, height),
                TextureFormat.R8 => new Size(width, height),
                TextureFormat.ETC_RGB4Crunched => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ETC2_RGBA8Crunched => GetSizePaddedToMultiple(width, height, 4),
                TextureFormat.ASTC_HDR_4x4 => new Size(width, height),
                TextureFormat.ASTC_HDR_5x5 => new Size(width, height),
                TextureFormat.ASTC_HDR_6x6 => new Size(width, height),
                TextureFormat.ASTC_HDR_8x8 => new Size(width, height),
                TextureFormat.ASTC_HDR_10x10 => new Size(width, height),
                TextureFormat.ASTC_HDR_12x12 => new Size(width, height),
                TextureFormat.RG32 => new Size(width, height),
                TextureFormat.RGB48 => new Size(width, height),
                TextureFormat.RGBA64 => new Size(width, height),
                _ => throw new NotImplementedException()
            };
        }

        public static Size GetBlockSize(TextureFormat textureFormat)
        {
            return textureFormat switch
            {
                TextureFormat.Alpha8 => new Size(1, 1),
                TextureFormat.ARGB4444 => new Size(1, 1),
                TextureFormat.RGB24 => new Size(1, 1),
                TextureFormat.RGBA32 => new Size(1, 1),
                TextureFormat.ARGB32 => new Size(1, 1),
                TextureFormat.ARGBFloat => new Size(1, 1),
                TextureFormat.RGB565 => new Size(1, 1),
                TextureFormat.BGR24 => new Size(1, 1),
                TextureFormat.R16 => new Size(1, 1),
                TextureFormat.DXT1 => new Size(4, 4),
                TextureFormat.DXT3 => new Size(4, 4),
                TextureFormat.DXT5 => new Size(4, 4),
                TextureFormat.RGBA4444 => new Size(1, 1),
                TextureFormat.BGRA32 => new Size(1, 1),
                TextureFormat.RHalf => new Size(1, 1),
                TextureFormat.RGHalf => new Size(1, 1),
                TextureFormat.RGBAHalf => new Size(1, 1),
                TextureFormat.RFloat => new Size(1, 1),
                TextureFormat.RGFloat => new Size(1, 1),
                TextureFormat.RGBAFloat => new Size(1, 1),
                TextureFormat.YUY2 => new Size(1, 1),
                TextureFormat.RGB9e5Float => new Size(1, 1),
                TextureFormat.RGBFloat => new Size(1, 1),
                TextureFormat.BC6H => new Size(1, 1),
                TextureFormat.BC7 => new Size(4, 4),
                TextureFormat.BC4 => new Size(4, 4),
                TextureFormat.BC5 => new Size(4, 4),
                TextureFormat.DXT1Crunched => new Size(4, 4),
                TextureFormat.DXT5Crunched => new Size(4, 4),
                // pvrtc are always po2 as far as I know, so any
                // cropping/padding we do will be futile
                TextureFormat.PVRTC_RGB2 => new Size(1, 1),
                TextureFormat.PVRTC_RGBA2 => new Size(1, 1),
                TextureFormat.PVRTC_RGB4 => new Size(1, 1),
                TextureFormat.PVRTC_RGBA4 => new Size(1, 1),
                TextureFormat.ETC_RGB4 => new Size(4, 4),
                TextureFormat.ATC_RGB4 => new Size(4, 4),
                TextureFormat.ATC_RGBA8 => new Size(4, 4),
                TextureFormat.BGRA32Old => new Size(1, 1),
                TextureFormat.EAC_R => new Size(4, 4),
                TextureFormat.EAC_R_SIGNED => new Size(4, 4),
                TextureFormat.EAC_RG => new Size(4, 4),
                TextureFormat.EAC_RG_SIGNED => new Size(4, 4),
                TextureFormat.ETC2_RGB4 => new Size(4, 4),
                TextureFormat.ETC2_RGBA1 => new Size(4, 4),
                TextureFormat.ETC2_RGBA8 => new Size(4, 4),
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
                TextureFormat.ETC_RGB4_3DS => new Size(4, 4),
                TextureFormat.ETC_RGBA8_3DS => new Size(4, 4),
                TextureFormat.RG16 => new Size(1, 1),
                TextureFormat.R8 => new Size(1, 1),
                TextureFormat.ETC_RGB4Crunched => new Size(4, 4),
                TextureFormat.ETC2_RGBA8Crunched => new Size(4, 4),
                TextureFormat.ASTC_HDR_4x4 => new Size(4, 4),
                TextureFormat.ASTC_HDR_5x5 => new Size(5, 5),
                TextureFormat.ASTC_HDR_6x6 => new Size(6, 6),
                TextureFormat.ASTC_HDR_8x8 => new Size(8, 8),
                TextureFormat.ASTC_HDR_10x10 => new Size(10, 10),
                TextureFormat.ASTC_HDR_12x12 => new Size(12, 12),
                TextureFormat.RG32 => new Size(1, 1),
                TextureFormat.RGB48 => new Size(1, 1),
                TextureFormat.RGBA64 => new Size(1, 1),
                _ => throw new NotImplementedException()
            };
        }
    }
}