using AssetsTools.NET.Extra;
using System;
using System.IO;

namespace AssetsTools.NET
{
    public class TextureFile
    {
        public string m_Name;
        public int m_ForcedFallbackFormat;
        public bool m_DownscaleFallback;
        public int m_Width;
        public int m_Height;
        public int m_CompleteImageSize;
        public int m_TextureFormat;
        public int m_MipCount;
        public bool m_MipMap;
        public bool m_IsReadable;
        public bool m_ReadAllowed;
        public bool m_StreamingMipmaps;
        public int m_StreamingMipmapsPriority;
        public int m_ImageCount;
        public int m_TextureDimension;
        public GLTextureSettings m_TextureSettings;
        public int m_LightmapFormat;
        public int m_ColorSpace;
        public byte[] pictureData;
        public StreamingInfo m_StreamData;

        public struct GLTextureSettings
        {
            public int m_FilterMode;
            public int m_Aniso;
            public float m_MipBias;
            public int m_WrapMode;
            public int m_WrapU;
            public int m_WrapV;
            public int m_WrapW;
        }

        public struct StreamingInfo
        {
            public ulong offset;
            public uint size;
            public string path;
        }

        public static TextureFile ReadTextureFile(AssetTypeValueField baseField)
        {
            TextureFile texture = new TextureFile();
            AssetTypeValueField tempField;

            texture.m_Name = baseField.Get("m_Name").GetValue().AsString();

            if (!(tempField = baseField.Get("m_ForcedFallbackFormat")).IsDummy())
                texture.m_ForcedFallbackFormat = tempField.GetValue().AsInt();

            if (!(tempField = baseField.Get("m_DownscaleFallback")).IsDummy())
                texture.m_DownscaleFallback = tempField.GetValue().AsBool();

            texture.m_Width = baseField.Get("m_Width").GetValue().AsInt();

            texture.m_Height = baseField.Get("m_Height").GetValue().AsInt();

            if (!(tempField = baseField.Get("m_CompleteImageSize")).IsDummy())
                texture.m_CompleteImageSize = tempField.GetValue().AsInt();

            texture.m_TextureFormat = baseField.Get("m_TextureFormat").GetValue().AsInt();

            if (!(tempField = baseField.Get("m_MipCount")).IsDummy())
                texture.m_MipCount = tempField.GetValue().AsInt();

            if (!(tempField = baseField.Get("m_MipMap")).IsDummy())
                texture.m_MipMap = tempField.GetValue().AsBool();

            texture.m_IsReadable = baseField.Get("m_IsReadable").GetValue().AsBool();

            if (!(tempField = baseField.Get("m_ReadAllowed")).IsDummy())
                texture.m_ReadAllowed = tempField.GetValue().AsBool();

            if (!(tempField = baseField.Get("m_StreamingMipmaps")).IsDummy())
                texture.m_StreamingMipmaps = tempField.GetValue().AsBool();

            if (!(tempField = baseField.Get("m_StreamingMipmapsPriority")).IsDummy())
                texture.m_StreamingMipmapsPriority = tempField.GetValue().AsInt();

            texture.m_ImageCount = baseField.Get("m_ImageCount").GetValue().AsInt();

            texture.m_TextureDimension = baseField.Get("m_TextureDimension").GetValue().AsInt();

            AssetTypeValueField textureSettings = baseField.Get("m_TextureSettings");

            texture.m_TextureSettings.m_FilterMode = textureSettings.Get("m_FilterMode").GetValue().AsInt();

            texture.m_TextureSettings.m_Aniso = textureSettings.Get("m_Aniso").GetValue().AsInt();

            texture.m_TextureSettings.m_MipBias = textureSettings.Get("m_MipBias").GetValue().AsFloat();

            if (!(tempField = textureSettings.Get("m_WrapMode")).IsDummy())
                texture.m_TextureSettings.m_WrapMode = tempField.GetValue().AsInt();

            if (!(tempField = textureSettings.Get("m_WrapU")).IsDummy())
                texture.m_TextureSettings.m_WrapU = tempField.GetValue().AsInt();

            if (!(tempField = textureSettings.Get("m_WrapV")).IsDummy())
                texture.m_TextureSettings.m_WrapV = tempField.GetValue().AsInt();

            if (!(tempField = textureSettings.Get("m_WrapW")).IsDummy())
                texture.m_TextureSettings.m_WrapW = tempField.GetValue().AsInt();

            if (!(tempField = baseField.Get("m_LightmapFormat")).IsDummy())
                texture.m_LightmapFormat = tempField.GetValue().AsInt();

            if (!(tempField = baseField.Get("m_ColorSpace")).IsDummy())
                texture.m_ColorSpace = tempField.GetValue().AsInt();

            AssetTypeValueField imageData = baseField.Get("image data");
            if (imageData.templateField.valueType == EnumValueTypes.ByteArray)
            {
                texture.pictureData = imageData.GetValue().AsByteArray().data;
            }
            else
            {
                int imageDataSize = imageData.GetValue().AsArray().size;
                texture.pictureData = new byte[imageDataSize];
                for (int i = 0; i < imageDataSize; i++)
                {
                    texture.pictureData[i] = (byte)imageData[i].GetValue().AsInt();
                }
            }

            AssetTypeValueField streamData;

            if (!(streamData = baseField.Get("m_StreamData")).IsDummy())
            {
                texture.m_StreamData.offset = streamData.Get("offset").GetValue().AsUInt64();
                texture.m_StreamData.size = streamData.Get("size").GetValue().AsUInt();
                texture.m_StreamData.path = streamData.Get("path").GetValue().AsString();
            }

            return texture;
        }

        //default setting for assetstools
        //usually you have to cd to the assets file
        public byte[] GetTextureData()
        {
            return GetTextureData(Directory.GetCurrentDirectory());
        }

        //new functions since I didn't like the way assetstools handled it
        public byte[] GetTextureData(AssetsFileInstance inst)
        {
            return GetTextureData(Path.GetDirectoryName(inst.path));
        }

        public byte[] GetTextureData(AssetsFile file)
        {
            string path = null;
            if (file.readerPar is FileStream fs)
            {
                path = Path.GetDirectoryName(fs.Name);
            }
            return GetTextureData(path);
        }

        public byte[] GetTextureData(string rootPath)
        {
            if (m_StreamData.size != 0 && m_StreamData.path != string.Empty)
            {
                string fixedStreamPath = m_StreamData.path;
                if (!Path.IsPathRooted(fixedStreamPath) && rootPath != null)
                {
                    fixedStreamPath = Path.Combine(rootPath, fixedStreamPath);
                }
                if (File.Exists(fixedStreamPath))
                {
                    Stream stream = File.OpenRead(fixedStreamPath);
                    stream.Position = (long)m_StreamData.offset;
                    pictureData = new byte[m_StreamData.size];
                    stream.Read(pictureData, 0, (int)m_StreamData.size);
                }
                else
                {
                    return null;
                }
            }
            int width = m_Width;
            int height = m_Height;
            TextureFormat texFmt = (TextureFormat)m_TextureFormat;
            return GetTextureDataFromBytes(pictureData, texFmt, width, height);
        }

        public static byte[] GetTextureDataFromBytes(byte[] data, TextureFormat texFmt, int width, int height)
        {
            switch (texFmt)
            {
                case TextureFormat.R8:
                    return RGBADecoders.ReadR8(data, width, height);
                case TextureFormat.R16:
                    return RGBADecoders.ReadR16(data, width, height);
                case TextureFormat.RG16:
                    return RGBADecoders.ReadRG16(data, width, height);
                case TextureFormat.RGB24:
                    return RGBADecoders.ReadRGB24(data, width, height);
                case TextureFormat.RGBA32:
                    return RGBADecoders.ReadRGBA32(data, width, height);
                case TextureFormat.ARGB32:
                    return RGBADecoders.ReadARGB32(data, width, height);
                case TextureFormat.RGBA4444:
                    return RGBADecoders.ReadRGBA4444(data, width, height);
                case TextureFormat.ARGB4444:
                    return RGBADecoders.ReadARGB4444(data, width, height);
                case TextureFormat.Alpha8:
                    return RGBADecoders.ReadAlpha8(data, width, height);
                case TextureFormat.DXT1:
                    return DXTDecoders.ReadDXT1(data, width, height);
                case TextureFormat.DXT5:
                    return DXTDecoders.ReadDXT5(data, width, height);
                case TextureFormat.BC7:
                    return BC7Decoder.ReadBC7(data, width, height);
                case TextureFormat.ETC_RGB4:
                    return ETCDecoders.ReadETC(data, width, height);
                case TextureFormat.ETC2_RGB4:
                    return ETCDecoders.ReadETC(data, width, height, true);
                default:
                    return null;
            }
        }
    }

    public enum TextureFormat
    {
        Alpha8 = 1, //Unity 1.5 or earlier (already in 1.2.2 according to documentation)
        ARGB4444, //Unity 3.0 (already in 1.2.2)
        RGB24, //Unity 1.5 or earlier (already in 1.2.2)
        RGBA32, //Unity 3.2 (not sure about 1.2.2)
        ARGB32, //Unity 1.5 or earlier (already in 1.2.2)
        UNUSED06,
        RGB565, //Unity 3.0 (already in 1.2.2)
        UNUSED08,
        R16, //Unity 5.0
        DXT1, //Unity 2.0 (already in 1.2.2)
        UNUSED11, //(DXT3 in 1.2.2?)
        DXT5, //Unity 2.0
        RGBA4444, //Unity 4.1
        BGRA32New, //Unity 4.5
        RHalf, //Unity 5.0
        RGHalf, //Unity 5.0
        RGBAHalf, //Unity 5.0
        RFloat, //Unity 5.0
        RGFloat, //Unity 5.0
        RGBAFloat, //Unity 5.0
        YUV2, //Unity 5.0
        RGB9e5Float, //Unity 5.6
        UNUSED23,
        BC6H, //Unity 5.5
        BC7, //Unity 5.5
        BC4, //Unity 5.5
        BC5, //Unity 5.5
        DXT1Crunched, //Unity 5.0 //SupportsTextureFormat version codes 0 (original) and 1 (Unity 2017.3)
        DXT5Crunched, //Unity 5.0 //SupportsTextureFormat version codes 0 (original) and 1 (Unity 2017.3)
        PVRTC_RGB2, //Unity 2.6
        PVRTC_RGBA2, //Unity 2.6
        PVRTC_RGB4, //Unity 2.6
        PVRTC_RGBA4, //Unity 2.6
        ETC_RGB4, //Unity 3.0
        ATC_RGB4, //Unity 3.4, removed in 2018.1
        ATC_RGBA8, //Unity 3.4, removed in 2018.1
        BGRA32Old, //Unity 3.4, removed in Unity 4.5
        UNUSED38, //TexFmt_ATF_RGB_DXT1, added in Unity 3.5, removed in Unity 5.0
        UNUSED39, //TexFmt_ATF_RGBA_JPG, added in Unity 3.5, removed in Unity 5.0
        UNUSED40, //TexFmt_ATF_RGB_JPG, added in Unity 3.5, removed in Unity 5.0
        EAC_R, //Unity 4.5
        EAC_R_SIGNED, //Unity 4.5
        EAC_RG, //Unity 4.5
        EAC_RG_SIGNED, //Unity 4.5
        ETC2_RGB4, //Unity 4.5
        ETC2_RGBA1, //Unity 4.5 //R4G4B4A1
        ETC2_RGBA8, //Unity 4.5 //R8G8B8A8
        ASTC_RGB_4x4, //Unity 4.5
        ASTC_RGB_5x5, //Unity 4.5
        ASTC_RGB_6x6, //Unity 4.5
        ASTC_RGB_8x8, //Unity 4.5
        ASTC_RGB_10x10, //Unity 4.5
        ASTC_RGB_12x12, //Unity 4.5
        ASTC_RGBA_4x4, //Unity 4.5
        ASTC_RGBA_5x5, //Unity 4.5
        ASTC_RGBA_6x6, //Unity 4.5
        ASTC_RGBA_8x8, //Unity 4.5
        ASTC_RGBA_10x10, //Unity 4.5
        ASTC_RGBA_12x12, //Unity 4.5
        ETC_RGB4_3DS, //Unity 5.0
        ETC_RGBA8_3DS, //Unity 5.0
        RG16, //Unity 2017.1
        R8, //Unity 2017.1
        ETC_RGB4Crunched, //Unity 2017.3  //SupportsTextureFormat version code 1
        ETC2_RGBA8Crunched //Unity 2017.3  //SupportsTextureFormat version code 1
    }
}
