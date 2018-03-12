namespace AssetsTools.NET
{
    public struct AssetsBundleHeader03
    {
        public string signature; //0-terminated; UnityWeb or UnityRaw
        public uint fileVersion; //big-endian; 3 : Unity 3.5 and 4;
        public string minPlayerVersion; //0-terminated; 3.x.x -> Unity 3.x.x/4.x.x; 5.x.x
        public string fileEngineVersion; //0-terminated; exact unity engine version
        public uint minimumStreamedBytes; //big-endian; not always the file's size
        public uint bundleDataOffs; //big-endian;
        public uint numberOfAssetsToDownload; //big-endian;
        public uint levelCount; //big-endian;
        public AssetsBundleOffsetPair[] pLevelList;
        public uint fileSize2; //big-endian; for fileVersion >= 2
        public uint unknown2; //big-endian; for fileVersion >= 3
        public byte unknown3;

        ///public bool Read(AssetsFileReader reader, LPARAM lPar, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileWriter writer, LPARAM lPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        public uint bundleCount; //big-endian;
    }
}
