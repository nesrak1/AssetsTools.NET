using AssetsTools.NET.Extra;
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

        public uint assetTablePos;
        public uint assetCount;

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
            
            assetCount = reader.ReadUInt32();
            reader.Align();
            assetTablePos = (uint)reader.BaseStream.Position;

            int assetInfoSize = AssetFileInfo.GetSize(header.format);
            if (0x0F <= header.format && header.format <= 0x10)
            {
                //for these two versions, the asset info is not aligned
                //for the last entry, so we have to do some weird stuff
                reader.BaseStream.Position += ((assetInfoSize + 3) >> 2 << 2) * (assetCount - 1) + assetInfoSize;
            }
            else
            {
                reader.BaseStream.Position += AssetFileInfo.GetSize(header.format) * assetCount;
            }
            if (header.format > 0x0B)
            {
                preloadTable = new PreloadList();
                preloadTable.Read(reader);
            }

            dependencies = new AssetsFileDependencyList();
            dependencies.Read(reader);
        }
        
        public void Close()
        {
            readerPar.Dispose();
        }

        public void Write(AssetsFileWriter writer, long filePos, List<AssetsReplacer> replacers, uint fileID, ClassDatabaseFile typeMeta = null)
        {
            if (filePos == -1)
                filePos = writer.Position;
            else
                writer.Position = filePos;

            header.Write(writer);

            for (int i = 0; i < replacers.Count; i++)
            {
                AssetsReplacer replacer = replacers[i];
                int replacerClassId = replacer.GetClassID();
                if (!typeTree.unity5Types.Any(t => t.classId == replacerClassId))
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

                    if (typeMeta != null)
                    {
                        ClassDatabaseType cldbType = AssetHelper.FindAssetClassByID(typeMeta, (uint)replacerClassId);
                        if (cldbType != null)
                        {
                            type = C2T5.Cldb2TypeTree(typeMeta, cldbType);
                        }
                    }

                    typeTree.unity5Types.Add(type);
                }
            }
            typeTree.Write(writer, header.format);

            int initialSize = (int)(AssetFileInfo.GetSize(header.format) * assetCount);
            int newSize = (int)(AssetFileInfo.GetSize(header.format) * (assetCount + replacers.Count));
            int appendedSize = newSize - initialSize;
            reader.Position = assetTablePos;

            List<AssetFileInfo> assetInfos = new List<AssetFileInfo>();
            Dictionary<long, AssetFileInfo> originalAssetInfos = new Dictionary<long, AssetFileInfo>();
            Dictionary<long, AssetsReplacer> currentReplacers = replacers.ToDictionary(r => r.GetPathID());
            uint currentOffset = 0;

            //calculate sizes/offsets for original assets, modify sizes if needed and skip those to be removed
            for (int i = 0; i < assetCount; i++)
            {
                AssetFileInfo info = new AssetFileInfo();
                info.Read(header.format, reader);
                originalAssetInfos.Add(info.index, info);
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
                AssetsReplacer replacer;
                if (currentReplacers.TryGetValue(newInfo.index, out replacer))
                {
                    currentReplacers.Remove(newInfo.index);
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
                currentOffset = (currentOffset + 7) >> 3 << 3; //pad to 8 bytes

                assetInfos.Add(newInfo);
            }

            //calculate sizes/offsets for new assets
            foreach (var replacerPair in currentReplacers)
            {
                AssetsReplacer replacer = replacerPair.Value;
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
                    currentOffset = (currentOffset + 7) >> 3 << 3; //pad to 8 bytes

                    assetInfos.Add(info);
                }
            }

            currentReplacers.Clear();

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
            }

            uint metadataSize = (uint)(writer.Position - filePos - 0x13); //0x13 is header - "endianness byte"? (if that's what it even is)
            if (header.format >= 0x16)
            {
                //remove larger variation fields as well
                metadataSize -= 0x1c;
            }

            //for padding only. if all initial data before assetData is more than 0x1000, this is skipped
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

            long firstFileOffset = writer.Position;

            //write all asset data
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
                    AssetFileInfo originalInfo;
                    if (originalAssetInfos.TryGetValue(info.index, out originalInfo))
                    {
                        reader.Position = header.firstFileOffset + originalInfo.curFileOffset;
                        byte[] assetData = reader.ReadBytes((int)originalInfo.curFileSize);
                        writer.Write(assetData);
                        if (i != assetInfos.Count - 1)
                            writer.Align8();
                    }
                }
            }

            AssetsFileHeader newHeader = new AssetsFileHeader()
            {
                metadataSize = header.metadataSize,
                fileSize = header.fileSize,
                format = header.format,
                firstFileOffset = header.firstFileOffset,
                endianness = header.endianness,
                unknown = header.unknown,
                unknown1 = header.unknown1,
                unknown2 = header.unknown2
            };

            newHeader.firstFileOffset = firstFileOffset;

            long fileSizeMarker = writer.Position - filePos;

            reader.Position = newHeader.firstFileOffset;

            writer.Position = filePos;
            newHeader.metadataSize = metadataSize;
            newHeader.fileSize = fileSizeMarker;
            newHeader.Write(writer);

            writer.Position = fileSizeMarker + filePos;
        }
    }
}
