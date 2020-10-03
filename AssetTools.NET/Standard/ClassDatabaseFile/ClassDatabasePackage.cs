using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
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

        public bool Read(AssetsFileReader reader)
        {
            header = new ClassDatabasePackageHeader();
            header.Read(reader);
            files = new ClassDatabaseFile[header.fileCount];
            long firstFile = reader.Position;
            AssetsFileReader newReader = reader;
            if ((header.compressionType & 0x80) == 0) //multiple blocks
            {
                //untested!
                //the compression is handled by the cldbs themselves
                for (int i = 0; i < header.fileCount; i++)
                {
                    newReader.Position = firstFile + header.files[i].offset;
                    byte[] data = newReader.ReadBytes((int)header.files[i].length);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (AssetsFileReader r = new AssetsFileReader(ms))
                    {
                        files[i] = new ClassDatabaseFile();
                        files[i].Read(r);
                    }
                }
            }
            else
            {
                if ((header.compressionType & 0x20) == 0) //not uncompressed
                {
                    firstFile = 0;
                    int compressedSize = (int)(header.stringTableOffset - newReader.Position);
                    int uncompressedSize = (int)header.fileBlockSize;
                    MemoryStream ms;
                    if ((header.compressionType & 0x1f) == 1) //lz4
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        ms = new MemoryStream(uncompressedBytes);
                    }
                    else if ((header.compressionType & 0x1f) == 2) //lzma
                    {
                        byte[] dbg = newReader.ReadBytes(compressedSize);
                        using (MemoryStream tempMs = new MemoryStream(dbg))
                        {
                            ms = SevenZipHelper.StreamDecompress(tempMs, uncompressedSize);
                        }
                    }
                    else
                    {
                        valid = false;
                        return valid;
                    }

                    newReader = new AssetsFileReader(ms);
                    newReader.bigEndian = false;
                }
                for (int i = 0; i < header.fileCount; i++)
                {
                    newReader.Position = firstFile + header.files[i].offset;
                    byte[] data = newReader.ReadBytes((int)header.files[i].length);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (AssetsFileReader r = new AssetsFileReader(ms))
                    {
                        files[i] = new ClassDatabaseFile();
                        files[i].Read(r);
                    }
                }
            }

            newReader = reader;
            newReader.Position = header.stringTableOffset;
            if ((header.compressionType & 0x40) == 0) //string table is compressed
            {
                if ((header.compressionType & 0x20) == 0) //not uncompressed
                {
                    int compressedSize = (int)header.stringTableLenCompressed;
                    int uncompressedSize = (int)header.stringTableLenUncompressed;
                    MemoryStream ms;
                    if ((header.compressionType & 0x1f) == 1) //lz4
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        ms = new MemoryStream(uncompressedBytes);
                    }
                    else if ((header.compressionType & 0x1f) == 2) //lzma
                    {
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            ms = SevenZipHelper.StreamDecompress(tempMs, uncompressedSize);
                        }
                    }
                    else
                    {
                        valid = false;
                        return valid;
                    }

                    newReader = new AssetsFileReader(ms);
                    newReader.bigEndian = false;
                }
            }
            stringTable = newReader.ReadBytes((int)header.stringTableLenUncompressed);
            for (int i = 0; i < header.fileCount; i++)
            {
                files[i].stringTable = stringTable;
            }

            valid = true;
            return valid;
        }
    }
}
