using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassDatabaseFile
    {
        public bool valid;
        public ClassDatabaseFileHeader header;

        public List<ClassDatabaseType> classes;

        public byte[] stringTable;
        public bool Read(AssetsFileReader reader)
        {
            header = new ClassDatabaseFileHeader();
            header.Read(reader, 0);
            if (header.header != "cldb" || header.fileVersion != 3 || header.compressionType != 0)
            {
                valid = false;
                return valid;
            }
            classes = new List<ClassDatabaseType>();
            long classTablePos = reader.BaseStream.Position;
            reader.BaseStream.Position = header.stringTablePos;
            stringTable = reader.ReadBytes((int)header.stringTableLen);
            reader.BaseStream.Position = classTablePos;
            uint size = reader.ReadUInt32();
            for (int i = 0; i < size; i++)
            {
                ClassDatabaseType cdt = new ClassDatabaseType();
                cdt.Read(reader, reader.Position, header.fileVersion);
                classes.Add(cdt);
            }
            valid = true;
            return valid;
        }
        public bool IsValid()
        {
            return valid;
        }
        
        public ClassDatabaseFile() { }
    }
}
