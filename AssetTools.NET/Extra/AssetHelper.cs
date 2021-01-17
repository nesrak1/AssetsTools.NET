﻿////////////////////////////
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
                if (type.classId == id)
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
                if (type.classId == id)
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

            if (type.fields.Count == 0) return type.name.GetString(cldb);
            if (type.fields.Count > 1 && type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "GameObject")
            {
                reader.Position = info.absoluteFilePos;
                int size = reader.ReadInt32();
                int componentSize = file.header.format > 0x10 ? 0xC : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 4;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "MonoBehaviour")
            {
                reader.Position = info.absoluteFilePos;
                reader.Position += 28;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return type.name.GetString(cldb);
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
        
        public static byte[] CreateBlankAssets(string engineVersion)
        {
            return CreateBlankAssets(engineVersion, new List<Type_0D>());
        }

        public static byte[] CreateBlankAssets(string engineVersion, List<Type_0D> types)
        {
            using MemoryStream ms = new MemoryStream();
            using AssetsFileWriter writer = new AssetsFileWriter(ms);
            AssetsFileHeader header = new AssetsFileHeader()
            {
                metadataSize = 0,
                fileSize = 0x1000,
                format = 0x11,
                firstFileOffset = 0x1000,
                endianness = 0,
                unknown = new byte[] { 0, 0, 0 }
            };

            TypeTree typeTree = new TypeTree()
            {
                unityVersion = engineVersion,
                version = 0x5,
                hasTypeTree = true,
                fieldCount = types.Count(),
                unity5Types = types
            };

            header.Write(writer);
            writer.bigEndian = false;
            typeTree.Write(writer, 0x11);
            writer.Write((uint)0);
            writer.Align();
            //preload table and dependencies
            writer.Write((uint)0);
            writer.Write((uint)0);

            //due to a write bug in at.net we have to pad to 0x1000
            while (ms.Position < 0x1000)
            {
                writer.Write((byte)0);
            }
            return ms.ToArray();
        }
    }
}
