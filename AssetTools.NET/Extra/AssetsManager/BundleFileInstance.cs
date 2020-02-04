using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class BundleFileInstance
    {
        public Stream stream;
        public string path;
        public string name;
        public AssetBundleFile file;
        public List<AssetsFileInstance> assetsFiles = new List<AssetsFileInstance>();

        public BundleFileInstance(Stream stream, string filePath, string root)
        {
            this.stream = stream;
            path = Path.GetFullPath(filePath);
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetBundleFile();
            file.Read(new AssetsFileReader(stream), true);
            assetsFiles.AddRange(
                Enumerable.Range(0, (int)file.bundleInf6.blockCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }
        public BundleFileInstance(FileStream stream, string root)
        {
            this.stream = stream;
            path = stream.Name;
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetBundleFile();
            file.Read(new AssetsFileReader(stream), true);
            assetsFiles.AddRange(
                Enumerable.Range(0, (int)file.bundleInf6.blockCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }
    }
}
