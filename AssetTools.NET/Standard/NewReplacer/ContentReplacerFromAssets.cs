using AssetsTools.NET.Extra;
using System.IO;

namespace AssetsTools.NET
{
    public class ContentReplacerFromAssets : IContentReplacer
    {
        public AssetsFile file;

        public ContentReplacerFromAssets(AssetsFile file)
        {
            this.file = file;
        }

        public ContentReplacerFromAssets(AssetsFileInstance inst)
        {
            file = inst.file;
        }

        public void Write(AssetsFileWriter writer)
        {
            // some parts of an assets file need to be aligned to a multiple of 4/8/16 bytes,
            // but for this to work correctly, the start of the file of course needs to be aligned too.
            // in a loose .assets file this is true by default, but inside a bundle file,
            // this might not be the case. therefore wrap the bundle output stream in a SegmentStream
            // which will make it look like the start of the new assets file is at position 0
            SegmentStream alignedStream = new SegmentStream(writer.BaseStream, writer.Position);
            AssetsFileWriter alignedWriter = new AssetsFileWriter(alignedStream);
            file.Write(alignedWriter, -1);
            writer.Position = writer.BaseStream.Length;
        }

        public bool HasPreview()
        {
            return false;
        }

        public Stream GetPreviewStream()
        {
            return null;
        }

        public ContentReplacerType GetReplacerType()
        {
            return ContentReplacerType.AddOrModify;
        }
    }
}
