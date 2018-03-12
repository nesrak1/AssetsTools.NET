using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromFile : AssetsReplacer
    {
        public AssetsReplacerFromFile(uint fileID, ulong pathID, int classID, ushort monoScriptIndex, FileStream stream, ulong offset, ulong size, uint copyBufferLen, bool freeFile)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
            this.stream = stream;
            this.offset = offset;
            this.size = size;
            this.copyBufferLen = copyBufferLen;
            this.freeFile = freeFile;
        }
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
}
