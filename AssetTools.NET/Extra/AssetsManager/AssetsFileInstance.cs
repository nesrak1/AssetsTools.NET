using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class AssetsFileInstance
    {
        public string path;
        public string name;
        public AssetsFile file;
        public AssetsFileTable table;
        public List<AssetsFileInstance> dependencies = new List<AssetsFileInstance>();
        public BundleFileInstance parentBundle = null;
        //for monobehaviours
        public Dictionary<uint, string> monoIdToName = new Dictionary<uint, string>();

        public Stream AssetsStream => file.readerPar;

        public AssetsFileInstance(Stream stream, string filePath, string root)
        {
            path = Path.GetFullPath(filePath);
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetsFile(new AssetsFileReader(stream));
            table = new AssetsFileTable(file);
            dependencies.AddRange(
                Enumerable.Range(0, file.dependencies.dependencyCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }
        public AssetsFileInstance(FileStream stream, string root)
        {
            path = stream.Name;
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetsFile(new AssetsFileReader(stream));
            table = new AssetsFileTable(file);
            dependencies.AddRange(
                Enumerable.Range(0, file.dependencies.dependencyCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }

        public AssetsFileInstance GetDependency(AssetsManager am, int depIdx)
        {
            if (dependencies[depIdx] == null)
            {
                string depPath = file.dependencies.dependencies[depIdx].assetPath;
                int instIndex = am.files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower());
                if (instIndex == -1)
                {
                    string pathDir = Path.GetDirectoryName(path);
                    string absPath = Path.Combine(pathDir, depPath);
                    string localAbsPath = Path.Combine(pathDir, Path.GetFileName(depPath));
                    if (File.Exists(absPath))
                    {
                        dependencies[depIdx] = am.LoadAssetsFile(File.OpenRead(absPath), true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        dependencies[depIdx] = am.LoadAssetsFile(File.OpenRead(localAbsPath), true);
                    }
                    else if (parentBundle != null)
                    {
                        dependencies[depIdx] = am.LoadAssetsFileFromBundle(parentBundle, depPath, true);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    dependencies[depIdx] = am.files[instIndex];
                }
            }
            return dependencies[depIdx];
        }
    }
}
