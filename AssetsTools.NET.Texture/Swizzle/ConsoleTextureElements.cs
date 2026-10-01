namespace AssetsTools.NET.Texture
{
    // Pixel and block sizes supported by the texture decoder.
    public static class ConsoleTextureElements
    {
        public static bool TryGetInfo(TextureFormat format, out int blockSize, out int bytes)
        {
            blockSize = 1;
            bytes = 0;
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                    bytes = 1;
                    return true;
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.RGBA4444:
                case TextureFormat.RG16:
                case TextureFormat.RHalf:
                    bytes = 2;
                    return true;
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.BGRA32Old:
                case TextureFormat.RGHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGB9e5Float:
                case TextureFormat.RG32:
                    bytes = 4;
                    return true;
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBA64:
                    bytes = 8;
                    return true;
                case TextureFormat.RGBAFloat:
                    bytes = 16;
                    return true;
                case TextureFormat.DXT1:
                case TextureFormat.BC4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB4:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.ATC_RGB4:
                    blockSize = 4;
                    bytes = 8;
                    return true;
                case TextureFormat.DXT3:
                case TextureFormat.DXT5:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                case TextureFormat.ATC_RGBA8:
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGBA_4x4:
                    blockSize = 4;
                    bytes = 16;
                    return true;
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGBA_5x5:
                    blockSize = 5;
                    bytes = 16;
                    return true;
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGBA_6x6:
                    blockSize = 6;
                    bytes = 16;
                    return true;
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGBA_8x8:
                    blockSize = 8;
                    bytes = 16;
                    return true;
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGBA_10x10:
                    blockSize = 10;
                    bytes = 16;
                    return true;
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_12x12:
                    blockSize = 12;
                    bytes = 16;
                    return true;
                // RGB24 is expanded by the caller. Other storage formats are unsupported.
                default:
                    return false;
            }
        }

        // Only these formats have image encoders.
        public static bool CanEncodeImage(TextureFormat format) => format switch
        {
            TextureFormat.Alpha8 or TextureFormat.R8 or TextureFormat.RGB24
            or TextureFormat.RGBA32 or TextureFormat.ARGB32 or TextureFormat.BGRA32
            or TextureFormat.BGRA32Old or TextureFormat.DXT1 or TextureFormat.DXT3
            or TextureFormat.DXT5 or TextureFormat.BC4 or TextureFormat.BC5
            or TextureFormat.BC6H or TextureFormat.BC7 => true,
            _ => false
        };
    }
}
