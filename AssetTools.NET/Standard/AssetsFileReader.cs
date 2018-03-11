////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System.IO;

namespace AssetsTools.NET
{
    /*
    //Creates a reader that can read split .assets files (opens all at once)
    public FileStream Create_AssetsReaderFromSplitFile(string baseFileName);
    public void Free_AssetsReaderFromSplitFile(FileStream FileStream);
    public ulong AssetsReaderFromSplitFile(ulong pos, ulong count, void* pBuf, FileStream par);

    public ulong AssetsReaderFromFile(ulong pos, ulong count, void* pBuf, FileStream par);
    public void AssetsVerifyLoggerFromFile(char* message);
    public void AssetsVerifyLoggerToConsole(char* message);

    public FileStream Create_AssetsReaderFromMemory(void* buf, size_t bufLen, bool copyBuf);
    public void Free_AssetsReaderFromMemory(FileStream FileStream, bool freeBuf = false);
    public ulong AssetsReaderFromMemory(ulong pos, ulong count, void* pBuf, FileStream par);

    //Creates a reader that begins at a specific offset from another reader
    public FileStream Create_AssetsWriterOffset(AssetsFileWriter origWriter, FileStream origPar, ulong offset);

    //Creates a reader that has a specific region of another reader
    public AssetsFileReader Create_PartialAssetsFileReader(AssetsFileReader reader, FileStream* pLPar,
        ulong rangeBegin, ulong rangeLength);
    public ulong PartialAssetsFileReader(ulong pos, ulong count, void* pBuf, FileStream par);
    public void Free_PartialAssetsFileReader(FileStream FileStream);
    public AssetsFileReader Free_PartialAssetsFileReader(FileStream* pFileStream);

    public ulong AssetsWriterToFile(ulong pos, ulong count, const void* pBuf, FileStream par);

    public FileStream Create_AssetsWriterToMemory(void* buf, size_t bufLen);
    public FileStream Create_AssetsWriterToMemoryDynamic();
    public void Get_AssetsWriterToMemory_Buf(FileStream FileStream, size_t* pPos, size_t* pSize);
    public void Free_AssetsWriterToMemory(FileStream FileStream, bool freeIfDynamic = true);
    public void Free_AssetsWriterToMemory_DynMem(void* p);
    public ulong AssetsWriterToMemory(ulong pos, ulong count, const void* pBuf, FileStream par);

    public ulong AssetsWriterOffset(ulong pos, ulong count, const void* pBuf, FileStream par);
    public void Free_AssetsWriterOffset(FileStream FileStream);
    */
}
