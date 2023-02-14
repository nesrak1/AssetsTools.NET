using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public class BundleFileInstance
    {
        public string path;
        public string name;
        public AssetBundleFile file;
        public List<AssetsFileInstance> loadedAssetsFiles;

        public Stream BundleStream => file.Reader.BaseStream;
        public Stream DataStream => file.DataReader.BaseStream;

        public BundleFileInstance(Stream stream, string filePath, bool unpackIfPacked = true)
        {
            path = Path.GetFullPath(filePath);
            name = Path.GetFileName(path);
            file = new AssetBundleFile();
            file.Read(new AssetsFileReader(stream));
            if (file.Header != null && file.DataIsCompressed && unpackIfPacked)
            {
                file = BundleHelper.UnpackBundle(file);
            }
            loadedAssetsFiles = new List<AssetsFileInstance>();
        }

        public BundleFileInstance(FileStream stream, bool unpackIfPacked = true)
            : this(stream, stream.Name, unpackIfPacked)
        {
        }
    }
}
