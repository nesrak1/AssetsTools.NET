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
            
            AssetsFileReader reader = bundle.reader;
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
            Stream stream = new SegmentStream(bundle.reader.BaseStream, offset, length);
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
            int numFiles = bundle.NumFiles;
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
            int numFiles = bundle.NumFiles;
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
            file.Unpack(file.reader, new AssetsFileWriter(ms));
            ms.Position = 0;

            AssetBundleFile newFile = new AssetBundleFile();
            newFile.Read(new AssetsFileReader(ms), false);

            if (freeOriginalStream)
            {
                file.reader.Close();
            }
            return newFile;
        }

        public static AssetBundleFile UnpackBundleToStream(AssetBundleFile file, Stream stream, bool freeOriginalStream = true)
        {
            file.Unpack(file.reader, new AssetsFileWriter(stream));
            stream.Position = 0;

            AssetBundleFile newFile = new AssetBundleFile();
            newFile.Read(new AssetsFileReader(stream), false);

            if (freeOriginalStream)
            {
                file.reader.Close();
            }
            return newFile;
        }

        public static AssetBundleDirectoryInfo06 GetDirInfo(AssetBundleFile bundle, int index)
        {
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            return dirInf[index];
        }

        public static AssetBundleDirectoryInfo06 GetDirInfo(AssetBundleFile bundle, string name)
        {
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInf[i];
                if (info.name == name)
                {
                    return info;
                }
            }
            return null;
        }

        public static void UnpackInfoOnly(this AssetBundleFile bundle)
        {
            AssetsFileReader reader = bundle.reader;

            reader.Position = 0;
            if (bundle.Read(reader, true))
            {
                reader.Position = bundle.bundleHeader6.GetBundleInfoOffset();
                MemoryStream blocksInfoStream;
                AssetsFileReader memReader;
                int compressedSize = (int)bundle.bundleHeader6.compressedSize;
                switch (bundle.bundleHeader6.GetCompressionType())
                {
                    case 1:
                        using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
                        {
                            blocksInfoStream = SevenZipHelper.StreamDecompress(mstream);
                        }
                        break;
                    case 2:
                    case 3:
                        byte[] uncompressedBytes = new byte[bundle.bundleHeader6.decompressedSize];
                        using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(uncompressedBytes, 0, (int)bundle.bundleHeader6.decompressedSize);
                            decoder.Dispose();
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    default:
                        blocksInfoStream = null;
                        break;
                }
                if (bundle.bundleHeader6.GetCompressionType() != 0)
                {
                    using (memReader = new AssetsFileReader(blocksInfoStream))
                    {
                        memReader.Position = 0;
                        bundle.bundleInf6.Read(0, memReader);
                    }
                }
            }
        }
    }
}
