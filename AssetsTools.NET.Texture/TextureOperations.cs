using StbImageWriteSharp;
using System;
using System.IO;
using StbWriteColorComponents = StbImageWriteSharp.ColorComponents;

namespace AssetsTools.NET.Texture
{
    public static class TextureOperations
    {
        public static unsafe void SwapRBComponents(byte[] rgbaData)
        {
            fixed (byte* ptr = rgbaData)
            {
                for (int i = 0; i < rgbaData.Length; i += 4)
                {
                    (ptr[i + 2], ptr[i]) = (ptr[i], ptr[i + 2]);
                }
            }
        }

        public static void FlipBGRA32Vertically(byte[] data, int width, int height)
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

        // copiloted, may not work correctly
        public static byte[] CropFromTopLeft(byte[] data, int width, int height, int cropToWidth, int cropToHeight)
        {
            if (cropToWidth > width || cropToHeight > height)
                throw new ArgumentException("Crop size is bigger than the original image size.");

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
    }
}