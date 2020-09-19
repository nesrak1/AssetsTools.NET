namespace AssetsTools.NET
{
    public class AssetsList
    {
        public uint pos;
        public uint count;
        public AssetsBundleEntry[] entries;
        public uint allocatedCount;
        ///public void Free();
        ///public bool Read(AssetsFileReader reader, LPARAM readerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileWriter writer, LPARAM writerPar, QWORD &curFilePos, AssetsFileVerifyLogger errorLogger = NULL);
        ///public bool Write(AssetsFileReader reader, LPARAM readerPar,
        ///    AssetsFileWriter writer, LPARAM lPar, bool doWriteAssets, QWORD &curReadPos, QWORD* curWritePos = NULL,
        ///    AssetsFileVerifyLogger errorLogger = NULL);
    }
}
