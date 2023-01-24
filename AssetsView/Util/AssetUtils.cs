using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.IO;
using System.Linq;

namespace AssetsView.Util
{
    public static class AssetUtils
    {
        public static bool AllDependenciesLoaded(AssetsManager am, AssetsFileInstance afi)
        {
            foreach (AssetsFileExternal dep in afi.file.Metadata.Externals)
            {
                string absAssetPath = dep.PathName;
                if (absAssetPath.StartsWith("archive:/"))
                {
                    return false; //todo
                }
                if (!Path.IsPathRooted(absAssetPath))
                {
                    absAssetPath = Path.Combine(Path.GetDirectoryName(afi.path), dep.PathName);
                }
                if (!am.Files.Any(d => d != null && Path.GetFileName(d.path).ToLower() == Path.GetFileName(absAssetPath).ToLower()))
                {
                    return false;
                }
            }
            return true;
        }

        // used as sort of a hack to handle both second and component
        // in m_Component which both have the pptr as the last field
        // (pre 5.5 has a first and second while post 5.5 has component)
        public static AssetTypeValueField GetLastChild(this AssetTypeValueField atvf)
        {
            return atvf[atvf.Children.Count - 1];
        }
    }
}
