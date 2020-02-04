using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromMemory : AssetsReplacer
    {
        public AssetsReplacerFromMemory(int fileID, long pathID, int classID, ushort monoScriptIndex, byte[] buffer/*, cbFreeMemoryResource freeResourceCallback*/)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
            this.buffer = buffer;
        }
        private int fileID;
        private long pathID;
        private int classID;
        private ushort monoScriptIndex;
        private byte[] buffer;
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AssetsReplacement_AddOrModify;
        }
        public override int GetFileID()
        {
            return fileID;
        }
        public override long GetPathID()
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
        public override long GetSize()
        {
            return buffer.Length;
        }
        public override long Write(AssetsFileWriter writer)
        {
            writer.Write(buffer);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            throw new NotImplementedException("not implemented");
        }
    }
}
