using System;
using System.IO;

namespace AssetsTools.NET
{
    public class SegmentStream : Stream
    {
        private readonly long length;

        public SegmentStream(Stream baseStream, long baseOffset)
            : this(baseStream, baseOffset, -1)
        {
        }

        public SegmentStream(Stream baseStream, long baseOffset, long length)
        {
            if (baseOffset < 0 || baseOffset > baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(baseOffset));

            if (length >= 0 && baseOffset + length > baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            BaseStream = baseStream;
            BaseOffset = baseOffset;
            this.length = length;
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
            get { return length >= 0 ? length : BaseStream.Length - BaseOffset; }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            BaseStream.Position = BaseOffset + Position;
            count = BaseStream.Read(buffer, offset, (int)Math.Min(count, Length - Position));
            Position += count;
            return count;
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
            if (length >= 0 && count > Length - Position)
                throw new ArgumentOutOfRangeException(nameof(count));

            BaseStream.Position = BaseOffset + Position;
            BaseStream.Write(buffer, offset, count);
            Position += count;
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;
    }
}
