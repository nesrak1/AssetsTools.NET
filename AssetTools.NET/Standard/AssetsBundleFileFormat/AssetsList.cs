namespace AssetsTools.NET
{
    public struct AssetsList
    {
        public uint pos;
        public uint count;
        public AssetsBundleEntry[] ppEntries;
        public uint allocatedCount;
        //AssetsBundleEntry entries[0];
        ///public void Free();
        ///public bool Read(AssetsFileReader reader, LPARAM readerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileWriter writer, LPARAM writerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileReader reader, LPARAM readerPar,
        ///    AssetsFileWriter writer, LPARAM lPar, bool doWriteAssets, QWORD &curReadPos, QWORD* curWritePos = NULL,
        ///    AssetsFileVerifyLogger errorLogger = NULL);
    }
}
