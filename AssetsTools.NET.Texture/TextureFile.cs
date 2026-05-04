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
        private int originalWidth; // todo: remove this, for testing
        private int originalHeight; // ...

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

        #region Asset reader/writer

        /// <summary>
        /// Parse <see cref="AssetTypeValueField"/> to a <see cref="TextureFile"/>.
        /// </summary>
        /// <param name="baseField">The base field to read from.</param>
        /// <returns>A parsed texture file.</returns>
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
            texture.originalWidth = texture.m_Width;

            texture.m_Height = baseField["m_Height"].AsInt;
            texture.originalHeight = texture.m_Height;

            if (!(tempField = baseField["m_CompleteImageSize"]).IsDummy)
                texture.m_CompleteImageSize = tempField.AsInt;

            texture.m_TextureFormat = baseField["m_TextureFormat"].AsInt;

            if (!(tempField = baseField["m_MipCount"]).IsDummy)
            {
                texture.m_MipCount = tempField.AsInt;
                texture.m_MipMap = texture.m_MipCount > 1;
            }

            if (!(tempField = baseField["m_MipMap"]).IsDummy)
            {
                texture.m_MipMap = tempField.AsBool;
                texture.m_MipCount = texture.m_MipMap
                    ? TextureOperations.GetMaxMipCount(texture.m_Width, texture.m_Height)
                    : 1;
            }

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

        /// <summary>
        /// Write a parsed <see cref="TextureFile"/> to a <see cref="AssetTypeValueField"/>.
        /// </summary>
        /// <param name="baseField">The base field to write to.</param>
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
                    children.Add(child);
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
                        children.Add(child);
                    }

                    platformBlob.Children = children;
                }
            }
        }

        #endregion

        #region PictureData/StreamData

        /// <summary>
        /// Loads .resS texture data from a bundle into the <see cref="pictureData"/> field.
        /// </summary>
        /// <param name="inst">The bundle file instance to use.</param>
        /// <returns>True if the .resS file was found for this texture.</returns>
        public bool FillPictureDataFromBundle(BundleFileInstance inst)
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

        /// <summary>
        /// Loads the .resS texture data from disk at a given path. If
        /// <see cref="pictureData"/> is already set, this will just return the
        /// <see cref="pictureData"/> as is.
        /// </summary>
        /// <param name="rootPath">The base path to load from (i.e., _Data folder with .resS files).</param>
        /// <returns>The read encoded texture bytes, or null if they couldn't be read.</returns>
        public byte[] FillPictureDataFromFile(string rootPath)
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
                    using FileStream stream = File.OpenRead(fixedStreamPath);
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

        /// <summary>
        /// Loads the .resS texture data from either disk or bundle, depending on if
        /// this assets file is in a bundle or on disk. If <see cref="pictureData"/>
        /// is already set, this will just return <see cref="pictureData"/> as is.
        /// </summary>
        /// <param name="inst">The assets file instance to use.</param>
        /// <returns>The read encoded texture bytes, or null if they couldn't be read.</returns>
        public byte[] FillPictureData(AssetsFileInstance inst)
        {
            if (inst.parentBundle != null && m_StreamData.path != string.Empty && (pictureData == null || pictureData.Length == 0))
            {
                if (FillPictureDataFromBundle(inst.parentBundle))
                    return pictureData;
            }

            string rootPath = Path.GetDirectoryName(inst.path);
            return FillPictureDataFromFile(rootPath);
        }

        /// <summary>
        /// Loads the .resS texture data from disk, only if this assets file is a file
        /// on disk opened with a file stream. If <see cref="pictureData"/> is already
        /// set, this will just return <see cref="pictureData"/> as is.
        /// </summary>
        /// <param name="file">The assets file to use.</param>
        /// <returns>The read encoded texture bytes, or null if they couldn't be read.</returns>
        public byte[] FillPictureData(AssetsFile file)
        {
            if (file.Reader.BaseStream is FileStream fs)
            {
                return FillPictureDataFromFile(Path.GetDirectoryName(fs.Name));
            }

            return pictureData;
        }

        /// <summary>
        /// Set the texture's picture data. This removes a reference to the
        /// .resS file, if one is set. This method is not capable of editing
        /// or removing the .resS to remove the referenced texture. Note:
        /// if your image is not encoded, use the following methods:
        /// <see cref="EncodeTextureImage(Stream, int, int)"/>,
        /// <see cref="EncodeTextureImage(string, int, int)"/>, or
        /// <see cref="EncodeTextureRaw(byte[], int, int, int, int, bool)"/>.
        /// </summary>
        /// <seealso cref=""/>
        /// <param name="encodedData">The encoded texture data to use.</param>
        /// <param name="width">The width of the new image, not including padding.</param>
        /// <param name="height">The height of the new image, not including padding.</param>
        /// <param name="textureFormat">The texeture format of the encoded texture data, or 0 to not change the format.</param>
        /// <param name="mipCount">The number of mips encoded in the encoded texture data.</param>
        public void SetPictureData(byte[] encodedData, int width, int height, TextureFormat textureFormat = 0, int mipCount = 1)
        {
            if (textureFormat != 0)
            {
                m_TextureFormat = (int)textureFormat;
            }

            // pad width and height based on texture format.
            // native encoder pads to the correct size but does not
            // report the padded size. here we calculate it again and
            // set the correct width and height.
            var paddedSize = TextureOperations.GetPaddedTextureSize(textureFormat, width, height);
            m_Width = paddedSize.Width;
            m_Height = paddedSize.Height;

            m_StreamData.path = "";
            m_StreamData.offset = 0;
            m_StreamData.size = 0;
            pictureData = encodedData;
            m_CompleteImageSize = encodedData.Length;

            m_MipCount = mipCount;
            m_MipMap = mipCount > 1;
        }

        /// <summary>
        /// Store new encoded mips into this texture's fields.
        /// </summary>
        private void FinalizeEncodedData(byte[][] mips, int width, int height, TextureFormat format)
        {
            // CreateSwizzler may change the texture format here, but this is probably
            // fine because the buggy texture format and the actual texture format should
            // both be valid for decoding this texture.
            ISwizzler swizzler = CreateSwizzler(width, height, ref format);
            byte[] flatData;
            if (swizzler != null && swizzler.CanBeSwizzled())
            {
                // swizzler will flatten mips to single array
                flatData = swizzler.ProcessSwizzle(mips, out int[] mipOffsets);
                m_PlatformBlob = swizzler.MakePlatformBlob(mipOffsets, (uint)flatData.Length);
            }
            else
            {
                // no swizzler so we will flatten mips ourselves
                // discard mip offsets because it's only used for swizzling
                flatData = TextureOperations.FlattenMips(mips, out _);
            }

            SetPictureData(flatData, width, height, format, mips.Length);
        }

        #endregion

        #region Swizzlers

        /// <summary>
        /// Creates an ISwizzler for deswizzling.
        /// </summary>
        private ISwizzler CreateDeswizzler(ref TextureFormat format)
        {
            // we are always assuming that we want to swizzle if the type is set
            if (swizzleType == SwizzleType.Switch)
            {
                // ignoring padded size
                var swizzle = new SwitchSwizzle(m_PlatformBlob, m_Width, m_Height, ref format, out _, out _);
                if (!swizzle.CanBeSwizzled())
                    return null;

                return swizzle;
            }

            return null;
        }

        /// <summary>
        /// Creates an ISwizzler for swizzling.
        /// </summary>
        private ISwizzler CreateSwizzler(int width, int height, ref TextureFormat format)
        {
            // we are always assuming that we want to swizzle if the type is set
            if (swizzleType == SwizzleType.Switch)
            {
                // ignoring padded size
                var swizzle = new SwitchSwizzle(width, height, ref format, out _, out _);
                if (!swizzle.CanBeSwizzled())
                    return null;

                return swizzle;
            }

            return null;
        }

        #endregion

        #region Public decoders

        /// <summary>
        /// Decode an encoded texture to raw BGRA32 or RGBA32 data.
        /// </summary>
        /// <param name="textureData">The raw texture data to use (see <see cref="FillPictureData(AssetsFileInstance)"/>).</param>
        /// <param name="useBgra">If true, output BGRA32. If false, output RGBA32.</param>
        /// <returns>The decoded texture data, or null if the texture did not read successfully.</returns>
        public byte[] DecodeTextureRaw(byte[] textureData, bool useBgra = true)
        {
            TextureFormat format = (TextureFormat)m_TextureFormat;
            ISwizzler swizzler = CreateDeswizzler(ref format);
            return DecodeManagedData(textureData, format, m_Width, m_Height, useBgra, swizzler);
        }

        /// <summary>
        /// Decode an encoded texture to an image (e.g., .png).
        /// </summary>
        /// <param name="textureData">The raw texture data to use (see <see cref="FillPictureData(AssetsFileInstance)"/>).</param>
        /// <param name="outputStream">The stream to write the output image to.</param>
        /// <param name="exportType">Which image format to use.</param>
        /// <param name="quality">The quality to use when using the JPG output format.</param>
        /// <returns>True if the texture read successfully.</returns>
        public bool DecodeTextureImage(byte[] textureData, Stream outputStream, ImageExportType exportType, int quality = 90)
        {
            TextureFormat format = (TextureFormat)m_TextureFormat;
            ISwizzler swizzler = CreateDeswizzler(ref format);
            return DecodeManagedImage(
                textureData, format, m_Width, m_Height,
                outputStream, exportType, quality, swizzler);
        }

        /// <summary>
        /// Decode an encoded texture to an image (e.g., .png).
        /// </summary>
        /// <param name="textureData">The raw texture data to use (see <see cref="FillPictureData(AssetsFileInstance)"/>).</param>
        /// <param name="outputPath">The file path to write the output image to.</param>
        /// <param name="exportType">Which image format to use.</param>
        /// <param name="quality">The quality to use when using the JPG output format.</param>
        /// <returns>True if the texture read successfully.</returns>
        public bool DecodeTextureImage(byte[] textureData, string outputPath, ImageExportType exportType, int quality = 90)
        {
            using FileStream fs = File.OpenWrite(outputPath);
            return DecodeTextureImage(textureData, fs, exportType, quality);
        }

        #endregion

        #region Public encoders

        private static void ClampMipCount(int width, int height, ref int mipCount)
        {
            var maxMipCount = TextureOperations.GetMaxMipCount(width, height);
            if (mipCount > maxMipCount)
                mipCount = maxMipCount;
        }

        /// <summary>
        /// Encode a raw RGBA32/BGRA32 image and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set. The
        /// image will be encoded with the format set by <see cref="m_TextureFormat"/>.
        /// </summary>
        /// <param name="textureData">The raw RGBA32/BGRA32 data to use.</param>
        /// <param name="width">The width of the new image.</param>
        /// <param name="height">The height of the new image.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <param name="useBgra">Should <paramref name="textureData"/> be read as BGRA32 instead of RGBA32?</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureRaw(byte[] textureData, int width, int height, int mipCount = 1, int quality = 3, bool useBgra = true)
        {
            TextureFormat format = (TextureFormat)m_TextureFormat;
            EncodeTextureRaw(textureData, width, height, format, mipCount, quality, useBgra);
        }

        /// <summary>
        /// Encode a raw RGBA32/BGRA32 image and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set.
        /// </summary>
        /// <param name="textureData">The raw RGBA32/BGRA32 data to use.</param>
        /// <param name="width">The width of the new image.</param>
        /// <param name="height">The height of the new image.</param>
        /// <param name="format">The texture format to encode with.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <param name="useBgra">Should <paramref name="textureData"/> be read as BGRA32 instead of RGBA32?</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureRaw(byte[] textureData, int width, int height, TextureFormat format, int mipCount = 1, int quality = 3, bool useBgra = true)
        {
            // try with native encoder
            byte[][] mips = EncodeNativeData(textureData, format, width, height, mipCount, quality, useBgra);
            if (mips != null)
            {
                FinalizeEncodedData(mips, width, height, format);
                return;
            }

            // fallback to managed encoder, limited to one mip
            mips = [EncodeManagedData(textureData, format, width, height, useBgra)];
            if (mips[0] == null)
            {
                throw new NotSupportedException("The current texture format is not supported for encoding.");
            }

            FinalizeEncodedData(mips, width, height, format);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set. The
        /// image will be encoded with the format set by <see cref="m_TextureFormat"/>.
        /// </summary>
        /// <param name="stream">The stream of an image file.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureImage(Stream stream, int mipCount = 1, int quality = 3)
        {
            TextureFormat format = (TextureFormat)m_TextureFormat;
            EncodeTextureImage(stream, format, mipCount, quality);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set.
        /// </summary>
        /// <param name="stream">The stream of an image file.</param>
        /// <param name="format">The texture format to encode with.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureImage(Stream stream, TextureFormat format, int mipCount = 1, int quality = 3)
        {
            int width, height;

            // try with native encoder
            byte[][] mips = EncodeNativeImage(stream, format, out width, out height, mipCount, quality);
            if (mips != null)
            {
                FinalizeEncodedData(mips, width, height, format);
                return;
            }

            // fallback to managed encoder, limited to one mip
            mips = [EncodeManagedImage(stream, format, out width, out height)];
            if (mips[0] == null)
            {
                throw new NotSupportedException("The current texture format is not supported for encoding.");
            }

            FinalizeEncodedData(mips, width, height, format);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set. The
        /// image will be encoded with the format set by <see cref="m_TextureFormat"/>.
        /// </summary>
        /// <param name="path">The path to an image file.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureImage(string path, int mipCount = 1, int quality = 3)
        {
            TextureFormat format = (TextureFormat)m_TextureFormat;
            EncodeTextureImage(path, format, mipCount, quality);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) and set the texture's picture
        /// data. Like <see cref="SetPictureData(byte[], int, int, TextureFormat, int)"/>,
        /// this removes a reference to the .resS file, if one is set.
        /// </summary>
        /// <param name="path">The path to an image file.</param>
        /// <param name="format">The texture format to encode with.</param>
        /// <param name="mipCount">The amount of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <exception cref="NotSupportedException">Thrown if the texture format is not supported for encoding.</exception>
        public void EncodeTextureImage(string path, TextureFormat format, int mipCount = 1, int quality = 3)
        {
            int width, height;

            // try with native encoder
            byte[][] mips = EncodeNativeImage(path, format, out width, out height, mipCount, quality);
            if (mips != null)
            {
                FinalizeEncodedData(mips, width, height, format);
                return;
            }

            // fallback to managed encoder, limited to one mip
            mips = [EncodeManagedImage(path, format, out width, out height)];
            if (mips[0] == null)
            {
                throw new NotSupportedException("The current texture format is not supported for encoding.");
            }

            FinalizeEncodedData(mips, width, height, format);
        }

        #endregion

        #region Advanced use decoder/encoder methods

        /// <summary>
        /// Decode raw image data using the managed decoder (AssetRipper.TextureDecoder)
        /// into raw data. If you want to decode a TextureFile's texture to an image file,
        /// use <see cref="DecodeManagedImage"/>.
        /// </summary>
        /// <param name="data">The raw encoded texture data to decode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="useBgra">Should the output be written as BGRA32 instead of RGBA32?</param>
        /// <param name="swizzler">The swizzler to use, if necessary for the platform.</param>
        /// <returns>The decoded texture data.</returns>
        public static byte[] DecodeManagedData(
            byte[] data, TextureFormat format, int width, int height,
            bool useBgra = true, ISwizzler swizzler = null)
        {
            if (swizzler != null)
            {
                data = swizzler.PreprocessDeswizzle(data, out width, out height);
            }

            byte[] output;
            if ((useBgra && (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old))
                || (!useBgra && format == TextureFormat.RGBA32))
            {
                output = new byte[width * height * 4];
                Array.Copy(data, output, width * height * 4);
            }
            else
            {
                /*
                var blockSize = TextureOperations.GetBlockSize(format);
                var needsPadding = (width % blockSize.Width) != 0 || (height % blockSize.Height) != 0;
                int croppedWidth = -1, croppedHeight = -1;
                if (needsPadding)
                {
                    croppedWidth = width;
                    croppedHeight = height;
                    width = (width + (blockSize.Width - 1)) / blockSize.Width * blockSize.Width;
                    height = (height + (blockSize.Height - 1)) / blockSize.Height * blockSize.Height;
                }
                */

                output = [];
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
                    TextureFormat.RGBFloat => RgbConverter.Convert<ColorRGB96Single, float, ColorBGRA32, byte>(data, width, height, out output),
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
                {
                    // something went wrong. just return null.
                    return null;
                }
            }

            if (swizzler != null)
                output = swizzler.PostprocessDeswizzle(output);
            if (!useBgra)
                TextureOperations.SwapRBComponentsInplace(output);

            return output;
        }

        /// <summary>
        /// Decode raw image data using the managed decoder (AssetRipper.TextureDecoder)
        /// into an image file (e.g., .png).
        /// </summary>
        /// <param name="data">The raw encoded texture data to decode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="outputStream">The stream to output the image to.</param>
        /// <param name="exportType">The export file format to use.</param>
        /// <param name="quality">The quality of the image, if JPG is chosen.</param>
        /// <param name="swizzler">The swizzler to use, if necessary for the platform.</param>
        /// <returns>The decoded texture as an image file.</returns>
        public static bool DecodeManagedImage(
            byte[] data, TextureFormat format, int width, int height,
            Stream outputStream, ImageExportType exportType,
            int quality = 90, ISwizzler swizzler = null)
        {
            byte[] textureData = DecodeManagedData(data, format, width, height, false, swizzler);
            if (textureData == null)
                return false;

            TextureOperations.FlipBGRA32VerticallyInplace(textureData, width, height);
            return TextureOperations.WriteRawImage(textureData, width, height, outputStream, exportType, quality);
        }

        /// <summary>
        /// Decode raw image data using the managed decoder (AssetRipper.TextureDecoder)
        /// into an image file (e.g., .png).
        /// </summary>
        /// <param name="data">The raw encoded texture data to decode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="outputPath">The file path to output the image to.</param>
        /// <param name="exportType">The export file format to use.</param>
        /// <param name="quality">The quality of the image, if JPG is chosen.</param>
        /// <param name="swizzler">The swizzler to use, if necessary for the platform.</param>
        /// <returns>The decoded texture as an image file.</returns>
        public static bool DecodeManagedImage(
            byte[] data, TextureFormat format, int width, int height,
            string outputPath, ImageExportType exportType,
            int quality = 90, ISwizzler swizzler = null)
        {
            using FileStream fs = File.OpenWrite(outputPath);
            return DecodeManagedImage(data, format, width, height, fs, exportType, quality, swizzler);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) to a compressed texture format.
        /// This method only supports basic RGBA formats, so it is not
        /// recommended unless you are on a platform where the native encoder
        /// does not work.
        /// </summary>
        /// <param name="data">The raw encoded texture data to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="useBgra">Should <paramref name="data"/> be read as BGRA32 instead of RGBA32?</param>
        /// <returns>The encoded texture.</returns>
        public static byte[] EncodeManagedData(
            byte[] data, TextureFormat format,
            int width, int height, bool useBgra = true)
        {
            var flippedData = TextureOperations.FlipRGBA32Vertically(data, width, height);
            if (!useBgra)
            {
                if (format == TextureFormat.RGBA32)
                    return flippedData;

                TextureOperations.SwapRBComponentsInplace(flippedData);
            }

            if (format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old)
                return flippedData;

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

        /// <summary>
        /// Encode an image file (e.g., .png) to a compressed texture format.
        /// This method only supports basic RGBA formats, so it is not
        /// recommended unless you are on a platform where the native encoder
        /// does not work.
        /// </summary>
        /// <param name="stream">The stream of an image file to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The encoded texture.</returns>
        public static byte[] EncodeManagedImage(
            Stream stream, TextureFormat format,
            out int width, out int height)
        {
            ImageResult imageResult = ImageResult.FromStream(stream, StbReadColorComponents.RedGreenBlueAlpha);
            width = imageResult.Width;
            height = imageResult.Height;
            TextureOperations.FlipBGRA32VerticallyInplace(imageResult.Data, width, height);
            return EncodeManagedData(imageResult.Data, format, width, height, false);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) to a compressed texture format.
        /// This method only supports basic RGBA formats, so it is not
        /// recommended unless you are on a platform where the native encoder
        /// does not work.
        /// </summary>
        /// <param name="path">The file path to an image file to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The encoded texture.</returns>
        public static byte[] EncodeManagedImage(
            string path, TextureFormat format,
            out int width, out int height)
        {
            using FileStream fs = File.OpenRead(path);
            return EncodeManagedImage(fs, format, out width, out height);
        }

        /// <summary>
        /// Encode a raw RGBA32/BGRA32 image to a compressed texture format.
        /// </summary>
        /// <param name="data">The raw encoded texture data to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipCount">The number of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <param name="useBgra">Should <paramref name="data"/> be read as BGRA32 instead of RGBA32?</param>
        /// <returns>The encoded texture data.</returns>
        public static byte[][] EncodeNativeData(
            byte[] data, TextureFormat format,
            int width, int height,
            int mipCount = 1, int quality = 3, bool useBgra = true)
        {
            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            var flippedData = TextureOperations.FlipRGBA32Vertically(data, width, height);
            if (!useBgra)
            {
                if (format == TextureFormat.RGBA32 && mipCount == 1)
                    return [flippedData];

                TextureOperations.SwapRBComponentsInplace(flippedData);
            }

            if ((format == TextureFormat.BGRA32 || format == TextureFormat.BGRA32Old) && mipCount == 1)
                return [flippedData];

            return TextureEncoderWrapper.ConvertImage(data, mipCount, format, width, height, quality);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) to a compressed texture format.
        /// </summary>
        /// <param name="stream">The stream of an image file to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipCount">The number of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <returns>The encoded texture data.</returns>
        public static byte[][] EncodeNativeImage(
            Stream stream, TextureFormat format,
            out int width, out int height,
            int mipCount = 1, int quality = 3)
        {
            width = height = 0;

            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            if (stream is FileStream fs)
                return EncodeNativeImage(fs.Name, format, out width, out height, mipCount, quality);

            // ok, this is pretty silly. cuttlefish is currently only setup to
            // read from file as a png/jpg/etc or from a pointer to raw data.
            // we don't have any way to pass a png/jpg/etc as bytes.
            // to handle this for now, we read the whole image into bytes and
            // pass it to stb to convert to rgba32.

            ImageResult imageResult = ImageResult.FromStream(stream, StbReadColorComponents.RedGreenBlueAlpha);
            width = imageResult.Width;
            height = imageResult.Height;

            return EncodeNativeData(imageResult.Data, format, width, height, mipCount, quality, false);
        }

        /// <summary>
        /// Encode an image file (e.g., .png) to a compressed texture format.
        /// </summary>
        /// <param name="path">The file path to an image file to encode.</param>
        /// <param name="format">The format of the texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipCount">The number of mips to encode.</param>
        /// <param name="quality">The quality of the new image, if the texture format is lossy.</param>
        /// <returns>The encoded texture data.</returns>
        public static byte[][] EncodeNativeImage(
            string path, TextureFormat format,
            out int width, out int height,
            int mipCount = 1, int quality = 3)
        {
            width = height = 0;

            if (!TextureEncoderWrapper.NativeLibrariesSupported())
                return null;

            return TextureEncoderWrapper.ConvertImage(path, mipCount, format, out width, out height, quality);
        }

        #endregion
    }
}
