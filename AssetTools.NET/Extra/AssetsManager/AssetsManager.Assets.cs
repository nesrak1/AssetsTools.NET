using System.IO;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public static string GetFileLookupKey(string path)
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

        private AssetsFileInstance LoadAssetsFileCacheless(AssetsFile file, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            AssetsFileInstance fileInst = new AssetsFileInstance(file, path);
            fileInst.parentBundle = bunInst;
            return LoadAssetsFileCacheless(fileInst, path, loadDeps, bunInst);
        }

        private AssetsFileInstance LoadAssetsFileCacheless(Stream stream, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            AssetsFileInstance fileInst = new AssetsFileInstance(stream, path);
            fileInst.parentBundle = bunInst;
            return LoadAssetsFileCacheless(fileInst, path, loadDeps, bunInst);
        }

        private AssetsFileInstance LoadAssetsFileCacheless(AssetsFileInstance fileInst, string path, bool loadDeps, BundleFileInstance bunInst = null)
        {
            string lookupKey = GetFileLookupKey(path);
            lock (FileLookup)
            {
                lock (Files)
                {
                    FileLookup[lookupKey] = fileInst;
                    Files.Add(fileInst);
                }
            }

            if (loadDeps)
            {
                LoadAssetsFileDependencies(fileInst, path, bunInst);
            }
            // not thread safe
            if (UseQuickLookup)
            {
                fileInst.file.GenerateQuickLookup();
            }
            return fileInst;
        }

        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from a stream with a path.
        /// Use the <see cref="FileStream"/> version of this method to skip the path argument.
        /// If a file with that name is already loaded, it will be returned instead.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path to set on the <see cref="AssetsFileInstance"/>.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <param name="bunInst">The parent bundle, if one exists.</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps = false, BundleFileInstance bunInst = null)
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

            if (stream != null)
            {
                return LoadAssetsFileCacheless(stream, path, loadDeps, bunInst);
            }
            return null;
        }

        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from a stream.
        /// Assigns the <see cref="AssetsFileInstance"/>'s path from the stream's file path.
        /// If a file with that name is already loaded, it will be returned instead.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps = false)
        {
            return LoadAssetsFile(stream, stream.Name, loadDeps);
        }

        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from a path.
        /// If a file with that name is already loaded, it will be returned instead.
        /// </summary>
        /// <param name="path">The path of the file to read from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps = false)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
            {
                if (loadDeps)
                {
                    LoadAssetsFileDependencies(fileInst, path, null);
                }
                return fileInst;
            }

            FileStream stream = File.OpenRead(path);
            return LoadAssetsFileCacheless(stream, stream.Name, loadDeps);
        }


        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from an existing loaded <see cref="AssetsFile"/>.
        /// If a file with that name is already loaded, it will be returned instead.
        /// </summary>
        /// <param name="file">The assets file to use.</param>
        /// <param name="path">The path of the file to read from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance AddAssetsFile(AssetsFile file, string path, bool loadDeps = false)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
                return fileInst;

            return LoadAssetsFileCacheless(file, path, loadDeps);
        }

        /// <summary>
        /// Unload an <see cref="AssetsFileInstance"/> by path.
        /// </summary>
        /// <param name="path">The path of the <see cref="AssetsFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadAssetsFile(string path)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AssetsFileInstance fileInst))
            {
                monoTypeTreeTemplateFieldCache.TryRemove(fileInst, out _);
                monoCldbTemplateFieldCache.TryRemove(fileInst, out _);
                refTypeManagerCache.TryRemove(fileInst, out _);

                lock (FileLookup)
                {
                    lock (Files)
                    {
                        Files.Remove(fileInst);
                        FileLookup.Remove(lookupKey);
                    }
                }
                fileInst.file.Close();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unload an <see cref="AssetsFileInstance"/>.
        /// </summary>
        /// <param name="fileInst">The <see cref="AssetsFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadAssetsFile(AssetsFileInstance fileInst)
        {
            fileInst.file.Close();

            if (Files.Contains(fileInst))
            {
                monoTypeTreeTemplateFieldCache.TryRemove(fileInst, out _);
                monoCldbTemplateFieldCache.TryRemove(fileInst, out _);
                refTypeManagerCache.TryRemove(fileInst, out _);

                string lookupKey = GetFileLookupKey(fileInst.path);
                lock (FileLookup)
                {
                    lock (Files)
                    {
                        FileLookup.Remove(lookupKey);
                        Files.Remove(fileInst);
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unload all <see cref="AssetsFileInstance"/>s.
        /// </summary>
        /// <param name="clearCache">Clear the cache? Cache is recommended if you plan on reopening files later.</param>
        /// <returns>True if there are files that can be cleared, and false if no files are loaded.</returns>
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
                lock (FileLookup)
                {
                    lock (Files)
                    {
                        Files.Clear();
                        FileLookup.Clear();
                    }
                }
                return true;
            }
            return false;
        }
    }
}
