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

        public BundleFileInstance(Stream stream, string filePath, string root, bool unpackIfPacked)
        {
            path = Path.GetFullPath(filePath);
            name = Path.Combine(root, Path.GetFileName(path));
            file = new AssetBundleFile();
            file.Read(new AssetsFileReader(stream));
            if (file.Header != null && file.Header.GetCompressionType() == 1 && unpackIfPacked)
            {
                file = BundleHelper.UnpackBundle(file);
            }
            loadedAssetsFiles = new List<AssetsFileInstance>();
        }

        public BundleFileInstance(FileStream stream, string root, bool unpackIfPacked)
            : this(stream, stream.Name, root, unpackIfPacked)
        {
        }
    }
}
