using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using LZ4ps;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET
{
    public class AssetBundleFile
    {
        /// <summary>
        /// Bundle header. Contains bundle engine version.
        /// </summary>
        public AssetBundleHeader Header { get; set; }
        /// <summary>
        /// List of compression blocks and file info (file names, address in file, etc.)
        /// </summary>
        public AssetBundleBlockAndDirInfo BlockAndDirInfo { get; set; }
        /// <summary>
        /// Reader for data block of bundle
        /// </summary>
        public AssetsFileReader DataReader { get; set; }
        /// <summary>
        /// Is data reader reading compressed data? Only LZMA bundles set this to true.
        /// </summary>
        public bool DataIsCompressed { get; set; }

        public AssetsFileReader Reader;

        public void Close()
        {
            Reader.Close();
            DataReader.Close();
        }

        public void Read(AssetsFileReader reader)
        {
            Reader = reader;
            Reader.Position = 0;
            Reader.BigEndian = true;

            string magic = reader.ReadNullTerminated(); // skipped and read by header
            uint version = reader.ReadUInt32();
            if (version >= 6 || version <= 8)
            {
                Reader.Position = 0;

                Header = new AssetBundleHeader();
                Header.Read(reader);

                if (Header.Version >= 7)
                {
                    reader.Align16();
                }

                if (Header.Signature == "UnityFS")
                {
                    UnpackInfoOnly();
                }
                else
                {
                    new NotImplementedException("Non UnityFS bundles are not supported yet.");
                }
            }
            else
            {
                new NotImplementedException($"Version {version} bundles are not supported yet.");
            }
        }

        public void Write(AssetsFileWriter writer, long filePos = 0)
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            if (Header.Signature != "UnityFS")
                throw new NotImplementedException("Non UnityFS bundles are not supported yet.");

            if (DataIsCompressed)
                throw new Exception("Bundles must be decompressed before writing.");

            long writeStart = filePos;
            if (filePos == -1)
                writeStart = writer.Position;
            else
                writer.Position = filePos;

            List<AssetBundleDirectoryInfo> directoryInfos = BlockAndDirInfo.DirectoryInfos;

            Header.Write(writer);

            if (Header.Version >= 7)
            {
                writer.Align16();
            }

            AssetBundleBlockInfo newBlockInfo = new AssetBundleBlockInfo
            {
                CompressedSize = 0,
                DecompressedSize = 0,
                Flags = 0x40
            };

            AssetBundleBlockAndDirInfo newBundleInf = new AssetBundleBlockAndDirInfo()
            {
                Hash = new Hash128(),
                BlockInfos = new AssetBundleBlockInfo[] { newBlockInfo }
            };

            List<AssetBundleDirectoryInfo> dirInfos = new List<AssetBundleDirectoryInfo>();

            // write all original file infos and skip those to be removed
            int dirCount = directoryInfos.Count;
            for (int i = 0; i < dirCount; i++)
            {
                AssetBundleDirectoryInfo dirInfo = directoryInfos[i];
                ContentReplacerType replacerType = dirInfo.ReplacerType;

                if (replacerType == ContentReplacerType.Remove)
                    continue;

                if (replacerType == ContentReplacerType.AddOrModify)
                {
                    if (dirInfo.Replacer == null)
                    {
                        throw new Exception($"{nameof(dirInfo.Replacer)} must be non-null when status is Modified!");
                    }
                }

                dirInfos.Add(new AssetBundleDirectoryInfo()
                {
                    // offset and size to be edited later
                    Offset = dirInfo.Offset,
                    DecompressedSize = dirInfo.DecompressedSize,
                    Flags = dirInfo.Flags,
                    Name = dirInfo.Name,
                    Replacer = dirInfo.Replacer,
                });
            }

            // write the listings
            long bundleInfPos = writer.Position;
            // this is only here to allocate enough space so it's fine if it's inaccurate
            newBundleInf.DirectoryInfos = dirInfos;
            newBundleInf.Write(writer);

            if ((Header.FileStreamHeader.Flags & AssetBundleFSHeaderFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                writer.Align16();
            }

            long assetDataPos = writer.Position;

            // write the updated directory infos
            for (int i = 0; i < dirInfos.Count; i++)
            {
                AssetBundleDirectoryInfo dirInfo = dirInfos[i];
                long startPosition = writer.Position;
                long newOffset = startPosition - assetDataPos;

                ContentReplacerType replacerType = dirInfo.ReplacerType;
                if (replacerType == ContentReplacerType.AddOrModify)
                {
                    dirInfo.Replacer.Write(writer);
                }
                else
                {
                    DataReader.Position = dirInfo.Offset;
                    Reader.BaseStream.CopyToCompat(writer.BaseStream, dirInfo.DecompressedSize);
                }

                dirInfo.Offset = newOffset;
                dirInfo.DecompressedSize = writer.Position - startPosition;
            }

            // now that we know what the sizes are of the written files, let's go back and fix them
            long finalSize = writer.Position;
            
            // this would suck :|
            if (finalSize - assetDataPos > uint.MaxValue)
            {
                throw new NotImplementedException("Data larger than max uint not supported yet.");
            }

            uint assetSize = (uint)(finalSize - assetDataPos);

            writer.Position = bundleInfPos;
            newBlockInfo.DecompressedSize = assetSize;
            newBlockInfo.CompressedSize = assetSize;
            newBundleInf.DirectoryInfos = dirInfos;
            newBundleInf.Write(writer);

            uint infoSize = (uint)(assetDataPos - bundleInfPos);

            writer.Position = writeStart;
            AssetBundleHeader newBundleHeader = new AssetBundleHeader
            {
                Signature = Header.Signature,
                Version = Header.Version,
                GenerationVersion = Header.GenerationVersion,
                EngineVersion = Header.EngineVersion,
                FileStreamHeader = new AssetBundleFSHeader
                {
                    TotalFileSize = finalSize,
                    CompressedSize = infoSize,
                    DecompressedSize = infoSize,
                    // unset "info at end" flag and compression value
                    Flags = Header.FileStreamHeader.Flags & ~AssetBundleFSHeaderFlags.BlockAndDirAtEnd & ~AssetBundleFSHeaderFlags.CompressionMask
                }
            };

            newBundleHeader.Write(writer);
        }

        public void Unpack(AssetsFileWriter writer)
        {
            if (Header == null)
                new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            if (Header.Signature != "UnityFS")
                new NotImplementedException("Non UnityFS bundles are not supported yet.");

            AssetBundleFSHeader fsHeader = Header.FileStreamHeader;
            AssetsFileReader reader = DataReader;

            AssetBundleBlockInfo[] blockInfos = BlockAndDirInfo.BlockInfos;
            List<AssetBundleDirectoryInfo> directoryInfos = BlockAndDirInfo.DirectoryInfos;

            AssetBundleHeader newBundleHeader = new AssetBundleHeader()
            {
                Signature = Header.Signature,
                Version = Header.Version,
                GenerationVersion = Header.GenerationVersion,
                EngineVersion = Header.EngineVersion,
                FileStreamHeader = new AssetBundleFSHeader
                {
                    TotalFileSize = 0,
                    CompressedSize = fsHeader.DecompressedSize,
                    DecompressedSize = fsHeader.DecompressedSize,
                    Flags = AssetBundleFSHeaderFlags.HasDirectoryInfo |
                    (
                        (fsHeader.Flags & AssetBundleFSHeaderFlags.BlockInfoNeedPaddingAtStart) != AssetBundleFSHeaderFlags.None ?
                        AssetBundleFSHeaderFlags.BlockInfoNeedPaddingAtStart :
                        AssetBundleFSHeaderFlags.None
                    )
                }
            };

            long fileSize = newBundleHeader.GetFileDataOffset();
            for (int i = 0; i < blockInfos.Length; i++)
            {
                fileSize += blockInfos[i].DecompressedSize;
            }
            newBundleHeader.FileStreamHeader.TotalFileSize = fileSize;

            AssetBundleBlockAndDirInfo newBundleInf = new AssetBundleBlockAndDirInfo()
            {
                Hash = new Hash128(),
                BlockInfos = new AssetBundleBlockInfo[blockInfos.Length],
                DirectoryInfos = new List<AssetBundleDirectoryInfo>(directoryInfos.Count)
            };

            // todo: we should just use one block here
            for (int i = 0; i < newBundleInf.BlockInfos.Length; i++)
            {
                newBundleInf.BlockInfos[i] = new AssetBundleBlockInfo()
                {
                    CompressedSize = blockInfos[i].DecompressedSize,
                    DecompressedSize = blockInfos[i].DecompressedSize,
                    // Set compression to none
                    Flags = (ushort)(blockInfos[i].Flags & (~0x3f))
                };
            }

            for (int i = 0; i < newBundleInf.DirectoryInfos.Count; i++)
            {
                newBundleInf.DirectoryInfos[i] = new AssetBundleDirectoryInfo()
                {
                    Offset = directoryInfos[i].Offset,
                    DecompressedSize = directoryInfos[i].DecompressedSize,
                    Flags = directoryInfos[i].Flags,
                    Name = directoryInfos[i].Name
                };
            }

            newBundleHeader.Write(writer);
            if (newBundleHeader.Version >= 7)
            {
                writer.Align16();
            }
            newBundleInf.Write(writer);
            if ((newBundleHeader.FileStreamHeader.Flags & AssetBundleFSHeaderFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                writer.Align16();
            }

            reader.Position = 0;

            if (DataIsCompressed)
            {
                for (int i = 0; i < newBundleInf.BlockInfos.Length; i++)
                {
                    AssetBundleBlockInfo info = blockInfos[i];
                    switch (info.GetCompressionType())
                    {
                        case 0:
                        {
                            reader.BaseStream.CopyToCompat(writer.BaseStream, info.CompressedSize);
                            break;
                        }
                        case 1:
                        {
                            SevenZipHelper.StreamDecompress(reader.BaseStream, writer.BaseStream, info.CompressedSize, info.DecompressedSize);
                            break;
                        }
                        case 2:
                        case 3:
                        {
                            using (MemoryStream tempMs = new MemoryStream())
                            {
                                reader.BaseStream.CopyToCompat(tempMs, info.CompressedSize);
                                tempMs.Position = 0;

                                using (Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs))
                                {
                                    decoder.CopyToCompat(writer.BaseStream, info.DecompressedSize);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < newBundleInf.BlockInfos.Length; i++)
                {
                    AssetBundleBlockInfo info = blockInfos[i];
                    reader.BaseStream.CopyToCompat(writer.BaseStream, info.DecompressedSize);
                }
            }
        }

        public void Pack(AssetsFileReader reader, AssetsFileWriter writer, AssetBundleCompressionType compType,
            bool blockDirAtEnd = true, IAssetBundleCompressProgress progress = null)
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            if (Header.Signature != "UnityFS")
                throw new NotImplementedException("Non UnityFS bundles are not supported yet.");

            if (DataIsCompressed)
                throw new Exception("Bundles must be decompressed before writing.");

            reader.Position = 0;
            writer.Position = 0;

            AssetBundleFSHeader newFsHeader = new AssetBundleFSHeader
            {
                TotalFileSize = 0,
                CompressedSize = 0,
                DecompressedSize = 0,
                Flags = AssetBundleFSHeaderFlags.LZ4HCCompressed | AssetBundleFSHeaderFlags.HasDirectoryInfo |
                    (blockDirAtEnd ? AssetBundleFSHeaderFlags.BlockAndDirAtEnd : AssetBundleFSHeaderFlags.None)
            };

            AssetBundleHeader newHeader = new AssetBundleHeader()
            {
                Signature = Header.Signature,
                Version = Header.Version,
                GenerationVersion = Header.GenerationVersion,
                EngineVersion = Header.EngineVersion,
                FileStreamHeader = newFsHeader
            };

            AssetBundleBlockAndDirInfo newBlockAndDirList = new AssetBundleBlockAndDirInfo()
            {
                Hash = new Hash128(),
                BlockInfos = null,
                DirectoryInfos = BlockAndDirInfo.DirectoryInfos
            };

            // write header now and overwrite it later
            long startPos = writer.Position;

            newHeader.Write(writer);
            if (newHeader.Version >= 7)
                writer.Align16();

            int headerSize = (int)(writer.Position - startPos);

            long totalCompressedSize = 0;
            List<AssetBundleBlockInfo> newBlocks = new List<AssetBundleBlockInfo>();
            List<Stream> newStreams = new List<Stream>(); // used if blockDirAtEnd == false

            Stream bundleDataStream = DataReader.BaseStream;
            bundleDataStream.Position = 0;

            int fileDataLength = (int)bundleDataStream.Length;

            switch (compType)
            {
                case AssetBundleCompressionType.LZMA:
                {
                    // write to one large lzma block
                    Stream writeStream;
                    if (blockDirAtEnd)
                        writeStream = writer.BaseStream;
                    else
                        writeStream = GetTempFileStream();

                    var lzmaProgress = new AssetBundleLZMAProgress(progress, bundleDataStream.Length);

                    long writeStreamStart = writeStream.Position;
                    SevenZipHelper.Compress(bundleDataStream, writeStream, lzmaProgress);
                    uint writeStreamLength = (uint)(writeStream.Position - writeStreamStart);

                    AssetBundleBlockInfo blockInfo = new AssetBundleBlockInfo()
                    {
                        CompressedSize = writeStreamLength,
                        DecompressedSize = (uint)fileDataLength,
                        Flags = 0x41
                    };

                    totalCompressedSize += blockInfo.CompressedSize;
                    newBlocks.Add(blockInfo);

                    if (!blockDirAtEnd)
                        newStreams.Add(writeStream);

                    if (progress != null)
                    {
                        progress.SetProgress(1.0f);
                    }

                    break;
                }
                case AssetBundleCompressionType.LZ4:
                {
                    // compress into 0x20000 blocks
                    BinaryReader bundleDataReader = new BinaryReader(bundleDataStream);

                    Stream writeStream;
                    if (blockDirAtEnd)
                        writeStream = writer.BaseStream;
                    else
                        writeStream = GetTempFileStream();

                    byte[] uncompressedBlock = bundleDataReader.ReadBytes(0x20000);
                    while (uncompressedBlock.Length != 0)
                    {
                        byte[] compressedBlock = LZ4Codec.Encode32HC(uncompressedBlock, 0, uncompressedBlock.Length);

                        if (progress != null)
                        {
                            progress.SetProgress((float)bundleDataReader.BaseStream.Position / bundleDataReader.BaseStream.Length);
                        }

                        if (compressedBlock.Length > uncompressedBlock.Length)
                        {
                            writeStream.Write(uncompressedBlock, 0, uncompressedBlock.Length);

                            AssetBundleBlockInfo blockInfo = new AssetBundleBlockInfo()
                            {
                                CompressedSize = (uint)uncompressedBlock.Length,
                                DecompressedSize = (uint)uncompressedBlock.Length,
                                Flags = 0x00
                            };

                            totalCompressedSize += blockInfo.CompressedSize;

                            newBlocks.Add(blockInfo);
                        }
                        else
                        {
                            writeStream.Write(compressedBlock, 0, compressedBlock.Length);

                            AssetBundleBlockInfo blockInfo = new AssetBundleBlockInfo()
                            {
                                CompressedSize = (uint)compressedBlock.Length,
                                DecompressedSize = (uint)uncompressedBlock.Length,
                                Flags = 0x03
                            };

                            totalCompressedSize += blockInfo.CompressedSize;

                            newBlocks.Add(blockInfo);
                        }

                        uncompressedBlock = bundleDataReader.ReadBytes(0x20000);
                    }

                    if (!blockDirAtEnd)
                        newStreams.Add(writeStream);

                    if (progress != null)
                    {
                        progress.SetProgress(1.0f);
                    }

                    break;
                }
                case AssetBundleCompressionType.None:
                {
                    AssetBundleBlockInfo blockInfo = new AssetBundleBlockInfo()
                    {
                        CompressedSize = (uint)fileDataLength,
                        DecompressedSize = (uint)fileDataLength,
                        Flags = 0x00
                    };

                    totalCompressedSize += blockInfo.CompressedSize;

                    newBlocks.Add(blockInfo);

                    if (blockDirAtEnd)
                        bundleDataStream.CopyToCompat(writer.BaseStream);
                    else
                        newStreams.Add(bundleDataStream);

                    break;
                }
            }

            newBlockAndDirList.BlockInfos = newBlocks.ToArray();

            byte[] bundleInfoBytes;
            using (MemoryStream memStream = new MemoryStream())
            {
                AssetsFileWriter infoWriter = new AssetsFileWriter(memStream);
                infoWriter.BigEndian = writer.BigEndian;
                newBlockAndDirList.Write(infoWriter);
                bundleInfoBytes = memStream.ToArray();
            }

            // listing is usually lz4 even if the data blocks are lzma
            byte[] bundleInfoBytesCom = LZ4Codec.Encode32HC(bundleInfoBytes, 0, bundleInfoBytes.Length);

            long totalFileSize = headerSize + bundleInfoBytesCom.Length + totalCompressedSize;
            newFsHeader.TotalFileSize = totalFileSize;
            newFsHeader.DecompressedSize = (uint)bundleInfoBytes.Length;
            newFsHeader.CompressedSize = (uint)bundleInfoBytesCom.Length;

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
            if (newHeader.Version >= 7)
                writer.Align16();
        }

        public void UnpackInfoOnly()
        {
            // todo, exceptions
            MemoryStream blocksInfoStream;
            AssetsFileReader memReader;

            Reader.Position = Header.GetBundleInfoOffset();
            if (Header.GetCompressionType() == 0)
            {
                BlockAndDirInfo = new AssetBundleBlockAndDirInfo();
                BlockAndDirInfo.Read(Reader);
            }
            else
            {
                int compressedSize = (int)Header.FileStreamHeader.CompressedSize;
                int decompressedSize = (int)Header.FileStreamHeader.DecompressedSize;

                switch (Header.GetCompressionType())
                {
                    case 1:
                    {
                        using (MemoryStream mstream = new MemoryStream(Reader.ReadBytes(compressedSize)))
                        {
                            blocksInfoStream = new MemoryStream();
                            SevenZipHelper.StreamDecompress(mstream, blocksInfoStream, compressedSize, decompressedSize);
                        }
                        break;
                    }
                    case 2:
                    case 3:
                    {
                        byte[] uncompressedBytes = new byte[Header.FileStreamHeader.DecompressedSize];
                        using (MemoryStream mstream = new MemoryStream(Reader.ReadBytes(compressedSize)))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(uncompressedBytes, 0, (int)Header.FileStreamHeader.DecompressedSize);
                            decoder.Dispose();
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
                    default:
                    {
                        blocksInfoStream = null;
                        break;
                    }
                }

                using (memReader = new AssetsFileReader(blocksInfoStream))
                {
                    memReader.Position = 0;
                    memReader.BigEndian = Reader.BigEndian;
                    BlockAndDirInfo = new AssetBundleBlockAndDirInfo();
                    BlockAndDirInfo.Read(memReader);
                }
            }

            // it hasn't been seen but it's possible we
            // find mixed lz4 and lzma. if so, that's bad news.
            switch (GetCompressionType(BlockAndDirInfo.BlockInfos))
            {
                case AssetBundleCompressionType.None:
                {
                    SegmentStream dataStream = new SegmentStream(Reader.BaseStream, Header.GetFileDataOffset());
                    DataReader = new AssetsFileReader(dataStream);
                    DataIsCompressed = false;
                    break;
                }
                case AssetBundleCompressionType.LZMA:
                {
                    SegmentStream dataStream = new SegmentStream(Reader.BaseStream, Header.GetFileDataOffset());
                    DataReader = new AssetsFileReader(dataStream);
                    DataIsCompressed = true;
                    break;
                }
                case AssetBundleCompressionType.LZ4:
                {
                    LZ4BlockStream dataStream = new LZ4BlockStream(Reader.BaseStream, Header.GetFileDataOffset(), BlockAndDirInfo.BlockInfos);
                    DataReader = new AssetsFileReader(dataStream);
                    DataIsCompressed = false;
                    break;
                }
            }

        }

        public AssetBundleCompressionType GetCompressionType(AssetBundleBlockInfo[] blockInfos)
        {
            for (int i = 0; i < blockInfos.Length; i++)
            {
                byte compType = blockInfos[i].GetCompressionType();
                if (compType == 2 || compType == 3)
                {
                    return AssetBundleCompressionType.LZ4;
                }
                else if (compType == 1)
                {
                    return AssetBundleCompressionType.LZMA;
                }
            }

            return AssetBundleCompressionType.None;
        }

        public bool IsAssetsFile(int index)
        {
            GetFileRange(index, out long offset, out long length);
            return AssetsFile.IsAssetsFile(DataReader, offset, length);
        }

        public int GetFileIndex(string name)
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            for (int i = 0; i < BlockAndDirInfo.DirectoryInfos.Count; i++)
            {
                if (BlockAndDirInfo.DirectoryInfos[i].Name == name)
                    return i;
            }

            return -1;
        }

        public string GetFileName(int index)
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            return BlockAndDirInfo.DirectoryInfos[index].Name;
        }

        public void GetFileRange(int index, out long offset, out long length)
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");

            AssetBundleDirectoryInfo entry = BlockAndDirInfo.DirectoryInfos[index];
            offset = entry.Offset;
            length = entry.DecompressedSize;
        }

        public List<string> GetAllFileNames()
        {
            if (Header == null)
                throw new Exception("Header must be loaded! (Did you forget to call bundle.Read?)");
            
            List<string> names = new List<string>();
            List<AssetBundleDirectoryInfo> dirInfos = BlockAndDirInfo.DirectoryInfos;
            foreach (AssetBundleDirectoryInfo dirInfo in dirInfos)
            {
                names.Add(dirInfo.Name);
            }

            return names;
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
        None,
        LZMA,
        LZ4
    }
}
