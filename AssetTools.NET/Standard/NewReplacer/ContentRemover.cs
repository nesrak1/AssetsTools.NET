using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public class ContentRemover : IContentReplacer
    {
        public void Write(AssetsFileWriter writer)
        {
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
            return ContentReplacerType.Remove;
        }
    }
}
