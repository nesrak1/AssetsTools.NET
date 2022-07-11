using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassDatabaseFile
    {
        public ClassDatabaseFileHeader Header;
        public List<ClassDatabaseType> Classes;
        public ClassDatabaseStringTable StringTable;

        public string GetString(ushort index) => StringTable.GetString(index);

        public void Read(AssetsFileReader reader)
        {
            Header.Read(reader);

            AssetsFileReader dReader = GetDecompressedReader(reader);
            int classCount = dReader.ReadInt32();
            Classes = new List<ClassDatabaseType>(classCount);
            for (int i = 0; i < classCount; i++)
            {
                ClassDatabaseType type = new ClassDatabaseType();
                type.Read(dReader);
                Classes.Add(type);
            }

            StringTable.Read(dReader);
        }

        public void Write(AssetsFileWriter writer)
        {

        }

        private AssetsFileReader GetDecompressedReader(AssetsFileReader reader)
        {
            AssetsFileReader newReader = reader;
            if (Header.CompressionType != 0)
            {
                MemoryStream ms;
                if (Header.CompressionType == 1) //lz4
                {
                    byte[] uncompressedBytes = new byte[Header.DecompressedSize];
                    using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes(Header.CompressedSize)))
                    {
                        Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                        decoder.Read(uncompressedBytes, 0, Header.DecompressedSize);
                        decoder.Dispose();
                    }
                    ms = new MemoryStream(uncompressedBytes);
                }
                else if (Header.CompressionType == 2) //lzma
                {
                    using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes(Header.CompressedSize)))
                    {
                        ms = SevenZipHelper.StreamDecompress(tempMs);
                    }
                }
                else
                {
                    throw new Exception($"Class database is using invalid compression type {Header.CompressionType}!");
                }

                newReader = new AssetsFileReader(ms);
            }

            return newReader;
        }
    }
}
