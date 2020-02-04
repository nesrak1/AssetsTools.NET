namespace AssetsTools.NET
{
    public abstract class AssetsReplacer
    {
        public abstract AssetsReplacementType GetReplacementType();

        public abstract int GetFileID();
        public abstract long GetPathID();
        public abstract int GetClassID();
        public abstract ushort GetMonoScriptID();
        //For add and modify
        public abstract long GetSize();
        public abstract long Write(AssetsFileWriter writer);
        //always writes 0 for the file id
        public abstract long WriteReplacer(AssetsFileWriter writer);
    }
}
