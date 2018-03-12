using System.IO;

namespace AssetsTools.NET
{
    public struct AssetsBundleFilePar
    {
        public AssetsBundleFile pFile;

        public AssetsBundleEntry pEntry3;
        public AssetsBundleDirectoryInfo06 pEntry6;

        public uint listIndex;
        public AssetsFileReader origFileReader;
        public FileStream origPar;
        public ulong curFilePos;
    }
}
