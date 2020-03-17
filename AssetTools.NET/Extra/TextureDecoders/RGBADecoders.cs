using System;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public class RGBADecoders
    {
        public static byte[] ReadRGBA32(Stream stream, int width, int height)
        {
            int len = width * height * 4;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            for (int i = 0; i < len; i += 4)
            {
                pixel[0] = bytes[i + 2];
                pixel[1] = bytes[i + 1];
                pixel[2] = bytes[i];
                pixel[3] = bytes[i + 3];
                Buffer.BlockCopy(pixel, 0, bytes, i, 4);
            }
            return bytes;
        }
        public static byte[] ReadRGBA4444(Stream stream, int width, int height)
        {
            int len = width * height * 2;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            for (int i = 0; i < len; i += 2)
            {
                pixel[0] = (byte)(bytes[i + 1] & 0xf);
                pixel[1] = (byte)(bytes[i] >> 4);
                pixel[2] = (byte)(bytes[i] & 0xf);
                pixel[3] = (byte)(bytes[i + 1] >> 4);
                Buffer.BlockCopy(pixel, 0, bytes, i / 2 * 4, 4);
            }
            return bytes;
        }
        public static byte[] ReadARGB32(Stream stream, int width, int height)
        {
            int len = width * height * 4;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            for (int i = 0; i < len; i += 4)
            {
                pixel[0] = bytes[i + 3];
                pixel[1] = bytes[i + 2];
                pixel[2] = bytes[i + 1];
                pixel[3] = bytes[i];
                Buffer.BlockCopy(pixel, 0, bytes, i, 4);
            }
            return bytes;
        }
        public static byte[] ReadARGB4444(Stream stream, int width, int height)
        {
            int len = width * height * 2;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            for (int i = 0; i < len; i += 2)
            {
                pixel[0] = (byte)(bytes[i + 1] >> 4);
                pixel[1] = (byte)(bytes[i + 1] & 0xf);
                pixel[2] = (byte)(bytes[i] >> 4);
                pixel[3] = (byte)(bytes[i] & 0xf);
                Buffer.BlockCopy(pixel, 0, bytes, i / 2 * 4, 4);
            }
            return bytes;
        }
        public static byte[] ReadRGB24(Stream stream, int width, int height)
        {
            int len = width * height * 3;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            pixel[3] = 0xFF;
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < len; i += 3)
            {
                pixel[0] = bytes[i + 2];
                pixel[1] = bytes[i + 1];
                pixel[2] = bytes[i];
                Buffer.BlockCopy(pixel, 0, data, i / 3 * 4, 4);
            }
            return data;
        }
        public static byte[] ReadRG16(Stream stream, int width, int height)
        {
            int len = width * height * 2;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            pixel[2] = 0x00;
            pixel[3] = 0xFF;
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < len; i += 2)
            {
                pixel[0] = bytes[i + 1];
                pixel[1] = bytes[i];
                Buffer.BlockCopy(pixel, 0, data, i / 2 * 4, 4);
            }
            return data;
        }
        public static byte[] ReadR16(Stream stream, int width, int height)
        {
            int len = width * height * 2;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            pixel[1] = 0x00;
            pixel[2] = 0x00;
            pixel[3] = 0xFF;
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < len; i += 2)
            {
                pixel[0] = (byte)((bytes[i+1] & bytes[i])/0x100);
                Buffer.BlockCopy(pixel, 0, data, i / 2 * 4, 4);
            }
            return data;
        }
        public static byte[] ReadR8(Stream stream, int width, int height)
        {
            int len = width * height;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            pixel[1] = 0x00;
            pixel[2] = 0x00;
            pixel[3] = 0xFF;
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < len; i++)
            {
                pixel[0] = bytes[i];
                Buffer.BlockCopy(pixel, 0, data, i * 4, 4);
            }
            return data;
        }
        public static byte[] ReadAlpha8(Stream stream, int width, int height)
        {
            int len = width * height;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            byte[] pixel = new byte[4];
            pixel[0] = 0xFF;
            pixel[1] = 0xFF;
            pixel[2] = 0xFF;
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < len; i++)
            {
                pixel[3] = bytes[i];
                Buffer.BlockCopy(pixel, 0, data, i * 4, 4);
            }
            return data;
        }
    }
}
