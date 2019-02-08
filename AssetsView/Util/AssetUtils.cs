using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Util
{
    public static class AssetUtils
    {
        public static bool AllDependenciesLoaded(AssetsManager am, AssetsFileInstance afi)
        {
            foreach (AssetsFileDependency dep in afi.file.dependencies.pDependencies)
            {
                string absAssetPath = dep.assetPath;
                if (!Path.IsPathRooted(absAssetPath))
                {
                    absAssetPath = Path.Combine(Path.GetDirectoryName(afi.path), dep.assetPath);
                }
                if (!am.files.Any(d => d != null && Path.GetFullPath(d.path).ToLower() == Path.GetFullPath(absAssetPath).ToLower()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
