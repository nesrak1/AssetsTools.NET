using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassPackageTypeTree
    {
        public DateTime CreationTime { get; set; }
        public List<UnityVersion> Versions { get; set; }
        public List<ClassPackageClassInfo> ClassInformation { get; set; }
        public ClassPackageCommonString CommonString { get; set; }
        public List<ClassPackageTypeNode> Nodes { get; set; }
        public ClassDatabaseStringTable StringTable { get; set; }

        /// <summary>
        /// Read the <see cref="ClassPackageTypeTree"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="fileVersion">The version of the file.</param>
        public void Read(AssetsFileReader reader, byte fileVersion)
        {
            CreationTime = DateTime.FromBinary(reader.ReadInt64());

            int versionCount = reader.ReadInt32();
            Versions = new List<UnityVersion>(versionCount);
            for (int i = 0; i < versionCount; i++)
            {
                Versions.Add(UnityVersion.FromUInt64(reader.ReadUInt64()));
            }

            int classCount = reader.ReadInt32();
            ClassInformation = new List<ClassPackageClassInfo>();
            for (int i = 0; i < classCount; i++)
            {
                ClassPackageClassInfo classInfo = new ClassPackageClassInfo();
                classInfo.Read(reader);
                ClassInformation.Add(classInfo);
            }

            long commonStringAddr = reader.Position;
            CommonString = new ClassPackageCommonString();
            ClassPackageCommonString.Skip(reader, fileVersion);

            int nodeCount = reader.ReadInt32();
            Nodes = new List<ClassPackageTypeNode>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                ClassPackageTypeNode node = new ClassPackageTypeNode();
                node.Read(reader);
                Nodes.Add(node);
            }

            StringTable = new ClassDatabaseStringTable();
            StringTable.Read(reader);

            reader.Position = commonStringAddr;
            CommonString.Read(reader, fileVersion, StringTable);
        }

        /// <summary>
        /// Write the <see cref="ClassPackageTypeTree"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="fileVersion">The version of the file.</param>
        public void Write(AssetsFileWriter writer, byte fileVersion)
        {
            writer.Write(CreationTime.ToBinary());

            writer.Write(Versions.Count);
            for (int i = 0; i < Versions.Count; i++)
            {
                writer.Write(Versions[i].ToUInt64());
            }

            writer.Write(ClassInformation.Count);
            for (int i = 0; i < ClassInformation.Count; i++)
            {
                ClassInformation[i].Write(writer);
            }

            CommonString.Write(writer, fileVersion, StringTable);

            writer.Write(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Write(writer);
            }

            StringTable.Write(writer);
        }
    }
}
