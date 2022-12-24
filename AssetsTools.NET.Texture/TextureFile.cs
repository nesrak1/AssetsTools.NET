using AssetRipper.TextureDecoder.Astc;
using AssetRipper.TextureDecoder.Atc;
using AssetRipper.TextureDecoder.Bc;
using AssetRipper.TextureDecoder.Dxt;
using AssetRipper.TextureDecoder.Etc;
using AssetRipper.TextureDecoder.Pvrtc;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using AssetRipper.TextureDecoder.Yuy2;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Texture
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

            texture.m_Name = baseField["m_Name"].AsString;

            if (!(tempField = baseField["m_ForcedFallbackFormat"]).IsDummy)
                texture.m_ForcedFallbackFormat = tempField.AsInt;

            if (!(tempField = baseField["m_DownscaleFallback"]).IsDummy)
                texture.m_DownscaleFallback = tempField.AsBool;

            texture.m_Width = baseField["m_Width"].AsInt;

            texture.m_Height = baseField["m_Height"].AsInt;

            if (!(tempField = baseField["m_CompleteImageSize"]).IsDummy)
                texture.m_CompleteImageSize = tempField.AsInt;

            texture.m_TextureFormat = baseField["m_TextureFormat"].AsInt;

            if (!(tempField = baseField["m_MipCount"]).IsDummy)
                texture.m_MipCount = tempField.AsInt;

            if (!(tempField = baseField["m_MipMap"]).IsDummy)
                texture.m_MipMap = tempField.AsBool;

            texture.m_IsReadable = baseField["m_IsReadable"].AsBool;

            if (!(tempField = baseField["m_ReadAllowed"]).IsDummy)
                texture.m_ReadAllowed = tempField.AsBool;

            if (!(tempField = baseField["m_StreamingMipmaps"]).IsDummy)
                texture.m_StreamingMipmaps = tempField.AsBool;

            if (!(tempField = baseField["m_StreamingMipmapsPriority"]).IsDummy)
                texture.m_StreamingMipmapsPriority = tempField.AsInt;

            texture.m_ImageCount = baseField["m_ImageCount"].AsInt;

            texture.m_TextureDimension = baseField["m_TextureDimension"].AsInt;

            AssetTypeValueField textureSettings = baseField["m_TextureSettings"];

            texture.m_TextureSettings.m_FilterMode = textureSettings["m_FilterMode"].AsInt;

            texture.m_TextureSettings.m_Aniso = textureSettings["m_Aniso"].AsInt;

            texture.m_TextureSettings.m_MipBias = textureSettings["m_MipBias"].AsFloat;

            if (!(tempField = textureSettings["m_WrapMode"]).IsDummy)
                texture.m_TextureSettings.m_WrapMode = tempField.AsInt;

            if (!(tempField = textureSettings["m_WrapU"]).IsDummy)
                texture.m_TextureSettings.m_WrapU = tempField.AsInt;

            if (!(tempField = textureSettings["m_WrapV"]).IsDummy)
                texture.m_TextureSettings.m_WrapV = tempField.AsInt;

            if (!(tempField = textureSettings["m_WrapW"]).IsDummy)
                texture.m_TextureSettings.m_WrapW = tempField.AsInt;

            if (!(tempField = baseField["m_LightmapFormat"]).IsDummy)
                texture.m_LightmapFormat = tempField.AsInt;

            if (!(tempField = baseField["m_ColorSpace"]).IsDummy)
                texture.m_ColorSpace = tempField.AsInt;

            AssetTypeValueField imageData = baseField["image data"];
            if (imageData.TemplateField.ValueType == AssetValueType.ByteArray)
            {
                texture.pictureData = imageData.AsByteArray;
            }
            else
            {
                int imageDataSize = imageData.Children.Count;
                texture.pictureData = new byte[imageDataSize];
                for (int i = 0; i < imageDataSize; i++)
                {
                    texture.pictureData[i] = (byte)imageData[i].AsInt;
                }
            }

            AssetTypeValueField streamData;

            if (!(streamData = baseField["m_StreamData"]).IsDummy)
            {
                texture.m_StreamData.offset = streamData["offset"].AsULong;
                texture.m_StreamData.size = streamData["size"].AsUInt;
                texture.m_StreamData.path = streamData["path"].AsString;
            }

            return texture;
        }

        public void WriteTo(AssetTypeValueField baseField)
        {
            AssetTypeValueField tempField;

            baseField["m_Name"].AsString = m_Name;

            if (!(tempField = baseField["m_ForcedFallbackFormat"]).IsDummy)
                tempField.AsInt = m_ForcedFallbackFormat;

            if (!(tempField = baseField["m_DownscaleFallback"]).IsDummy)
                tempField.AsBool = m_DownscaleFallback;

            baseField["m_Width"].AsInt = m_Width;

            baseField["m_Height"].AsInt = m_Height;

            if (!(tempField = baseField["m_CompleteImageSize"]).IsDummy)
                tempField.AsInt = m_CompleteImageSize;

            baseField["m_TextureFormat"].AsInt = m_TextureFormat;

            if (!(tempField = baseField["m_MipCount"]).IsDummy)
                tempField.AsInt = m_MipCount;

            if (!(tempField = baseField["m_MipMap"]).IsDummy)
                tempField.AsBool = m_MipMap;

            baseField["m_IsReadable"].AsBool = m_IsReadable;

            if (!(tempField = baseField["m_ReadAllowed"]).IsDummy)
                tempField.AsBool = m_ReadAllowed;

            if (!(tempField = baseField["m_StreamingMipmaps"]).IsDummy)
                tempField.AsBool = m_StreamingMipmaps;

            if (!(tempField = baseField["m_StreamingMipmapsPriority"]).IsDummy)
                tempField.AsInt = m_StreamingMipmapsPriority;

            baseField["m_ImageCount"].AsInt = m_ImageCount;

            baseField["m_TextureDimension"].AsInt = m_TextureDimension;

            AssetTypeValueField textureSettings = baseField["m_TextureSettings"];

            textureSettings["m_FilterMode"].AsInt = m_TextureSettings.m_FilterMode;
            textureSettings["m_Aniso"].AsInt = m_TextureSettings.m_Aniso;
            textureSettings["m_MipBias"].AsFloat = m_TextureSettings.m_MipBias;

            if (!(tempField = textureSettings["m_WrapMode"]).IsDummy)
                tempField.AsInt = m_TextureSettings.m_WrapMode;

            if (!(tempField = textureSettings["m_WrapU"]).IsDummy)
                tempField.AsInt = m_TextureSettings.m_WrapU;

            if (!(tempField = textureSettings["m_WrapV"]).IsDummy)
                tempField.AsInt = m_TextureSettings.m_WrapV;

            if (!(tempField = textureSettings["m_WrapW"]).IsDummy)
                tempField.AsInt = m_TextureSettings.m_WrapW;

            if (!(tempField = baseField["m_LightmapFormat"]).IsDummy)
                tempField.AsInt = m_LightmapFormat;

            if (!(tempField = baseField["m_ColorSpace"]).IsDummy)
                tempField.AsInt = m_ColorSpace;

            AssetTypeValueField imageData = baseField["image data"];
            if (imageData.TemplateField.ValueType == AssetValueType.ByteArray)
            {
                imageData.AsByteArray = pictureData;
            }
            else
            {
                imageData.AsArray = new AssetTypeArrayInfo(pictureData.Length);

                List<AssetTypeValueField> children = new List<AssetTypeValueField>(pictureData.Length);
                for (int i = 0; i < pictureData.Length; i++)
                {
                    AssetTypeValueField child = ValueBuilder.DefaultValueFieldFromArrayTemplate(imageData);
                    child.AsByte = pictureData[i];
                    children[i] = child;
                }

                imageData.Children = children;
            }

            AssetTypeValueField streamData;

            if (!(streamData = baseField["m_StreamData"]).IsDummy)
            {
                streamData["offset"].AsULong = m_StreamData.offset;
                streamData["size"].AsUInt = m_StreamData.size;
                streamData["path"].AsString = m_StreamData.path;
            }
        }

        public bool SetPictureDataFromBundle(BundleFileInstance inst)
        {
            StreamingInfo streamInfo = m_StreamData;

            string searchPath = streamInfo.path;

            if (streamInfo.path.StartsWith("archive:/"))
                searchPath = searchPath.Substring(9);

            searchPath = Path.GetFileName(searchPath);

            AssetBundleFile bundle = inst.file;

            AssetsFileReader reader = bundle.DataReader;
            AssetBundleDirectoryInfo[] dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
            bool foundFile = false;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo info = dirInf[i];
                if (info.Name == searchPath)
                {
                    reader.Position = info.Offset + (long)streamInfo.offset;
                    pictureData = reader.ReadBytes((int)streamInfo.size);
                    m_StreamData.offset = 0;
                    m_StreamData.size = 0;
                    m_StreamData.path = "";
                    foundFile = true;
                    break;
                }
            }
            return foundFile;
        }

        public byte[] GetTextureData(AssetsFileInstance inst)
        {
            if (inst.parentBundle != null && m_StreamData.path != string.Empty)
            {
                SetPictureDataFromBundle(inst.parentBundle);
            }
            return GetTextureData(Path.GetDirectoryName(inst.path));
        }

        public byte[] GetTextureData(AssetsFile file)
        {
            string path = null;
            if (file.Reader.BaseStream is FileStream fs)
            {
                path = Path.GetDirectoryName(fs.Name);
            }
            return GetTextureData(path);
        }

        public byte[] GetTextureData(string rootPath, bool useBgra = true)
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
            return DecodeManaged(pictureData, (TextureFormat)m_TextureFormat, m_Width, m_Height, useBgra);
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
                    bundle.Reader.Position = resourceFileOffset + (long)m_StreamData.offset;
                    bundle.Reader.Read(pictureData, 0, pictureData.Length);
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

        public static byte[] DecodeManaged(byte[] data, TextureFormat format, int width, int height, bool useBgra = true)
        {
            byte[] output = Array.Empty<byte>();
            int size = format switch
            {
                TextureFormat.Alpha8 => RgbConverter.Convert<ColorA8, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB4444 => RgbConverter.Convert<ColorARGB16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB24 => RgbConverter.Convert<ColorRGB24, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA32 => RgbConverter.Convert<ColorRGBA32, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB32 => RgbConverter.Convert<ColorARGB32, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R16 => RgbConverter.Convert<ColorR16, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA4444 => RgbConverter.Convert<ColorRGBA16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.BGRA32 => data.Length,
                TextureFormat.RG16 => RgbConverter.Convert<ColorRG16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R8 => RgbConverter.Convert<ColorR8, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RHalf => RgbConverter.Convert<ColorRHalf, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGHalf => RgbConverter.Convert<ColorRGHalf, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAHalf => RgbConverter.Convert<ColorRGBAHalf, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RFloat => RgbConverter.Convert<ColorRSingle, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGFloat => RgbConverter.Convert<ColorRGSingle, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAFloat => RgbConverter.Convert<ColorRGBASingle, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB9e5Float => RgbConverter.Convert<ColorRGB9e5, double, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RG32 => RgbConverter.Convert<ColorRG32, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB48 => RgbConverter.Convert<ColorRGB48, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA64 => RgbConverter.Convert<ColorRGBA64, ushort, ColorBGRA32, byte>(data, width, height, out output),

                TextureFormat.DXT1 => DxtDecoder.DecompressDXT1(data, width, height, out output),
                TextureFormat.DXT3 => DxtDecoder.DecompressDXT3(data, width, height, out output),
                TextureFormat.DXT5 => DxtDecoder.DecompressDXT5(data, width, height, out output),
                TextureFormat.BC4 => BcDecoder.DecompressBC4(data, width, height, out output),
                TextureFormat.BC5 => BcDecoder.DecompressBC5(data, width, height, out output),
                TextureFormat.BC6H => BcDecoder.DecompressBC6H(data, width, height, false, out output),
                TextureFormat.BC7 => BcDecoder.DecompressBC7(data, width, height, out output),

                TextureFormat.ETC_RGB4 => EtcDecoder.DecompressETC(data, width, height, out output),
                TextureFormat.ETC2_RGB4 => EtcDecoder.DecompressETC2(data, width, height, out output),
                TextureFormat.ETC2_RGBA1 => EtcDecoder.DecompressETC2A1(data, width, height, out output),
                TextureFormat.ETC2_RGBA8 => EtcDecoder.DecompressETC2A8(data, width, height, out output),
                TextureFormat.EAC_R => EtcDecoder.DecompressEACRUnsigned(data, width, height, out output),
                TextureFormat.EAC_R_SIGNED => EtcDecoder.DecompressEACRSigned(data, width, height, out output),
                TextureFormat.EAC_RG => EtcDecoder.DecompressEACRGUnsigned(data, width, height, out output),
                TextureFormat.EAC_RG_SIGNED => EtcDecoder.DecompressEACRGSigned(data, width, height, out output),

                TextureFormat.ASTC_RGB_4x4 or
                TextureFormat.ASTC_RGBA_4x4 => AstcDecoder.DecodeASTC(data, width, height, 4, 4, out output),
                TextureFormat.ASTC_RGB_5x5 or
                TextureFormat.ASTC_RGBA_5x5 => AstcDecoder.DecodeASTC(data, width, height, 5, 5, out output),
                TextureFormat.ASTC_RGB_6x6 or
                TextureFormat.ASTC_RGBA_6x6 => AstcDecoder.DecodeASTC(data, width, height, 6, 6, out output),
                TextureFormat.ASTC_RGB_8x8 or
                TextureFormat.ASTC_RGBA_8x8 => AstcDecoder.DecodeASTC(data, width, height, 8, 8, out output),
                TextureFormat.ASTC_RGB_10x10 or
                TextureFormat.ASTC_RGBA_10x10 => AstcDecoder.DecodeASTC(data, width, height, 10, 10, out output),
                TextureFormat.ASTC_RGB_12x12 or
                TextureFormat.ASTC_RGBA_12x12 => AstcDecoder.DecodeASTC(data, width, height, 12, 12, out output),

                TextureFormat.ATC_RGB4 => AtcDecoder.DecompressAtcRgb4(data, width, height, out output),
                TextureFormat.ATC_RGBA8 => AtcDecoder.DecompressAtcRgba8(data, width, height, out output),

                TextureFormat.PVRTC_RGB2 or
                TextureFormat.PVRTC_RGBA2 => PvrtcDecoder.DecompressPVRTC(data, width, height, true, out output),
                TextureFormat.PVRTC_RGB4 or
                TextureFormat.PVRTC_RGBA4 => PvrtcDecoder.DecompressPVRTC(data, width, height, false, out output),

                TextureFormat.YUY2 => Yuy2Decoder.DecompressYUY2(data, width, height, out output),
                
                _ => 0
            };
            
            if (size == 0)
                return null;

            return output;
        }

        public static byte[] Encode(byte[] data, TextureFormat format, int width, int height)
        {
            return format switch
            {
                TextureFormat.R8 => RGBAEncoders.EncodeR8(data, width, height),
                TextureFormat.R16 => RGBAEncoders.EncodeR16(data, width, height),
                TextureFormat.RG16 => RGBAEncoders.EncodeRG16(data, width, height),
                TextureFormat.RGB24 => RGBAEncoders.EncodeRGB24(data, width, height),
                TextureFormat.RGB565 => RGBAEncoders.EncodeRGB565(data, width, height),
                TextureFormat.RGBA32 => RGBAEncoders.EncodeRGBA32(data, width, height),
                TextureFormat.ARGB32 => RGBAEncoders.EncodeARGB32(data, width, height),
                TextureFormat.RGBA4444 => RGBAEncoders.EncodeRGBA4444(data, width, height),
                TextureFormat.ARGB4444 => RGBAEncoders.EncodeARGB4444(data, width, height),
                TextureFormat.Alpha8 => RGBAEncoders.EncodeAlpha8(data, width, height),
                TextureFormat.RHalf => RGBAEncoders.EncodeRHalf(data, width, height),
                TextureFormat.RGHalf => RGBAEncoders.EncodeRGHalf(data, width, height),
                TextureFormat.RGBAHalf => RGBAEncoders.EncodeRGBAHalf(data, width, height),
                _ => null
            };
        }
    }
}
