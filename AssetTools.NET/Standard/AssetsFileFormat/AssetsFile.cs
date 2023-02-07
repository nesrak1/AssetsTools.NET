using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetsTools.NET
{
    public class AssetsFile
    {
        /// <summary>
        /// Assets file header.
        /// </summary>
        public AssetsFileHeader Header { get; set; }
        /// <summary>
        /// Contains metadata about the file (TypeTree, engine version, dependencies, etc.)
        /// </summary>
        public AssetsFileMetadata Metadata { get; set; }

        public AssetsFileReader Reader { get; set; }

        public void Close()
        {
            Reader.Close();
        }

        public void Read(AssetsFileReader reader)
        {
            Reader = reader;
            
            Header = new AssetsFileHeader();
            Header.Read(reader);

            Metadata = new AssetsFileMetadata();
            Metadata.Read(reader, Header);
        }

        public void Read(Stream stream)
        {
            Read(new AssetsFileReader(stream));
        }

        public void Write(AssetsFileWriter writer, long filePos, List<AssetsReplacer> replacers, ClassDatabaseFile typeMeta = null)
        {
            long writeStart = filePos;
            if (filePos == -1)
                writeStart = writer.Position;
            else
                writer.Position = filePos;

            // We'll write the header now even if we replace it
            // later so we start the metadata in the right spot
            Header.Write(writer);

            List<TypeTreeType> typeTreeTypes = Metadata.TypeTreeTypes;

            foreach (AssetsReplacer replacer in replacers.Where(r => r.GetReplacementType() == AssetsReplacementType.AddOrModify))
            {
                int replacerClassId = replacer.GetClassID();
                ushort replacerScriptIndex = replacer.GetMonoScriptID();

                bool typeInTree;
                if (Header.Version >= 16)
                    typeInTree = typeTreeTypes.Any(t => t.TypeId == replacerClassId && t.ScriptTypeIndex == replacerScriptIndex);
                else
                    typeInTree = typeTreeTypes.Any(t => t.TypeId == replacerClassId); // script index is always 0xffff in type tree

                if (!typeInTree)
                {
                    TypeTreeType type = null;

                    if (typeMeta != null)
                    {
                        ClassDatabaseType cldbType = typeMeta.FindAssetClassByID(replacerClassId);
                        if (cldbType != null)
                        {
                            type = ClassDatabaseToTypeTree.Convert(typeMeta, cldbType);

                            // In original AssetsTools, if you tried to use a new monoId it would just try to use
                            // the highest existing scriptIndex that existed without making a new one (unless there
                            // were no MonoBehavours ofc) this isn't any better as we just assign a plain MonoBehaviour
                            // type tree to a type that probably has more fields. I don't really know of a better
                            // way to handle this at the moment as cldbs cannot differentiate monoIds.
                            type.ScriptTypeIndex = replacerScriptIndex;
                        }
                    }

                    if (type == null)
                    {
                        type = new TypeTreeType
                        {
                            TypeId = replacerClassId,
                            IsStrippedType = false,
                            ScriptTypeIndex = replacerScriptIndex,
                            ScriptIdHash = Hash128.NewBlankHash(),
                            TypeHash = Hash128.NewBlankHash(),
                            Nodes = new List<TypeTreeNode>(),
                            StringBufferBytes = new byte[0],
                            TypeDependencies = new int[0]
                        };
                    }

                    typeTreeTypes.Add(type);
                }
            }

            Dictionary<long, AssetFileInfo> oldAssetInfosByPathId = new Dictionary<long, AssetFileInfo>();
            Dictionary<long, AssetsReplacer> replacersByPathId = replacers.ToDictionary(r => r.GetPathID());
            List<AssetFileInfo> newAssetInfos = new List<AssetFileInfo>();

            // Collect unchanged assets (that aren't getting removed)
            foreach (AssetFileInfo oldAssetInfo in Metadata.AssetInfos)
            {
                oldAssetInfosByPathId.Add(oldAssetInfo.PathId, oldAssetInfo);

                if (replacersByPathId.ContainsKey(oldAssetInfo.PathId))
                    continue;

                AssetFileInfo newAssetInfo = new AssetFileInfo
                {
                    PathId = oldAssetInfo.PathId,
                    TypeIdOrIndex = oldAssetInfo.TypeIdOrIndex,
                    ClassId = oldAssetInfo.ClassId,
                    ScriptTypeIndex = oldAssetInfo.ScriptTypeIndex,
                    Stripped = oldAssetInfo.Stripped
                };

                newAssetInfos.Add(newAssetInfo);
            }

            // Collect modified and new assets
            foreach (AssetsReplacer replacer in replacers.Where(r => r.GetReplacementType() == AssetsReplacementType.AddOrModify))
            {
                AssetFileInfo newAssetInfo = new AssetFileInfo
                {
                    PathId = replacer.GetPathID(),
                    ClassId = (ushort)replacer.GetClassID(), // For older unity versions
                    ScriptTypeIndex = replacer.GetMonoScriptID(),
                    Stripped = 0
                };

                if (Header.Version < 16)
                {
                    // v < 16, class id for monobehaviour is 0x72 and
                    // type id or index is the negative number
                    if (replacer.GetClassID() < 0)
                        newAssetInfo.ClassId = 0x72;

                    newAssetInfo.TypeIdOrIndex = replacer.GetClassID();
                }
                else
                {
                    if (replacer.GetMonoScriptID() == 0xffff)
                        newAssetInfo.TypeIdOrIndex = typeTreeTypes.FindIndex(t => t.TypeId == replacer.GetClassID());
                    else
                        newAssetInfo.TypeIdOrIndex = typeTreeTypes.FindIndex(t => t.TypeId == replacer.GetClassID() && t.ScriptTypeIndex == replacer.GetMonoScriptID());
                }

                newAssetInfos.Add(newAssetInfo);
            }
            
            // Required by Unity (I guess?)
            newAssetInfos.Sort((i1, i2) => i1.PathId.CompareTo(i2.PathId));

            AssetsFileMetadata newMetadata = new AssetsFileMetadata
            {
                UnityVersion = Metadata.UnityVersion,
                TargetPlatform = Metadata.TargetPlatform,
                TypeTreeEnabled = Metadata.TypeTreeEnabled,
                TypeTreeTypes = typeTreeTypes,
                AssetInfos = newAssetInfos,
                ScriptTypes = Metadata.ScriptTypes, // todo
                Externals = Metadata.Externals, // todo
                RefTypes = Metadata.RefTypes,
                UserInformation = Metadata.UserInformation
            };

            long newMetadataStart = writer.Position;
            newMetadata.Write(writer, Header.Version);
            int newMetadataSize = (int)(writer.Position - newMetadataStart);

            if (writer.Position < 0x1000)
            {
                // For padding only: if we're already past address 0x1000, this is skipped
                while (writer.Position < 0x1000)
                {
                    writer.Write((byte)0x00);
                }
            }
            else
            {
                // Otherwise align to 16 bytes, even if already aligned
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
                newAssetInfo.ByteStart = writer.Position - newFirstFileOffset;

                if (replacersByPathId.TryGetValue(newAssetInfo.PathId, out AssetsReplacer replacer))
                {
                    replacer.Write(writer);
                }
                else
                {
                    AssetFileInfo oldAssetInfo = oldAssetInfosByPathId[newAssetInfo.PathId];
                    Reader.Position = Header.DataOffset + oldAssetInfo.ByteStart;
                    Reader.BaseStream.CopyToCompat(writer.BaseStream, oldAssetInfo.ByteSize);
                }

                newAssetInfo.ByteSize = (uint)(writer.Position - (newFirstFileOffset + newAssetInfo.ByteStart));
                if (i != newAssetInfos.Count - 1)
                    writer.Align8();
            }

            long newFileSize = writer.Position - writeStart;

            // Write new header
            AssetsFileHeader newHeader = new AssetsFileHeader
            {
                MetadataSize = newMetadataSize,
                FileSize = newFileSize,
                Version = Header.Version,
                DataOffset = newFirstFileOffset,
                Endianness = Header.Endianness
            };

            writer.Position = writeStart;
            newHeader.Write(writer);
            
            // Write new asset infos again (this time with offsets and sizes filled in)
            writer.Position = newMetadataStart;
            newMetadata.Write(writer, Header.Version);
            
            // Set writer position back to end of file
            writer.Position = writeStart + newFileSize;
        }

        public ushort GetScriptIndex(AssetFileInfo info)
        {
            if (Header.Version < 0x10)
                return info.ScriptTypeIndex;
            else
                return Metadata.TypeTreeTypes[info.TypeIdOrIndex].ScriptTypeIndex;
        }

        public static bool IsAssetsFile(string filePath)
        {
            using AssetsFileReader reader = new AssetsFileReader(filePath);
            return IsAssetsFile(reader, 0, reader.BaseStream.Length);
        }

        public static bool IsAssetsFile(AssetsFileReader reader, long offset, long length)
        {
            reader.BigEndian = true;

            // todo: not fully implemented
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

            string emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.\\n]", "");
            string fullVersion = Regex.Replace(possibleVersion, "[^a-zA-Z0-9\\.\\n]", "");
            return emptyVersion == "" && fullVersion.Length > 0;
        }

        // for convenience
        public AssetFileInfo GetAssetInfo(long pathId) => Metadata.GetAssetInfo(pathId);
        public void GenerateQuickLookupTree() => Metadata.GenerateQuickLookupTree();
        public List<AssetFileInfo> GetAssetsOfType(int typeId) => Metadata.GetAssetsOfType(typeId);
        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId) => Metadata.GetAssetsOfType(typeId);
        public List<AssetFileInfo> GetAssetsOfType(int typeId, ushort scriptIndex) => Metadata.GetAssetsOfType(typeId, scriptIndex);
        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId, ushort scriptIndex) => Metadata.GetAssetsOfType(typeId, scriptIndex);

        public List<AssetFileInfo> AssetInfos => Metadata.AssetInfos;
    }
}
