using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassDatabaseStringTable
    {
        public List<string> Strings { get; set; }

        public void Read(AssetsFileReader reader)
        {
            int stringCount = reader.ReadInt32();
            Strings = new List<string>(stringCount);
            for (int i = 0; i < stringCount; i++)
            {
                Strings.Add(reader.ReadString());
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(Strings.Count);
            for (int i = 0; i < Strings.Count; i++)
            {
                writer.Write(Strings[i]);
            }
        }

        public ushort AddString(string str)
        {
            int index = Strings.IndexOf(str);
            if (index == -1)
            {
                index = Strings.Count;
                Strings.Add(str);
            }
            return (ushort)index;
        }

        public string GetString(ushort index)
        {
            return Strings[index];
        }
    }
}
