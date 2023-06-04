using System;
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

        /// <summary>
        /// Read the <see cref="TypeTreeNode"/> with the provided reader and format version.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="version">The version of the file.</param>
        public void Read(AssetsFileReader reader, uint version)
        {
            Version = reader.ReadUInt16();
            Level = reader.ReadByte();
            TypeFlags = (TypeTreeNodeFlags)reader.ReadByte();
            TypeStrOffset = reader.ReadUInt32();
            NameStrOffset = reader.ReadUInt32();
            ByteSize = reader.ReadInt32();
            Index = reader.ReadUInt32();
            MetaFlags = reader.ReadUInt32();
            if (version >= 0x12)
            {
                RefTypeHash = reader.ReadUInt64();
            }
        }

        /// <summary>
        /// Write the <see cref="TypeTreeNode"/> with the provided writer and format version.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="version">The version of the file.</param>
        public void Write(AssetsFileWriter writer, uint version)
        {
            writer.Write(Version);
            writer.Write(Level);
            writer.Write((byte)TypeFlags);
            writer.Write(TypeStrOffset);
            writer.Write(NameStrOffset);
            writer.Write(ByteSize);
            writer.Write(Index);
            writer.Write(MetaFlags);
            if (version >= 0x12)
            {
                writer.Write(RefTypeHash);
            }
        }

        /// <summary>
        /// Get the type name from the string table (from <see cref="TypeTreeType.StringBuffer"/>).
        /// </summary>
        /// <param name="stringTable">The string table to use.</param>
        /// <param name="commonStringTable">
        /// The common string table to use, if the builtin one is outdated.
        /// See <see cref="ClassDatabaseFile.CommonStringBufferIndices"/>.
        /// </param>
        /// <returns>The node type name.</returns>
        public string GetTypeString(string stringTable, string commonStringTable = null)
        {
            return ReadStringTableString(stringTable, commonStringTable ?? TypeTreeType.COMMON_STRING_TABLE, TypeStrOffset);
        }

        /// <summary>
        /// Get the name name from the string table (from <see cref="TypeTreeType.StringBuffer"/>).
        /// </summary>
        /// <param name="stringTable">The string table to use.</param>
        /// <param name="commonStringTable">
        /// The common string table to use, if the builtin one is outdated.
        /// See <see cref="ClassDatabaseFile.CommonStringBufferIndices"/>.
        /// </param>
        /// <returns>The node name.</returns>
        public string GetNameString(string stringTable, string commonStringTable = null)
        {
            return ReadStringTableString(stringTable, commonStringTable ?? TypeTreeType.COMMON_STRING_TABLE, NameStrOffset);
        }

        private string ReadStringTableString(string stringTable, string commonStringTable, uint offset)
        {
            if (offset >= 0x80000000)
            {
                offset &= ~0x80000000;
                stringTable = commonStringTable;
            }

            StringBuilder str = new StringBuilder();
            int pos = (int)offset;
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
