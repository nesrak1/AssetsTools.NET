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

        public void Write(AssetsFileWriter writer, bool finalWrite)
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

        public long GetSize()
        {
            long size = 0;
            size += file.Header.GetSize();
            size += file.Metadata.GetSize(file.Header.Version);
            if (size < 0x1000)
            {
                size = 0x1000;
            }
            else
            {
                size += 16;
            }

            foreach (AssetFileInfo assetInfo in file.Metadata.AssetInfos)
            {
                if (assetInfo.Replacer != null)
                {
                    if (assetInfo.Replacer.HasPreview())
                    {
                        size += assetInfo.Replacer.GetPreviewStream().Length;
                    }
                    else
                    {
                        // slower, but we weren't left with any other option.
                        // this should probably be some kind of dummy reader
                        // so it doesn't use memory, but this will work for now.
                        using (AssetsFileWriter writer = new AssetsFileWriter(new MemoryStream()))
                        {
                            assetInfo.Replacer.Write(writer, false);
                            size += writer.BaseStream.Length;
                        }
                    }
                }
                else
                {
                    size += assetInfo.ByteSize;
                }

                size = (size + 7) & ~7;
            }

            return 0;
        }
    }
}
