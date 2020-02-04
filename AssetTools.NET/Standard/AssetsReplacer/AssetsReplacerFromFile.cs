using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromFile : AssetsReplacer
    {
        public AssetsReplacerFromFile(int fileID, long pathID, int classID, ushort monoScriptIndex, FileStream stream, long offset, long size)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
            this.stream = stream;
            this.offset = offset;
            this.size = size;
        }
        private int fileID;
        private long pathID;
        private int classID;
        private ushort monoScriptIndex;
        private FileStream stream;
        private long offset;
        private long size;
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
            return size;
        }
        public override long Write(AssetsFileWriter writer)
        {
            stream.Position = (int)offset;
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
