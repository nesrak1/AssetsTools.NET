using System.IO;

namespace AssetsTools.NET
{
    public class AssetsRemover : AssetsReplacer
    {
        public AssetsRemover(uint fileID, ulong pathID, int classID, ushort monoScriptIndex = 0xFFFF)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
        }
        private uint fileID;
        private ulong pathID;
        private int classID;
        private ushort monoScriptIndex;
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AssetsReplacement_Remove;
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
            return 0;
        }
        public override ulong Write(ulong pos, AssetsFileWriter writer)
        {
            return writer.Position;
        }
        public override ulong WriteReplacer(ulong pos, AssetsFileWriter writer)
        {
            //-no idea what this is supposed to write
            return writer.Position;
        }
    }
}
