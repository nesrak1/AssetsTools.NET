using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        internal string GetFileLookupKey(string path)
        {
            return Path.GetFullPath(path).ToLower();
        }

        private void LoadAssetsFileDependencies(AssetsFileInstance fileInst, string path, BundleFileInstance bunInst)
        {
            if (bunInst == null)
                LoadDependencies(fileInst);
            else
                LoadBundleDependencies(fileInst, bunInst, Path.GetDirectoryName(path));
        }

        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
            {
                if (loadDeps)
                {
                    LoadAssetsFileDependencies(fileInst, path, bunInst);
                }
                return fileInst;
            }
            else
            {
                return LoadAssetsFileCacheless(stream, path, loadDeps, bunInst);
            }
        }

        private AssetsFileInstance LoadAssetsFileCacheless(Stream stream, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            AssetsFileInstance fileInst = new AssetsFileInstance(stream, path);
            fileInst.parentBundle = bunInst;

            string lookupKey = GetFileLookupKey(path);
            FileLookup[lookupKey] = fileInst;
            Files.Add(fileInst);

            if (loadDeps)
            {
                LoadAssetsFileDependencies(fileInst, path, bunInst);
            }
            return fileInst;
        }

        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps)
        {
            return LoadAssetsFileCacheless(stream, stream.Name, loadDeps);
        }

        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
                return fileInst;

            return LoadAssetsFile(File.OpenRead(path), loadDeps);
        }

        public bool UnloadAssetsFile(string path)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
            {
                monoTypeTreeTemplateFieldCache.Remove(fileInst);
                monoCldbTemplateFieldCache.Remove(fileInst);
                refTypeManagerCache.Remove(fileInst);

                Files.Remove(fileInst);
                FileLookup.Remove(lookupKey);
                fileInst.file.Close();
                return true;
            }
            return false;
        }

        public bool UnloadAssetsFile(AssetsFileInstance fileInst)
        {
            fileInst.file.Close();

            if (Files.Contains(fileInst))
            {
                monoTypeTreeTemplateFieldCache.Remove(fileInst);
                monoCldbTemplateFieldCache.Remove(fileInst);
                refTypeManagerCache.Remove(fileInst);

                string lookupKey = GetFileLookupKey(fileInst.path);
                FileLookup.Remove(lookupKey);
                Files.Remove(fileInst);
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

            monoTypeTreeTemplateFieldCache.Clear();
            monoCldbTemplateFieldCache.Clear();
            refTypeManagerCache.Clear();

            if (Files.Count != 0)
            {
                foreach (AssetsFileInstance assetsInst in Files)
                {
                    assetsInst.file.Close();
                }
                Files.Clear();
                FileLookup.Clear();
                return true;
            }
            return false;
        }
    }
}
