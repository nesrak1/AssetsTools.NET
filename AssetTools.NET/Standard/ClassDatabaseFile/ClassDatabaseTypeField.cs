using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassDatabaseTypeNode
    {
        public ushort TypeName { get; set; }
        public ushort FieldName { get; set; }
        public int ByteSize { get; set; }
        public ushort Version { get; set; }
        public byte TypeFlags { get; set; }
        public uint MetaFlag { get; set; }
        public List<ClassDatabaseTypeNode> Children { get; set; }

        /// <summary>
        /// Read the <see cref="ClassDatabaseTypeNode"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            TypeName = reader.ReadUInt16();
            FieldName = reader.ReadUInt16();
            ByteSize = reader.ReadInt32();
            Version = reader.ReadUInt16();
            TypeFlags = reader.ReadByte();
            MetaFlag = reader.ReadUInt32();
            
            int childrenCount = reader.ReadUInt16();
            Children = new List<ClassDatabaseTypeNode>(childrenCount);
            for (int i = 0; i < childrenCount; i++)
            {
                ClassDatabaseTypeNode child = new ClassDatabaseTypeNode();
                child.Read(reader);
                Children.Add(child);
            }
        }

        /// <summary>
        /// Write the <see cref="ClassDatabaseTypeNode"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(FieldName);
            writer.Write(ByteSize);
            writer.Write(TypeFlags);
            writer.Write(MetaFlag);

            writer.Write((ushort)Children.Count);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Write(writer);
            }
        }
    }
}
