using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using LZ4ps;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetsTools.NET
{
    public class AssetBundleFile
    {
        public AssetBundleHeader03 bundleHeader3;
        public AssetBundleHeader06 bundleHeader6;

        public AssetsList assetsLists3;
        public AssetBundleBlockAndDirectoryList06 bundleInf6;

        public AssetsFileReader reader;

        public void Close()
        {
            reader.Close();
        }
        public bool Read(AssetsFileReader reader, bool allowCompressed = false)
        {
            this.reader = reader;
            reader.ReadNullTerminated();
            uint version = reader.ReadUInt32();
            if (version == 6 || version == 7)
            {
                reader.Position = 0;
                bundleHeader6 = new AssetBundleHeader06();
                bundleHeader6.Read(reader);
                if (bundleHeader6.fileVersion >= 7)
                {
                    reader.Align16();
                }
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
        public bool Write(AssetsFileWriter writer, List<BundleReplacer> replacers, ClassDatabaseFile typeMeta = null)
        {
            bundleHeader6.Write(writer);

            if (bundleHeader6.fileVersion >= 7)
            {
                writer.Align16();
            }

            AssetBundleBlockAndDirectoryList06 newBundleInf6 = new AssetBundleBlockAndDirectoryList06()
            {
                checksumLow = 0,
                checksumHigh = 0
            };
            //I could map the assets to their blocks but I don't
            //have any more-than-1-block files to test on
            //this should work just fine as far as I know
            newBundleInf6.blockInf = new AssetBundleBlockInfo06[]
            {
                new AssetBundleBlockInfo06
                {
                    compressedSize = 0,
                    decompressedSize = 0,
                    flags = 0x40
                }
            };

            //assets that did not have their data modified but need
            //the original info to read from the original file
            var newToOriginalDirInfoLookup = new Dictionary<AssetBundleDirectoryInfo06, AssetBundleDirectoryInfo06>();
            List<AssetBundleDirectoryInfo06> originalDirInfos = new List<AssetBundleDirectoryInfo06>();
            List<AssetBundleDirectoryInfo06> dirInfos = new List<AssetBundleDirectoryInfo06>();
            List<BundleReplacer> currentReplacers = replacers.ToList();
            //this is kind of useless at the moment but leaving it here
            //because if the AssetsFile size can be precalculated in the
            //future, we can use this to skip rewriting sizes
            long currentOffset = 0;

            //write all original files, modify sizes if needed and skip those to be removed
            for (int i = 0; i < bundleInf6.directoryCount; i++)
            {
                AssetBundleDirectoryInfo06 info = bundleInf6.dirInf[i];
                originalDirInfos.Add(info);
                AssetBundleDirectoryInfo06 newInfo = new AssetBundleDirectoryInfo06()
                {
                    offset = currentOffset,
                    decompressedSize = info.decompressedSize,
                    flags = info.flags,
                    name = info.name
                };
                BundleReplacer replacer = currentReplacers.FirstOrDefault(n => n.GetOriginalEntryName() == newInfo.name);
                if (replacer != null)
                {
                    currentReplacers.Remove(replacer);
                    if (replacer.GetReplacementType() == BundleReplacementType.AddOrModify)
                    {
                        newInfo = new AssetBundleDirectoryInfo06()
                        {
                            offset = currentOffset,
                            decompressedSize = replacer.GetSize(),
                            flags = info.flags,
                            name = replacer.GetEntryName()
                        };
                    }
                    else if (replacer.GetReplacementType() == BundleReplacementType.Rename)
                    {
                        newInfo = new AssetBundleDirectoryInfo06()
                        {
                            offset = currentOffset,
                            decompressedSize = info.decompressedSize,
                            flags = info.flags,
                            name = replacer.GetEntryName()
                        };
                        newToOriginalDirInfoLookup[newInfo] = info;
                    }
                    else if (replacer.GetReplacementType() == BundleReplacementType.Remove)
                    {
                        continue;
                    }
                }
                else
                {
                    newToOriginalDirInfoLookup[newInfo] = info;
                }

                if (newInfo.decompressedSize != -1)
                {
                    currentOffset += newInfo.decompressedSize;
                }
            
                dirInfos.Add(newInfo);
            }

            //write new files
            while (currentReplacers.Count > 0)
            {
                BundleReplacer replacer = currentReplacers[0];
                if (replacer.GetReplacementType() == BundleReplacementType.AddOrModify)
                {
                    AssetBundleDirectoryInfo06 info = new AssetBundleDirectoryInfo06()
                    {
                        offset = currentOffset,
                        decompressedSize = replacer.GetSize(),
                        flags = (uint)(replacer.HasSerializedData() ? 0x04 : 0x00),
                        name = replacer.GetEntryName()
                    };
                    currentOffset += info.decompressedSize;

                    dirInfos.Add(info);
                }
                currentReplacers.Remove(replacer);
            }

            //write the listings
            long bundleInfPos = writer.Position;
            newBundleInf6.dirInf = dirInfos.ToArray(); //this is only here to allocate enough space so it's fine if it's inaccurate
            newBundleInf6.Write(writer);

            long assetDataPos = writer.Position;

            //actually write the file data to the bundle now
            for (int i = 0; i < dirInfos.Count; i++)
            {
                AssetBundleDirectoryInfo06 info = dirInfos[i];
                BundleReplacer replacer = replacers.FirstOrDefault(n => n.GetEntryName() == info.name);
                if (replacer != null)
                {
                    if (replacer.GetReplacementType() == BundleReplacementType.AddOrModify)
                    {
                        long startPos = writer.Position;
                        long endPos = replacer.Write(writer);
                        long size = endPos - startPos;

                        dirInfos[i].decompressedSize = size;
                        dirInfos[i].offset = startPos - assetDataPos;
                    }
                    else if (replacer.GetReplacementType() == BundleReplacementType.Remove)
                    {
                        continue;
                    }
                }
                else
                {
                    if (newToOriginalDirInfoLookup.TryGetValue(info, out AssetBundleDirectoryInfo06 originalInfo))
                    {
                        long startPos = writer.Position;

                        reader.Position = bundleHeader6.GetFileDataOffset() + originalInfo.offset;
                        reader.BaseStream.CopyToCompat(writer.BaseStream, originalInfo.decompressedSize);

                        dirInfos[i].offset = startPos - assetDataPos;
                    }
                }
            }

            //now that we know what the sizes are of the written files let's go back and fix them
            long finalSize = writer.Position;
            uint assetSize = (uint)(finalSize - assetDataPos);

            writer.Position = bundleInfPos;
            newBundleInf6.blockInf[0].decompressedSize = assetSize;
            newBundleInf6.blockInf[0].compressedSize = assetSize;
            newBundleInf6.dirInf = dirInfos.ToArray();
            newBundleInf6.Write(writer);

            uint infoSize = (uint)(assetDataPos - bundleInfPos);

            writer.Position = 0;
            AssetBundleHeader06 newBundleHeader6 = new AssetBundleHeader06()
            {
                signature = bundleHeader6.signature,
                fileVersion = bundleHeader6.fileVersion,
                minPlayerVersion = bundleHeader6.minPlayerVersion,
                fileEngineVersion = bundleHeader6.fileEngineVersion,
                totalFileSize = finalSize,
                compressedSize = infoSize,
                decompressedSize = infoSize,
                flags = bundleHeader6.flags & unchecked((uint)~0x80) & unchecked((uint)~0x3f) //unset info at end flag and compression value
            };
            newBundleHeader6.Write(writer);

            return true;
        }

        public bool Unpack(AssetsFileReader reader, AssetsFileWriter writer)
        {
            reader.Position = 0;
            if (Read(reader, true))
            {
                reader.Position = bundleHeader6.GetBundleInfoOffset();
                MemoryStream blocksInfoStream;
                AssetsFileReader memReader;
                int compressedSize = (int)bundleHeader6.compressedSize;
                switch (bundleHeader6.GetCompressionType())
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
                if (bundleHeader6.GetCompressionType() != 0)
                {
                    using (memReader = new AssetsFileReader(blocksInfoStream))
                    {
                        memReader.Position = 0;
                        bundleInf6.Read(0, memReader);
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
                    flags = bundleHeader6.flags & (0x40 | 0x200) //set compression and block position to 0
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
                    newBundleInf6.blockInf[i] = new AssetBundleBlockInfo06()
                    {
                        compressedSize = bundleInf6.blockInf[i].decompressedSize,
                        decompressedSize = bundleInf6.blockInf[i].decompressedSize,
                        flags = (ushort)(bundleInf6.blockInf[i].flags & 0xC0) //set compression to none
                    };
                }
                newBundleInf6.dirInf = new AssetBundleDirectoryInfo06[newBundleInf6.directoryCount];
                for (int i = 0; i < newBundleInf6.directoryCount; i++)
                {
                    newBundleInf6.dirInf[i] = new AssetBundleDirectoryInfo06()
                    {
                        offset = bundleInf6.dirInf[i].offset,
                        decompressedSize = bundleInf6.dirInf[i].decompressedSize,
                        flags = bundleInf6.dirInf[i].flags,
                        name = bundleInf6.dirInf[i].name
                    };
                }
                newBundleHeader6.Write(writer);
                if (newBundleHeader6.fileVersion >= 7)
                {
                    writer.Align16();
                }
                newBundleInf6.Write(writer);
                if ((newBundleHeader6.flags & 0x200) != 0)
                {
                    writer.Align16();
                }

                reader.Position = bundleHeader6.GetFileDataOffset();
                for (int i = 0; i < newBundleInf6.blockCount; i++)
                {
                    AssetBundleBlockInfo06 info = bundleInf6.blockInf[i];
                    switch (info.GetCompressionType())
                    {
                        case 0:
                            reader.BaseStream.CopyToCompat(writer.BaseStream, info.compressedSize);
                            break;
                        case 1:
                            SevenZipHelper.StreamDecompress(reader.BaseStream, writer.BaseStream, info.compressedSize, info.decompressedSize);
                            break;
                        case 2:
                        case 3:
                            using (MemoryStream tempMs = new MemoryStream())
                            {
                                reader.BaseStream.CopyToCompat(tempMs, info.compressedSize);
                                tempMs.Position = 0;

                                using (Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs))
                                {
                                    decoder.CopyToCompat(writer.BaseStream, info.decompressedSize);
                                }
                            }
                            break;
                    }
                }
                return true;
            }
            return false;
        }
        public bool Pack(AssetsFileReader reader, AssetsFileWriter writer, AssetBundleCompressionType compType, bool blockDirAtEnd = true)
        {
            reader.Position = 0;
            writer.Position = 0;
            if (Read(reader, false))
            {
                AssetBundleHeader06 newHeader = new AssetBundleHeader06()
                {
                    signature = bundleHeader6.signature,
                    fileVersion = bundleHeader6.fileVersion,
                    minPlayerVersion = bundleHeader6.minPlayerVersion,
                    fileEngineVersion = bundleHeader6.fileEngineVersion,
                    totalFileSize = 0,
                    compressedSize = 0,
                    decompressedSize = 0,
                    flags = (uint)(0x43 | (blockDirAtEnd ? 0x80 : 0x00))
                };

                AssetBundleBlockAndDirectoryList06 newBlockAndDirList = new AssetBundleBlockAndDirectoryList06()
                {
                    checksumLow = 0,
                    checksumHigh = 0,
                    blockCount = 0,
                    blockInf = null,
                    directoryCount = bundleInf6.directoryCount,
                    dirInf = bundleInf6.dirInf
                };

                //write header now and overwrite it later
                long startPos = writer.Position;

                newHeader.Write(writer);
                if (newHeader.fileVersion >= 7)
                    writer.Align16();

                int headerSize = (int)(writer.Position - startPos);

                long totalCompressedSize = 0;
                List<AssetBundleBlockInfo06> newBlocks = new List<AssetBundleBlockInfo06>();
                List<Stream> newStreams = new List<Stream>(); //used if blockDirAtEnd == false

                long fileDataOffset = bundleHeader6.GetFileDataOffset();
                int fileDataLength = (int)(bundleHeader6.totalFileSize - fileDataOffset);

                SegmentStream bundleDataStream = new SegmentStream(reader.BaseStream, fileDataOffset, fileDataLength);

                switch (compType)
                {
                    case AssetBundleCompressionType.LZMA:
                    {
                        Stream writeStream;
                        if (blockDirAtEnd)
                            writeStream = writer.BaseStream;
                        else
                            writeStream = GetTempFileStream();

                        long writeStreamStart = writeStream.Position;
                        SevenZipHelper.Compress(bundleDataStream, writeStream);
                        uint writeStreamLength = (uint)(writeStream.Position - writeStreamStart);

                        AssetBundleBlockInfo06 blockInfo = new AssetBundleBlockInfo06()
                        {
                            compressedSize = writeStreamLength,
                            decompressedSize = (uint)fileDataLength,
                            flags = 0x41
                        };

                        totalCompressedSize += blockInfo.compressedSize;
                        newBlocks.Add(blockInfo);

                        if (!blockDirAtEnd)
                            newStreams.Add(writeStream);

                        break;
                    }
                    case AssetBundleCompressionType.LZ4:
                    {
                        //compress into 0x20000 blocks
                        BinaryReader bundleDataReader = new BinaryReader(bundleDataStream);
                        byte[] uncompressedBlock = bundleDataReader.ReadBytes(0x20000);
                        while (uncompressedBlock.Length != 0)
                        {
                            Stream writeStream;
                            if (blockDirAtEnd)
                                writeStream = writer.BaseStream;
                            else
                                writeStream = GetTempFileStream();

                            byte[] compressedBlock = LZ4Codec.Encode32HC(uncompressedBlock, 0, uncompressedBlock.Length);

                            if (compressedBlock.Length > uncompressedBlock.Length)
                            {
                                writeStream.Write(uncompressedBlock, 0, uncompressedBlock.Length);

                                AssetBundleBlockInfo06 blockInfo = new AssetBundleBlockInfo06()
                                {
                                    compressedSize = (uint)uncompressedBlock.Length,
                                    decompressedSize = (uint)uncompressedBlock.Length,
                                    flags = 0x0
                                };

                                totalCompressedSize += blockInfo.compressedSize;

                                newBlocks.Add(blockInfo);
                            }
                            else
                            {
                                writeStream.Write(compressedBlock, 0, compressedBlock.Length);

                                AssetBundleBlockInfo06 blockInfo = new AssetBundleBlockInfo06()
                                {
                                    compressedSize = (uint)compressedBlock.Length,
                                    decompressedSize = (uint)uncompressedBlock.Length,
                                    flags = 0x3
                                };

                                totalCompressedSize += blockInfo.compressedSize;

                                newBlocks.Add(blockInfo);
                            }

                            if (!blockDirAtEnd)
                                newStreams.Add(writeStream);

                            uncompressedBlock = bundleDataReader.ReadBytes(0x20000);
                        }
                        break;
                    }
                    case AssetBundleCompressionType.NONE:
                    {
                        AssetBundleBlockInfo06 blockInfo = new AssetBundleBlockInfo06()
                        {
                            compressedSize = (uint)fileDataLength,
                            decompressedSize = (uint)fileDataLength,
                            flags = 0x00
                        };

                        totalCompressedSize += blockInfo.compressedSize;

                        newBlocks.Add(blockInfo);

                        if (blockDirAtEnd)
                            bundleDataStream.CopyToCompat(writer.BaseStream);
                        else
                            newStreams.Add(bundleDataStream);

                        break;
                    }
                }

                newBlockAndDirList.blockInf = newBlocks.ToArray();

                byte[] bundleInfoBytes;
                using (MemoryStream memStream = new MemoryStream())
                {
                    AssetsFileWriter infoWriter = new AssetsFileWriter(memStream);
                    newBlockAndDirList.Write(infoWriter);
                    bundleInfoBytes = memStream.ToArray();
                }

                //listing is usually lz4 even if the data blocks are lzma
                byte[] bundleInfoBytesCom = LZ4Codec.Encode32HC(bundleInfoBytes, 0, bundleInfoBytes.Length);

                long totalFileSize = headerSize + bundleInfoBytesCom.Length + totalCompressedSize;
                newHeader.totalFileSize = totalFileSize;
                newHeader.decompressedSize = (uint)bundleInfoBytes.Length;
                newHeader.compressedSize = (uint)bundleInfoBytesCom.Length;

                if (!blockDirAtEnd)
                {
                    writer.Write(bundleInfoBytesCom);
                    foreach (Stream newStream in newStreams)
                    {
                        newStream.Position = 0;
                        newStream.CopyToCompat(writer.BaseStream);
                        newStream.Close();
                    }
                }
                else
                {
                    writer.Write(bundleInfoBytesCom);
                }

                writer.Position = 0;
                newHeader.Write(writer);
                if (newHeader.fileVersion >= 7)
                    writer.Align16();

                return true;
            }
            return false;
        }

        public int NumFiles
        {
            get
            {
                if (bundleHeader3 != null)
                    return assetsLists3.entries.Length;

                if (bundleHeader6 != null)
                    return bundleInf6.dirInf.Length;

                return 0;
            }
        }

        public bool IsAssetsFile(int index)
        {
            GetFileRange(index, out long offset, out long length);
            return AssetsFile.IsAssetsFile(reader, offset, length);
        }

        [Obsolete]
        public bool IsAssetsFile(AssetsFileReader reader, AssetBundleDirectoryInfo06 entry)
        {
            long offset = bundleHeader6.GetFileDataOffset() + entry.offset;
            return AssetsFile.IsAssetsFile(reader, offset, entry.decompressedSize);
        }

        public int GetFileIndex(string name)
        {
            if (bundleHeader3 != null)
            {
                for (int i = 0; i < assetsLists3.entries.Length; i++)
                {
                    if (assetsLists3.entries[i].name == name)
                        return i;
                }
            }
            else if (bundleHeader6 != null)
            {
                for (int i = 0; i < bundleInf6.dirInf.Length; i++)
                {
                    if (bundleInf6.dirInf[i].name == name)
                        return i;
                }
            }

            return -1;
        }

        public string GetFileName(int index)
        {
            if (bundleHeader3 != null)
                return assetsLists3.entries[index].name;

            if (bundleHeader6 != null)
                return bundleInf6.dirInf[index].name;

            return null;
        }

        internal void GetFileRange(int index, out long offset, out long length)
        {
            if (bundleHeader3 != null)
            {
                AssetsBundleEntry entry = assetsLists3.entries[index];
                offset = bundleHeader3.bundleDataOffs + entry.offset;
                length = entry.length;
            }
            else if (bundleHeader6 != null)
            {
                AssetBundleDirectoryInfo06 entry = bundleInf6.dirInf[index];
                offset = bundleHeader6.GetFileDataOffset() + entry.offset;
                length = entry.decompressedSize;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private FileStream GetTempFileStream()
        {
            string tempFilePath = Path.GetTempFileName();
            FileStream tempFileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            return tempFileStream;
        }
    }

    public enum AssetBundleCompressionType
    {
        NONE = 0,
        LZMA,
        LZ4
    }
}
