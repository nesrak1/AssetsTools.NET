////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace AssetsTools.NET
{
    public struct AssetsBundleDirectoryInfo06
    {
        public ulong offset;
        public ulong decompressedSize;
        public uint flags;
        public string name;
        ///public ulong GetAbsolutePos(AssetsBundleHeader06 pHeader)
        ///public ulong GetAbsolutePos(AssetsBundleFile pFile)
    };
    public struct AssetsBundleBlockInfo06
    {
        public uint decompressedSize;
        public uint compressedSize;
        public ushort flags; //(flags & 0x40) : is streamed; (flags & 0x3F) :  compression info;
        public byte GetCompressionType() { return (byte)(flags & 0x3F); }
        //example flags (LZMA, streamed) : 0x41
        //example flags (LZ4, not streamed) : 0x03
    };
    public struct AssetsBundleBlockAndDirectoryList06
    {
        public ulong checksumLow;
        public ulong checksumHigh;
        public uint blockCount;
        public AssetsBundleBlockInfo06[] blockInf;
        public uint directoryCount;
        public AssetsBundleDirectoryInfo06[] dirInf;

        ///void Free();
        public bool Read(ulong filePos, AssetsFileReader reader/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            reader.Position = filePos;
            checksumLow = reader.ReadUInt64();
            checksumHigh = reader.ReadUInt64();
            blockCount = reader.ReadUInt32();
            blockInf = new AssetsBundleBlockInfo06[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                blockInf[i].decompressedSize = reader.ReadUInt32();
                blockInf[i].compressedSize = reader.ReadUInt32();
                blockInf[i].flags = reader.ReadUInt16();
            }
            directoryCount = reader.ReadUInt32();
            dirInf = new AssetsBundleDirectoryInfo06[directoryCount];
            for (int i = 0; i < directoryCount; i++)
            {
                dirInf[i].offset = reader.ReadUInt64();
                dirInf[i].decompressedSize = reader.ReadUInt64();
                dirInf[i].flags = reader.ReadUInt32();
                dirInf[i].name = reader.ReadNullTerminated();
            }
            return true;
        }
        //Write doesn't compress
        public bool Write(AssetsFileWriter writer, ulong curFilePos/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            writer.Position = curFilePos;
            writer.Write(checksumHigh);
            writer.Write(checksumLow);
            writer.Write(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                writer.Write(blockInf[i].decompressedSize);
                writer.Write(blockInf[i].compressedSize);
                writer.Write(blockInf[i].flags);
            }
            writer.Write(directoryCount);
            for (int i = 0; i < directoryCount; i++)
            {
                writer.Write(dirInf[i].offset);
                writer.Write(dirInf[i].decompressedSize);
                writer.Write(dirInf[i].flags);
                writer.WriteNullTerminated(dirInf[i].name);
            }
            return true;
        }
    };
    
    //Unity 5.3+
    public struct AssetsBundleHeader06
    {
        //no alignment in this struct!
        public string signature; //0-terminated; UnityFS, UnityRaw, UnityWeb or UnityArchive
        public uint fileVersion; //big-endian, = 6
        public string minPlayerVersion; //0-terminated; 5.x.x
        public string fileEngineVersion; //0-terminated; exact unity engine version
        public ulong totalFileSize;
        //sizes for the blocks info :
        public uint compressedSize;
        public uint decompressedSize;
        //(flags & 0x3F) is the compression mode (0 = none; 1 = LZMA; 2-3 = LZ4)
        //(flags & 0x40) says whether the bundle has directory info
        //(flags & 0x80) says whether the block and directory list is at the end
        public uint flags;

        ///public bool ReadInitial(AssetsFileReader reader, LPARAM lPar, AssetsFileVerifyLogger errorLogger = NULL);
        public bool Read(AssetsFileReader reader/*, AssetsFileVerifyLogger errorLogger = null*/)
        {
            signature = reader.ReadNullTerminated();
            fileVersion = reader.ReadUInt32();
            minPlayerVersion = reader.ReadNullTerminated();
            fileEngineVersion = reader.ReadNullTerminated();
            totalFileSize = reader.ReadUInt64();
            compressedSize = reader.ReadUInt32();
            decompressedSize = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            return true;
        }
        public bool Write(AssetsFileWriter writer, ulong curFilePos/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            writer.Position = curFilePos;
            writer.WriteNullTerminated(signature);
            writer.Write(fileVersion);
            writer.WriteNullTerminated(minPlayerVersion);
            writer.WriteNullTerminated(fileEngineVersion);
            writer.Write(totalFileSize);
            writer.Write(compressedSize);
            writer.Write(decompressedSize);
            writer.Write(flags);
            return true;
        }
        public ulong GetBundleInfoOffset()
        {
            if ((this.flags & 0x80) != 0)
            {
                if (this.totalFileSize == 0)
                    return unchecked((ulong)-1);
                return this.totalFileSize - this.compressedSize;
            }
            else
            {
                //if (!strcmp(this->signature, "UnityWeb") || !strcmp(this->signature, "UnityRaw"))
                //	return 9;
                ulong ret = (ulong)(minPlayerVersion.Length + fileEngineVersion.Length + 0x1A);
                if ((this.flags & 0x100) != 0)
                    return (ret + 0x0A);
                else
                    return (ret + (ulong)signature.Length + 1);
            }
        }
        public uint GetFileDataOffset()
        {
            uint ret = 0;
            if (this.signature == "UnityArchive")
                return this.compressedSize;
            else if (this.signature == "UnityFS" || this.signature == "UnityWeb")
            {
                ret = (uint)minPlayerVersion.Length + (uint)fileEngineVersion.Length + 0x1A;
                if ((this.flags & 0x100) != 0)
                    ret += 0x0A;
                else
                    ret += (uint)signature.Length + 1;
            }
            if ((this.flags & 0x80) == 0)
                ret += this.compressedSize;
            return ret;
        }
    };

    public struct AssetsBundleHeader03
    {
        public string signature; //0-terminated; UnityWeb or UnityRaw
        public uint fileVersion; //big-endian; 3 : Unity 3.5 and 4;
        public string minPlayerVersion; //0-terminated; 3.x.x -> Unity 3.x.x/4.x.x; 5.x.x
        public string fileEngineVersion; //0-terminated; exact unity engine version
        public uint minimumStreamedBytes; //big-endian; not always the file's size
        public uint bundleDataOffs; //big-endian;
        public uint numberOfAssetsToDownload; //big-endian;
        public uint levelCount; //big-endian;
        public AssetsBundleOffsetPair[] pLevelList;
	    public uint fileSize2; //big-endian; for fileVersion >= 2
        public uint unknown2; //big-endian; for fileVersion >= 3
        public byte unknown3;

        ///public bool Read(AssetsFileReader reader, LPARAM lPar, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileWriter writer, LPARAM lPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        public uint bundleCount; //big-endian;
    };

    public struct AssetsBundleEntry
    {
        public uint offset;
        public uint length;
        public byte name;
        ///public uint GetAbsolutePos(AssetsBundleHeader03 pHeader);//, DWORD listIndex);
        ///public uint GetAbsolutePos(AssetsBundleFile pFile);//, DWORD listIndex);
    };
    public struct AssetsList
    {
        public uint pos;
        public uint count;
        public AssetsBundleEntry[] ppEntries;
        public uint allocatedCount;
        //AssetsBundleEntry entries[0];
        ///public void Free();
        ///public bool Read(AssetsFileReader reader, LPARAM readerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileWriter writer, LPARAM writerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileReader reader, LPARAM readerPar,
        ///    AssetsFileWriter writer, LPARAM lPar, bool doWriteAssets, QWORD &curReadPos, QWORD* curWritePos = NULL,
        ///    AssetsFileVerifyLogger errorLogger = NULL);
    };
    public struct AssetsBundleOffsetPair
    {
        public uint compressed;
        public uint uncompressed;
    };

    public struct AssetsBundleFilePar
    {
        public AssetsBundleFile pFile;
        
		public AssetsBundleEntry pEntry3;
        public AssetsBundleDirectoryInfo06 pEntry6;

        public uint listIndex;
        public AssetsFileReader origFileReader;
        public FileStream origPar;
        public ulong curFilePos;
    };
    //-This class is basically a combo of disassembly from uabe and unitystudio's code
    public class AssetsBundleFile
    {
        //AssetsFileReader reader;
		public AssetsBundleHeader03 bundleHeader3;
        public AssetsBundleHeader06 bundleHeader6;

		public AssetsList assetsLists3;
        public AssetsBundleBlockAndDirectoryList06 bundleInf6;

		public uint listCount;

        ///public AssetsBundleFile();
        ///public ~AssetsBundleFile();
        public void Close()
        {
            //-wat am i closing again
        }
        //-todo, bundleheader3 is read regardless of version 3 or 6, see https://7daystodie.com/forums/showthread.php?22675-Unity-Assets-Bundle-Extractor&p=761867&viewfull=1#post761867
        //-"just check if bundleHeader3.fileVersion is 3 or 6."
        public bool Read(AssetsFileReader reader, /*AssetsFileVerifyLogger errorLogger = NULL,*/ bool allowCompressed = false)
        {
            reader.ReadNullTerminated();
            if (reader.ReadUInt32() == 6)
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
                    } else
                    {
                        new NotImplementedException("Non UnityFS bundles are not supported yet.");
                    }
                }
            } else if (reader.ReadUInt32() == 3)
            {
                new NotImplementedException("Version 3 bundles are not supported yet.");
            } else
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
        //-todo, optionally export to memory (may already be possible dunno for sure)
        //-also todo, use a faster custom bundle decompressor. currently a copy paste of unity studio's
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
    };
}
