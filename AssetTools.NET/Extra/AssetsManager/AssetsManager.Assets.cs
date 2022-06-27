using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetsFile LoadAssetsFile(Stream stream, string name, bool loadDeps, AssetBundleFile parentBundle = null)
        {
            AssetsFile assetsFile;
            string pathLower = name.ToLower();
            if (!files.ContainsKey(pathLower))
            {
                assetsFile = new AssetsFile(stream, pathLower);
                bundleMap[assetsFile] = parentBundle;
                files[pathLower] = assetsFile;
            }
            else
            {
                assetsFile = files[pathLower];
            }

            if (loadDeps)
            {
                if (parentBundle == null)
                    LoadDependencies(assetsFile);
                else
                    LoadBundleDependencies(assetsFile, parentBundle);
            }
            return assetsFile;
        }

        public AssetsFile LoadAssetsFile(FileStream stream, bool loadDeps)
        {
            if (activeDirectory == string.Empty)
                activeDirectory = Path.GetDirectoryName(stream.Name);

            return LoadAssetsFile(stream, stream.Name, loadDeps);
        }

        public AssetsFile LoadAssetsFile(string path, bool loadDeps)
        {
            if (activeDirectory == string.Empty)
                SetActiveDirectory(Path.GetDirectoryName(path));

            return LoadAssetsFile(File.OpenRead(path), loadDeps);
        }

        public bool UnloadAssetsFile(string path)
        {
            string fullPathLower = Path.GetFullPath(path).ToLower();
            if (files.ContainsKey(fullPathLower))
            {
                AssetsFile assetsInst = files[fullPathLower];
                if (bundleMap.TryGetValue(assetsInst, out AssetBundleFile bundleFile))
                {
                    bundleLoadedFilesMap[bundleFile].Remove(assetsInst);
                    bundleMap.Remove(assetsInst);
                }
                files.Remove(fullPathLower);
                assetsInst.Close();
                return true;
            }
            return false;
        }

        public bool UnloadAllAssetsFiles(bool clearCache = false)
        {
            if (clearCache)
            {
                templateFieldCache.Clear();
                monoTemplateFieldCache.Clear();
            }

            if (files.Count != 0)
            {
                foreach (var filePair in files)
                {
                    filePair.Value.Close();
                }
                foreach (var bundleAssetsPair in bundleLoadedFilesMap)
                {
                    bundleAssetsPair.Value.Clear();
                }
                bundleMap.Clear();
                files.Clear();
                return true;
            }
            return false;
        }

        public void UnloadAll(bool unloadClassData = false)
        {
            UnloadAllAssetsFiles(true);
            UnloadAllBundleFiles();
            if (unloadClassData)
            {
                classPackage = null;
                classDatabase = null;
            }
        }
    }
}
