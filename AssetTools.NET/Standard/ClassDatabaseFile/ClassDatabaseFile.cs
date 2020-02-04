using SevenZip.Compression.LZMA;
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
            header.Read(reader);
            if (header.header != "cldb" ||
                !(header.fileVersion == 3 || header.fileVersion == 1) ||
                header.compressionType != 0)
            {
                valid = false;
                return valid;
            }
            classes = new List<ClassDatabaseType>();
            long classTablePos = reader.Position;
            reader.BaseStream.Position = header.stringTablePos;
            stringTable = reader.ReadBytes((int)header.stringTableLen);
            reader.Position = classTablePos;
            uint size = reader.ReadUInt32();
            for (int i = 0; i < size; i++)
            {
                ClassDatabaseType cdt = new ClassDatabaseType();
                cdt.Read(reader, header.fileVersion);
                classes.Add(cdt);
            }
            valid = true;
            return valid;
        }

        public ulong Write(AssetsFileWriter writer, int optimizeStringTable, uint compress, bool writeStringTable = true)
        {
            header.Write(writer);
            writer.Write(classes.Count);
            for (int i = 0; i < classes.Count; i++)
            {
                classes[i].Write(writer, header.fileVersion);
            }
            long stringTablePos = writer.Position;
            writer.Write(stringTable);
            long stringTableLen = writer.Position - stringTablePos;
            long fileSize = writer.Position;
            header.stringTablePos = (uint)stringTablePos;
            header.stringTableLen = (uint)stringTableLen;
            header.uncompressedSize = (uint)fileSize;
            writer.Position = 0;
            header.Write(writer);
            return 0;
        }

        public bool IsValid()
        {
            return valid;
        }
        
        public ClassDatabaseFile() { }
    }
}
