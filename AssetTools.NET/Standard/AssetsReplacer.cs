using System.IO;

namespace AssetsTools.NET.Standard
{
    public enum AssetsReplacementType
    {
        AssetsReplacement_AddOrModify,
        AssetsReplacement_Remove
    }
    public abstract class AssetsReplacer
    {
		public abstract AssetsReplacementType GetReplacementType();

		public abstract uint GetFileID();
		public abstract ulong GetPathID();
		public abstract int GetClassID();
		public abstract ushort GetMonoScriptID();
		//For add and modify
		public abstract ulong GetSize();
		public abstract ulong Write(ulong pos, AssetsFileWriter writer);
		//always writes 0 for the file id
		public abstract ulong WriteReplacer(ulong pos, AssetsFileWriter writer);
    }
    public class AssetsReplacerFromFile : AssetsReplacer
    {
        private uint fileID;
        private ulong pathID;
        private int classID;
        private ushort monoScriptIndex;
        private FileStream stream;
        private ulong offset;
        private ulong size;
        private uint copyBufferLen;
        private bool freeFile;
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AssetsReplacement_AddOrModify;
        }
        public override uint GetFileID()
        {
            return fileID;
        }
        public override ulong GetPathID()
        {
            return pathID;
        }
        public override int GetClassID()
        {
            return classID;
        }
        public override ushort GetMonoScriptID()
        {
            return monoScriptIndex;
        }
        public override ulong GetSize()
        {
            return size;
        }
        public override ulong Write(ulong pos, AssetsFileWriter writer)
        {
            return writer.Position;
        }
        public override ulong WriteReplacer(ulong pos, AssetsFileWriter writer)
        {
            return writer.Position;
        }
    }

    /*AssetsReplacer ReadAssetsReplacer(QWORD &pos, AssetsFileReader reader, LPARAM readerPar, bool prefReplacerInMemory = false);
    AssetsReplacer MakeAssetRemover(DWORD fileID, QWORD pathID, int classID, WORD monoScriptIndex = 0xFFFF);
    AssetsReplacer MakeAssetModifierFromReader(DWORD fileID, QWORD pathID, int classID, WORD monoScriptIndex,
            AssetsFileReader reader, LPARAM readerPar, QWORD size, QWORD readerPos = 0,
            uint copyBufferLen = 0);
    AssetsReplacer MakeAssetModifierFromMemory(DWORD fileID, QWORD pathID, int classID, WORD monoScriptIndex, void* buffer, size_t size, cbFreeMemoryResource freeResourceCallback);
    AssetsReplacer MakeAssetModifierFromFile(DWORD fileID, QWORD pathID, int classID, WORD monoScriptIndex, FILE* pFile, QWORD offs, QWORD size, size_t copyBufferLen = 0, bool freeFile = true);
    void FreeAssetsReplacer(AssetsReplacer* pReplacer);*/
}
