using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class AssetsFileInstance
    {
        public Stream stream;
        public string path;
        public string name;
        public AssetsFile file;
        public AssetsFileTable table;
        public List<AssetsFileInstance> dependencies = new List<AssetsFileInstance>();
        //for monobehaviours
        public Dictionary<uint, AssetTypeTemplateField> templateFieldCache = new Dictionary<uint, AssetTypeTemplateField>();

        public AssetsFileInstance(Stream stream, string filePath, string root)
        {
            this.stream = stream;
            path = Path.GetFullPath(filePath);
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetsFile(new AssetsFileReader(stream));
            table = new AssetsFileTable(file);
            dependencies.AddRange(
                Enumerable.Range(0, (int)file.dependencies.dependencyCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }
        public AssetsFileInstance(FileStream stream, string root)
        {
            this.stream = stream;
            path = stream.Name;
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetsFile(new AssetsFileReader(stream));
            table = new AssetsFileTable(file);
            dependencies.AddRange(
                Enumerable.Range(0, (int)file.dependencies.dependencyCount)
                          .Select(d => (AssetsFileInstance)null)
            );
        }
    }
}
