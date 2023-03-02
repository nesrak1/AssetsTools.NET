using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetTypeReference
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string AsmName { get; set; }

        public AssetTypeReference() { }
        
        public AssetTypeReference(string className, string nameSpace, string asmName)
        {
            ClassName = className;
            Namespace = nameSpace;
            AsmName = asmName;
        }

        public void ReadMetadata(AssetsFileReader reader)
        {
            ClassName = reader.ReadNullTerminated();
            Namespace = reader.ReadNullTerminated();
            AsmName = reader.ReadNullTerminated();
        }

        public void ReadAsset(AssetsFileReader reader)
        {
            ClassName = reader.ReadCountStringInt32(); reader.Align();
            Namespace = reader.ReadCountStringInt32(); reader.Align();
            AsmName = reader.ReadCountStringInt32(); reader.Align();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(ClassName);
            writer.WriteNullTerminated(Namespace);
            writer.WriteNullTerminated(AsmName);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AssetTypeReference otherObj))
                return false;

            return ClassName == otherObj.ClassName &&
                Namespace == otherObj.Namespace &&
                AsmName == otherObj.AsmName;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + ClassName.GetHashCode();
            hash = hash * 23 + Namespace.GetHashCode();
            hash = hash * 23 + AsmName.GetHashCode();
            return hash;
        }
    }
}
