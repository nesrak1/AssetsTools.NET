using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromMemory : AssetsReplacer
    {
        private readonly int fileId;
        private readonly long pathId;
        private readonly int classId;
        private readonly ushort monoScriptIndex;
        private readonly byte[] buffer;

        public AssetsReplacerFromMemory(int fileId, long pathId, int classId, ushort monoScriptIndex, byte[] buffer/*, cbFreeMemoryResource freeResourceCallback*/)
        {
            this.fileId = fileId;
            this.pathId = pathId;
            this.classId = classId;
            this.monoScriptIndex = monoScriptIndex;
            this.buffer = buffer;
        }
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AddOrModify;
        }
        public override int GetFileID()
        {
            return fileId;
        }
        public override long GetPathID()
        {
            return pathId;
        }
        public override int GetClassID()
        {
            return classId;
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
