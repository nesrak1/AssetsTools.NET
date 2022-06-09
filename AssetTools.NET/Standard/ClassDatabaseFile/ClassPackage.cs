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
            if (Header.CompressionType == 1) //lz4
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
            else if (Header.CompressionType == 2) //lzma
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
                Magic = "cldb",
                FileVersion = 4,
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

        /*
        private void AddStringTableEntry(ClassDatabaseFile cldb, StringBuilder strTable, Dictionary<string, uint> strMap, ClassDatabaseFileString str)
        {
            string stringValue = str.GetString(cldb);

            if (strTable != null)
            {
                //search for string first and use that index if possible
                if (!strMap.ContainsKey(stringValue))
                {
                    strMap[stringValue] = (uint)strTable.Length;
                    strTable.Append(stringValue + '\0');
                }
                str.str.stringTableOffset = strMap[stringValue];
            }
            else
            {
                //always add string
                str.str.stringTableOffset = (uint)strTable.Length;
                strTable.Append(stringValue + '\0');
            }
        }

        public bool RemoveFile(int index)
        {
            if (Files.Count < index)
            {
                Files.RemoveAt(index);
                Header.Files.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool ImportFile(AssetsFileReader reader)
        {
            ClassDatabaseFile cldb = new ClassDatabaseFile();
            bool valid = cldb.Read(reader);
            if (valid)
            {
                Files.Add(cldb);
                Header.Files.Add(new ClassDatabaseFileRef()
                {
                    offset = 0,
                    length = 0,
                    name = ""
                });
                return true;
            }
            return false;
        }
        */
    }
}
