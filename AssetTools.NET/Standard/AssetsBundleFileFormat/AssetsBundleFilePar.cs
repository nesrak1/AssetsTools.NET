using System.IO;

namespace AssetsTools.NET
{
    public class AssetsBundleFilePar
    {
        public AssetBundleFile file;

        public AssetsBundleEntry entryUnity3;
        public AssetBundleDirectoryInfo06 entryUnity6;

        public uint listIndex;
        public AssetsFileReader origFileReader;
        public FileStream origPar;
        public ulong curFilePos;
    }
}
