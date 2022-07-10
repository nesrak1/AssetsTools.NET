using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, string root = "", BundleFileInstance bunInst = null)
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index == -1)
            {
                instance = new AssetsFileInstance(stream, path, root);
                instance.parentBundle = bunInst;
                files.Add(instance);
            }
            else
            {
                instance = files[index];
            }

            if (loadDeps)
            {
                if (bunInst == null)
                    LoadDependencies(instance, Path.GetDirectoryName(path));
                else
                    LoadBundleDependencies(instance, bunInst, Path.GetDirectoryName(path));
            }
            if (updateAfterLoad)
                UpdateDependencies(instance);
            return instance;
        }

        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(stream, stream.Name, loadDeps, root);
        }

        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(File.OpenRead(path), loadDeps, root);
        }

        public bool UnloadAssetsFile(string path)
        {
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index != -1)
            {
                AssetsFileInstance assetsInst = files[index];
                assetsInst.file.Close();
                files.Remove(assetsInst);
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
                foreach (AssetsFileInstance assetsInst in files)
                {
                    assetsInst.file.Close();
                }
                files.Clear();
                return true;
            }
            return false;
        }
    }
}
