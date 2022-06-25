using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetBundleFile LoadBundleFile(Stream stream, string path, bool unpackIfPacked = true)
        {
            AssetBundleFile bunInst;
            string fullPathLower = Path.GetFullPath(path).ToLower();
            if (!bundles.ContainsKey(fullPathLower))
            {
                bunInst = new AssetBundleFile();
                bunInst.Read(new AssetsFileReader(stream));
                bundles[fullPathLower] = bunInst;
                bundleLoadedFilesMap[bunInst] = new List<AssetsFile>();
            }
            else
            {
                bunInst = bundles[fullPathLower];
            }
            return bunInst;
        }

        public AssetBundleFile LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            return LoadBundleFile(stream, Path.GetFullPath(stream.Name), unpackIfPacked);
        }

        public AssetBundleFile LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(File.OpenRead(path), unpackIfPacked);
        }

        public bool UnloadBundleFile(string path, bool unloadAssets = true)
        {
            string fullPathLower = Path.GetFullPath(path).ToLower();
            if (bundles.ContainsKey(fullPathLower))
            {
                AssetBundleFile bunInst = bundles[fullPathLower];
                bunInst.Close();

                if (unloadAssets)
                {
                    // could be faster...
                    foreach (var assetBunPair in bundleMap)
                    {
                        if (assetBunPair.Value == bunInst)
                            assetBunPair.Key.Close();
                    }
                }

                bundles.Remove(fullPathLower);
                bundleLoadedFilesMap.Remove(bunInst);
                return true;
            }
            return false;
        }

        public bool UnloadAllBundleFiles(bool unloadAssets = true)
        {
            if (bundles.Count != 0)
            {
                foreach (var nameBunPair in bundles)
                {
                    AssetBundleFile bundle = nameBunPair.Value;

                    bundle.Close();

                    if (unloadAssets)
                    {
                        foreach (var assetBunPair in bundleMap)
                        {
                            if (assetBunPair.Value == bundle)
                                assetBunPair.Key.Close();
                        }
                    }
                }

                bundles.Clear();
                bundleLoadedFilesMap.Clear();
                return true;
            }
            return false;
        }

        public AssetsFile LoadAssetsFileFromBundle(AssetBundleFile bunInst, int index, bool loadDeps = false)
        {
            string assetMemPath = Path.Combine(bunInst.Name, bunInst.GetFileName(index));
            string fullPathLower = Path.GetFullPath(assetMemPath).ToLower();
            if (!files.ContainsKey(fullPathLower))
            {
                if (bunInst.IsAssetsFile(index))
                {
                    bunInst.GetFileRange(index, out long offset, out long length);
                    SegmentStream stream = new SegmentStream(bunInst.Reader.BaseStream, offset, length);
                    AssetsFile assetsInst = LoadAssetsFile(stream, assetMemPath, loadDeps, parentBundle: bunInst);
                    bundleLoadedFilesMap[bunInst].Add(assetsInst);
                    return assetsInst;
                }
            }
            else
            {
                return files[fullPathLower];
            }
            return null;
        }

        public AssetsFile LoadAssetsFileFromBundle(AssetBundleFile bunInst, string name, bool loadDeps = false)
        {
            int index = bunInst.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetsFileFromBundle(bunInst, index, loadDeps);
        }
    }
}
