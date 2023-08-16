using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    /// <summary>
    /// A wrapper around an <see cref="AssetsFile"/> with information such as the path to the file
    /// (used for handling dependencies) and the bundle it belongs to.
    /// </summary>
    public class AssetsFileInstance
    {
        /// <summary>
        /// The full path to the file. This path can be fake if it is not from disk.
        /// </summary>
        public string path;
        /// <summary>
        /// The name of the file. This is the file name part of the path.
        /// </summary>
        public string name;
        /// <summary>
        /// The base <see cref="AssetsFile"/>.
        /// </summary>
        public AssetsFile file;
        /// <summary>
        /// The bundle this file is a part of, if there is one.
        /// </summary>
        public BundleFileInstance parentBundle = null;

        internal Dictionary<int, AssetsFileInstance> dependencyCache;

        /// <summary>
        /// The stream the assets file uses.
        /// </summary>
        public Stream AssetsStream => file.Reader.BaseStream;

        public AssetsFileInstance(Stream stream, string filePath)
        {
            path = Path.GetFullPath(filePath);
            name = Path.GetFileName(path);
            file = new AssetsFile();
            file.Read(new AssetsFileReader(stream));
            dependencyCache = new Dictionary<int, AssetsFileInstance>();
        }
        public AssetsFileInstance(FileStream stream)
        {
            path = stream.Name;
            name = Path.GetFileName(path);
            file = new AssetsFile();
            file.Read(new AssetsFileReader(stream));
            dependencyCache = new Dictionary<int, AssetsFileInstance>();
        }

        public AssetsFileInstance GetDependency(AssetsManager am, int depIdx)
        {
            if (!dependencyCache.ContainsKey(depIdx) || dependencyCache[depIdx] == null)
            {
                string depPath = file.Metadata.Externals[depIdx].PathName;

                if (depPath == string.Empty)
                {
                    return null;
                }

                if (depPath.StartsWith("archive:/"))
                {
                    depPath = depPath.Substring(depPath.IndexOf('/', "archive:/".Length) + 1);
                }

                if (!am.FileLookup.TryGetValue(am.GetFileLookupKey(depPath), out AssetsFileInstance inst))
                {
                    string pathDir = Path.GetDirectoryName(path);
                    string absPath = Path.Combine(pathDir, depPath);
                    string localAbsPath = Path.Combine(pathDir, Path.GetFileName(depPath));

                    if (File.Exists(absPath))
                    {
                        dependencyCache[depIdx] = am.LoadAssetsFile(absPath, true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        dependencyCache[depIdx] = am.LoadAssetsFile(localAbsPath, true);
                    }
                    else if (parentBundle != null)
                    {
                        AssetBundleFile bundle = parentBundle.file;
                        bool depInBundle = bundle.BlockAndDirInfo.DirectoryInfos.Any(di => di.Name == depPath);
                        if (depInBundle)
                        {
                            dependencyCache[depIdx] = am.LoadAssetsFileFromBundle(parentBundle, depPath, true);
                        }
                        else
                        {
                            string bundlePathDir = Path.GetDirectoryName(pathDir);
                            string bundleLocalAbsPath = Path.Combine(bundlePathDir, Path.GetFileName(depPath));
                            if (File.Exists(bundleLocalAbsPath))
                            {
                                dependencyCache[depIdx] = am.LoadAssetsFile(bundleLocalAbsPath, true);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    dependencyCache[depIdx] = inst;
                }
            }
            return dependencyCache[depIdx];
        }
    }
}
