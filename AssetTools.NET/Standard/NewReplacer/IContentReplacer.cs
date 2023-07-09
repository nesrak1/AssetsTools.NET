using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public interface IContentReplacer
    {
        void Write(AssetsFileWriter writer);
        bool HasPreview();
        Stream GetPreviewStream();
        ContentReplacerType GetReplacerType();
    }
}
