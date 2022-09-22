using SevenZip;

namespace AssetsTools.NET
{
    internal class AssetBundleLZMAProgress : ICodeProgress
    {
        private IAssetBundleCompressProgress progress;
        private long currentSize;
        private long length;

        public AssetBundleLZMAProgress(IAssetBundleCompressProgress progress, long length)
        {
            this.progress = progress;
            this.currentSize = 0;
            this.length = length;
        }

        public void SetProgress(long inSize, long outSize)
        {
            if (progress != null)
            {
                progress.SetProgress((float)inSize / length);
            }
        }
    }
}