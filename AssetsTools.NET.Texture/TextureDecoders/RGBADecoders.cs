using System;

namespace AssetsTools.NET.Texture
{
    public static class RGBADecoders
    {
        public static byte[] ReadR8(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 1, outputPos += 4)
            {
                byte r = input[inputPos];
                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadAlpha8(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 1, outputPos += 4)
            {
                byte a = input[inputPos];
                output[outputPos + 0] = 0xFF;
                output[outputPos + 1] = 0xFF;
                output[outputPos + 2] = 0xFF;
                output[outputPos + 3] = a;
            }
            return output;
        }

        public static byte[] ReadR16(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                byte r = input[inputPos + 1];
                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadRHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                byte r = ReadHalf(input, inputPos);

                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadRG16(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                byte r = input[inputPos + 0];
                byte g = input[inputPos + 1];
                output[outputPos + 1] = g;
                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadRGHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 4)
            {
                byte r = ReadHalf(input, inputPos + 0);
                byte g = ReadHalf(input, inputPos + 2);

                output[outputPos + 1] = g;
                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadRGB24(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 3, outputPos += 4)
            {
                byte r = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte b = input[inputPos + 2];
                output[outputPos + 0] = b;
                output[outputPos + 1] = g;
                output[outputPos + 2] = r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadRGB565(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                int rgb = BitConverter.ToUInt16(input, inputPos);

                int r = (rgb >> 11) & 0x1F;
                r = (r << 3) | (r & 7);

                int g = (rgb >> 5) & 0x3F;
                g = (g << 2) | (g & 3);

                int b = rgb & 0x1F;
                b = (b << 3) | (b & 7);

                output[outputPos + 0] = (byte)b;
                output[outputPos + 1] = (byte)g;
                output[outputPos + 2] = (byte)r;
                output[outputPos + 3] = 0xFF;
            }
            return output;
        }

        public static byte[] ReadARGB4444(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                int a = input[inputPos + 1] >> 4;
                int r = input[inputPos + 1] & 0xf;
                int g = input[inputPos] >> 4;
                int b = input[inputPos] & 0xf;
                output[outputPos + 0] = (byte)((b << 4) | b);
                output[outputPos + 1] = (byte)((g << 4) | g);
                output[outputPos + 2] = (byte)((r << 4) | r);
                output[outputPos + 3] = (byte)((a << 4) | a);
            }
            return output;
        }

        public static byte[] ReadRGBA4444(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 2, outputPos += 4)
            {
                int r = input[inputPos + 1] >> 4;
                int g = input[inputPos + 1] & 0xf;
                int b = input[inputPos] >> 4;
                int a = input[inputPos] & 0xf;
                output[outputPos + 0] = (byte)((b << 4) | b);
                output[outputPos + 1] = (byte)((g << 4) | g);
                output[outputPos + 2] = (byte)((r << 4) | r);
                output[outputPos + 3] = (byte)((a << 4) | a);
            }
            return output;
        }

        public static byte[] ReadRGBAHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 8, outputPos += 4)
            {
                byte r = ReadHalf(input, inputPos + 0);
                byte g = ReadHalf(input, inputPos + 2);
                byte b = ReadHalf(input, inputPos + 4);
                byte a = ReadHalf(input, inputPos + 6);
                output[outputPos + 0] = b;
                output[outputPos + 1] = g;
                output[outputPos + 2] = r;
                output[outputPos + 3] = a;
            }
            return output;
        }

        public static byte[] ReadARGB32(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int pos = 0; pos < output.Length; pos += 4)
            {
                byte a = input[pos + 0];
                byte r = input[pos + 1];
                byte g = input[pos + 2];
                byte b = input[pos + 3];
                output[pos + 0] = b;
                output[pos + 1] = g;
                output[pos + 2] = r;
                output[pos + 3] = a;
            }
            return output;
        }

        public static byte[] ReadRGBA32(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int pos = 0; pos < output.Length; pos += 4)
            {
                byte r = input[pos + 0];
                byte g = input[pos + 1];
                byte b = input[pos + 2];
                byte a = input[pos + 3];
                output[pos + 0] = b;
                output[pos + 1] = g;
                output[pos + 2] = r;
                output[pos + 3] = a;
            }
            return output;
        }

        private static byte ReadHalf(byte[] input, int pos)
        {
            ushort half = BitConverter.ToUInt16(input, pos);
            float single = HalfHelper.HalfToSingle(half);
            return (byte)Math.Round(Math.Max(Math.Min(single, 1f), 0f) * 255f);
        }
    }
}
