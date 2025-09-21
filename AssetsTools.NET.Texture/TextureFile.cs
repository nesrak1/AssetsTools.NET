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
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using StbReadColorComponents = StbImageSharp.ColorComponents;

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
        public byte[] m_PlatformBlob;

        // not assigned by default since we can't gurarantee
        // the swizzle type yet (but may in the future)
        public SwizzleType swizzleType = SwizzleType.None;

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

            AssetTypeValueField platformBlob = baseField["m_PlatformBlob.Array"];
            if (!platformBlob.IsDummy)
            {
                if (platformBlob.TemplateField.ValueType == AssetValueType.ByteArray)
                {
                    texture.m_PlatformBlob = platformBlob.AsByteArray;
                }
                else
                {
                    int platformBlobSize = platformBlob.Children.Count;
                    texture.m_PlatformBlob = new byte[platformBlobSize];
                    for (int i = 0; i < platformBlobSize; i++)
                    {
                        texture.m_PlatformBlob[i] = (byte)platformBlob[i].AsInt;
                    }
                }
            }
            else
            {
                texture.m_PlatformBlob = Array.Empty<byte>();
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

            AssetTypeValueField platformBlob = baseField["m_PlatformBlob.Array"];
            if (!platformBlob.IsDummy)
            {
                if (platformBlob.TemplateField.ValueType == AssetValueType.ByteArray)
                {
                    platformBlob.AsByteArray = m_PlatformBlob;
                }
                else
                {
                    platformBlob.AsArray = new AssetTypeArrayInfo(m_PlatformBlob.Length);

                    List<AssetTypeValueField> children = new List<AssetTypeValueField>(m_PlatformBlob.Length);
                    for (int i = 0; i < m_PlatformBlob.Length; i++)
                    {
                        AssetTypeValueField child = ValueBuilder.DefaultValueFieldFromArrayTemplate(platformBlob);
                        child.AsByte = m_PlatformBlob[i];
                        children[i] = child;
                    }

                    platformBlob.Children = children;
                }
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
            AssetBundleDirectoryInfo info = BundleHelper.GetDirInfo(inst.file, searchPath);
            if (info == null)
            {
                return false;
            }

            reader.Position = info.Offset + (long)streamInfo.offset;
            pictureData = reader.ReadBytes((int)streamInfo.size);
            m_StreamData.offset = 0;
            m_StreamData.size = 0;
            m_StreamData.path = "";

            return true;
        }

        public byte[] FillPictureData(AssetsFileInstance inst)
        {
            if (inst.parentBundle != null && m_StreamData.path != string.Empty && (pictureData == null || pictureData.Length == 0))
            {
                SetPictureDataFromBundle(inst.parentBundle);
            }

            string rootPath = Path.GetDirectoryName(inst.path);
            return FillPictureData(rootPath);
        }

        public byte[] FillPictureData(AssetsFile file)
        {
            if (file.Reader.BaseStream is FileStream fs)
            {
                return FillPictureData(Path.GetDirectoryName(fs.Name));
            }

            return pictureData;
        }

        public byte[] FillPictureData(string rootPath)
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

            return pictureData;
        }

        public ISwizzler GetSwizzler()
        {
            if (swizzleType == SwizzleType.Switch)
            {
                if (SwitchSwizzle.IsSwitchSwizzled(m_PlatformBlob, swizzleType))
                {
                    return new SwitchSwizzle(this);
                }
            }

            return null;
        }

        public byte[] DecodeTextureRaw(byte[] textureData, bool useBgra = true)
        {
            ISwizzler swizzler = GetSwizzler();
            return DecodeManagedData(textureData, (TextureFormat)m_TextureFormat, m_Width, m_Height, useBgra, swizzler);
        }

        public bool DecodeTextureImage(byte[] textureData, Stream outputStream, ImageExportType exportType, int quality = 90)
        {
            ISwizzler swizzler = GetSwizzler();
            return DecodeManagedImage(
                textureData, (TextureFormat)m_TextureFormat, m_Width, m_Height,
                outputStream, exportType, quality, swizzler);
        }

        public bool DecodeTextureImage(byte[] textureData, string outputPath, ImageExportType exportType, int quality = 90)
        {
            using FileStream fs = File.OpenWrite(outputPath);
            ISwizzler swizzler = GetSwizzler();
            return DecodeManagedImage(
                textureData, (TextureFormat)m_TextureFormat, m_Width, m_Height,
                fs, exportType, quality, swizzler);
        }

        public void SetPictureData(byte[] encodedData, int width, int height)
        {
            m_Width = width;
            m_Height = height;
            m_StreamData.path = "";
            m_StreamData.offset = 0;
            m_StreamData.size = 0;
            pictureData = encodedData;
            m_CompleteImageSize = encodedData.Length;
        }

        public void EncodeTextureRaw(byte[] textureData, int width, int height, int quality = 3, bool useBgra = true)
        {
            byte[] encoded = EncodeNativeData(textureData, (TextureFormat)m_TextureFormat, width, height, quality, useBgra);
            encoded ??= EncodeManagedData(textureData, (TextureFormat)m_TextureFormat, width, height, useBgra);
            if (encoded == null)
                throw new NotSupportedException("The current texture format is not supported for encoding.");

            SetPictureData(encoded, width, height);
        }

        public void EncodeTextureImage(Stream stream, int quality = 3)
        {
            int width, height;
            byte[] encoded = EncodeNativeImage(stream, (TextureFormat)m_TextureFormat, out width, out height, quality);
            encoded ??= EncodeManagedImage(stream, (TextureFormat)m_TextureFormat, out width, out height);
            if (encoded == null)
                throw new NotSupportedException("The current texture format is not supported for encoding.");

            SetPictureData(encoded, width, height);
        }

        public void EncodeTextureImage(string path, int quality = 3)
        {
            int width, height;
            byte[] encoded = EncodeNativeImage(path, (TextureFormat)m_TextureFormat, out width, out height, quality);
            encoded ??= EncodeManagedImage(path, (TextureFormat)m_TextureFormat, out width, out height);
            if (encoded == null)
                throw new NotSupportedException("The current texture format is not supported for encoding.");

            SetPictureData(encoded, width, height);
        }

        public static byte[] DecodeManagedData(
            byte[] data, TextureFormat format, int width, int height, bool useBgra = true, ISwizzler swizzler = null)
        {
            int originalWidth = width;
            int originalHeight = height;
            if (swizzler != null)
                data = swizzler.PreprocessDeswizzle(data, out format, out width, out height);

            if ((useBgra && (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)) || (!useBgra && format == TextureFormat.RGBA32))
            {
                byte[] newData = new byte[width * height * 4];
                Array.Copy(data, newData, width * height * 4);
                return newData;
            }

            byte[] output = Array.Empty<byte>();
            int size = format switch
            {
                // different versions of .net == different versions of texture decoder == different type names :/
#if NET7_0_OR_GREATER
                TextureFormat.Alpha8 => RgbConverter.Convert<ColorA<byte>, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB4444 => RgbConverter.Convert<ColorARGB16, byte, ColorBGRA32, byte>(data, width, height, out output),
                //TextureFormat.ARGBFloat => not supported :(
                TextureFormat.RGB24 => RgbConverter.Convert<ColorRGB<byte>, byte, ColorBGRA32, byte>(data, width, height, out output),
                //TextureFormat.BGR24 => not supported :(
                TextureFormat.RGBA32 => RgbConverter.Convert<ColorRGBA<byte>, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB565 => RgbConverter.Convert<ColorRGB16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB32 => RgbConverter.Convert<ColorARGB32, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R16 => RgbConverter.Convert<ColorR<ushort>, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA4444 => RgbConverter.Convert<ColorRGBA16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.BGRA32 or TextureFormat.BGRA32Old => data.Length,
                TextureFormat.RG16 => RgbConverter.Convert<ColorRG<byte>, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R8 => RgbConverter.Convert<ColorR<byte>, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RHalf => RgbConverter.Convert<ColorR<Half>, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGHalf => RgbConverter.Convert<ColorRG<Half>, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAHalf => RgbConverter.Convert<ColorRGBA<Half>, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RFloat => RgbConverter.Convert<ColorR<float>, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGFloat => RgbConverter.Convert<ColorRG<float>, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBFloat => RgbConverter.Convert<ColorRGB<float>, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAFloat => RgbConverter.Convert<ColorRGBA<float>, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB9e5Float => RgbConverter.Convert<ColorRGB9e5, double, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RG32 => RgbConverter.Convert<ColorRG<ushort>, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB48 => RgbConverter.Convert<ColorRGB<ushort>, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA64 => RgbConverter.Convert<ColorRGBA<ushort>, ushort, ColorBGRA32, byte>(data, width, height, out output),
#else
                TextureFormat.Alpha8 => RgbConverter.Convert<ColorA8, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB4444 => RgbConverter.Convert<ColorARGB16, byte, ColorBGRA32, byte>(data, width, height, out output),
                //TextureFormat.ARGBFloat => not supported :(
                TextureFormat.RGB24 => RgbConverter.Convert<ColorRGB24, byte, ColorBGRA32, byte>(data, width, height, out output),
                //TextureFormat.BGR24 => not supported :(
                TextureFormat.RGBA32 => RgbConverter.Convert<ColorRGBA32, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB565 => RgbConverter.Convert<ColorRGB16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.ARGB32 => RgbConverter.Convert<ColorARGB32, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R16 => RgbConverter.Convert<ColorR16, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA4444 => RgbConverter.Convert<ColorRGBA16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.BGRA32 or TextureFormat.BGRA32Old => data.Length,
                TextureFormat.RG16 => RgbConverter.Convert<ColorRG16, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.R8 => RgbConverter.Convert<ColorR8, byte, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RHalf => RgbConverter.Convert<ColorR16Half, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGHalf => RgbConverter.Convert<ColorRG32Half, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAHalf => RgbConverter.Convert<ColorRGBA64Half, Half, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RFloat => RgbConverter.Convert<ColorR32Single, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGFloat => RgbConverter.Convert<ColorRG64Single, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBAFloat => RgbConverter.Convert<ColorRGBA128Single, float, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB9e5Float => RgbConverter.Convert<ColorRGB9e5, double, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RG32 => RgbConverter.Convert<ColorRG32, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGB48 => RgbConverter.Convert<ColorRGB48, ushort, ColorBGRA32, byte>(data, width, height, out output),
                TextureFormat.RGBA64 => RgbConverter.Convert<ColorRGBA64, ushort, ColorBGRA32, byte>(data, width, height, out output),
#endif

                TextureFormat.DXT1 => DxtDecoder.DecompressDXT1(data, width, height, out output),
                TextureFormat.DXT3 => DxtDecoder.DecompressDXT3(data, width, height, out output),
                TextureFormat.DXT5 => DxtDecoder.DecompressDXT5(data, width, height, out output),
                TextureFormat.BC4 => Bc4.Decompress(data, width, height, out output),
                TextureFormat.BC5 => Bc5.Decompress(data, width, height, out output),
                TextureFormat.BC6H => Bc6h.Decompress(data, width, height, false, out output),
                TextureFormat.BC7 => Bc7.Decompress(data, width, height, out output),

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

                TextureFormat.DXT1Crunched => CrunchDecoder.Decompress(data, width, height, TextureFormat.DXT1Crunched, out output),
                TextureFormat.DXT5Crunched => CrunchDecoder.Decompress(data, width, height, TextureFormat.DXT5Crunched, out output),
                TextureFormat.ETC_RGB4Crunched => CrunchDecoder.Decompress(data, width, height, TextureFormat.ETC_RGB4Crunched, out output),

                _ => 0
            };

            if (size == 0)
                return null;

            if (swizzler != null)
                output = swizzler.PostprocessDeswizzle(output);

            if (!useBgra)
                TextureOperations.SwapRBComponents(output);

            return output;
        }

        public static bool DecodeManagedImage(
            byte[] data, TextureFormat format, int width, int height,
            Stream outputStream, ImageExportType exportType, int quality = 90, ISwizzler swizzler = null)
        {
            byte[] textureData = DecodeManagedData(data, format, width, height, false, swizzler);
            if (textureData == null)
            {
                return false;
            }

            TextureOperations.FlipBGRA32Vertically(textureData, width, height);
            return TextureOperations.WriteRawImage(textureData, width, height, outputStream, exportType, quality);
        }

        public static bool DecodeManagedImage(
            byte[] data, TextureFormat format, int width, int height,
            string outputPath, ImageExportType exportType, int quality = 100)
        {
            using FileStream fs = File.OpenWrite(outputPath);
            return DecodeManagedImage(
                data, format, width, height,
                fs, exportType, quality);
        }

        public static byte[] EncodeManagedData(byte[] data, TextureFormat format, int width, int height, bool useBgra = true)
        {
            if (useBgra)
            {
                if (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)
                    return data;
            }
            else
            {
                if (format == TextureFormat.RGBA32)
                    return data;

                TextureOperations.SwapRBComponents(data);

                if (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)
                    return data;
            }

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

        public static byte[] EncodeManagedImage(Stream stream, TextureFormat format, out int width, out int height)
        {
            ImageResult imageResult = ImageResult.FromStream(stream, StbReadColorComponents.RedGreenBlueAlpha);
            width = imageResult.Width;
            height = imageResult.Height;
            TextureOperations.FlipBGRA32Vertically(imageResult.Data, width, height);
            return EncodeManagedData(imageResult.Data, format, width, height, false);
        }

        public static byte[] EncodeManagedImage(string path, TextureFormat format, out int width, out int height)
        {
            using FileStream fs = File.OpenRead(path);
            return EncodeManagedImage(fs, format, out width, out height);
        }

        public static byte[] EncodeNativeData(byte[] data, TextureFormat format, int width, int height, int quality = 3, bool useBgra = true)
        {
            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            if (useBgra)
            {
                if (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)
                    return data;
            }
            else
            {
                if (format == TextureFormat.RGBA32)
                    return data;

                TextureOperations.SwapRBComponents(data);

                if (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)
                    return data;
            }

            return TextureEncoderWrapper.ConvertImage(data, format, width, height, quality);
        }

        public static byte[] EncodeNativeImage(Stream stream, TextureFormat format, out int width, out int height, int quality = 3)
        {
            width = height = 0;

            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            if (stream is FileStream fs)
            {
                return TextureEncoderWrapper.ConvertImage(fs.Name, format, out width, out height, quality);
            }
            else
            {
                // ok, this is pretty silly. cuttlefish is currently only setup to
                // read from file as a png/jpg/etc or from a pointer to raw data.
                // we don't have any way to pass a png/jpg/etc as bytes.
                // to handle this for now, we read the whole image into bytes and
                // pass it to stb to convert to rgba32.

                using MemoryStream tmpMem = new MemoryStream();
                stream.CopyTo(tmpMem);

                ImageResult imageResult = ImageResult.FromStream(stream, StbReadColorComponents.RedGreenBlueAlpha);
                width = imageResult.Width;
                height = imageResult.Height;

                return EncodeNativeData(tmpMem.ToArray(), format, width, height, quality, false);
            }
        }

        public static byte[] EncodeNativeImage(string path, TextureFormat format, out int width, out int height, int quality = 3)
        {
            width = height = 0;

            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            return TextureEncoderWrapper.ConvertImage(path, format, out width, out height, quality);
        }
    }
}
