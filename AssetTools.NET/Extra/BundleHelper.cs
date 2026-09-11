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

            AssetsFileReader reader = bundle.DataReader;
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
            Stream stream = new SegmentStream(bundle.DataReader.BaseStream, offset, length);
            AssetsFileReader reader = new AssetsFileReader(stream);
            AssetsFile file = new AssetsFile();
            file.Read(reader);
            return file;
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
            int numFiles = bundle.BlockAndDirInfo.DirectoryInfos.Count;
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
            int numFiles = bundle.BlockAndDirInfo.DirectoryInfos.Count;
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
                file.DataReader.Close();
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
                file.DataReader.Close();
            }
            return newFile;
        }

        public static AssetBundleDirectoryInfo GetDirInfo(AssetBundleFile bundle, int index)
        {
            List<AssetBundleDirectoryInfo> dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
            return dirInf[index];
        }

        public static AssetBundleDirectoryInfo GetDirInfo(AssetBundleFile bundle, string name)
        {
            List<AssetBundleDirectoryInfo> dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
            for (int i = 0; i < dirInf.Count; i++)
            {
                AssetBundleDirectoryInfo info = dirInf[i];
                if (info.Name == name)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the CRC32 (IEEE) of the bundle data stream.
        /// This is the same CRC verified by Unity when loading an AssetBundle. It is independent of the 
        /// compression method and depends only on the actual content; for compressed bundles, 
        /// the data is unpacked before the CRC is calculated.
        /// </summary>
        /// <param name="bundle">Loaded bundle to calculate CRC for.</param>
        /// <returns>CRC32 value.</returns>
        public static uint CalculateBundleCrc32(AssetBundleFile bundle)
        {
            AssetBundleFile crcBundle = bundle;
            bool closeCrcBundle = false;

            if (bundle.DataIsCompressed)
            {
                crcBundle = UnpackBundle(bundle, false);
                closeCrcBundle = true;
            }

            try
            {
                long totalDataLen = 0;
                AssetBundleBlockInfo[] blocks = crcBundle.BlockAndDirInfo.BlockInfos;
                for (int i = 0; i < blocks.Length; i++)
                {
                    totalDataLen += blocks[i].DecompressedSize;
                }

                AssetsFileReader dataReader = crcBundle.DataReader;
                long oldPos = dataReader.Position;
                try
                {
                    dataReader.Position = 0;
                    uint crc = Crc32Helper.InitialValue;
                    long consumed = 0;

                    while (consumed < totalDataLen)
                    {
                        int toRead = (int)Math.Min(Crc32Helper.ChunkSize, totalDataLen - consumed);
                        byte[] chunk = dataReader.ReadBytes(toRead);
                        if (chunk.Length != toRead)
                        {
                            throw new EndOfStreamException(
                                $"Unexpected end of bundle data while calculating CRC at 0x{consumed:X}."
                            );
                        }

                        crc = Crc32Helper.Feed(crc, chunk);
                        consumed += toRead;
                    }

                    return Crc32Helper.Finalize(crc);
                }
                finally
                {
                    dataReader.Position = oldPos;
                }
            }
            finally
            {
                if (closeCrcBundle)
                {
                    try
                    {
                        crcBundle.Close();
                    }
                    catch
                    {
                        // some streams may have been closed during unpack operations
                        // so we will ignore this
                    }
                }
            }
        }
    }
}
