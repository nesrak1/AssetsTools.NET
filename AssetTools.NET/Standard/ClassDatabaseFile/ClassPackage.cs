using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using LZ4ps;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageFile
    {
        public ClassPackageHeader Header { get; set; }
        public ClassPackageTypeTree TpkTypeTree { get; set; }

        public void Read(AssetsFileReader reader)
        {
            Header = new ClassPackageHeader();
            Header.Read(reader);

            AssetsFileReader newReader;
            if (Header.CompressionType == ClassFileCompressionType.Lz4)
            {
                byte[] uncompressedBytes = new byte[Header.DecompressedSize];
                using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes((int)Header.CompressedSize)))
                {
                    Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                    decoder.Read(uncompressedBytes, 0, (int)Header.DecompressedSize);
                    decoder.Dispose();
                }
                MemoryStream ms = new MemoryStream(uncompressedBytes);
                newReader = new AssetsFileReader(ms);
                newReader.Position = 0;
            }
            else if (Header.CompressionType == ClassFileCompressionType.Lzma)
            {
                using (MemoryStream tempMs = new MemoryStream(reader.ReadBytes((int)Header.CompressedSize)))
                {
                    MemoryStream ms = SevenZipHelper.StreamDecompress(tempMs, (int)Header.DecompressedSize);
                    newReader = new AssetsFileReader(ms);
                    newReader.Position = 0;
                }
            }
            else
            {
                newReader = reader;
            }

            TpkTypeTree = new ClassPackageTypeTree();
            TpkTypeTree.Read(newReader);
        }

        public void Read(string path) => Read(new AssetsFileReader(File.OpenRead(path)));

        public void Write(AssetsFileWriter writer, ClassFileCompressionType compressionType)
        {
            Header.CompressionType = compressionType;

            MemoryStream dStream;
            AssetsFileWriter dWriter;

            dStream = new MemoryStream();
            dWriter = new AssetsFileWriter(dStream);

            TpkTypeTree.Write(dWriter);

            if (Header.CompressionType == ClassFileCompressionType.Lz4)
            {
                byte[] data = LZ4Codec.Encode32HC(dStream.ToArray(), 0, (int)dStream.Length);

                Header.CompressedSize = (uint)data.Length;
                Header.DecompressedSize = (uint)dStream.Length;

                Header.Write(writer);
                writer.Write(data);
            }
            else if (Header.CompressionType == ClassFileCompressionType.Lzma)
            {
                MemoryStream cStream = new MemoryStream();
                SevenZipHelper.Compress(dStream, cStream);

                Header.CompressedSize = (uint)cStream.Length;
                Header.DecompressedSize = (uint)dStream.Length;

                Header.Write(writer);
                cStream.CopyToCompat(writer.BaseStream);
            }
            else
            {
                Header.CompressedSize = (uint)dStream.Length;
                Header.DecompressedSize = (uint)dStream.Length;

                Header.Write(writer);
                dStream.CopyToCompat(writer.BaseStream);
            }
        }

        public void Write(string path, ClassFileCompressionType compressionType)
            => Write(new AssetsFileWriter(File.OpenWrite(path)), compressionType);

        public ClassDatabaseFile GetClassDatabase(string version)
        {
            return GetClassDatabase(new UnityVersion(version));
        }

        public ClassDatabaseFile GetClassDatabase(UnityVersion version)
        {
            if (Header == null)
                throw new Exception("Header not loaded! (Did you forget to call package.Read?)");

            ClassDatabaseFile cldb = new ClassDatabaseFile();
            cldb.Header = new ClassDatabaseFileHeader()
            {
                Magic = "CLDB",
                FileVersion = 1,
                Version = version,
                CompressionType = 0,
                CompressedSize = 0,
                DecompressedSize = 0
            };

            cldb.Classes = new List<ClassDatabaseType>();
            foreach (ClassPackageClassInfo info in TpkTypeTree.ClassInformation)
            {
                ClassPackageType tpkType = info.GetTypeForVersion(version);
                if (tpkType == null)
                {
                    continue;
                }

                ClassDatabaseType cldbType = new ClassDatabaseType()
                {
                    ClassId = info.ClassId,
                    Name = tpkType.Name,
                    BaseName = tpkType.BaseName,
                    Flags = tpkType.Flags
                };

                if (tpkType.EditorRootNode != ushort.MaxValue)
                {
                    cldbType.EditorRootNode = ConvertNodes(tpkType.EditorRootNode);
                }
                if (tpkType.ReleaseRootNode != ushort.MaxValue)
                {
                    cldbType.ReleaseRootNode = ConvertNodes(tpkType.ReleaseRootNode);
                }

                cldb.Classes.Add(cldbType);
            }

            cldb.StringTable = TpkTypeTree.StringTable;

            byte commonStringCount = TpkTypeTree.CommonString.GetCommonStringLengthForVersion(version);
            cldb.CommonStringBufferIndices = new List<ushort>(commonStringCount);
            for (int i = 0; i < commonStringCount; i++)
            {
                cldb.CommonStringBufferIndices.Add(TpkTypeTree.CommonString.StringBufferIndices[i]);
            }

            return cldb;
        }
        
        private ClassDatabaseTypeNode ConvertNodes(ushort tpkNodeIdx)
        {
            ClassPackageTypeNode tpkNode = TpkTypeTree.Nodes[tpkNodeIdx];
            ClassDatabaseTypeNode cldbNode = new ClassDatabaseTypeNode()
            {
                TypeName = tpkNode.TypeName,
                FieldName = tpkNode.FieldName,
                ByteSize = tpkNode.ByteSize,
                Version = tpkNode.Version,
                TypeFlags = tpkNode.TypeFlags,
                MetaFlag = tpkNode.MetaFlag
            };

            int childrenCount = tpkNode.SubNodes.Length;
            cldbNode.Children = new List<ClassDatabaseTypeNode>(childrenCount);
            for (int i = 0; i < childrenCount; i++)
            {
                cldbNode.Children.Add(ConvertNodes(tpkNode.SubNodes[i]));
            }

            return cldbNode;
        }
    }
}
