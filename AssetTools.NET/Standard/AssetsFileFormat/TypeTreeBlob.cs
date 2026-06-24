using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class TypeTreeBlob
    {
        /// <summary>
        /// Nodes for this type. This list will be empty if the type is stripped.
        /// </summary>
        public List<TypeTreeNode> Nodes { get; set; }
        /// <summary>
        /// String table bytes for this type.
        /// </summary>
        public byte[] StringBufferBytes { get; set; }

        private const int TYPE_TREE_HEADER_MAGIC = 0x7474686d;

        /// <summary>
        /// Read the <see cref="TypeTreeBlob"/> with the provided reader and format version.
        /// <paramref name="version"/> must be set in versions &lt; 23. In versions &gt;= 32,
        /// setting version to <see cref="uint.MaxValue"/> will read a version from the header.
        /// <see cref="uint.MaxValue"/> should be used when reading external type trees.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="version">The version of the file, or max int to auto-detect.</param>
        public void Read(AssetsFileReader reader, uint version)
        {
            if (version >= 23 || version == uint.MaxValue)
            {
                uint extTypeTreeMagic = reader.ReadUInt32();
                if (extTypeTreeMagic != TYPE_TREE_HEADER_MAGIC)
                {
                    throw new Exception("Expected tthm in extended type tree type");
                }

                uint extTypeTreeVer = reader.ReadUInt32();
                if (version != uint.MaxValue)
                {
                    if (extTypeTreeVer != version)
                    {
                        throw new Exception($"Expected version {version} in extended type tree type, found {extTypeTreeVer}");
                    }
                }
                else
                {
                    version = extTypeTreeVer;
                }
            }

            int typeTreeNodeCount = reader.ReadInt32();
            int stringBufferLen = reader.ReadInt32();

            Nodes = new List<TypeTreeNode>(typeTreeNodeCount);
            for (int i = 0; i < typeTreeNodeCount; i++)
            {
                TypeTreeNode typeField = new TypeTreeNode();
                typeField.Read(reader, version);
                Nodes.Add(typeField);
            }

            StringBufferBytes = reader.ReadBytes(stringBufferLen);
        }

        /// <summary>
        /// Write the <see cref="TypeTreeBlob"/> with the provided writer and format version.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="version">The version of the file.</param>
        public void Write(AssetsFileWriter writer, uint version)
        {
            if (version >= 23)
            {
                writer.Write(TYPE_TREE_HEADER_MAGIC);
                writer.Write(version);
            }

            writer.Write(Nodes.Count);
            writer.Write(StringBufferBytes.Length);

            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Write(writer, version);
            }

            writer.Write(StringBufferBytes);
        }
    }
}
