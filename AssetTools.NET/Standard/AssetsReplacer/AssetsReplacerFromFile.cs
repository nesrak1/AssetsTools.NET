using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromFile : AssetsReplacer
    {
        private readonly int fileId;
        private readonly long pathId;
        private readonly int classId;
        private readonly ushort monoScriptIndex;
        private readonly FileStream stream;
        private readonly long offset;
        private readonly long size;

        public AssetsReplacerFromFile(int fileId, long pathId, int classId, ushort monoScriptIndex, FileStream stream, long offset, long size)
        {
            this.fileId = fileId;
            this.pathId = pathId;
            this.classId = classId;
            this.monoScriptIndex = monoScriptIndex;
            this.stream = stream;
            this.offset = offset;
            this.size = size;
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
            return size;
        }
        public override long Write(AssetsFileWriter writer)
        {
            byte[] assetData = new byte[size];
            stream.Read(assetData, (int)offset, (int)size);
            writer.Write(assetData);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            throw new NotImplementedException("not implemented");
        }
    }
}
