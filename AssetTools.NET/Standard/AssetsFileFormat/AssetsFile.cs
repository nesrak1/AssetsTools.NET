using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            if (header.format > 0x0B)
            {
                preloadTable = new PreloadList();
                preloadTable.Read(reader.Position, reader, header.format, reader.bigEndian);
            }
            
            dependencies = new AssetsFileDependencyList();
            dependencies.Read(reader.Position, reader, header.format, reader.bigEndian);
        }

        //set fileID to -1 if all replacers are for this .assets file but don't have the fileID set to the same one
        //typeMeta is used to add the type information (hash and type fields) for format >= 0x10 if necessary
        public ulong Write(AssetsFileWriter writer, ulong filePos, AssetsReplacer[] pReplacers, uint fileID, ClassDatabaseFile typeMeta = null)
        {
            header.Write(writer.Position, writer);

            for (int i = 0; i < pReplacers.Length; i++)
            {
                AssetsReplacer replacer = pReplacers[i];
                if (!typeTree.pTypes_Unity5.Any(t => t.classId == replacer.GetClassID()))
                {
                    Type_0D type = new Type_0D()
                    {
                        classId = replacer.GetClassID(),
                        unknown16_1 = 0,
                        scriptIndex = 0xFFFF,
                        unknown5 = 0,
                        unknown6 = 0,
                        unknown7 = 0,
                        unknown8 = 0,
                        typeFieldsExCount = 0,
                        stringTableLen = 0,
                        pStringTable = ""
                    };
                    typeTree.pTypes_Unity5.Concat(new Type_0D[] { type });
                }
            }
            typeTree.Write(writer.Position, writer, header.format);

            int initialSize = (int)(AssetFileInfo.GetSize(header.format) * AssetCount);
            int newSize = (int)(AssetFileInfo.GetSize(header.format) * (AssetCount + pReplacers.Length));
            int appendedSize = newSize - initialSize;
            reader.Position = AssetTablePos;

            List<AssetFileInfo> originalAssetInfos = new List<AssetFileInfo>();
            List<AssetFileInfo> assetInfos = new List<AssetFileInfo>();
            List<AssetsReplacer> currentReplacers = pReplacers.ToList();
            uint currentOffset = 0;

            //-write all original assets, modify sizes if needed and skip those to be removed
            for (int i = 0; i < AssetCount; i++)
            {
                AssetFileInfo info = new AssetFileInfo();
                info.Read(header.format, reader.Position, reader, reader.bigEndian);
                originalAssetInfos.Add(info);
                AssetsReplacer replacer = currentReplacers.FirstOrDefault(n => n.GetPathID() == info.index);
                if (replacer != null)
                {
                    currentReplacers.Remove(replacer);
                    if (replacer.GetReplacementType() == AssetsReplacementType.AssetsReplacement_AddOrModify)
                    {
                        int classIndex = Array.FindIndex(typeTree.pTypes_Unity5, t => t.classId == replacer.GetClassID());
                        info = new AssetFileInfo()
                        {
                            index = replacer.GetPathID(),
                            offs_curFile = currentOffset,
                            curFileSize = (uint)replacer.GetSize(),
                            curFileTypeOrIndex = (uint)classIndex,
                            inheritedUnityClass = (ushort)replacer.GetClassID(), //-what is this
                            scriptIndex = replacer.GetMonoScriptID(),
                            unknown1 = 0
                        };
                    }
                    else if (replacer.GetReplacementType() == AssetsReplacementType.AssetsReplacement_Remove)
                    {
                        continue;
                    }
                }
                currentOffset += info.curFileSize;
                uint pad = 8 - (currentOffset % 8);
                if (pad != 8) currentOffset += pad;

                assetInfos.Add(info);
            }

            //-write new assets
            while (currentReplacers.Count > 0)
            {
                AssetsReplacer replacer = currentReplacers.First();
                if (replacer.GetReplacementType() == AssetsReplacementType.AssetsReplacement_AddOrModify)
                {
                    int classIndex = Array.FindIndex(typeTree.pTypes_Unity5, t => t.classId == replacer.GetClassID());
                    AssetFileInfo info = new AssetFileInfo()
                    {
                        index = replacer.GetPathID(),
                        offs_curFile = currentOffset,
                        curFileSize = (uint)replacer.GetSize(),
                        curFileTypeOrIndex = (uint)classIndex,
                        inheritedUnityClass = (ushort)replacer.GetClassID(),
                        scriptIndex = replacer.GetMonoScriptID(),
                        unknown1 = 0
                    };
                    currentOffset += info.curFileSize;
                    uint pad = 8 - (currentOffset % 8);
                    if (pad != 8) currentOffset += pad;

                    assetInfos.Add(info);
                }
                currentReplacers.Remove(replacer);
            }

            writer.Write(assetInfos.Count);
            writer.Align();
            for (int i = 0; i < assetInfos.Count; i++)
            {
                assetInfos[i].Write(header.format, writer.Position, writer);
            }

            preloadTable.Write(writer.Position, writer, header.format);

            dependencies.Write(writer.Position, writer, header.format);

            uint metadataSize = (uint)writer.Position - 0x13;

            //-for padding only. if all initial data before assetData is more than 0x1000, this is skipped
            while (writer.Position < 0x1000/*header.offs_firstFile*/)
            {
                writer.Write((byte)0x00);
            }

            writer.Align16();

            uint offs_firstFile = (uint)writer.Position;

            for (int i = 0; i < assetInfos.Count; i++)
            {
                AssetFileInfo info = assetInfos[i];
                AssetsReplacer replacer = pReplacers.FirstOrDefault(n => n.GetPathID() == info.index);
                if (replacer != null)
                {
                    if (replacer.GetReplacementType() == AssetsReplacementType.AssetsReplacement_AddOrModify)
                    {
                        replacer.Write(writer.Position, writer);
                        writer.Align8();
                    }
                    else if (replacer.GetReplacementType() == AssetsReplacementType.AssetsReplacement_Remove)
                    {
                        continue;
                    }
                }
                else
                {
                    AssetFileInfo originalInfo = originalAssetInfos.FirstOrDefault(n => n.index == info.index);
                    if (originalInfo != null)
                    {
                        reader.Position = header.offs_firstFile + originalInfo.offs_curFile;
                        byte[] assetData = reader.ReadBytes((int)originalInfo.curFileSize);
                        writer.Write(assetData);
                        writer.Align8();
                    }
                }
            }

            header.offs_firstFile = offs_firstFile;

            ulong fileSizeMarker = writer.Position;

            reader.Position = header.offs_firstFile;

            writer.Position = 0;
            header.metadataSize = metadataSize;
            header.fileSize = (uint)fileSizeMarker;
            header.Write(writer.Position, writer);
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
