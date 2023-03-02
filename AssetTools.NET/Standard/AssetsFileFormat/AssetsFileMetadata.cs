using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class AssetsFileMetadata
    {
        /// <summary>
        /// Engine version this file uses.
        /// </summary>
        public string UnityVersion { get; set; }
        /// <summary>
        /// Target platform this file uses.
        /// </summary>
        public uint TargetPlatform { get; set; }
        /// <summary>
        /// Marks whether the type info contains type tree data.
        /// </summary>
        public bool TypeTreeEnabled { get; set; }
        /// <summary>
        /// List of type tree types.
        /// </summary>
        public List<TypeTreeType> TypeTreeTypes { get; set; }
        /// <summary>
        /// List of asset infos. Do not modify this directly. Instead, use Write.
        /// </summary>
        public List<AssetFileInfo> AssetInfos { get; set; }
        /// <summary>
        /// List of script type pointers. This list should match up with ScriptTypeIndex in the type tree types list.
        /// </summary>
        public List<AssetPPtr> ScriptTypes { get; set; }
        /// <summary>
        /// List of externals (references to other files).
        /// </summary>
        public List<AssetsFileExternal> Externals { get; set; }
        /// <summary>
        /// List of reference types.
        /// </summary>
        public List<TypeTreeType> RefTypes { get; set; }
        /// <summary>
        /// Unknown.
        /// </summary>
        public string UserInformation { get; set; }

        private Dictionary<long, int> _quickLookup = null;

        public void Read(AssetsFileReader reader, AssetsFileHeader header)
        {
            Read(reader, header.Version, header.DataOffset);
        }

        public void Read(AssetsFileReader reader, uint version)
        {
            Read(reader, version, -1);
        }

        public void Read(AssetsFileReader reader, uint version, long dataOffset)
        {
            _quickLookup = null;

            UnityVersion = reader.ReadNullTerminated();
            TargetPlatform = reader.ReadUInt32();
            if (version >= 13)
            {
                TypeTreeEnabled = reader.ReadBoolean();
            }

            int fieldCount = reader.ReadInt32();
            TypeTreeTypes = new List<TypeTreeType>(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                TypeTreeType typeTreeType = new TypeTreeType();
                typeTreeType.Read(reader, version, TypeTreeEnabled, false);
                TypeTreeTypes.Add(typeTreeType);
            }

            int assetCount = reader.ReadInt32();
            reader.Align();
            AssetInfos = new List<AssetFileInfo>(assetCount);
            for (int i = 0; i < assetCount; i++)
            {
                AssetFileInfo fileInfo = new AssetFileInfo();
                fileInfo.Read(reader, version);

                // todo, check correct version
                fileInfo.TypeId = fileInfo.GetTypeId(this, version);
                if (dataOffset != -1)
                {
                    fileInfo.AbsoluteByteStart = fileInfo.GetAbsoluteByteStart(dataOffset);
                }

                AssetInfos.Add(fileInfo);
            }

            int scriptTypeCount = reader.ReadInt32();
            ScriptTypes = new List<AssetPPtr>(scriptTypeCount);
            for (int i = 0; i < scriptTypeCount; i++)
            {
                int fileId = reader.ReadInt32();
                reader.Align(); // only align after fileId
                long pathId = reader.ReadInt64();
                // no alignment needed here since pathId is a multiple of 4
                AssetPPtr pptr = new AssetPPtr(fileId, pathId);
                ScriptTypes.Add(pptr);
            }

            int externalCount = reader.ReadInt32();
            Externals = new List<AssetsFileExternal>(externalCount);
            for (int i = 0; i < externalCount; i++)
            {
                AssetsFileExternal external = new AssetsFileExternal();
                external.Read(reader);
                Externals.Add(external);
            }

            if (version >= 20)
            {
                int refTypeCount = reader.ReadInt32();
                RefTypes = new List<TypeTreeType>(refTypeCount);
                for (int i = 0; i < refTypeCount; i++)
                {
                    TypeTreeType typeTreeType = new TypeTreeType();
                    typeTreeType.Read(reader, version, TypeTreeEnabled, true);
                    RefTypes.Add(typeTreeType);
                }
            }

            if (version >= 5)
            {
                UserInformation = reader.ReadNullTerminated();
            }
        }

        public void Write(AssetsFileWriter writer, uint version)
        {
            writer.WriteNullTerminated(UnityVersion);
            writer.Write(TargetPlatform);
            if (version >= 13)
            {
                writer.Write(TypeTreeEnabled);
            }

            writer.Write(TypeTreeTypes.Count);
            for (int i = 0; i < TypeTreeTypes.Count; i++)
            {
                TypeTreeTypes[i].Write(writer, version, TypeTreeEnabled);
            }
            
            writer.Write(AssetInfos.Count);
            writer.Align();
            for (int i = 0; i < AssetInfos.Count; i++)
            {
                AssetInfos[i].Write(writer, version);
            }
            
            writer.Write(ScriptTypes.Count);
            for (int i = 0; i < ScriptTypes.Count; i++)
            {
                writer.Write(ScriptTypes[i].FileId);
                writer.Align(); // only align after fileId
                writer.Write(ScriptTypes[i].PathId);
            }
            
            writer.Write(Externals.Count);
            for (int i = 0; i < Externals.Count; i++)
            {
                Externals[i].Write(writer);
            }

            if (version >= 20)
            {
                writer.Write(RefTypes.Count);
                for (int j = 0; j < RefTypes.Count; j++)
                {
                    RefTypes[j].Write(writer, version, TypeTreeEnabled);
                }
            }

            if (version >= 5)
            {
                writer.WriteNullTerminated(UserInformation);
            }
        }

        public AssetFileInfo GetAssetInfo(long pathId)
        {
            if (_quickLookup != null)
            {
                if (_quickLookup.ContainsKey(pathId))
                {
                    return AssetInfos[_quickLookup[pathId]];
                }
            }
            else
            {
                for (int i = 0; i < AssetInfos.Count; i++)
                {
                    AssetFileInfo info = AssetInfos[i];
                    if (info.PathId == pathId)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        public void GenerateQuickLookupTree()
        {
            _quickLookup = new Dictionary<long, int>();
            for (int i = 0; i < AssetInfos.Count; i++)
            {
                AssetFileInfo info = AssetInfos[i];
                _quickLookup[info.PathId] = i;
            }
        }

        public List<AssetFileInfo> GetAssetsOfType(int typeId)
        {
            List<AssetFileInfo> infos = new List<AssetFileInfo>();
            foreach (AssetFileInfo info in AssetInfos)
            {
                if (info.TypeId == typeId)
                {
                    infos.Add(info);
                }
            }
            return infos;
        }

        public List<AssetFileInfo> GetAssetsOfType(int typeId, ushort scriptIndex)
        {
            List<AssetFileInfo> infos = new List<AssetFileInfo>();
            foreach (AssetFileInfo info in AssetInfos)
            {
                if (scriptIndex != 0xffff)
                {
                    if (info.TypeId < 0)
                    {
                        if (info.ScriptTypeIndex != scriptIndex)
                            continue;

                        if (typeId != (int)AssetClassID.MonoBehaviour && !(typeId < 0 && info.TypeId == typeId))
                            continue;
                    }
                    else if (info.TypeId == (int)AssetClassID.MonoBehaviour)
                    {
                        if (FindTypeTreeTypeByID(info.TypeId, info.ScriptTypeIndex).ScriptTypeIndex != scriptIndex)
                            continue;

                        if (typeId != (int)AssetClassID.MonoBehaviour)
                            continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (info.TypeId != typeId)
                        continue;
                }

                infos.Add(info);
            }
            return infos;
        }

        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId)
        {
            return GetAssetsOfType((int)typeId);
        }

        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId, ushort scriptIndex)
        {
            return GetAssetsOfType((int)typeId, scriptIndex);
        }

        public TypeTreeType FindTypeTreeTypeByID(int id)
        {
            foreach (TypeTreeType type in TypeTreeTypes)
            {
                if (type.TypeId == id)
                    return type;
            }
            return null;
        }

        public TypeTreeType FindTypeTreeTypeByID(int id, ushort scriptIndex)
        {
            // todo: use metadata for better version checking
            foreach (TypeTreeType type in TypeTreeTypes)
            {
                if (type.TypeId == id)
                {
                    // 5.5+ monobehaviours and 5.4- any other asset
                    if (type.ScriptTypeIndex == scriptIndex)
                        return type;
                    // 5.4- monobehaviours (script index cannot be trusted in this version, so ignore it)
                    if (id < 0 && type.ScriptTypeIndex == 0xffff)
                        return type;
                }
            }
            return null;
        }

        public TypeTreeType FindTypeTreeTypeByScriptIndex(ushort scriptIndex)
        {
            foreach (TypeTreeType type in TypeTreeTypes)
            {
                if (type.ScriptTypeIndex == scriptIndex)
                    return type;
            }
            return null;
        }

        public TypeTreeType FindTypeTreeTypeByName(string name)
        {
            foreach (TypeTreeType type in TypeTreeTypes)
            {
                if (type.Nodes[0].GetTypeString(type.StringBuffer) == name)
                    return type;
            }
            return null;
        }

        public TypeTreeType FindRefTypeByIndex(ushort scriptIndex)
        {
            foreach (TypeTreeType type in RefTypes)
            {
                if (type.ScriptTypeIndex == scriptIndex)
                    return type;
            }
            return null;
        }
    }
}
