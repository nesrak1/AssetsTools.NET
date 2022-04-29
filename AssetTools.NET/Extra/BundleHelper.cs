////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public static class BundleHelper
    {
        public static byte[] LoadAssetDataFromBundle(AssetBundleFile bundle, int index)
        {
            bundle.GetFileRange(index, out long offset, out long length);
            
            AssetsFileReader reader = bundle.Reader;
            reader.Position = offset;
            return reader.ReadBytes((int)length);
        }

        public static byte[] LoadAssetDataFromBundle(AssetBundleFile bundle, string name)
        {
            int index = bundle.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetDataFromBundle(bundle, index);
        }

        public static AssetsFile LoadAssetFromBundle(AssetBundleFile bundle, int index)
        {
            bundle.GetFileRange(index, out long offset, out long length);
            Stream stream = new SegmentStream(bundle.Reader.BaseStream, offset, length);
            AssetsFileReader reader = new AssetsFileReader(stream);
            return new AssetsFile(reader);
        }

        public static AssetsFile LoadAssetFromBundle(AssetBundleFile bundle, string name)
        {
            int index = bundle.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetFromBundle(bundle, index);
        }

        public static List<byte[]> LoadAllAssetsDataFromBundle(AssetBundleFile bundle)
        {
            List<byte[]> files = new List<byte[]>();
            int numFiles = bundle.BlockAndDirInfo.DirectoryInfos.Length;
            for (int i = 0; i < numFiles; i++)
            {
                if (bundle.IsAssetsFile(i))
                {
                    files.Add(LoadAssetDataFromBundle(bundle, i));
                }
            }
            return files;
        }

        public static List<AssetsFile> LoadAllAssetsFromBundle(AssetBundleFile bundle)
        {
            List<AssetsFile> files = new List<AssetsFile>();
            int numFiles = bundle.BlockAndDirInfo.DirectoryInfos.Length;
            for (int i = 0; i < numFiles; i++)
            {
                if (bundle.IsAssetsFile(i))
                {
                    files.Add(LoadAssetFromBundle(bundle, i));
                }
            }
            return files;
        }

        public static AssetBundleFile UnpackBundle(AssetBundleFile file, bool freeOriginalStream = true)
        {
            MemoryStream ms = new MemoryStream();
            file.Unpack(new AssetsFileWriter(ms));
            ms.Position = 0;

            AssetBundleFile newFile = new AssetBundleFile();
            newFile.Read(new AssetsFileReader(ms));

            if (freeOriginalStream)
            {
                file.Reader.Close();
            }
            return newFile;
        }

        public static AssetBundleFile UnpackBundleToStream(AssetBundleFile file, Stream stream, bool freeOriginalStream = true)
        {
            file.Unpack(new AssetsFileWriter(stream));
            stream.Position = 0;

            AssetBundleFile newFile = new AssetBundleFile();
            newFile.Read(new AssetsFileReader(stream));

            if (freeOriginalStream)
            {
                file.Reader.Close();
            }
            return newFile;
        }

        public static AssetBundleDirectoryInfo GetDirInfo(AssetBundleFile bundle, int index)
        {
            AssetBundleDirectoryInfo[] dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
            return dirInf[index];
        }

        public static AssetBundleDirectoryInfo GetDirInfo(AssetBundleFile bundle, string name)
        {
            AssetBundleDirectoryInfo[] dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo info = dirInf[i];
                if (info.Name == name)
                {
                    return info;
                }
            }
            return null;
        }

        public static void UnpackInfoOnly(this AssetBundleFile bundle)
        {
            AssetsFileReader reader = bundle.Reader;

            // todo, exceptions

            reader.Position = bundle.Header.GetBundleInfoOffset();
            MemoryStream blocksInfoStream;
            AssetsFileReader memReader;
            int compressedSize = (int)bundle.Header.FileStreamHeader.CompressedSize;
            switch (bundle.Header.GetCompressionType())
            {
                case 1:
                    using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
                    {
                        blocksInfoStream = SevenZipHelper.StreamDecompress(mstream);
                    }
                    break;
                case 2:
                case 3:
                    byte[] uncompressedBytes = new byte[bundle.Header.FileStreamHeader.CompressedSize];
                    using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
                    {
                        var decoder = new Lz4DecoderStream(mstream);
                        decoder.Read(uncompressedBytes, 0, (int)bundle.Header.FileStreamHeader.CompressedSize);
                        decoder.Dispose();
                    }
                    blocksInfoStream = new MemoryStream(uncompressedBytes);
                    break;
                default:
                    blocksInfoStream = null;
                    break;
            }
            if (bundle.Header.GetCompressionType() != 0)
            {
                using (memReader = new AssetsFileReader(blocksInfoStream))
                {
                    memReader.Position = 0;
                    bundle.BlockAndDirInfo.Read(memReader);
                }
            }
        }
    }
}
