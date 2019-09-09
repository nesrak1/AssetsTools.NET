namespace AssetsTools.NET
{
    public struct TypeField_0D
    {
        public ushort version;                      //0x00
        public byte depth;                          //0x02 //specifies the amount of parents
        public byte isArray;                        //0x03
        public uint typeStringOffset;               //0x04 //-the hardcoded table is offset by 0x80000000
        public uint nameStringOffset;               //0x08 //-same here
        public uint size;                           //0x0C //size in bytes; if not static (if it contains an array), set to -1
        public uint index;                          //0x10
        public uint flags;                          //0x14
        public byte[] unknown;                      //0x18
        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint format, bool bigEndian)
        {
            version = reader.ReadUInt16();
            depth = reader.ReadByte();
            isArray = reader.ReadByte();
            typeStringOffset = reader.ReadUInt32();
            nameStringOffset = reader.ReadUInt32();
            size = reader.ReadUInt32();
            index = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            if (format >= 0x12)
                unknown = reader.ReadBytes(8);
            else
                unknown = new byte[0];
            return reader.Position;
        }
        public ulong Write(ulong curFilePos, AssetsFileWriter writer, uint format)
        {
            writer.Write(version);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(typeStringOffset);
            writer.Write(nameStringOffset);
            writer.Write(size);
            writer.Write(index);
            writer.Write(flags);
            if (format >= 0x12)
                writer.Write(unknown);
            return writer.Position;
        }
        public enum TypeFieldArrayType
        {
            Array       = 0b0001,
            Ref         = 0b0010,
            Registry    = 0b0100,
            ArrayOfRefs = 0b1000
        }
        ///public string GetTypeString(string stringTable, ulong stringTableLen);
        ///public string GetNameString(string stringTable, ulong stringTableLen);
    }
}
