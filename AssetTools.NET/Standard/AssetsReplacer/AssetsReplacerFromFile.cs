using System.IO;

namespace AssetsTools.NET.Standard.AssetsReplacer
{
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
}
