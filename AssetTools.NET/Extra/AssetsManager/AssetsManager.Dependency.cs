using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        /*
        public void UpdateDependencies(AssetsFile ofFile)
        {
            var depList = ofFile.file.Metadata.Externals;
            for (int i = 0; i < depList.Count; i++)
            {
                AssetsFileExternal dep = depList[i];
                int index = files.FindIndex(f => Path.GetFileName(dep.PathName.ToLower()) == Path.GetFileName(f.path.ToLower()));
                if (index != -1)
                {
                    ofFile.dependencies[i] = files[index];
                }
            }
        }
        public void UpdateDependencies()
        {
            foreach (AssetsFile file in files)
            {
                UpdateDependencies(file);
            }
        }*/

        public void LoadDependencies(AssetsFile assetsFile)
        {
            for (int i = 0; i < assetsFile.Metadata.Externals.Count; i++)
            {
                string depPath = Path.GetFileName(assetsFile.Metadata.Externals[i].PathName);
                string depPathLower = depPath.ToLower();

                if (depPath == string.Empty)
                {
                    continue;
                }

                if (!files.ContainsKey(depPathLower))
                {
                    string absPath = Path.Combine(activeDirectory, depPath);
                    if (File.Exists(absPath))
                    {
                        LoadAssetsFile(File.OpenRead(absPath), true);
                    }
                }
            }
        }

        public void LoadBundleDependencies(AssetsFile assetsFile, AssetBundleFile bundleFile)
        {
            for (int i = 0; i < assetsFile.Metadata.Externals.Count; i++)
            {
                string depPath = Path.GetFileName(assetsFile.Metadata.Externals[i].PathName);
                string depPathLower = depPath.ToLower();
                if (!files.ContainsKey(depPathLower))
                {
                    int bunIndex = Array.FindIndex(bundleFile.BlockAndDirInfo.DirectoryInfos, d => Path.GetFileName(d.Name).ToLower() == depPath);

                    string absPath = Path.Combine(activeDirectory, depPath);

                    if (bunIndex != -1)
                    {
                        LoadAssetsFileFromBundle(bundleFile, bunIndex, true);
                    }
                    else if (File.Exists(absPath))
                    {
                        LoadAssetsFile(File.OpenRead(absPath), true);
                    }
                }
            }
        }
    }
}
