using System;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsFile
    {
        public AssetsFileHeader header;
        public TypeTree typeTree;

        public PreloadList preloadTable;
        public AssetsFileDependencyList dependencies;

        public uint AssetTablePos;
        public uint AssetCount;

        public AssetsFileReader reader;
        public Stream readerPar;

        public AssetsFile(AssetsFileReader reader)
        {
            this.reader = reader;
            readerPar = reader.BaseStream;
            header = new AssetsFileHeader();
            header.Read(0, reader);
            typeTree = new TypeTree();
            typeTree.Read(reader.Position, reader, header.format, reader.bigEndian);
            AssetCount = reader.ReadUInt32();
            reader.Align();
            AssetTablePos = Convert.ToUInt32(reader.BaseStream.Position);
            reader.BaseStream.Position += AssetFileInfo.GetSize(header.format) * AssetCount;
            preloadTable = new PreloadList();
            preloadTable.Read(reader.Position, reader, header.format, reader.bigEndian);
            dependencies = new AssetsFileDependencyList();
            dependencies.Read(reader.Position, reader, header.format, reader.bigEndian);
        }

        //set fileID to -1 if all replacers are for this .assets file but don't have the fileID set to the same one
        //typeMeta is used to add the type information (hash and type fields) for format >= 0x10 if necessary
        public ulong Write(AssetsFileWriter writer, ulong filePos, /*AssetsReplacer[] pReplacers, ulong replacerCount, */uint fileID, ClassDatabaseFile typeMeta = null)
        {
            header.Write(writer.Position, writer);
            typeTree.Write(writer.Position, writer, header.format);
            writer.Write(AssetCount);
            writer.Align();
            uint size = AssetFileInfo.GetSize(header.format) * AssetCount;
            for (int i = 0; i < size; i++)
            {
                writer.Write((byte)0x00);
            }
            preloadTable.Write(writer.Position, writer, header.format);
            dependencies.Write(writer.Position, writer, header.format);
            ulong endMarker = writer.Position;
            header.metadataSize = (uint)writer.Position - 0x14;
            writer.Position = 0;
            header.Write(writer.Position, writer);
            writer.Position = endMarker;
            return writer.Position;
        }

        ///public bool GetAssetFile(ulong fileInfoOffset, AssetsFileReader reader, AssetFile pBuf, FileStream readerPar);
        ///public ulong GetAssetFileOffs(ulong fileInfoOffset, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByIndex(ulong fileIndex, AssetFile pBuf, uint pSize, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByIndex(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByName(string name, AssetFile pBuf, uint pSize, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffs(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetFileList(AssetsFileReader reader, FileStream readerPar);
        ///public bool VerifyAssetsFile(AssetsFileVerifyLogger logger = null);
    }
}
