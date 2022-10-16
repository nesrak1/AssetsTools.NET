using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public BundleFileInstance LoadBundleFile(Stream stream, string path, bool unpackIfPacked = true)
        {
            BundleFileInstance bunInst;
            int index = bundles.FindIndex(f => f.path.ToLower() == path.ToLower());
            if (index == -1)
            {
                bunInst = new BundleFileInstance(stream, path, "", unpackIfPacked);
                bundles.Add(bunInst);
            }
            else
            {
                bunInst = bundles[index];
            }
            return bunInst;
        }

        public BundleFileInstance LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            return LoadBundleFile(stream, Path.GetFullPath(stream.Name), unpackIfPacked);
        }

        public BundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(File.OpenRead(path), unpackIfPacked);
        }

        public bool UnloadBundleFile(string path)
        {
            int index = bundles.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index != -1)
            {
                BundleFileInstance bunInst = bundles[index];
                bunInst.file.Close();

                foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                {
                    assetsInst.file.Close();
                }

                bundles.Remove(bunInst);
                return true;
            }
            return false;
        }

        public bool UnloadAllBundleFiles()
        {
            if (bundles.Count != 0)
            {
                foreach (BundleFileInstance bunInst in bundles)
                {
                    bunInst.file.Close();

                    foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                    {
                        assetsInst.file.Close();
                    }
                }
                bundles.Clear();
                return true;
            }
            return false;
        }

        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, int index, bool loadDeps = false)
        {
            string assetMemPath = Path.Combine(bunInst.path, bunInst.file.GetFileName(index));

            int listIndex = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(assetMemPath).ToLower());
            if (listIndex == -1)
            {
                if (bunInst.file.IsAssetsFile(index))
                {
                    bunInst.file.GetFileRange(index, out long offset, out long length);
                    SegmentStream stream = new SegmentStream(bunInst.DataStream, offset, length);
                    AssetsFileInstance assetsInst = LoadAssetsFile(stream, assetMemPath, loadDeps, bunInst: bunInst);
                    bunInst.loadedAssetsFiles.Add(assetsInst);
                    return assetsInst;
                }
            }
            else
            {
                return files[listIndex];
            }
            return null;
        }

        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, string name, bool loadDeps = false)
        {
            int index = bunInst.file.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetsFileFromBundle(bunInst, index, loadDeps);
        }
    }
}
