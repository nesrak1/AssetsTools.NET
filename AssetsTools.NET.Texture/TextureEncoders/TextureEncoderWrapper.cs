using System;
using System.Runtime.InteropServices;

namespace AssetsTools.NET.Texture
{
    public static partial class TextureEncoderWrapper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct TextureDataMip
        {
            public int Width;
            public int Height;
            public int Size;
            public IntPtr Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureDataBuffer
        {
            public int Width;
            public int Height;
            public int MipCount;
            public IntPtr Mips;
        }

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SanityCheck(int number);

        // todo: cuttlefish uses freeimage which uses fopen
        // strings will be not support non-ansi until we patch
        // cuttlefish to use freeimage's windows overload
        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadTextureFromFile(string path, int mips = 1);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr LoadTextureFromBuffer(byte[] data, int size, int width, int height, int mips = 1);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern TextureDataBuffer ConvertAndFreeTexture(IntPtr image, TextureFormat format, int quality = 3);

        [DllImport("textureencoder", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeTextureDataBuffer(IntPtr mips, int mipCount);

        [DllImport("PVRTexLib", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint PVRTexLib_GetFormatBitsPerPixel(ulong u64PixelFormat);

        private const ulong RGBA8888 =
            (byte)'r' + ((byte)'g' << 8) + ((byte)'b' << 16) + ((byte)'a' << 24) +
            (8L << 32) + (8L << 40) + (8L << 48) + (8L << 56);

        public static byte[][] ConvertImage(
            string path, int mipCount, TextureFormat textureFormat,
            out int width, out int height,
            int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                width = 0;
                height = 0;
                return null;
            }

            IntPtr image = LoadTextureFromFile(path, mipCount);
            if (image == IntPtr.Zero)
                throw new Exception($"{nameof(LoadTextureFromFile)} returned null.");

            return DoConversion(image, textureFormat, quality, out width, out height);
        }

        public static byte[] ConvertImageFlat(
            string path, int mipCount, TextureFormat textureFormat,
            out int width, out int height,
            int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                width = 0;
                height = 0;
                return null;
            }

            IntPtr image = LoadTextureFromFile(path, mipCount);
            if (image == IntPtr.Zero)
                throw new Exception($"{nameof(LoadTextureFromFile)} returned null.");

            return DoConversionFlat(image, textureFormat, quality, out width, out height);
        }

        public static byte[][] ConvertImage(
            byte[] rgbaData, int mipCount, TextureFormat textureFormat,
            int width, int height,
            int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                width = 0;
                height = 0;
                return null;
            }

            IntPtr image = LoadTextureFromBuffer(rgbaData, mipCount, width, height, mipCount);
            if (image == IntPtr.Zero)
                throw new Exception($"{nameof(LoadTextureFromFile)} returned null.");

            return DoConversion(image, textureFormat, quality, out width, out height);
        }

        public static byte[] ConvertImageFlat(
            byte[] rgbaData, int mipCount, TextureFormat textureFormat,
            int width, int height,
            int quality = 3)
        {
            if (IsPvrtlFormat(textureFormat) && PvrtlStubInUse())
            {
                width = 0;
                height = 0;
                return null;
            }

            IntPtr image = LoadTextureFromBuffer(rgbaData, mipCount, width, height, mipCount);
            if (image == IntPtr.Zero)
                throw new Exception($"{nameof(LoadTextureFromFile)} returned null.");

            return DoConversionFlat(image, textureFormat, quality, out width, out height);
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

        private static byte[][] DoConversion(
            IntPtr image, TextureFormat textureFormat, int quality,
            out int width, out int height)
        {
            TextureDataBuffer dataBuffer = ConvertAndFreeTexture(image, textureFormat, quality);
            ThrowIfConvertFailed(ref dataBuffer);

            width = dataBuffer.Width;
            height = dataBuffer.Height;

            return ProcessTextureDataBuffer(ref dataBuffer);
        }

        private static byte[] DoConversionFlat(
            IntPtr image, TextureFormat textureFormat, int quality,
            out int width, out int height)
        {
            TextureDataBuffer dataBuffer = ConvertAndFreeTexture(image, textureFormat, quality);
            ThrowIfConvertFailed(ref dataBuffer);

            width = dataBuffer.Width;
            height = dataBuffer.Height;

            return ProcessTextureDataBufferFlat(ref dataBuffer);
        }

        private static void ThrowIfConvertFailed(ref TextureDataBuffer dataBuf)
        {
            int width = dataBuf.Width;
            int height = dataBuf.Height;
            if (height == -1)
            {
                throw width switch
                {
                    -1 => new Exception($"{nameof(ConvertAndFreeTexture)} failed to encode texture."),
                    -2 => new Exception($"{nameof(ConvertAndFreeTexture)} failed to encode mipmaps."),
                    -3 => new Exception($"{nameof(ConvertAndFreeTexture)} failed to allocate memory."),
                    _ => new Exception($"{nameof(ConvertAndFreeTexture)} returned an unknown error.")
                };
            }
        }

        private static byte[][] ProcessTextureDataBuffer(ref TextureDataBuffer dataBuf)
        {
            IntPtr mipsPtr = dataBuf.Mips;
            int mipCount = dataBuf.MipCount;

            byte[][] datas = new byte[mipCount][];
            for (int i = 0; i < mipCount; i++)
            {
                IntPtr thisMipPtr = IntPtr.Add(mipsPtr, i * Marshal.SizeOf<TextureDataMip>());
                var mip = Marshal.PtrToStructure<TextureDataMip>(thisMipPtr);
                byte[] data = new byte[mip.Size];
                Marshal.Copy(mip.Data, data, 0, mip.Size);
                datas[i] = data;
            }

            FreeTextureDataBuffer(mipsPtr, mipCount);
            return datas;
        }

        private static byte[] ProcessTextureDataBufferFlat(ref TextureDataBuffer dataBuf)
        {
            IntPtr mipsPtr = dataBuf.Mips;
            int mipCount = dataBuf.MipCount;

            int dataSize = 0;
            for (int i = 0; i < mipCount; i++)
            {
                IntPtr thisMipPtr = IntPtr.Add(mipsPtr, i * Marshal.SizeOf<TextureDataMip>());
                var mip = Marshal.PtrToStructure<TextureDataMip>(thisMipPtr);
                dataSize += mip.Size;
            }

            byte[] data = new byte[dataSize];
            int dataPtr = 0;
            for (int i = 0; i < mipCount; i++)
            {
                IntPtr thisMipPtr = IntPtr.Add(mipsPtr, i * Marshal.SizeOf<TextureDataMip>());
                var mip = Marshal.PtrToStructure<TextureDataMip>(thisMipPtr);
                Marshal.Copy(mip.Data, data, dataPtr, mip.Size);
                dataPtr += mip.Size;
            }

            FreeTextureDataBuffer(mipsPtr, mipCount);
            return data;
        }

        private static bool IsPvrtlFormat(TextureFormat format)
        {
            return format == TextureFormat.PVRTC_RGB2 ||
                   format == TextureFormat.PVRTC_RGBA2 ||
                   format == TextureFormat.PVRTC_RGB4 ||
                   format == TextureFormat.PVRTC_RGBA4;
        }

        private static bool PvrtlStubInUse()
        {
            // the pvrtexlib stub is used so we don't ship the real library (which has non-foss licensing)
            // if real pvrtexlib is being used, this will return the regular RGBA8888 bpp
            return PVRTexLib_GetFormatBitsPerPixel(RGBA8888) == 12345;
        }
    }
}