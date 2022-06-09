using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    /*
    public class ClassDatabaseFile
    {
        public ClassDatabaseFileHeader header;
        public List<ClassDatabaseType> classes;
        public byte[] stringTable;

        public void Read(AssetsFileReader reader)
        {
            header = new ClassDatabaseFileHeader();
            header.Read(reader);
            if (header.header != "cldb" || header.fileVersion > 4 || header.fileVersion < 1)
            {
                throw new Exception($"Cldb has invalid header or the version is unsupported!");
            }
            classes = new List<ClassDatabaseType>();

            long classTablePos = reader.Position;
            AssetsFileReader newReader = reader;
            if (header.compressionType != 0)
            {
                classTablePos = 0;
                MemoryStream ms;
                if (header.compressionType == 1) //lz4
                {
                    byte[] uncompressedBytes = new byte[header.uncompressedSize];
                    using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes((int)header.compressedSize)))
                    {
                        Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                        decoder.Read(uncompressedBytes, 0, (int)header.uncompressedSize);
                        decoder.Dispose();
                    }
                    ms = new MemoryStream(uncompressedBytes);
                }
                else if (header.compressionType == 2) //lzma
                {
                    using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes((int)header.compressedSize)))
                    {
                        ms = SevenZipHelper.StreamDecompress(tempMs);
                    }
                }
                else
                {
                    throw new Exception($"Cldb is using invalid compression type {header.compressionType & 0x1f}!");
                }

                newReader = new AssetsFileReader(ms);
                newReader.BigEndian = false;
            }

            newReader.Position = header.stringTablePos;
            stringTable = newReader.ReadBytes((int)header.stringTableLen);
            newReader.Position = classTablePos;
            uint size = newReader.ReadUInt32();
            for (int i = 0; i < size; i++)
            {
                ClassDatabaseType cdt = new ClassDatabaseType();
                cdt.Read(newReader, header.fileVersion, header.flags);
                classes.Add(cdt);
            }
        }

        public void Write(AssetsFileWriter writer, int optimizeStringTable = 1, int compress = 1, bool writeStringTable = true)
        {
            long filePos = writer.BaseStream.Position;

            byte[] newStrTable = stringTable;

            //"optimize string table (slow)" mode 2 not supported
            //ex: >AABB\0>localAABB\0 can be just >local>AABB\0
            if (optimizeStringTable == 1)
            {
                StringBuilder strTableBuilder = new StringBuilder();
                Dictionary<string, uint> strTableMap = new Dictionary<string, uint>();
                for (int i = 0; i < classes.Count; i++)
                {
                    ClassDatabaseType type = classes[i];

                    AddStringTableEntry(strTableBuilder, strTableMap, type.Name);
                
                    if (header.fileVersion == 4 && (header.flags & 1) != 0)
                    {
                        AddStringTableEntry(strTableBuilder, strTableMap, type.AssemblyFileName);
                    }
                    
                    List<ClassDatabaseTypeField> fields = type.Fields;
                    for (int j = 0; j < fields.Count; j++)
                    {
                        ClassDatabaseTypeField field = fields[j];
                        AddStringTableEntry(strTableBuilder, strTableMap, field.FieldName);
                        AddStringTableEntry(strTableBuilder, strTableMap, field.TypeName);
                        fields[j] = field;
                    }
                }
            }

            header.Write(writer);
            writer.Write(classes.Count);
            for (int i = 0; i < classes.Count; i++)
            {
                classes[i].Write(writer, header.fileVersion, header.flags);
            }

            long stringTablePos = writer.Position;

            //set false only for tpk packing, don't set false anytime else!
            if (writeStringTable)
            {
                writer.Write(newStrTable);
            }

            long fileEndPos = writer.Position;

            long stringTableLen = writer.Position - stringTablePos;
            long fileSize = writer.Position;

            header.stringTablePos = (uint)stringTablePos;
            header.stringTableLen = (uint)stringTableLen;
            header.uncompressedSize = (uint)fileSize;

            writer.Position = filePos;
            header.Write(writer);

            writer.Position = fileEndPos;
        }

        private void AddStringTableEntry(StringBuilder strTable, Dictionary<string, uint> strMap, ClassDatabaseFileString str)
        {
            string stringValue = str.GetString(this);

            if (!strMap.ContainsKey(stringValue))
            {
                strMap[stringValue] = (uint)strTable.Length;
                strTable.Append(stringValue + '\0');
            }
            str.str.stringTableOffset = strMap[stringValue];
        }
        
        public ClassDatabaseFile() { }
    }*/
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
                type.Read(dReader, Header.FileVersion);
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
