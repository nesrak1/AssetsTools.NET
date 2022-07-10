using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index == -1)
            {
                instance = new AssetsFileInstance(stream, path);
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
                    LoadDependencies(instance);
                else
                    LoadBundleDependencies(instance, bunInst, Path.GetDirectoryName(path));
            }
            return instance;
        }

        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps)
        {
            return LoadAssetsFile(stream, stream.Name, loadDeps);
        }

        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps)
        {
            return LoadAssetsFile(File.OpenRead(path), loadDeps);
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
