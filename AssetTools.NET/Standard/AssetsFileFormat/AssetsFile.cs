using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetsTools.NET
{
    public class AssetsFile
    {
        public AssetsFileHeader header;
        public TypeTree typeTree;

        public PreloadList preloadTable;
        public AssetsFileDependencyList dependencies;

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

        public void Write(AssetsFileWriter writer, long filePos, List<AssetsReplacer> replacers, uint fileID = 0, ClassDatabaseFile typeMeta = null)
        {
            if (filePos == -1)
                filePos = writer.Position;
            else
                writer.Position = filePos;

            header.Write(writer);

            foreach (AssetsReplacer replacer in replacers)
            {
                int replacerClassId = replacer.GetClassID();
                ushort replacerScriptIndex = replacer.GetMonoScriptID();
                if (!typeTree.unity5Types.Any(t => t.classId == replacerClassId && t.scriptIndex == replacerScriptIndex))
                {
                    Type_0D type = null;

                    if (typeMeta != null)
                    {
                        ClassDatabaseType cldbType = AssetHelper.FindAssetClassByID(typeMeta, (uint)replacerClassId);
                        if (cldbType != null)
                        {
                            type = C2T5.Cldb2TypeTree(typeMeta, cldbType);

                            //in original AssetsTools, if you tried to use a new monoId it would just try to use
                            //the highest existing scriptIndex that existed without making a new one (unless there
                            //were no monobehavours ofc) this isn't any better as we just assign a plain monobehaviour
                            //typetree to a type that probably has more fields. I don't really know of a better way to
                            //handle this at the moment as cldbs cannot differentiate monoids.
                            type.scriptIndex = replacerScriptIndex;
                        }
                    }

                    if (type == null)
                    {
                        type = new Type_0D
                               {
                                   classId = replacerClassId,
                                   unknown16_1 = 0,
                                   scriptIndex = replacerScriptIndex,
                                   typeHash1 = 0,
                                   typeHash2 = 0,
                                   typeHash3 = 0,
                                   typeHash4 = 0,
                                   typeFieldsExCount = 0,
                                   stringTableLen = 0,
                                   stringTable = ""
                               };
                    }

                    typeTree.unity5Types.Add(type);
                }
            }
            typeTree.Write(writer, header.format);

            Dictionary<long, AssetFileInfo> oldAssetInfosByPathId = new Dictionary<long, AssetFileInfo>();
            Dictionary<long, AssetsReplacer> replacersByPathId = replacers.ToDictionary(r => r.GetPathID());
            List<AssetFileInfo> newAssetInfos = new List<AssetFileInfo>();

            // Collect unchanged assets (that aren't getting removed)
            reader.Position = assetTablePos;
            for (int i = 0; i < assetCount; i++)
            {
                AssetFileInfo oldAssetInfo = new AssetFileInfo();
                oldAssetInfo.Read(header.format, reader);
                oldAssetInfosByPathId.Add(oldAssetInfo.index, oldAssetInfo);

                if (replacersByPathId.ContainsKey(oldAssetInfo.index))
                    continue;

                AssetFileInfo newAssetInfo = new AssetFileInfo
                                             {
                                                 index = oldAssetInfo.index,
                                                 curFileTypeOrIndex = oldAssetInfo.curFileTypeOrIndex,
                                                 inheritedUnityClass = oldAssetInfo.inheritedUnityClass,
                                                 scriptIndex = oldAssetInfo.scriptIndex,
                                                 unknown1 = oldAssetInfo.unknown1
                                             };
                newAssetInfos.Add(newAssetInfo);
            }

            // Collect modified and new assets
            foreach (AssetsReplacer replacer in replacers.Where(r => r.GetReplacementType() == AssetsReplacementType.AddOrModify))
            {
                AssetFileInfo newAssetInfo = new AssetFileInfo
                                             {
                                                 index = replacer.GetPathID(),
                                                 inheritedUnityClass = (ushort)replacer.GetClassID(), //for older unity versions
                                                 scriptIndex = replacer.GetMonoScriptID(),
                                                 unknown1 = 0
                                             };

                if (header.format < 0x10)
                {
                    newAssetInfo.curFileTypeOrIndex = replacer.GetClassID();
                }
                else
                {
                    if (replacer.GetMonoScriptID() == 0xFFFF)
                        newAssetInfo.curFileTypeOrIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID());
                    else
                        newAssetInfo.curFileTypeOrIndex = typeTree.unity5Types.FindIndex(t => t.classId == replacer.GetClassID() && t.scriptIndex == replacer.GetMonoScriptID());
                }

                newAssetInfos.Add(newAssetInfo);
            }

            newAssetInfos.Sort((i1, i2) => i1.index.CompareTo(i2.index));

            // Write asset infos (will write again later on to update the offsets and sizes)
            writer.Write(newAssetInfos.Count);
            writer.Align();
            long newAssetTablePos = writer.Position;
            foreach (AssetFileInfo newAssetInfo in newAssetInfos)
            {
                newAssetInfo.Write(header.format, writer);
            }

            preloadTable.Write(writer);
            dependencies.Write(writer);

            // Temporary fix for secondaryTypeCount and friends
            if (header.format >= 0x14)
            {
                writer.Write(0); //secondaryTypeCount
            }

            uint newMetadataSize = (uint)(writer.Position - filePos - 0x13); //0x13 is header - "endianness byte"? (if that's what it even is)
            if (header.format >= 0x16)
            {
                // Remove larger variation fields as well
                newMetadataSize -= 0x1c;
            }

            // For padding only. if all initial data before assetData is more than 0x1000, this is skipped
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

            long newFirstFileOffset = writer.Position;

            // Write all asset data
            for (int i = 0; i < newAssetInfos.Count; i++)
            {
                AssetFileInfo newAssetInfo = newAssetInfos[i];
                newAssetInfo.curFileOffset = writer.Position - newFirstFileOffset;

                if (replacersByPathId.TryGetValue(newAssetInfo.index, out AssetsReplacer replacer))
                {
                    replacer.Write(writer);
                }
                else
                {
                    AssetFileInfo oldAssetInfo = oldAssetInfosByPathId[newAssetInfo.index];
                    reader.Position = header.firstFileOffset + oldAssetInfo.curFileOffset;
                    reader.BaseStream.CopyToCompat(writer.BaseStream, oldAssetInfo.curFileSize);
                }

                newAssetInfo.curFileSize = (uint)(writer.Position - (newFirstFileOffset + newAssetInfo.curFileOffset));
                if (i != newAssetInfos.Count - 1)
                    writer.Align8();
            }

            long newFileSize = writer.Position - filePos;

            // Write new header
            AssetsFileHeader newHeader = new AssetsFileHeader
                                         {
                                             metadataSize = newMetadataSize,
                                             fileSize = newFileSize,
                                             format = header.format,
                                             firstFileOffset = newFirstFileOffset,
                                             endianness = header.endianness,
                                             unknown = header.unknown,
                                             unknown1 = header.unknown1,
                                             unknown2 = header.unknown2
                                         };

            writer.Position = filePos;
            newHeader.Write(writer);

            // Write new asset infos again (this time with offsets and sizes filled in)
            writer.Position = newAssetTablePos;
            foreach (AssetFileInfo newAssetInfo in newAssetInfos)
            {
                newAssetInfo.Write(header.format, writer);
            }

            // Set writer position back to end of file
            writer.Position = filePos + newFileSize;
        }

        public static bool IsAssetsFile(string filePath)
        {
            using AssetsFileReader reader = new AssetsFileReader(filePath);
            return IsAssetsFile(reader, 0, reader.BaseStream.Length);
        }

        public static bool IsAssetsFile(AssetsFileReader reader, long offset, long length)
        {
            //todo - not fully implemented
            if (length < 0x30)
                return false;

            reader.Position = offset;
            string possibleBundleHeader = reader.ReadStringLength(5);
            if (possibleBundleHeader == "Unity")
                return false;

            reader.Position = offset + 0x08;
            int possibleFormat = reader.ReadInt32();
            if (possibleFormat > 99)
                return false;

            reader.Position = offset + 0x14;

            if (possibleFormat >= 0x16)
            {
                reader.Position += 0x1c;
            }

            string possibleVersion = "";
            char curChar;
            while (reader.Position < reader.BaseStream.Length && (curChar = (char)reader.ReadByte()) != 0x00)
            {
                possibleVersion += curChar;
                if (possibleVersion.Length > 0xFF)
                {
                    return false;
                }
            }

            string emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.]", "");
            string fullVersion = Regex.Replace(possibleVersion, "[^a-zA-Z0-9\\.]", "");
            return emptyVersion == "" && fullVersion.Length > 0;
        }
    }
}
