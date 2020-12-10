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
    }
}
