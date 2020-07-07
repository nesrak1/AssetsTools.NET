using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsRemover : AssetsReplacer
    {
        private readonly int fileID;
        private readonly long pathID;
        private readonly int classID;
        private readonly ushort monoScriptIndex;

        public AssetsRemover(int fileID, long pathID, int classID, ushort monoScriptIndex = 0xFFFF)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
        }
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.Remove;
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
            return 0;
        }
        public override long Write(AssetsFileWriter writer)
        {
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            throw new NotImplementedException("not implemented");
        }
    }
}
