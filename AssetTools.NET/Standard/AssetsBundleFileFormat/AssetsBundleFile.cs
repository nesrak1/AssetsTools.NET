﻿using AssetsTools.NET.Extra;
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
            switch (version)
            {
                case 3:
                    reader.Position = 0;
                    bundleHeader3 = new AssetBundleHeader03();
                    bundleHeader3.Read(reader);
                    if (bundleHeader3.signature != "UnityRaw")
                        throw new NotImplementedException("Non UnityRaw bundles are not supported yet.");

                    if (bundleHeader3.blockList.Any(b => b.compressed != b.uncompressed))
                        throw new NotImplementedException("Compressed UnityRaw bundles are not supported yet.");
                    
                    assetsLists3 = new AssetsList();
                    assetsLists3.Read(reader);
                    return true;

                case 6:
                case 7:
                    reader.Position = 0;
                    bundleHeader6 = new AssetBundleHeader06();
                    bundleHeader6.Read(reader);
                    if (bundleHeader6.fileVersion >= 7)
                        reader.Align16();

                    if (bundleHeader6.signature != "UnityFS")
                        throw new NotImplementedException("Non UnityFS bundles are not supported yet.");

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

                default:
                    throw new Exception("AssetsBundleFile.Read : Unknown file version!");
            }
        }

        public bool Write(AssetsFileWriter writer, List<BundleReplacer> replacers, ClassDatabaseFile typeMeta = null)
        {
            if (bundleHeader3 != null)
                return Write03(writer, replacers);

            if (bundleHeader6 != null)
                return Write06(writer, replacers);

            return false;
        }

        private bool Write03(AssetsFileWriter writer, List<BundleReplacer> replacers)
        {
            AssetBundleHeader03 newBundleHeader3 = bundleHeader3.Clone();
            newBundleHeader3.blockList = new[] { new AssetsBundleOffsetPair() };
            newBundleHeader3.Write(writer);

            newBundleHeader3.bundleDataOffs = (uint)writer.Position;

            Dictionary<string, BundleReplacer> addingReplacers =
                replacers.Where(r => r.GetReplacementType() == BundleReplacementType.AddOrModify)
                         .ToDictionary(r => r.GetEntryName());

            List<AssetsBundleEntry> newEntries = new List<AssetsBundleEntry>();
            Dictionary<AssetsBundleEntry, AssetsBundleEntry> newEntryToOldEntry = new Dictionary<AssetsBundleEntry, AssetsBundleEntry>();
            Dictionary<AssetsBundleEntry, BundleReplacer> newEntryToReplacer = new Dictionary<AssetsBundleEntry, BundleReplacer>();
            foreach (AssetsBundleEntry entry in assetsLists3.entries)
            {
                addingReplacers.Remove(entry.name);

                AssetsBundleEntry newEntry = null;
                BundleReplacer replacer = replacers.FirstOrDefault(r => r.GetOriginalEntryName() == entry.name);
                if (replacer == null || replacer.GetReplacementType() == BundleReplacementType.AddOrModify)
                    newEntry = new AssetsBundleEntry { name = entry.name };
                else if (replacer.GetReplacementType() == BundleReplacementType.Rename)
                    newEntry = new AssetsBundleEntry { name = replacer.GetEntryName() };

                if (newEntry != null)
                {
                    newEntries.Add(newEntry);
                    newEntryToOldEntry.Add(newEntry, entry);
                    if (replacer != null && replacer.GetReplacementType() == BundleReplacementType.AddOrModify)
                        newEntryToReplacer.Add(newEntry, replacer);
                }
            }

            foreach (BundleReplacer replacer in addingReplacers.Values)
            {
                AssetsBundleEntry newEntry = new AssetsBundleEntry { name = replacer.GetEntryName() };
                newEntries.Add(newEntry);
                newEntryToReplacer.Add(newEntry, replacer);
            }

            bundleHeader3.numberOfAssetsToDownload = (uint)newEntries.Count;

            AssetsList newAssetsList = new AssetsList { entries = newEntries.ToArray() };
            newAssetsList.Write(writer);
            writer.Align16();

            bundleHeader3.assetsListSize = (uint)writer.Position - bundleHeader3.bundleDataOffs;

            foreach (AssetsBundleEntry newEntry in newEntries)
            {
                long newEntryPosition = writer.Position;
                newEntry.offset = (uint)(writer.Position - bundleHeader3.bundleDataOffs);

                if (newEntryToReplacer.TryGetValue(newEntry, out BundleReplacer replacer))
                {
                    replacer.Write(writer);
                }
                else
                {
                    AssetsBundleEntry oldEntry = newEntryToOldEntry[newEntry];
                    reader.Position = bundleHeader3.bundleDataOffs + oldEntry.offset;
                    reader.BaseStream.CopyToCompat(writer.BaseStream, oldEntry.length);
                }

                newEntry.length = (uint)(writer.Position - newEntryPosition);
            }

            newBundleHeader3.minimumStreamedBytes = (uint)writer.Position;
            newBundleHeader3.blockList[0].compressed = (uint)writer.Position - newBundleHeader3.bundleDataOffs;
            newBundleHeader3.blockList[0].uncompressed = (uint)writer.Position - newBundleHeader3.bundleDataOffs;
            newBundleHeader3.fileSize2 = (uint)writer.Position;

            writer.Position = 0;
            newBundleHeader3.Write(writer);
            newAssetsList.Write(writer);

            writer.Position = newBundleHeader3.fileSize2;
            return true;
        }

        private bool Write06(AssetsFileWriter writer, List<BundleReplacer> replacers)
        {
            bundleHeader6.Write(writer);

            if (bundleHeader6.fileVersion >= 7)
            {
                writer.Align16();
            }

            AssetBundleBlockAndDirectoryList06 newBundleInf6 = new AssetBundleBlockAndDirectoryList06
            {
                checksumLow = 0,
                checksumHigh = 0
            };
            //I could map the assets to their blocks but I don't
            //have any more-than-1-block files to test on
            //this should work just fine as far as I know
            newBundleInf6.blockInf = new[]
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

            //write all original files, modify sizes if needed and skip those to be removed
            for (int i = 0; i < bundleInf6.directoryCount; i++)
            {
                AssetBundleDirectoryInfo06 info = bundleInf6.dirInf[i];
                originalDirInfos.Add(info);
                AssetBundleDirectoryInfo06 newInfo = new AssetBundleDirectoryInfo06
                {
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
                        newInfo = new AssetBundleDirectoryInfo06
                        {
                            flags = info.flags,
                            name = replacer.GetEntryName()
                        };
                    }
                    else if (replacer.GetReplacementType() == BundleReplacementType.Rename)
                    {
                        newInfo = new AssetBundleDirectoryInfo06
                        {
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
                        flags = 0x04, //idk it just works (tm)
                        name = replacer.GetEntryName()
                    };

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

        public static bool IsBundleFile(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);
            if (stream.Length < 8)
                return false;

            byte[] magic = new byte[8];
            stream.Read(magic, 0, 8);

            foreach (string supportedMagicString in new[] { "UnityRaw", "UnityFS" })
            {
                byte[] supportedMagicBytes = Encoding.ASCII.GetBytes(supportedMagicString);
                if (magic.Take(supportedMagicBytes.Length).SequenceEqual(supportedMagicBytes))
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
            if (bundleHeader3 != null)
                return IsAssetsFile(assetsLists3.entries[index]);

            if (bundleHeader6 != null)
                return IsAssetsFile(bundleInf6.dirInf[index]);

            return false;
        }

        public bool IsAssetsFile(AssetsBundleEntry entry)
        {
            long offset = bundleHeader3.bundleDataOffs + entry.offset;
            return AssetsFile.IsAssetsFile(reader, offset, entry.length);
        }

        public bool IsAssetsFile(AssetBundleDirectoryInfo06 entry)
        {
            long offset = bundleHeader6.GetFileDataOffset() + entry.offset;
            return AssetsFile.IsAssetsFile(reader, offset, entry.decompressedSize);
        }

        public int GetAssetsFileIndex(string name)
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
                AssetBundleDirectoryInfo06[] dirInf = bundleInf6.dirInf;
                for (int i = 0; i < dirInf.Length; i++)
                {
                    if (dirInf[i].name == name)
                        return i;
                }
            }

            return -1;
        }

        public string GetAssetsFileName(int index)
        {
            if (bundleHeader3 != null)
                return assetsLists3.entries[index].name;

            if (bundleHeader6 != null)
                return bundleInf6.dirInf[index].name;

            return null;
        }

        internal void GetAssetsFileRange(int index, out long offset, out int length)
        {
            if (bundleHeader3 != null)
            {
                AssetsBundleEntry entry = assetsLists3.entries[index];
                offset = bundleHeader3.bundleDataOffs + entry.offset;
                length = (int)entry.length;
            }
            else if (bundleHeader6 != null)
            {
                offset = bundleHeader6.GetFileDataOffset() + bundleInf6.dirInf[index].offset;
                length = (int)bundleInf6.dirInf[index].decompressedSize;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public enum AssetBundleCompressionType
    {
        NONE = 0,
        LZMA,
        LZ4
    }
}
