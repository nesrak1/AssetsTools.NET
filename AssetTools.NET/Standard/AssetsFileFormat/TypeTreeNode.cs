using System.Text;

namespace AssetsTools.NET
{
    public class TypeTreeNode
    {
        /// <summary>
        /// Version of the node.
        /// </summary>
        public ushort Version { get; set; }
        /// <summary>
        /// Level of the node (0 for root, 1 for child, etc.)
        /// </summary>
        public byte Level { get; set; }
        /// <summary>
        /// Information about whether the node is an array, registry, etc.
        /// </summary>
        public TypeTreeNodeFlags TypeFlags { get; set; }
        /// <summary>
        /// Offset of the type string in the string table.
        /// </summary>
        public uint TypeStrOffset { get; set; }
        /// <summary>
        /// Offset of the name string in the string table.
        /// </summary>
        public uint NameStrOffset { get; set; }
        /// <summary>
        /// Byte size of the field's type (for example, int is 4).
        /// If the field isn't a value type, then this value is a sum of all children sizes.
        /// If the size is variable, this is set to -1.
        /// </summary>
        public int ByteSize { get; set; }
        /// <summary>
        /// Index in the type tree. This should always be the same as the index in the array.
        /// </summary>
        public uint Index { get; set; }
        /// <summary>
        /// 0x4000 if aligned.
        /// </summary>
        public uint MetaFlags { get; set; }
        /// <summary>
        /// Unknown.
        /// </summary>
        public ulong RefTypeHash { get; set; }

        public void Read(AssetsFileReader reader, uint format)
        {
            Version = reader.ReadUInt16();
            Level = reader.ReadByte();
            TypeFlags = (TypeTreeNodeFlags)reader.ReadByte();
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
            writer.Write((byte)TypeFlags);
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
