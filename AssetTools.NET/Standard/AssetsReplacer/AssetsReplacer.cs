namespace AssetsTools.NET
{
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
}
