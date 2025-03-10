using System.IO;

namespace AssetsTools.NET
{
    public interface IContentReplacer
    {
        /// <summary>
        /// Write the content with provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="finalWrite">Is this the final write? Good for checking when to close a stream.</param>
        void Write(AssetsFileWriter writer, bool finalWrite);
        /// <summary>
        /// Does the content has a preview stream? This will be true if the data
        /// is readily available (i.e. buffer or stream) and false if the data
        /// isn't readily available because it needs to be calculated (assets).
        /// </summary>
        bool HasPreview();
        /// <summary>
        /// Returns the preview stream. The position is not guaranteed to be at
        /// the beginning of the stream.
        /// </summary>
        Stream GetPreviewStream();
        /// <summary>
        /// The replacer type such as modified or removed.
        /// </summary>
        ContentReplacerType GetReplacerType();
        /// <summary>
        /// The maximum size this replacer can write to.
        /// If the size is already known, return that size. Otherwise,
        /// return a worst-case size for the replacer.
        /// </summary>
        long GetSize();
    }
}
