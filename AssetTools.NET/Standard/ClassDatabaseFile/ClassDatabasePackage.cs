using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassDatabasePackage
    {
        public bool valid;

        public ClassDatabasePackageHeader header;
        public ClassDatabaseFile[] files;
        public byte[] stringTable;

        public void Read(AssetsFileReader reader)
        {
            header = new ClassDatabasePackageHeader();
            header.Read(reader);
            if ((header.compressionType & 0x20) == 0x00)
                throw new NotImplementedException("Please uncompress the package before loading.");
            files = new ClassDatabaseFile[header.fileCount];
            ulong firstFile = reader.Position;
            for (int i = 0; i < header.fileCount; i++)
            {
                reader.Position = firstFile + header.files[i].offset;
                byte[] data = reader.ReadBytes((int)header.files[i].length);
                using (MemoryStream ms = new MemoryStream(data))
                using (AssetsFileReader r = new AssetsFileReader(ms))
                {
                    files[i] = new ClassDatabaseFile();
                    files[i].Read(r);
                }
            }
            stringTable = reader.ReadBytes((int)header.stringTableLenUncompressed);
            for (int i = 0; i < header.fileCount; i++)
            {
                files[i].stringTable = stringTable;
            }
        }
    }
}
