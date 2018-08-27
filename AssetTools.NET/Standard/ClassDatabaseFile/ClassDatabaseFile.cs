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
            header.Read(reader, 0);
            if (header.header != "cldb" || header.fileVersion != 3 || header.compressionType != 0)
            {
                valid = false;
                return valid;
            }
            classes = new List<ClassDatabaseType>();
            ulong classTablePos = reader.Position;
            reader.BaseStream.Position = header.stringTablePos;
            stringTable = reader.ReadBytes((int)header.stringTableLen);
            reader.Position = classTablePos;
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

        public ulong Write(AssetsFileWriter writer, ulong filePos, int optimizeStringTable, uint compress, bool writeStringTable = true)
        {
            header.Write(writer, writer.Position);
            writer.Write(classes.Count);
            for (int i = 0; i < classes.Count; i++)
            {
                classes[i].Write(writer, filePos, header.fileVersion);
            }
            ulong stringTablePos = writer.Position;
            writer.Write(stringTable);
            ulong stringTableLen = writer.Position - stringTablePos;
            ulong fileSize = writer.Position;
            header.stringTablePos = (uint)stringTablePos;
            header.stringTableLen = (uint)stringTableLen;
            header.uncompressedSize = (uint)fileSize;
            writer.Position = 0;
            header.Write(writer, writer.Position);
            return 0;
        }

        public bool IsValid()
        {
            return valid;
        }
        
        public ClassDatabaseFile() { }
    }
}
