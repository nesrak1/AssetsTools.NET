using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using LZ4ps;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        flags = 0x04, //idk it just works (tm)
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
        public bool Pack(AssetsFileReader reader, AssetsFileWriter writer, AssetBundleCompressionType compType)
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
                    flags = 0x43
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

                List<AssetBundleBlockInfo06> newBlocks = new List<AssetBundleBlockInfo06>();

                reader.Position = bundleHeader6.GetFileDataOffset();
                int fileDataLength = (int)(bundleHeader6.totalFileSize - reader.Position);
                byte[] fileData = reader.ReadBytes(fileDataLength);

                //todo, we just write everything to memory and then write to file
                //we could calculate the blocks we need ahead of time and correctly
                //size the block listing before this so we can write directly to file
                byte[] compressedFileData;
                switch (compType)
                {
                    case AssetBundleCompressionType.LZMA:
                    {
                        compressedFileData = SevenZipHelper.Compress(fileData);
                        newBlocks.Add(new AssetBundleBlockInfo06()
                        {
                            compressedSize = (uint)compressedFileData.Length,
                            decompressedSize = (uint)fileData.Length,
                            flags = 0x41
                        });
                        break;
                    }
                    case AssetBundleCompressionType.LZ4:
                    {
                        using (var memStreamCom = new MemoryStream())
                        using (var binaryWriter = new BinaryWriter(memStreamCom))
                        {
                            using (var memStreamUnc = new MemoryStream(fileData))
                            using (var binaryReader = new BinaryReader(memStreamUnc))
                            {
                                //compress into 0x20000 blocks
                                byte[] uncompressedBlock = binaryReader.ReadBytes(131072);
                                while (uncompressedBlock.Length != 0)
                                {
                                    byte[] compressedBlock = LZ4Codec.Encode32HC(uncompressedBlock, 0, uncompressedBlock.Length);

                                    if (compressedBlock.Length > uncompressedBlock.Length)
                                    {
                                        newBlocks.Add(new AssetBundleBlockInfo06()
                                        {
                                            compressedSize = (uint)uncompressedBlock.Length,
                                            decompressedSize = (uint)uncompressedBlock.Length,
                                            flags = 0x0
                                        });
                                        binaryWriter.Write(uncompressedBlock);
                                    }
                                    else
                                    {
                                        newBlocks.Add(new AssetBundleBlockInfo06()
                                        {
                                            compressedSize = (uint)compressedBlock.Length,
                                            decompressedSize = (uint)uncompressedBlock.Length,
                                            flags = 0x3
                                        });
                                        binaryWriter.Write(compressedBlock);
                                    }

                                    uncompressedBlock = binaryReader.ReadBytes(131072);
                                }
                            }

                            compressedFileData = memStreamCom.ToArray();
                        }
                        break;
                    }
                    case AssetBundleCompressionType.NONE:
                    {
                        compressedFileData = fileData;
                        newBlocks.Add(new AssetBundleBlockInfo06()
                        {
                            compressedSize = (uint)fileData.Length,
                            decompressedSize = (uint)fileData.Length,
                            flags = 0x00
                        });
                        break;
                    }
                    default:
                    {
                        return false;
                    }
                }

                newBlockAndDirList.blockInf = newBlocks.ToArray();

                byte[] bundleInfoBytes;
                using (var memStream = new MemoryStream())
                {
                    var afw = new AssetsFileWriter(memStream);
                    newBlockAndDirList.Write(afw);
                    bundleInfoBytes = memStream.ToArray();
                }

                if (bundleInfoBytes == null || bundleInfoBytes.Length == 0)
                    return false;

                //listing is usually lz4 even if the data blocks are lzma
                byte[] bundleInfoBytesCom = LZ4Codec.Encode32HC(bundleInfoBytes, 0, bundleInfoBytes.Length);

                byte[] bundleHeaderBytes = null;
                using (var memStream = new MemoryStream())
                {
                    var afw = new AssetsFileWriter(memStream);
                    newHeader.Write(afw);
                    bundleHeaderBytes = memStream.ToArray();
                }

                if (bundleHeaderBytes == null || bundleHeaderBytes.Length == 0)
                    return false;

                uint totalFileSize = (uint)(bundleHeaderBytes.Length + bundleInfoBytesCom.Length + compressedFileData.Length);
                newHeader.totalFileSize = totalFileSize;
                newHeader.decompressedSize = (uint)bundleInfoBytes.Length;
                newHeader.compressedSize = (uint)bundleInfoBytesCom.Length;

                newHeader.Write(writer);
                if (newHeader.fileVersion >= 7)
                    writer.Align16();

                writer.Write(bundleInfoBytesCom);
                writer.Write(compressedFileData);

                return true;
            }
            return false;
        }
        public bool IsAssetsFile(AssetsFileReader reader, AssetBundleDirectoryInfo06 entry)
        {
            //todo - not fully implemented
            long offset = bundleHeader6.GetFileDataOffset() + entry.offset;
            if (entry.decompressedSize < 0x30)
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

            if (possibleFormat >= 0x16)
            {
                reader.Position += 0x1c;
            }

            string possibleVersion = "";
            char curChar;
            while (reader.Position < reader.BaseStream.Length && (curChar = (char)reader.ReadByte()) != 0x00)
            {
                possibleVersion += curChar;
                if (possibleVersion.Length > 0xFF)
                {
                    return false;
                }
            }

            string emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.]", "");
            string fullVersion = Regex.Replace(possibleVersion, "[^a-zA-Z0-9\\.]", "");
            return emptyVersion == "" && fullVersion.Length > 0;
        }
    }
    public enum AssetBundleCompressionType
    {
        NONE = 0,
        LZMA,
        LZ4
    }
}
