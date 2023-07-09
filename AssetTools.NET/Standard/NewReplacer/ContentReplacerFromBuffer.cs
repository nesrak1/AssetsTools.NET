using System.IO;

namespace AssetsTools.NET
{
    public class ContentReplacerFromBuffer : IContentReplacer
    {
        private readonly byte[] buffer;

        private MemoryStream previewStream;

        public ContentReplacerFromBuffer(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(buffer);
        }

        public bool HasPreview()
        {
            return true;
        }

        public Stream GetPreviewStream()
        {
            if (previewStream == null)
            {
                previewStream = new MemoryStream(buffer);
            }

            previewStream.Position = 0;
            return previewStream;
        }

        public ContentReplacerType GetReplacerType()
        {
            return ContentReplacerType.AddOrModify;
        }
    }
}
