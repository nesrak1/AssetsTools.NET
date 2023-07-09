using AssetsTools.NET.Extra;
using System;
using System.IO;

namespace AssetsTools.NET
{
    public class ContentReplacerFromStream : IContentReplacer
    {
        private readonly Stream stream;
        private readonly long offset;
        private readonly long length;
        private readonly bool closeOnWrite;

        public ContentReplacerFromStream(Stream stream, long offset = 0, int length = -1, bool closeOnWrite = false)
        {
            this.stream = stream;
            this.offset = offset;
            this.length = length;
            this.closeOnWrite = closeOnWrite;
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("Stream needs to be seekable.");
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            if (offset != -1)
            {
                stream.Position = offset;
            }

            stream.CopyToCompat(writer.BaseStream, length);

            if (closeOnWrite)
            {
                stream.Close();
            }
        }

        public bool HasPreview()
        {
            return true;
        }

        public Stream GetPreviewStream()
        {
            if (offset == 0 && length == -1)
            {
                return stream;
            }

            // stream starts at position 0, so no need to seek
            return new SegmentStream(stream, offset, length);
        }

        public ContentReplacerType GetReplacerType()
        {
            return ContentReplacerType.AddOrModify;
        }
    }
}
