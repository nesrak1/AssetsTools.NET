using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public class LZ4BlockStream : Stream
    {
        private readonly long length;
        private readonly long blockSize;

        private AssetBundleBlockInfo[] blockInfos;
        private long[] blockPoses;

        private Dictionary<int, MemoryStream> decompressedBlockMap;
        private Queue<int> decompressedBlockQueue;
        private const int MAX_BLOCK_MAP_SIZE = 10;

        public LZ4BlockStream(Stream baseStream, long baseOffset, AssetBundleBlockInfo[] blockInfos)
        {
            if (baseOffset < 0 || baseOffset > baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(baseOffset));

            if (length >= 0 && baseOffset + length > baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            decompressedBlockMap = new Dictionary<int, MemoryStream>();
            decompressedBlockQueue = new Queue<int>();

            if (blockInfos.Length == 0)
            {
                length = 0;
                blockSize = 0x20000;

                BaseStream = new MemoryStream();
                BaseOffset = baseOffset;

                this.blockInfos = new AssetBundleBlockInfo[0];
                blockPoses = new long[0];
                return;
            }

            // if block decompressed sizes are the same, we can use modulo
            // if they're different, then we have a problem on our hands
            // I've only seen a constant block size (0x20000) so ... idk

            long compressedFileOff = blockInfos[0].CompressedSize;
            blockPoses = new long[blockInfos.Length];
            blockPoses[0] = 0;

            blockSize = GetLz4BlockSize(blockInfos);

            length = blockInfos[0].DecompressedSize;

            for (int i = 1; i < blockInfos.Length; i++)
            {
                if (blockInfos[i].DecompressedSize != blockSize && i != blockInfos.Length - 1)
                {
                    throw new NotImplementedException("Cannot handle bundles with multiple block sizes yet.");
                }

                length += blockInfos[i].DecompressedSize;

                blockPoses[i] = compressedFileOff;
                compressedFileOff += blockInfos[i].CompressedSize;
            }

            if (blockSize > int.MaxValue)
            {
                throw new NotImplementedException("Block size too large!");
            }

            this.blockInfos = blockInfos;

            BaseStream = baseStream;
            BaseOffset = baseOffset;
        }

        public Stream BaseStream
        {
            get;
        }

        public long BaseOffset
        {
            get;
        }

        public override long Position
        {
            get;
            set;
        }

        public override long Length
        {
            get { return length; }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readCount = 0;

            while (readCount < count)
            {
                MemoryStream blockStream;

                int blockIndex = (int)(Position / blockSize);
                if (!decompressedBlockMap.ContainsKey(blockIndex))
                {
                    if (decompressedBlockMap.Count >= MAX_BLOCK_MAP_SIZE)
                    {
                        int frontItem = decompressedBlockQueue.Dequeue();
                        decompressedBlockMap[frontItem].Close();
                        decompressedBlockMap.Remove(frontItem);
                    }

                    BaseStream.Position = BaseOffset + blockPoses[blockIndex];

                    MemoryStream compressedStream = new MemoryStream();
                    BaseStream.CopyToCompat(compressedStream, blockInfos[blockIndex].CompressedSize);
                    compressedStream.Position = 0;

                    byte compressionType = blockInfos[blockIndex].GetCompressionType();
                    if (compressionType == 0)
                    {
                        blockStream = compressedStream;
                    }
                    else if (compressionType == 2 || compressionType == 3)
                    {
                        byte[] blockData = new byte[blockInfos[blockIndex].DecompressedSize];
                        using (Lz4DecoderStream decoder = new Lz4DecoderStream(compressedStream))
                        {
                            decoder.Read(blockData, 0, blockData.Length);
                        }
                        blockStream = new MemoryStream(blockData);
                    }
                    else
                    {
                        throw new Exception("Invalid block compression type in supposed LZ4 only stream!");
                    }

                    decompressedBlockMap[blockIndex] = blockStream;
                    decompressedBlockQueue.Enqueue(blockIndex);
                }
                else
                {
                    blockStream = decompressedBlockMap[blockIndex];
                }

                blockStream.Position = Position % blockSize;
                int thisReadCount = blockStream.Read(buffer, offset + readCount, (int)Math.Min(blockStream.Length, count - readCount));
                
                if (thisReadCount == 0)
                {
                    break;
                }

                readCount += thisReadCount;
                Position += thisReadCount;
            }

            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;

                case SeekOrigin.Current:
                    newPosition = Position + offset;
                    break;

                case SeekOrigin.End:
                    newPosition = Position + Length + offset;
                    break;

                default:
                    throw new ArgumentException();
            }

            if (newPosition < 0 || newPosition > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            Position = newPosition;
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(LZ4BlockStream)} cannot be written to, only read from.");
        }

        private long GetLz4BlockSize(AssetBundleBlockInfo[] blockInfos)
        {
            // we need to do this instead of just reading the first block
            // because if a compressed block ends up being bigger than the
            // actual decompressed size, unity will just use an uncompressed
            // block instead. so we need to find the first compressed block.
            for (int i = 0; i < blockInfos.Length; i++)
            {
                if (blockInfos[i].GetCompressionType() == 2 || blockInfos[i].GetCompressionType() == 3)
                {
                    return blockInfos[i].DecompressedSize;
                }
            }

            if (blockInfos[0].GetCompressionType() == 0)
            {
                return blockInfos[0].DecompressedSize;
            }

            throw new Exception("No LZ4 blocks were found in block infos. Can't find block size.");
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;
    }
}
