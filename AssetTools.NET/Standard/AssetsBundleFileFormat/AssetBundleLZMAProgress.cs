using SevenZip;

namespace AssetsTools.NET
{
    internal class AssetBundleLZMAProgress : ICodeProgress
    {
        private readonly IAssetBundleCompressProgress _progress;
        private readonly long _length;

        public AssetBundleLZMAProgress(IAssetBundleCompressProgress progress, long length)
        {
            _progress = progress;
            _length = length;
        }

        public void SetProgress(long inSize, long outSize)
        {
            if (_progress != null)
            {
                _progress.SetProgress((float)inSize / _length);
            }
        }
    }
}