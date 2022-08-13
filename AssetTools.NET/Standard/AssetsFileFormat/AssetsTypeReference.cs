using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsTypeReference
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string AsmName { get; set; }

        public void Read(AssetsFileReader reader)
        {
            ClassName = reader.ReadNullTerminated();
            Namespace = reader.ReadNullTerminated();
            AsmName = reader.ReadNullTerminated();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(ClassName);
            writer.WriteNullTerminated(Namespace);
            writer.WriteNullTerminated(AsmName);
        }
    }
}
