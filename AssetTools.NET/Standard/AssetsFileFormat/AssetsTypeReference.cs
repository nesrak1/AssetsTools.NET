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
            // todo
        }

        public void Write(AssetsFileWriter writer)
        {
            // todo
        }
    }
}
