using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsBundleFile
    {
        //AssetsFileReader reader;
        public AssetsBundleHeader03 bundleHeader3;
        public AssetsBundleHeader06 bundleHeader6;

        public AssetsList assetsLists3;
        public AssetsBundleBlockAndDirectoryList06 bundleInf6;

        public uint listCount;

        private AssetsFileReader reader;

        ///public AssetsBundleFile();
        ///public ~AssetsBundleFile();
        public void Close()
        {
            reader.Close();
        }
        public bool Read(AssetsFileReader reader, /*AssetsFileVerifyLogger errorLogger = NULL,*/ bool allowCompressed = false)
        {
            this.reader = reader;
            reader.ReadNullTerminated();
            uint version = reader.ReadUInt32();
            if (version == 6)
            {
                reader.Position = 0;
                bundleHeader6 = new AssetsBundleHeader06();
                if (bundleHeader6.Read(reader))
                {
                    if (bundleHeader6.signature == "UnityFS")
                    {
                        bundleInf6 = new AssetsBundleBlockAndDirectoryList06();
                        if ((bundleHeader6.flags & 0x3F) != 0)
                        {
                            if (allowCompressed)
                            {
                                return true;
                            }
                            else
                            {
                                Close();
                                return false;
                            }
                        }
                        else
                        {
                            return bundleInf6.Read(bundleHeader6.GetBundleInfoOffset(), reader);
                        }
                    }
                    else
                    {
                        new NotImplementedException("Non UnityFS bundles are not supported yet.");
                    }
                }
            }
            else if (version == 3)
            {
                new NotImplementedException("Version 3 bundles are not supported yet.");
            }
            else
            {
                new Exception("AssetsBundleFile.Read : Unknown file version!");
            }
            return false;
        }
        ///public bool Write(AssetsFileReader reader, LPARAM readerPar,
        ///    AssetsFileWriter writer, LPARAM writerPar,
        ///
        ///    class BundleReplacer **pReplacers, size_t replacerCount,
        ///    AssetsFileVerifyLogger errorLogger = NULL, ClassDatabaseFile* typeMeta = NULL);
        //-todo, use a faster custom bundle decompressor. currently a copy paste of unity studio's
        public bool Unpack(AssetsFileReader reader, AssetsFileWriter writer)
        {
            if (Read(reader, true))
            {
                reader.Position = bundleHeader6.GetBundleInfoOffset();
                MemoryStream blocksInfoStream;
                AssetsFileReader memReader;
                switch (bundleHeader6.flags & 0x3F)
                {
                    case 1:
                        blocksInfoStream = SevenZipHelper.StreamDecompress(new MemoryStream(reader.ReadBytes((int)bundleHeader6.compressedSize)));
                        break;
                    case 2:
                    case 3:
                        byte[] uncompressedBytes = new byte[bundleHeader6.decompressedSize];
                        using (var mstream = new MemoryStream(reader.ReadBytes((int)bundleHeader6.compressedSize)))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(uncompressedBytes, 0, (int)bundleHeader6.decompressedSize);
                            decoder.Dispose();
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    default:
                        blocksInfoStream = null;
                        break;
                }
                if ((bundleHeader6.flags & 0x3F) != 0)
                {
                    using (memReader = new AssetsFileReader(blocksInfoStream))
                    {
                        bundleInf6.Read(0, memReader);
                    }
                }
                reader.Position = bundleHeader6.GetFileDataOffset();
                byte[][] blocksData = new byte[bundleInf6.blockCount][];
                for (int i = 0; i < bundleInf6.blockCount; i++)
                {
                    AssetsBundleBlockInfo06 info = bundleInf6.blockInf[i];
                    byte[] data = reader.ReadBytes((int)info.compressedSize);
                    switch (info.flags & 0x3F)
                    {
                        case 0:
                            blocksData[i] = data;
                            break;
                        case 1:
                            blocksData[i] = new byte[info.decompressedSize];
                            using (MemoryStream mstream = new MemoryStream(data))
                            {
                                MemoryStream decoder = SevenZipHelper.StreamDecompress(mstream, info.decompressedSize);
                                decoder.Read(blocksData[i], 0, (int)info.decompressedSize);
                                decoder.Dispose();
                            }
                            break;
                        case 2:
                        case 3:
                            blocksData[i] = new byte[info.decompressedSize];
                            using (MemoryStream mstream = new MemoryStream(data))
                            {
                                var decoder = new Lz4DecoderStream(mstream);
                                decoder.Read(blocksData[i], 0, (int)info.decompressedSize);
                                decoder.Dispose();
                            }
                            break;
                    }
                }
                AssetsBundleHeader06 newBundleHeader6 = new AssetsBundleHeader06()
                {
                    signature = bundleHeader6.signature,
                    fileVersion = bundleHeader6.fileVersion,
                    minPlayerVersion = bundleHeader6.minPlayerVersion,
                    fileEngineVersion = bundleHeader6.fileEngineVersion,
                    totalFileSize = 0,
                    compressedSize = bundleHeader6.decompressedSize,
                    decompressedSize = bundleHeader6.decompressedSize,
                    flags = bundleHeader6.flags & 0x40 //set compression and block position to 0
                };
                ulong fileSize = newBundleHeader6.GetFileDataOffset();
                for (int i = 0; i < bundleInf6.blockCount; i++)
                    fileSize += bundleInf6.blockInf[i].decompressedSize;
                newBundleHeader6.totalFileSize = fileSize;
                AssetsBundleBlockAndDirectoryList06 newBundleInf6 = new AssetsBundleBlockAndDirectoryList06()
                {
                    checksumLow = 0, //-todo, figure out how to make real checksums, uabe sets these to 0 too
                    checksumHigh = 0,
                    blockCount = bundleInf6.blockCount,
                    directoryCount = bundleInf6.directoryCount
                };
                newBundleInf6.blockInf = new AssetsBundleBlockInfo06[newBundleInf6.blockCount];
                for (int i = 0; i < newBundleInf6.blockCount; i++)
                {
                    newBundleInf6.blockInf[i] = new AssetsBundleBlockInfo06();
                    newBundleInf6.blockInf[i].compressedSize = bundleInf6.blockInf[i].decompressedSize;
                    newBundleInf6.blockInf[i].decompressedSize = bundleInf6.blockInf[i].decompressedSize;
                    newBundleInf6.blockInf[i].flags = (ushort)(bundleInf6.blockInf[i].flags & 0xC0); //set compression to none
                }
                newBundleInf6.dirInf = new AssetsBundleDirectoryInfo06[newBundleInf6.directoryCount];
                for (int i = 0; i < newBundleInf6.directoryCount; i++)
                {
                    newBundleInf6.dirInf[i].offset = bundleInf6.dirInf[i].offset;
                    newBundleInf6.dirInf[i].decompressedSize = bundleInf6.dirInf[i].decompressedSize;
                    newBundleInf6.dirInf[i].flags = bundleInf6.dirInf[i].flags;
                    newBundleInf6.dirInf[i].name = bundleInf6.dirInf[i].name;
                }
                newBundleHeader6.Write(writer, 0);
                newBundleInf6.Write(writer, writer.Position);
                for (int i = 0; i < newBundleInf6.blockCount; i++)
                {
                    writer.Write(blocksData[i]);
                }
                return true;
            }
            return false;
        }
        ///public bool Pack(AssetsFileReader reader, LPARAM lPar, AssetsFileWriter writer, LPARAM writerPar);
        ///public bool IsAssetsFile(AssetsFileReader reader, LPARAM pLPar, AssetsBundleDirectoryInfo06* pEntry);
        ///public bool IsAssetsFile(AssetsFileReader reader, LPARAM pLPar, AssetsBundleEntry* pEntry);
        ///public AssetsFileReader MakeAssetsFileReader(AssetsFileReader reader, LPARAM* pLPar, AssetsBundleDirectoryInfo06* pEntry);
        ///public AssetsFileReader MakeAssetsFileReader(AssetsFileReader reader, LPARAM* pLPar, AssetsBundleEntry* pEntry);
        //void FreeAssetsFileReader(LPARAM *pLPar, AssetsFileReader *pReader);
    }
}
