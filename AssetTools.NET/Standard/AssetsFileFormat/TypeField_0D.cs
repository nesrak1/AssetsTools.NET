namespace AssetsTools.NET
{
    public struct TypeField_0D
    {
        public ushort version;                      //0x00
        public byte depth;                          //0x02 //specifies the amount of parents
        public bool isArray;                        //0x03
        public uint typeStringOffset;               //0x04 //-the hardcoded table is offset by 0x80000000
        public uint nameStringOffset;               //0x08 //-same here
        public uint size;                           //0x0C //size in bytes; if not static (if it contains an array), set to -1
        public uint index;                          //0x10
        public uint flags;                          //0x14
        public ulong Read(ulong absFilePos, AssetsFileReader reader, bool bigEndian)
        {
            version = reader.ReadUInt16();
            depth = reader.ReadByte();
            isArray = reader.ReadBoolean();
            typeStringOffset = reader.ReadUInt32();
            nameStringOffset = reader.ReadUInt32();
            size = reader.ReadUInt32();
            index = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            return reader.Position;
        }
        public ulong Write(ulong curFilePos, AssetsFileWriter writer)
        {
            writer.Write(version);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(typeStringOffset);
            writer.Write(nameStringOffset);
            writer.Write(size);
            writer.Write(index);
            writer.Write(flags);
            return writer.Position;
        }
        ///public string GetTypeString(string stringTable, ulong stringTableLen);
        ///public string GetNameString(string stringTable, ulong stringTableLen);
    }
}
