using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public void LoadDependencies(AssetsFileInstance ofFile)
        {
            string fileDir = Path.GetDirectoryName(ofFile.path);
            for (int i = 0; i < ofFile.file.Metadata.Externals.Count; i++)
            {
                string depPath = ofFile.file.Metadata.Externals[i].PathName;

                if (depPath == string.Empty)
                {
                    continue;
                }

                if (Files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower()) == -1)
                {
                    string absPath = Path.Combine(fileDir, depPath);
                    string localAbsPath = Path.Combine(fileDir, Path.GetFileName(depPath));
                    if (File.Exists(absPath))
                    {
                        LoadAssetsFile(absPath, true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        LoadAssetsFile(localAbsPath, true);
                    }
                }
            }
        }

        public void LoadBundleDependencies(AssetsFileInstance ofFile, BundleFileInstance ofBundle, string path)
        {
            for (int i = 0; i < ofFile.file.Metadata.Externals.Count; i++)
            {
                string depPath = ofFile.file.Metadata.Externals[i].PathName;
                if (Files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower()) == -1)
                {
                    string bunPath = Path.GetFileName(depPath);
                    int bunIndex = Array.FindIndex(ofBundle.file.BlockAndDirInfo.DirectoryInfos, d => Path.GetFileName(d.Name) == bunPath);

                    // todo: use bundle's path, not assets file's

                    // by default, the directory of an assets file is the bundle's file path (somepath\bundle.unity3d\file.assets)
                    // we back out again to get the directory the bundle is in
                    string noBunPath = Path.Combine(path, "..");
                    string nbAbsPath = Path.Combine(noBunPath, depPath);
                    string nbLocalAbsPath = Path.Combine(noBunPath, Path.GetFileName(depPath));

                    // if the user chose to set the path to the directory the bundle is in,
                    // we need to check for that as well
                    string absPath = Path.Combine(path, depPath);
                    string localAbsPath = Path.Combine(path, Path.GetFileName(depPath));

                    if (bunIndex != -1)
                    {
                        LoadAssetsFileFromBundle(ofBundle, bunIndex, true);
                    }
                    else if (File.Exists(absPath))
                    {
                        LoadAssetsFile(absPath, true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        LoadAssetsFile(localAbsPath, true);
                    }
                    else if (File.Exists(nbAbsPath))
                    {
                        LoadAssetsFile(nbAbsPath, true);
                    }
                    else if (File.Exists(nbLocalAbsPath))
                    {
                        LoadAssetsFile(nbLocalAbsPath, true);
                    }
                }
            }
        }
    }
}
