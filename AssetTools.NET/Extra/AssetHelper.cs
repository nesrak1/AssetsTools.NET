////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;
using System.Collections.Generic;

namespace AssetsTools.NET.Extra
{
    public static class AssetHelper
    {
        public static uint FixAudioID(uint id)
        {
            if (id == 0xf1)      //AudioMixerController
                id = 0xf0;       //AudioMixer
            else if (id == 0xf3) //AudioMixerGroupController
                id = 0x111;      //AudioMixerGroup
            else if (id == 0xf5) //AudioMixerSnapshotController
                id = 0x110;      //AudioMixerSnapshot
            return id;
        }

        public static ClassDatabaseType FindAssetClassByID(ClassDatabaseFile cldb, uint id)
        {
            id = FixAudioID(id);
            foreach (ClassDatabaseType type in cldb.classes)
            {
                if ((uint)type.classId == id)
                    return type;
            }
            return null;
        }

        public static ClassDatabaseType FindAssetClassByName(ClassDatabaseFile cldb, string name)
        {
            foreach (ClassDatabaseType type in cldb.classes)
            {
                if (type.name.GetString(cldb) == name)
                    return type;
            }
            return null;
        }

        public static Type_0D FindTypeTreeTypeByID(TypeTree typeTree, uint id)
        {
            foreach (Type_0D type in typeTree.unity5Types)
            {
                if ((uint)type.classId == id)
                    return type;
            }
            return null;
        }

        public static Type_0D FindTypeTreeTypeByID(TypeTree typeTree, uint id, ushort scriptIndex)
        {
            foreach (Type_0D type in typeTree.unity5Types)
            {
                //5.5+
                if ((uint)type.classId == id && type.scriptIndex == scriptIndex)
                    return type;
                //5.4-
                if (type.classId < 0 && id == 0x72 && (type.scriptIndex - 0x10000 == type.classId))
                    return type;
            }
            return null;
        }

        public static Type_0D FindTypeTreeTypeByScriptIndex(TypeTree typeTree, ushort scriptIndex)
        {
            foreach (Type_0D type in typeTree.unity5Types)
            {
                if (type.scriptIndex == scriptIndex)
                    return type;
            }
            return null;
        }

        public static Type_0D FindTypeTreeTypeByName(TypeTree typeTree, string name)
        {
            foreach (Type_0D type in typeTree.unity5Types)
            {
                if (type.typeFieldsEx[0].GetTypeString(type.stringTable) == name)
                    return type;
            }
            return null;
        }

        public static ushort GetScriptIndex(AssetsFile file, AssetFileInfoEx info)
        {
            if (file.header.format < 0x10)
                return info.scriptIndex;
            else
                return file.typeTree.unity5Types[info.curFileTypeOrIndex].scriptIndex;
        }

        public static string GetAssetNameFast(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfoEx info)
        {
            ClassDatabaseType type = FindAssetClassByID(cldb, info.curFileType);
            AssetsFileReader reader = file.reader;

            if (file.typeTree.hasTypeTree)
            {
                ushort scriptId = GetScriptIndex(file, info);

                Type_0D ttType = FindTypeTreeTypeByID(file.typeTree, info.curFileType, scriptId);

                string ttTypeName = ttType.typeFieldsEx[0].GetTypeString(ttType.stringTable);
                if (ttType.typeFieldsEx.Length == 0) return type.name.GetString(cldb); //fallback to cldb
                if (ttType.typeFieldsEx.Length > 1 && ttType.typeFieldsEx[1].GetNameString(ttType.stringTable) == "m_Name")
                {
                    reader.Position = info.absoluteFilePos;
                    return reader.ReadCountStringInt32();
                }
                //todo, use the typetree since we have it already, there could be extra fields
                else if (ttTypeName == "GameObject")
                {
                    reader.Position = info.absoluteFilePos;
                    int size = reader.ReadInt32();
                    int componentSize = file.header.format > 0x10 ? 0x0c : 0x10;
                    reader.Position += size * componentSize;
                    reader.Position += 0x04;
                    return reader.ReadCountStringInt32();
                }
                else if (ttTypeName == "MonoBehaviour")
                {
                    reader.Position = info.absoluteFilePos;
                    reader.Position += 0x1c;
                    string name = reader.ReadCountStringInt32();
                    if (name != "")
                    {
                        return name;
                    }
                }
                return ttTypeName;
            }

            string typeName = type.name.GetString(cldb);
            if (type.fields.Count == 0) return type.name.GetString(cldb);
            if (type.fields.Count > 1 && type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "GameObject")
            {
                reader.Position = info.absoluteFilePos;
                int size = reader.ReadInt32();
                int componentSize = file.header.format > 0x10 ? 0x0c : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 0x04;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "MonoBehaviour")
            {
                reader.Position = info.absoluteFilePos;
                reader.Position += 0x1c;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return typeName;
        }

        //no classdatabase but may not work
        public static string GetAssetNameFastNaive(AssetsFile file, AssetFileInfoEx info)
        {
            AssetsFileReader reader = file.reader;

            if (AssetsFileExtra.HasName(info.curFileType))
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (info.curFileType == 0x01)
            {
                reader.Position = info.absoluteFilePos;
                int size = reader.ReadInt32();
                int componentSize = file.header.format > 0x10 ? 0xC : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 4;
                return reader.ReadCountStringInt32();
            }
            else if (info.curFileType == 0x72)
            {
                reader.Position = info.absoluteFilePos;
                reader.Position += 28;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return string.Empty;
        }

        public static AssetFileInfoEx GetAssetInfo(this AssetsFileTable table, string name, bool caseSensitive = true)
        {
            if (!caseSensitive)
                name = name.ToLower();
            for (int i = 0; i < table.assetFileInfo.Length; i++)
            {
                AssetFileInfoEx info = table.assetFileInfo[i];
                string infoName = GetAssetNameFastNaive(table.file, info);
                if (!caseSensitive)
                    infoName = infoName.ToLower();
                if (infoName == name)
                {
                    return info;
                }
            }
            return null;
        }

        public static AssetFileInfoEx GetAssetInfo(this AssetsFileTable table, string name, uint typeId, bool caseSensitive = true)
        {
            if (!caseSensitive)
                name = name.ToLower();
            for (int i = 0; i < table.assetFileInfo.Length; i++)
            {
                AssetFileInfoEx info = table.assetFileInfo[i];
                string infoName = GetAssetNameFastNaive(table.file, info);
                if (!caseSensitive)
                    infoName = infoName.ToLower();
                if (info.curFileType == typeId && infoName == name)
                {
                    return info;
                }
            }
            return null;
        }

        public static List<AssetFileInfoEx> GetAssetsOfType(this AssetsFileTable table, int typeId)
        {
            List<AssetFileInfoEx> infos = new List<AssetFileInfoEx>();
            foreach (AssetFileInfoEx info in table.assetFileInfo)
            {
                if (info.curFileType == typeId)
                {
                    infos.Add(info);
                }
            }
            return infos;
        }
    }
}
