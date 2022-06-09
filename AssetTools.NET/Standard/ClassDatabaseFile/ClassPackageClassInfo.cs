using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageClassInfo
    {
        public int ClassId { get; set; }
        public List<KeyValuePair<UnityVersion, ClassPackageType>> Classes { get; set; }

        public void Read(AssetsFileReader reader)
        {
            ClassId = reader.ReadInt32();

            int classCount = reader.ReadInt32();
            Classes = new List<KeyValuePair<UnityVersion, ClassPackageType>>(classCount);
            for (int i = 0; i < classCount; i++)
            {
                UnityVersion version = UnityVersion.FromUInt64(reader.ReadUInt64());
                bool hasClassData = reader.ReadBoolean();
                ClassPackageType type = null;
                if (hasClassData)
                {
                    type = new ClassPackageType();
                    type.Read(reader, ClassId);
                }
                Classes.Add(new KeyValuePair<UnityVersion, ClassPackageType>(version, type));
            }
        }

        public ClassPackageType GetTypeForVersion(UnityVersion version)
        {
            ClassPackageType lastType = Classes[0].Value;
            UnityVersion lastVer = Classes[0].Key;
            for (int i = 0; i < Classes.Count; i++)
            {
                if (Classes[i].Key.ToUInt64() >= version.ToUInt64())
                {
                    System.Diagnostics.Debug.WriteLine($"for {(AssetClassID)ClassId} selecting {lastVer} (by {version}, next {Classes[i].Key})");
                    return lastType;
                }
                lastType = Classes[i].Value;
                lastVer = Classes[i].Key;
            }

            System.Diagnostics.Debug.WriteLine($"for {(AssetClassID)ClassId} selecting last choice {lastVer} (by {version})");
            return lastType;
        }
    }
}
