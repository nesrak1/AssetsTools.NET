using AssetsTools.NET.Extra;
using System;
using System.IO;
using System.Linq;

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

        public void WriteTo(AssetTypeValueField baseField)
        {
            AssetTypeValueField tempField;

            baseField.Get("m_Name").GetValue().Set(m_Name);

            if (!(tempField = baseField.Get("m_ForcedFallbackFormat")).IsDummy())
                tempField.GetValue().Set(m_ForcedFallbackFormat);

            if (!(tempField = baseField.Get("m_DownscaleFallback")).IsDummy())
                tempField.GetValue().Set(m_DownscaleFallback);

            baseField.Get("m_Width").GetValue().Set(m_Width);

            baseField.Get("m_Height").GetValue().Set(m_Height);

            if (!(tempField = baseField.Get("m_CompleteImageSize")).IsDummy())
                tempField.GetValue().Set(m_CompleteImageSize);

            baseField.Get("m_TextureFormat").GetValue().Set(m_TextureFormat);

            if (!(tempField = baseField.Get("m_MipCount")).IsDummy())
                tempField.GetValue().Set(m_MipCount);

            if (!(tempField = baseField.Get("m_MipMap")).IsDummy())
                tempField.GetValue().Set(m_MipMap);

            baseField.Get("m_IsReadable").GetValue().Set(m_IsReadable);

            if (!(tempField = baseField.Get("m_ReadAllowed")).IsDummy())
                tempField.GetValue().Set(m_ReadAllowed);

            if (!(tempField = baseField.Get("m_StreamingMipmaps")).IsDummy())
                tempField.GetValue().Set(m_StreamingMipmaps);

            if (!(tempField = baseField.Get("m_StreamingMipmapsPriority")).IsDummy())
                tempField.GetValue().Set(m_StreamingMipmapsPriority);

            baseField.Get("m_ImageCount").GetValue().Set(m_ImageCount);

            baseField.Get("m_TextureDimension").GetValue().Set(m_TextureDimension);

            AssetTypeValueField textureSettings = baseField.Get("m_TextureSettings");

            textureSettings.Get("m_FilterMode").GetValue().Set(m_TextureSettings.m_FilterMode);
            textureSettings.Get("m_Aniso").GetValue().Set(m_TextureSettings.m_Aniso);
            textureSettings.Get("m_MipBias").GetValue().Set(m_TextureSettings.m_MipBias);

            if (!(tempField = textureSettings.Get("m_WrapMode")).IsDummy())
                tempField.GetValue().Set(m_TextureSettings.m_WrapMode);

            if (!(tempField = textureSettings.Get("m_WrapU")).IsDummy())
                tempField.GetValue().Set(m_TextureSettings.m_WrapU);

            if (!(tempField = textureSettings.Get("m_WrapV")).IsDummy())
                tempField.GetValue().Set(m_TextureSettings.m_WrapV);

            if (!(tempField = textureSettings.Get("m_WrapW")).IsDummy())
                tempField.GetValue().Set(m_TextureSettings.m_WrapW);

            if (!(tempField = baseField.Get("m_LightmapFormat")).IsDummy())
                tempField.GetValue().Set(m_LightmapFormat);

            if (!(tempField = baseField.Get("m_ColorSpace")).IsDummy())
                tempField.GetValue().Set(m_ColorSpace);

            AssetTypeValueField imageData = baseField.Get("image data");
            if (imageData.templateField.valueType == EnumValueTypes.ByteArray)
            {
                imageData.GetValue().Set(pictureData);
            }
            else
            {
                imageData.GetValue().Set(new AssetTypeArray(pictureData.Length));

                AssetTypeValueField[] children = new AssetTypeValueField[pictureData.Length];
                for (int i = 0; i < pictureData.Length; i++)
                {
                    AssetTypeValueField child = ValueBuilder.DefaultValueFieldFromArrayTemplate(imageData);
                    child.GetValue().Set(pictureData[i]);
                    children[i] = child;
                }

                imageData.SetChildrenList(children);
            }

            AssetTypeValueField streamData;

            if (!(streamData = baseField.Get("m_StreamData")).IsDummy())
            {
                streamData.Get("offset").GetValue().Set(m_StreamData.offset);
                streamData.Get("size").GetValue().Set(m_StreamData.size);
                streamData.Get("path").GetValue().Set(m_StreamData.path);
            }
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
            if ((pictureData == null || pictureData.Length == 0) && !string.IsNullOrEmpty(m_StreamData.path) && m_StreamData.size != 0)
            {
                string fixedStreamPath = m_StreamData.path;
                if (!Path.IsPathRooted(fixedStreamPath) && rootPath != null)
                {
                    fixedStreamPath = Path.Combine(rootPath, fixedStreamPath);
                }
                if (File.Exists(fixedStreamPath))
                {
                    using Stream stream = File.OpenRead(fixedStreamPath);
                    stream.Position = (long)m_StreamData.offset;
                    pictureData = new byte[m_StreamData.size];
                    stream.Read(pictureData, 0, (int)m_StreamData.size);
                }
                else
                {
                    return null;
                }
            }
            return Decode(pictureData, (TextureFormat)m_TextureFormat, m_Width, m_Height);
        }

        public byte[] GetTextureData(string rootPath, AssetBundleFile bundle)
        {
            if ((pictureData == null || pictureData.Length == 0) && m_StreamData.path != null && m_StreamData.path.StartsWith("archive:/") && bundle != null)
            {
                string resourceFileName = m_StreamData.path.Split('/').Last();
                int resourceFileIndex = bundle.GetFileIndex(resourceFileName);
                if (resourceFileIndex >= 0)
                {
                    bundle.GetFileRange(resourceFileIndex, out long resourceFileOffset, out _);
                    pictureData = new byte[m_StreamData.size];
                    bundle.reader.Position = resourceFileOffset + (long)m_StreamData.offset;
                    bundle.reader.Read(pictureData, 0, pictureData.Length);
                }
            }

            return GetTextureData(rootPath);
        }

        public void SetTextureData(byte[] bgra, int width, int height)
        {
            byte[] encoded = Encode(bgra, (TextureFormat)m_TextureFormat, width, height);
            if (encoded == null)
                throw new NotSupportedException("The current texture format is not supported for encoding.");

            SetTextureDataRaw(encoded, width, height);
        }

        public void SetTextureDataRaw(byte[] encodedData, int width, int height)
        {
            m_Width = width;
            m_Height = height;
            m_StreamData.path = null;
            m_StreamData.offset = 0;
            m_StreamData.size = 0;
            pictureData = encodedData;
            m_CompleteImageSize = encodedData.Length;
        }

        [Obsolete("Renamed to " + nameof(Decode))]
        public static byte[] GetTextureDataFromBytes(byte[] data, TextureFormat format, int width, int height)
        {
            return Decode(data, format, width, height);
        }

        public static byte[] Decode(byte[] data, TextureFormat format, int width, int height)
        {
            return format switch
                   {
                       TextureFormat.R8 =>        RGBADecoders.ReadR8(data, width, height),
                       TextureFormat.R16 =>       RGBADecoders.ReadR16(data, width, height),
                       TextureFormat.RG16 =>      RGBADecoders.ReadRG16(data, width, height),
                       TextureFormat.RGB24 =>     RGBADecoders.ReadRGB24(data, width, height),
                       TextureFormat.RGB565 =>    RGBADecoders.ReadRGB565(data, width, height),
                       TextureFormat.RGBA32 =>    RGBADecoders.ReadRGBA32(data, width, height),
                       TextureFormat.ARGB32 =>    RGBADecoders.ReadARGB32(data, width, height),
                       TextureFormat.RGBA4444 =>  RGBADecoders.ReadRGBA4444(data, width, height),
                       TextureFormat.ARGB4444 =>  RGBADecoders.ReadARGB4444(data, width, height),
                       TextureFormat.Alpha8 =>    RGBADecoders.ReadAlpha8(data, width, height),
                       TextureFormat.RHalf =>     RGBADecoders.ReadRHalf(data, width, height),
                       TextureFormat.RGHalf =>    RGBADecoders.ReadRGHalf(data, width, height),
                       TextureFormat.RGBAHalf =>  RGBADecoders.ReadRGBAHalf(data, width, height),
                       TextureFormat.DXT1 =>      DXTDecoders.ReadDXT1(data, width, height),
                       TextureFormat.DXT5 =>      DXTDecoders.ReadDXT5(data, width, height),
                       TextureFormat.BC7 =>       BC7Decoder.ReadBC7(data, width, height),
                       TextureFormat.ETC_RGB4 =>  ETCDecoders.ReadETC(data, width, height),
                       TextureFormat.ETC2_RGB4 => ETCDecoders.ReadETC(data, width, height, true),
                       _ => null
                   };
        }

        public static byte[] Encode(byte[] data, TextureFormat format, int width, int height)
        {
            return format switch
                   {
                       TextureFormat.R8 =>       RGBAEncoders.EncodeR8(data, width, height),
                       TextureFormat.R16 =>      RGBAEncoders.EncodeR16(data, width, height),
                       TextureFormat.RG16 =>     RGBAEncoders.EncodeRG16(data, width, height),
                       TextureFormat.RGB24 =>    RGBAEncoders.EncodeRGB24(data, width, height),
                       TextureFormat.RGB565 =>   RGBAEncoders.EncodeRGB565(data, width, height),
                       TextureFormat.RGBA32 =>   RGBAEncoders.EncodeRGBA32(data, width, height),
                       TextureFormat.ARGB32 =>   RGBAEncoders.EncodeARGB32(data, width, height),
                       TextureFormat.RGBA4444 => RGBAEncoders.EncodeRGBA4444(data, width, height),
                       TextureFormat.ARGB4444 => RGBAEncoders.EncodeARGB4444(data, width, height),
                       TextureFormat.Alpha8 =>   RGBAEncoders.EncodeAlpha8(data, width, height),
                       TextureFormat.RHalf =>    RGBAEncoders.EncodeRHalf(data, width, height),
                       TextureFormat.RGHalf =>   RGBAEncoders.EncodeRGHalf(data, width, height),
                       TextureFormat.RGBAHalf => RGBAEncoders.EncodeRGBAHalf(data, width, height),
                       _ => null
                   };
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
