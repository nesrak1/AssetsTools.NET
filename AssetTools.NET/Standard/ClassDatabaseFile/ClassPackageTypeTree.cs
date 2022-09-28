using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

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

		public void Read(AssetsFileReader reader)
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

			CommonString = new ClassPackageCommonString();
			CommonString.Read(reader);

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
		}

		public void Write(AssetsFileWriter writer)
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

			CommonString.Write(writer);

			writer.Write(Nodes.Count);
			for (int i = 0; i < Nodes.Count; i++)
			{
				Nodes[i].Write(writer);
			}

			StringTable.Write(writer);
		}
	}
}
