using System;
using System.Runtime.InteropServices;

namespace AssetsTools.NET.Texture
{
    public class TextureEncoderWrapper
    {
        public struct TextureDataBuffer
        {
            public int width;
            public int height;
            public int size;
            public IntPtr data;
        }

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SanityCheck(int number);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadTextureFromFile(string path);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr LoadTextureFromBuffer(byte* data, int size, int width, int height);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern TextureDataBuffer ConvertAndFreeTexture(IntPtr image, TextureFormat format, int quality = 3, int mips = 1);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeTextureDataBuffer(IntPtr imageData);

        [DllImport("PVRTexLib", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint PVRTexLib_GetFormatBitsPerPixel(ulong u64PixelFormat);

        private static ulong RGBA8888 = (
            (byte)'r' + ((byte)'g' << 8) + ((byte)'b' << 16) + ((byte)'a' << 24) +
            (8L << 32) + (8L << 40) + (8L << 48) + (8L << 56));

        private static bool IsPvrtlFormat(TextureFormat format)
        {
            return format == TextureFormat.PVRTC_RGB2 ||
                   format == TextureFormat.PVRTC_RGBA2 ||
                   format == TextureFormat.PVRTC_RGB4 ||
                   format == TextureFormat.PVRTC_RGBA4;
        }

        public static byte[] ConvertImage(string path, TextureFormat textureFormat, out int width, out int height, int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                width = 0;
                height = 0;
                return null;
            }

            IntPtr image = LoadTextureFromFile(path);
            if (image == IntPtr.Zero)
            {
                throw new Exception($"{nameof(LoadTextureFromFile)} returned null.");
            }

            TextureDataBuffer dataBuffer = ConvertAndFreeTexture(image, textureFormat, quality);
            ThrowIfConvertFailed(ref dataBuffer);

            width = dataBuffer.width;
            height = dataBuffer.height;

            IntPtr dataPtr = dataBuffer.data;
            int dataSize = dataBuffer.size;

            byte[] data = new byte[dataSize];
            Marshal.Copy(dataPtr, data, 0, dataSize);

            FreeTextureDataBuffer(dataPtr);
            return data;
        }

        public static byte[] ConvertImage(byte[] rgbaData, TextureFormat textureFormat, int width, int height, int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                return null;
            }

            IntPtr image;
            unsafe
            {
                fixed (byte* rgbaDataPtr = rgbaData)
                {
                    image = LoadTextureFromBuffer(rgbaDataPtr, rgbaData.Length, width, height);
                }
            }
            if (image == IntPtr.Zero)
            {
                throw new Exception($"{nameof(LoadTextureFromBuffer)} returned null.");
            }

            IntPtr dataPtr = IntPtr.Zero;
            byte[] data = null;

            TextureDataBuffer dataBuffer = ConvertAndFreeTexture(image, textureFormat, quality);
            ThrowIfConvertFailed(ref dataBuffer);

            dataPtr = dataBuffer.data;
            int dataSize = dataBuffer.size;

            data = new byte[dataSize];
            Marshal.Copy(dataPtr, data, 0, dataSize);

            FreeTextureDataBuffer(dataPtr);
            return data;
        }

        public static bool NativeLibrariesSupported()
        {
            try
            {
                int sanityCheck = SanityCheck(123);
                return sanityCheck == 246;
            }
            catch
            {
                return false;
            }
        }

        private static void ThrowIfConvertFailed(ref TextureDataBuffer dataBuf)
        {
            int width = dataBuf.width;
            int height = dataBuf.height;
            if (height == -1)
            {
                IntPtr dataPtr = dataBuf.data;
                if (dataPtr != IntPtr.Zero)
                {
                    FreeTextureDataBuffer(dataPtr);
                }

                if (width == -1)
                {
                    throw new Exception($"{nameof(ConvertAndFreeTexture)} failed to encode texture.");
                }
                else if (width == -2)
                {
                    throw new Exception($"{nameof(ConvertAndFreeTexture)} failed to encode mipmaps.");
                }
                else if (width == -3)
                {
                    throw new Exception($"{nameof(ConvertAndFreeTexture)} failed to allocate memory.");
                }
                else
                {
                    throw new Exception($"{nameof(ConvertAndFreeTexture)} returned an unknown error.");
                }
            }
        }

        private static bool PvrtlStubInUse()
        {
            return PVRTexLib_GetFormatBitsPerPixel(RGBA8888) == 12345;
        }
    }
}