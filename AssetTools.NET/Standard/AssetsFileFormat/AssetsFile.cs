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
        public string unknownString;

        public uint AssetTablePos;
        public uint AssetCount;

        public AssetsFileReader reader;
        public Stream readerPar;

        public AssetsFile(AssetsFileReader reader)
        {
            this.reader = reader;
            readerPar = reader.BaseStream;
            
            header = new AssetsFileHeader();
            header.Read(reader);

            typeTree = new TypeTree();
            typeTree.Read(reader, header.format);
            
            AssetCount = reader.ReadUInt32();
            reader.Align();
            AssetTablePos = (uint)reader.BaseStream.Position;

            int assetInfoSize = AssetFileInfo.GetSize(header.format);
            if (0x0F <= header.format && header.format <= 0x10)
            {
                //for these two versions, the asset info is not aligned
                //for the last entry, so we have to do some weird stuff
                reader.BaseStream.Position += ((assetInfoSize + 3) >> 2 << 2) * (AssetCount - 1) + assetInfoSize;
            }
            else
            {
                reader.BaseStream.Position += AssetFileInfo.GetSize(header.format) * AssetCount;
            }
            if (header.format > 0x0B)
            {
                preloadTable = new PreloadList();
                preloadTable.Read(reader);
            }

            dependencies = new AssetsFileDependencyList();
            dependencies.Read(reader);
        }

        public void Write(AssetsFileWriter writer, ulong filePos, List<AssetsReplacer> replacers, uint fileID, ClassDatabaseFile typeMeta = null)
        {
            header.Write(writer);

            for (int i = 0; i < replacers.Count; i++)
            {
                AssetsReplacer replacer = replacers[i];
                if (!typeTree.unity5Types.Any(t => t.classId == replacer.GetClassID()))
                {
                    Type_0D type = new Type_0D()
                    {
                        classId = replacer.GetClassID(),
                        unknown16_1 = 0,
                        scriptIndex = 0xFFFF,
                        typeHash1 = 0,
                        typeHash2 = 0,
                        typeHash3 = 0,
                        typeHash4 = 0,
                        typeFieldsExCount = 0,
                        stringTableLen = 0,
                        stringTable = ""
                    };
                    typeTree.unity5Types.Add(type);
                }
            }
            typeTree.Write(writer, header.format);

            int initialSize = (int)(AssetFileInfo.GetSize(header.format) * AssetCount);
            int newSize = (int)(AssetFileInfo.GetSize(header.format) * (AssetCount + replacers.Count));
            int appendedSize = newSize - initialSize;
            reader.Position = AssetTablePos;

            List<AssetFileInfo> originalAssetInfos = new List<AssetFileInfo>();
            List<AssetFileInfo> assetInfos = new List<AssetFileInfo>();
            List<AssetsReplacer> currentReplacers = replacers.ToList();
            uint currentOffset = 0;

            //-write all original assets, modify sizes if needed and skip those to be removed
            for (int i = 0; i < AssetCount; i++)
            {
                AssetFileInfo info = new AssetFileInfo();
                info.Read(header.format, reader);
                originalAssetInfos.Add(info);
                AssetFileInfo newInfo = new AssetFileInfo()
                {
                    index = info.index,
                    curFileOffset = currentOffset,
                    curFileSize = info.curFileSize,
                    curFileTypeOrIndex = info.curFileTypeOrIndex,
                    inheritedUnityClass = info.inheritedUnityClass,
                    scriptIndex = info.scriptIndex,
                    unknown1 = info.unknown1
                };
                AssetsReplacer replacer = currentReplacers.FirstOrDefault(n => n.GetPathID() == newInfo.index);
                if (replacer != null)
                {
                    currentReplacers.Remove(replacer);
                    if (replacer.GetReplacementType() == AssetsReplacementType.AddOrModify)
                    {
                        int classIndex;
                        if (replacer.GetMonoScriptID() == 0xFFFF)
                            classIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID());
                        else
                            classIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID() && t.scriptIndex == replacer.GetMonoScriptID());
                        newInfo = new AssetFileInfo()
                        {
                            index = replacer.GetPathID(),
                            curFileOffset = currentOffset,
                            curFileSize = (uint)replacer.GetSize(),
                            curFileTypeOrIndex = classIndex,
                            inheritedUnityClass = (ushort)replacer.GetClassID(), //for older unity versions
                            scriptIndex = replacer.GetMonoScriptID(),
                            unknown1 = 0
                        };
                    }
                    else if (replacer.GetReplacementType() == AssetsReplacementType.Remove)
                    {
                        continue;
                    }
                }
                currentOffset += newInfo.curFileSize;
                uint pad = 8 - (currentOffset % 8);
                if (pad != 8) currentOffset += pad;

                assetInfos.Add(newInfo);
            }

            //-write new assets
            while (currentReplacers.Count > 0)
            {
                AssetsReplacer replacer = currentReplacers[0];
                if (replacer.GetReplacementType() == AssetsReplacementType.AddOrModify)
                {
                    int classIndex;
                    if (replacer.GetMonoScriptID() == 0xFFFF)
                        classIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID());
                    else
                        classIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID() && t.scriptIndex == replacer.GetMonoScriptID());
                    AssetFileInfo info = new AssetFileInfo()
                    {
                        index = replacer.GetPathID(),
                        curFileOffset = currentOffset,
                        curFileSize = (uint)replacer.GetSize(),
                        curFileTypeOrIndex = classIndex,
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
                assetInfos[i].Write(header.format, writer);
            }

            preloadTable.Write(writer);

            dependencies.Write(writer);

            //temporary fix for secondarytypecount and friends
            if (header.format >= 0x14)
            {
                writer.Write(0); //secondaryTypeCount
                //writer.Write((byte)0); //unknownString length
            }

            uint metadataSize = (uint)(writer.Position - 0x13); //0x13 is header - "endianness byte"? (if that's what it even is)

            //-for padding only. if all initial data before assetData is more than 0x1000, this is skipped
            if (writer.Position < 0x1000)
            {
                while (writer.Position < 0x1000)
                {
                    writer.Write((byte)0x00);
                }
            }
            else
            {
                if (writer.Position % 16 == 0)
                    writer.Position += 16;
                else
                    writer.Align16();
            }

            long offs_firstFile = writer.Position;

            for (int i = 0; i < assetInfos.Count; i++)
            {
                AssetFileInfo info = assetInfos[i];
                AssetsReplacer replacer = replacers.FirstOrDefault(n => n.GetPathID() == info.index);
                if (replacer != null)
                {
                    if (replacer.GetReplacementType() == AssetsReplacementType.AddOrModify)
                    {
                        replacer.Write(writer);
                        if (i != assetInfos.Count - 1)
                            writer.Align8();
                    }
                    else if (replacer.GetReplacementType() == AssetsReplacementType.Remove)
                    {
                        continue;
                    }
                }
                else
                {
                    AssetFileInfo originalInfo = originalAssetInfos.FirstOrDefault(n => n.index == info.index);
                    if (originalInfo != null)
                    {
                        reader.Position = header.firstFileOffset + originalInfo.curFileOffset;
                        byte[] assetData = reader.ReadBytes((int)originalInfo.curFileSize);
                        writer.Write(assetData);
                        if (i != assetInfos.Count - 1)
                            writer.Align8();
                    }
                }
            }

            header.firstFileOffset = offs_firstFile;

            long fileSizeMarker = writer.Position;

            reader.Position = header.firstFileOffset;

            writer.Position = 0;
            header.metadataSize = metadataSize;
            header.fileSize = fileSizeMarker;
            header.Write(writer);
        }

        ///public bool GetAssetFile(ulong fileInfoOffset, AssetsFileReader reader, AssetFile buf, FileStream readerPar);
        ///public ulong GetAssetFileOffs(ulong fileInfoOffset, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByIndex(ulong fileIndex, AssetFile buf, uint size, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByIndex(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByName(string name, AssetFile buf, uint size, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffs(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetFileList(AssetsFileReader reader, FileStream readerPar);
        ///public bool VerifyAssetsFile(AssetsFileVerifyLogger logger = null);
    }
}
