using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromMemory : AssetsReplacer
    {
        public AssetsReplacerFromMemory(uint fileID, ulong pathID, int classID, ushort monoScriptIndex, byte[] buffer/*, cbFreeMemoryResource freeResourceCallback*/)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
            this.buffer = buffer;
        }
        private uint fileID;
        private ulong pathID;
        private int classID;
        private ushort monoScriptIndex;
        private byte[] buffer;
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
            return (ulong)buffer.Length;
        }
        public override ulong Write(ulong pos, AssetsFileWriter writer)
        {
            writer.Write(buffer);
            return writer.Position;
        }
        public override ulong WriteReplacer(ulong pos, AssetsFileWriter writer)
        {
            //-no idea what this is supposed to write
            return writer.Position;
        }
    }
}
