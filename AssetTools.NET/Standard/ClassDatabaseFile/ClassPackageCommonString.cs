using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageCommonString
    {
        public Dictionary<UnityVersion, byte> VersionInformation { get; set; }
        public List<ushort> StringBufferIndices { get; set; }

        public void Read(AssetsFileReader reader)
        {
            int versionCount = reader.ReadInt32();
            VersionInformation = new Dictionary<UnityVersion, byte>(versionCount);
            for (int i = 0; i < versionCount; i++)
            {
                UnityVersion version = UnityVersion.FromUInt64(reader.ReadUInt64());
                byte stringCount = reader.ReadByte();
                VersionInformation[version] = stringCount;
            }

            int indicesCount = reader.ReadInt32();
            StringBufferIndices = new List<ushort>(indicesCount);
            for (int i = 0; i < indicesCount; i++)
            {
                StringBufferIndices.Add(reader.ReadUInt16());
            }
        }
    }
}
