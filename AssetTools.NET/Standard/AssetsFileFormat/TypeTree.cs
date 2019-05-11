namespace AssetsTools.NET
{
    public struct TypeTree
    {
        public string unityVersion; //null-terminated; stored for .assets format > 6
        public uint version; //big endian; stored for .assets format > 6
        public bool hasTypeTree; //stored for .assets format >= 13; Unity 5 only stores some metadata if it's set to false
        public uint fieldCount; //big endian;

        public Type_0D[] pTypes_Unity5;
        public Type_07[] pTypes_Unity4;

        public uint dwUnknown; //actually belongs to the asset list; stored for .assets format < 14
        public uint _fmt; //not stored here in the .assets file, the variable is just to remember the .assets file version

        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint version, bool bigEndian)
        {
            unityVersion = reader.ReadNullTerminated();
            this.version = reader.ReadUInt32();
            hasTypeTree = reader.ReadBoolean();
            fieldCount = reader.ReadUInt32();
            pTypes_Unity5 = new Type_0D[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                Type_0D type0d = new Type_0D();
                type0d.Read(hasTypeTree, reader.Position, reader, version, version, bigEndian);
                pTypes_Unity5[i] = type0d;
            }
            if (version < 0x0E)
            {
                dwUnknown = reader.ReadUInt24();
            }
            _fmt = version; //-todo: figure out what the heck this is for. if ver = -1 on write does it set it to default or something?
            return reader.Position;
        }//Minimum AssetsFile format : 6
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint version)
        {
            writer.WriteNullTerminated(unityVersion);
            writer.Write(this.version);
            writer.Write(hasTypeTree);
            writer.Write(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                pTypes_Unity5[i].Write(hasTypeTree, writer.Position, writer, version);
            }
            if (version < 0x0E)
            {
                writer.WriteUInt24(dwUnknown);
            }
            return writer.Position;
        }

        ///public void Clear();
    }
}
