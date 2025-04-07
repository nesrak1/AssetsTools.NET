using System;

namespace AssetsTools.NET.Texture
{
    internal static class TextureOperations
    {
        internal static unsafe void SwapRBComponents(byte[] rgbaData)
        {
            fixed (byte* ptr = rgbaData)
            {
                for (int i = 0; i < rgbaData.Length; i += 4)
                {
                    (ptr[i + 2], ptr[i]) = (ptr[i], ptr[i + 2]);
                }
            }
        }

        internal static void FlipBGRA32Vertically(byte[] data, int width, int height)
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
        internal static byte[] CropFromTopLeft(byte[] data, int width, int height, int cropToWidth, int cropToHeight)
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
    }

}