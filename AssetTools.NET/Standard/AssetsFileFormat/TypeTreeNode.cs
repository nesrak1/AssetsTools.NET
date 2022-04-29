using System.Text;

namespace AssetsTools.NET
{
    public class TypeTreeNode
    {
        public ushort Version { get; set; }
        public byte Level { get; set; }
        /// <summary>
        /// 1 if array
        /// </summary>
        public byte TypeFlags { get; set; }
        public uint TypeStrOffset { get; set; }
        public uint NameStrOffset { get; set; }
        public int ByteSize { get; set; }
        public uint Index { get; set; }
        /// <summary>
        /// 0x4000 if aligned
        /// </summary>
        public uint MetaFlags { get; set; }
        public ulong RefTypeHash { get; set; }

        public void Read(AssetsFileReader reader, uint format)
        {
            Version = reader.ReadUInt16();
            Level = reader.ReadByte();
            TypeFlags = reader.ReadByte();
            TypeStrOffset = reader.ReadUInt32();
            NameStrOffset = reader.ReadUInt32();
            ByteSize = reader.ReadInt32();
            Index = reader.ReadUInt32();
            MetaFlags = reader.ReadUInt32();
            if (format >= 0x12)
            {
                RefTypeHash = reader.ReadUInt64();
            }
        }

        public void Write(AssetsFileWriter writer, uint format)
        {
            writer.Write(Version);
            writer.Write(Level);
            writer.Write(TypeFlags);
            writer.Write(TypeStrOffset);
            writer.Write(NameStrOffset);
            writer.Write(ByteSize);
            writer.Write(Index);
            writer.Write(MetaFlags);
            if (format >= 0x12)
            {
                writer.Write(RefTypeHash);
            }
        }

        // todo: refactor
        public string GetTypeString(string stringTable)
        {
            StringBuilder str = new StringBuilder();
            uint newTypeStringOffset = TypeStrOffset;
            if (newTypeStringOffset >= 0x80000000)
            {
                newTypeStringOffset -= 0x80000000;
                stringTable = TypeTreeType.COMMON_STRING_TABLE;
            }
            int pos = (int)newTypeStringOffset;
            char c;
            while ((c = stringTable[pos]) != 0x00)
            {
                str.Append(c);
                pos++;
            }
            return str.ToString();
        }

        public string GetNameString(string stringTable)
        {
            StringBuilder str = new StringBuilder();
            uint newNameStringOffset = NameStrOffset;
            if (newNameStringOffset >= 0x80000000)
            {
                newNameStringOffset -= 0x80000000;
                stringTable = TypeTreeType.COMMON_STRING_TABLE;
            }
            int pos = (int)newNameStringOffset;
            char c;
            while ((c = stringTable[pos]) != 0x00)
            {
                str.Append(c);
                pos++;
            }
            return str.ToString();
        }
    }
}
