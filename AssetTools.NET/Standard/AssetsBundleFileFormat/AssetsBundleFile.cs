using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetsTools.NET
{
    public class AssetBundleFile
    {
        //AssetsFileReader reader;
        public AssetBundleHeader03 bundleHeader3;
        public AssetBundleHeader06 bundleHeader6;

        public AssetsList assetsLists3;
        public AssetBundleBlockAndDirectoryList06 bundleInf6;

        public AssetsFileReader reader;

        ///public AssetsBundleFile();
        ///public ~AssetsBundleFile();
        public void Close()
        {
            reader.Close();
        }
        public bool Read(AssetsFileReader reader, bool allowCompressed = false)
        {
            this.reader = reader;
            reader.ReadNullTerminated();
            uint version = reader.ReadUInt32();
            if (version == 6)
            {
                reader.Position = 0;
                bundleHeader6 = new AssetBundleHeader06();
                bundleHeader6.Read(reader);
                if (bundleHeader6.signature == "UnityFS")
                {
                    bundleInf6 = new AssetBundleBlockAndDirectoryList06();
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
                        bundleInf6.Read(bundleHeader6.GetBundleInfoOffset(), reader);
                        return true;
                    }
                }
                else
                {
                    new NotImplementedException("Non UnityFS bundles are not supported yet.");
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
        ///    class BundleReplacer **replacers, size_t replacerCount,
        ///    AssetsFileVerifyLogger errorLogger = NULL, ClassDatabaseFile* typeMeta = NULL);
        //-todo, use a faster custom bundle decompressor. currently a copy paste of unity studio's
        public bool Unpack(AssetsFileReader reader, AssetsFileWriter writer)
        {
            if (Read(reader, true))
            {
                reader.Position = bundleHeader6.GetBundleInfoOffset();
                MemoryStream blocksInfoStream;
                AssetsFileReader memReader;
                int compressedSize = (int)bundleHeader6.compressedSize;
                switch (bundleHeader6.flags & 0x3F)
                {
                    case 1:
                        using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
                        {
                            blocksInfoStream = SevenZipHelper.StreamDecompress(mstream);
                        }
                        break;
                    case 2:
                    case 3:
                        byte[] uncompressedBytes = new byte[bundleHeader6.decompressedSize];
                        using (MemoryStream mstream = new MemoryStream(reader.ReadBytes(compressedSize)))
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
                    AssetBundleBlockInfo06 info = bundleInf6.blockInf[i];
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
                AssetBundleHeader06 newBundleHeader6 = new AssetBundleHeader06()
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
                long fileSize = newBundleHeader6.GetFileDataOffset();
                for (int i = 0; i < bundleInf6.blockCount; i++)
                    fileSize += bundleInf6.blockInf[i].decompressedSize;
                newBundleHeader6.totalFileSize = fileSize;
                AssetBundleBlockAndDirectoryList06 newBundleInf6 = new AssetBundleBlockAndDirectoryList06()
                {
                    checksumLow = 0, //-todo, figure out how to make real checksums, uabe sets these to 0 too
                    checksumHigh = 0,
                    blockCount = bundleInf6.blockCount,
                    directoryCount = bundleInf6.directoryCount
                };
                newBundleInf6.blockInf = new AssetBundleBlockInfo06[newBundleInf6.blockCount];
                for (int i = 0; i < newBundleInf6.blockCount; i++)
                {
                    newBundleInf6.blockInf[i] = new AssetBundleBlockInfo06();
                    newBundleInf6.blockInf[i].compressedSize = bundleInf6.blockInf[i].decompressedSize;
                    newBundleInf6.blockInf[i].decompressedSize = bundleInf6.blockInf[i].decompressedSize;
                    newBundleInf6.blockInf[i].flags = (ushort)(bundleInf6.blockInf[i].flags & 0xC0); //set compression to none
                }
                newBundleInf6.dirInf = new AssetBundleDirectoryInfo06[newBundleInf6.directoryCount];
                for (int i = 0; i < newBundleInf6.directoryCount; i++)
                {
                    newBundleInf6.dirInf[i].offset = bundleInf6.dirInf[i].offset;
                    newBundleInf6.dirInf[i].decompressedSize = bundleInf6.dirInf[i].decompressedSize;
                    newBundleInf6.dirInf[i].flags = bundleInf6.dirInf[i].flags;
                    newBundleInf6.dirInf[i].name = bundleInf6.dirInf[i].name;
                }
                newBundleHeader6.Write(writer);
                newBundleInf6.Write(writer);
                for (int i = 0; i < newBundleInf6.blockCount; i++)
                {
                    writer.Write(blocksData[i]);
                }
                return true;
            }
            return false;
        }
        ///public bool Pack(AssetsFileReader reader, LPARAM lPar, AssetsFileWriter writer, LPARAM writerPar);
        public bool IsAssetsFile(AssetsFileReader reader, AssetBundleDirectoryInfo06 entry)
        {
            //todo - not fully implemented
            long offset = bundleHeader6.GetFileDataOffset() + entry.offset;
            if (entry.decompressedSize < 0x20)
                return false;

            reader.Position = offset;
            string possibleBundleHeader = reader.ReadStringLength(7);
            if (possibleBundleHeader == "UnityFS")
                return false;

            reader.Position = offset + 0x08;
            int possibleFormat = reader.ReadInt32();
            if (possibleFormat > 99)
                return false;

            reader.Position = offset + 0x14;
            string possibleVersion = reader.ReadNullTerminated();
            string emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.]", "");
            string fullVersion = Regex.Replace(possibleVersion, "[^a-zA-Z0-9\\.]", "");
            return emptyVersion == "" && fullVersion.Length > 0;
        }
        ///public bool IsAssetsFile(AssetsFileReader reader, LPARAM lPar, AssetsBundleEntry* entry);
        ///public AssetsFileReader MakeAssetsFileReader(AssetsFileReader reader, LPARAM* lPar, AssetsBundleDirectoryInfo06* entry);
        ///public AssetsFileReader MakeAssetsFileReader(AssetsFileReader reader, LPARAM* lPar, AssetsBundleEntry* entry);
        //void FreeAssetsFileReader(LPARAM *lPar, AssetsFileReader *reader);
    }
}
