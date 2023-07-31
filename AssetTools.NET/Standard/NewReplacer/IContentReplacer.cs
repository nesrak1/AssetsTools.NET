using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public interface IContentReplacer
    {
        /// <summary>
        /// Write the content with provided writer.
        /// </summary>
        void Write(AssetsFileWriter writer);
        /// <summary>
        /// Does the content has a preview stream? This will be true if the data
        /// is readily available (i.e. buffer or stream) and false if the data
        /// isn't readily available because it needs to be calculated (assets).
        /// </summary>
        bool HasPreview();
        /// <summary>
        /// Returns the preview stream.
        /// </summary>
        Stream GetPreviewStream();
        /// <summary>
        /// The replacer type such as modified or removed.
        /// </summary>
        ContentReplacerType GetReplacerType();
    }
}
