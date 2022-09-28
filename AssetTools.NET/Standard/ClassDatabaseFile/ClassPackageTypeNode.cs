using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageTypeNode
    {
        public ushort TypeName { get; set; }
        public ushort FieldName { get; set; }
        public int ByteSize { get; set; }
        public ushort Version { get; set; }
        public byte TypeFlags { get; set; }
        public uint MetaFlag { get; set; }
        public ushort[] SubNodes { get; set; }

        public void Read(AssetsFileReader reader)
        {
            TypeName = reader.ReadUInt16();
            FieldName = reader.ReadUInt16();
            ByteSize = reader.ReadInt32();
            Version = reader.ReadUInt16();
            TypeFlags = reader.ReadByte();
            MetaFlag = reader.ReadUInt32();

            ushort subNodeCount = reader.ReadUInt16();
            SubNodes = new ushort[subNodeCount];
            for (int i = 0; i < subNodeCount; i++)
            {
                SubNodes[i] = reader.ReadUInt16();
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(TypeName);
            writer.Write(FieldName);
            writer.Write(ByteSize);
            writer.Write(Version);
            writer.Write(TypeFlags);
            writer.Write(MetaFlag);

            writer.Write(SubNodes.Length);
            for (int i = 0; i < SubNodes.Length; i++)
            {
                writer.Write(SubNodes[i]);
            }
        }
    }
}
