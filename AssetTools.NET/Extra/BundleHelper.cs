////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public static class BundleHelper
    {
        public static byte[] LoadAssetDataFromBundle(AssetBundleFile bundle, int index)
        {
            AssetsFileReader reader = bundle.reader;
            int start = (int)(bundle.bundleHeader6.GetFileDataOffset() + bundle.bundleInf6.dirInf[index].offset);
            int length = (int)bundle.bundleInf6.dirInf[index].decompressedSize;
            reader.Position = start;
            return reader.ReadBytes(length);
        }
        public static AssetsFile LoadAssetFromBundle(AssetBundleFile bundle, int index)
        {
            byte[] data = LoadAssetDataFromBundle(bundle, index);
            MemoryStream ms = new MemoryStream(data);
            AssetsFileReader r = new AssetsFileReader(ms);
            return new AssetsFile(r);
        }
        public static AssetsFile LoadAssetFromBundle(AssetBundleFile bundle, string name)
        {
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInf[i];
                if (info.name == name)
                {
                    return LoadAssetFromBundle(bundle, i);
                }
            }
            return null;
        }
        public static byte[] LoadAssetDataFromBundle(AssetBundleFile bundle, string name)
        {
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInf[i];
                if (info.name == name)
                {
                    return LoadAssetDataFromBundle(bundle, i);
                }
            }
            return null;
        }
        public static List<AssetsFile> LoadAllAssetsFromBundle(AssetBundleFile bundle)
        {
            List<AssetsFile> files = new List<AssetsFile>();
            AssetsFileReader reader = bundle.reader;
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInf[i];
                if (bundle.IsAssetsFile(reader, info))
                {
                    files.Add(LoadAssetFromBundle(bundle, i));
                }
            }
            return files;
        }
        public static List<byte[]> LoadAllAssetsDataFromBundle(AssetBundleFile bundle)
        {
            List<byte[]> files = new List<byte[]>();
            AssetsFileReader reader = bundle.reader;
            AssetBundleDirectoryInfo06[] dirInf = bundle.bundleInf6.dirInf;
            for (int i = 0; i < dirInf.Length; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInf[i];
                if (bundle.IsAssetsFile(reader, info))
                {
                    files.Add(LoadAssetDataFromBundle(bundle, i));
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
        
        public static AssetBundleFile CreateBlankBundle(string engineVersion, int contentSize)
        {
            AssetBundleHeader06 header = new AssetBundleHeader06()
            {
                signature = "UnityFS",
                fileVersion = 6,
                minPlayerVersion = "5.x.x",
                fileEngineVersion = engineVersion,
                totalFileSize = 0x82 + engineVersion.Length + contentSize,
                compressedSize = 0x5B,
                decompressedSize = 0x5B,
                flags = 0x40
            };
            AssetBundleBlockInfo06 blockInf = new AssetBundleBlockInfo06
            {
                decompressedSize = (uint)contentSize,
                compressedSize = (uint)contentSize,
                flags = 0x0040
            };
            AssetBundleDirectoryInfo06 dirInf = new AssetBundleDirectoryInfo06
            {
                offset = 0,
                decompressedSize = (uint)contentSize,
                flags = 4,
                name = GenerateCabName()
            };
            AssetBundleBlockAndDirectoryList06 info = new AssetBundleBlockAndDirectoryList06()
            {
                checksumLow = 0,
                checksumHigh = 0,
                blockCount = 1,
                blockInf = new AssetBundleBlockInfo06[]
                {
                    blockInf
                },
                directoryCount = 1,
                dirInf = new AssetBundleDirectoryInfo06[]
                {
                    dirInf
                }
            };
            AssetBundleFile bundle = new AssetBundleFile()
            {
                bundleHeader6 = header,
                bundleInf6 = info
            };
            return bundle;
        }

        private static string GenerateCabName()
        {
            string alphaNum = "0123456789abcdef";
            string output = "CAB-";
            Random rand = new Random();
            for (int i = 0; i < 32; i++)
            {
                output += alphaNum[rand.Next(0, alphaNum.Length)];
            }
            return output;
        }
    }
}
