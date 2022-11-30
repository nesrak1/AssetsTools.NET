namespace AssetsTools.NET.Texture
{
    public static class RGBAEncoders
    {
        public static byte[] EncodeR8(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 1)
            {
                byte r = input[inputPos + 2];
                output[outputPos] = r;
            }
            return output;
        }

        public static byte[] EncodeAlpha8(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 1)
            {
                byte a = input[inputPos + 3];
                output[outputPos] = a;
            }
            return output;
        }

        public static byte[] EncodeR16(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte r = input[inputPos + 2];
                output[outputPos + 1] = r;
            }
            return output;
        }

        public static byte[] EncodeRHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte r = input[inputPos + 2];
                WriteHalf(output, outputPos, r);
            }
            return output;
        }

        public static byte[] EncodeRG16(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                output[outputPos + 0] = r;
                output[outputPos + 1] = g;
            }
            return output;
        }

        public static byte[] EncodeRGHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 4)
            {
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                WriteHalf(output, outputPos + 0, r);
                WriteHalf(output, outputPos + 2, g);
            }
            return output;
        }

        public static byte[] EncodeRGB24(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 3];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 3)
            {
                byte b = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                output[outputPos + 0] = r;
                output[outputPos + 1] = g;
                output[outputPos + 2] = b;
            }
            return output;
        }

        public static byte[] EncodeRGB565(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte b = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                int rgb = ((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3);
                output[outputPos + 0] = (byte)rgb;
                output[outputPos + 1] = (byte)(rgb >> 8);
            }
            return output;
        }

        public static byte[] EncodeARGB4444(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte b = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                byte a = input[inputPos + 3];
                output[outputPos + 0] = (byte)((g & 0xF0) | (b >> 4));
                output[outputPos + 1] = (byte)((a & 0xF0) | (r >> 4));
            }
            return output;
        }

        public static byte[] EncodeRGBA4444(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 2];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 2)
            {
                byte b = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                byte a = input[inputPos + 3];
                output[outputPos + 0] = (byte)((b & 0xF0) | (a >> 4));
                output[outputPos + 1] = (byte)((r & 0xF0) | (g >> 4));
            }
            return output;
        }

        public static byte[] EncodeRGBAHalf(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 8];
            for (int inputPos = 0, outputPos = 0; outputPos < output.Length; inputPos += 4, outputPos += 8)
            {
                byte b = input[inputPos + 0];
                byte g = input[inputPos + 1];
                byte r = input[inputPos + 2];
                byte a = input[inputPos + 3];
                WriteHalf(output, outputPos + 0, r);
                WriteHalf(output, outputPos + 2, g);
                WriteHalf(output, outputPos + 4, b);
                WriteHalf(output, outputPos + 6, a);
            }
            return output;
        }

        public static byte[] EncodeARGB32(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int pos = 0; pos < output.Length; pos += 4)
            {
                byte b = input[pos + 0];
                byte g = input[pos + 1];
                byte r = input[pos + 2];
                byte a = input[pos + 3];
                output[pos + 0] = a;
                output[pos + 1] = r;
                output[pos + 2] = g;
                output[pos + 3] = b;
            }
            return output;
        }

        public static byte[] EncodeRGBA32(byte[] input, int width, int height)
        {
            byte[] output = new byte[width * height * 4];
            for (int pos = 0; pos < output.Length; pos += 4)
            {
                byte b = input[pos + 0];
                byte g = input[pos + 1];
                byte r = input[pos + 2];
                byte a = input[pos + 3];
                output[pos + 0] = r;
                output[pos + 1] = g;
                output[pos + 2] = b;
                output[pos + 3] = a;
            }
            return output;
        }

        private static void WriteHalf(byte[] output, int pos, byte value)
        {
            float single = value / 255.0f;
            ushort half = HalfHelper.SingleToHalf(single);
            output[pos + 0] = (byte)half;
            output[pos + 1] = (byte)(half >> 8);
        }
    }
}
