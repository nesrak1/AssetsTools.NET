using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public interface IAssetBundleCompressProgress
    {
        public void SetProgress(float progress);
    }
}
