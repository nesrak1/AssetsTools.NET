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

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(ClassId);

            writer.Write(Classes.Count);
            for (int i = 0; i < Classes.Count; i++)
            {
                writer.Write(Classes[i].Key.ToUInt64());
                if (Classes[i].Value != null)
                {
                    writer.Write((byte)1);
                    Classes[i].Value.Write(writer);
                }
                else
                {
                    writer.Write((byte)0);
                }
            }
        }

        public ClassPackageType GetTypeForVersion(UnityVersion version)
        {
            if (Classes.Count == 0)
            {
                return null;
            }

            ClassPackageType lastType = Classes[0].Value;
            for (int i = 0; i < Classes.Count; i++)
            {
                if (Classes[i].Key.ToUInt64() >= version.ToUInt64())
                {
                    return lastType;
                }
                lastType = Classes[i].Value;
            }

            return lastType;
        }
    }
}
