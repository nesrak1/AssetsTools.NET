namespace AssetsTools.NET.Extra
{
    public static class AssetHelper
    {
        public static int FixAudioID(int id)
        {
            if (id == 0xf1)      //AudioMixerController
                id = 0xf0;       //AudioMixer
            else if (id == 0xf3) //AudioMixerGroupController
                id = 0x111;      //AudioMixerGroup
            else if (id == 0xf5) //AudioMixerSnapshotController
                id = 0x110;      //AudioMixerSnapshot
            return id;
        }

        public static ClassDatabaseType FindAssetClassByID(ClassDatabaseFile cldb, int id)
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

        public static TypeTreeType FindTypeTreeTypeByID(AssetsFileMetadata metadata, int id)
        {
            foreach (TypeTreeType type in metadata.TypeTreeTypes)
            {
                if (type.TypeId == id)
                    return type;
            }
            return null;
        }

        public static TypeTreeType FindTypeTreeTypeByID(AssetsFileMetadata metadata, int id, ushort scriptIndex)
        {
            foreach (TypeTreeType type in metadata.TypeTreeTypes)
            {
                //5.5+
                if (type.TypeId == id && type.ScriptTypeIndex == scriptIndex)
                    return type;
                //5.4-
                if (type.TypeId < 0 && id == 0x72 && (type.ScriptTypeIndex - 0x10000 == type.TypeId))
                    return type;
            }
            return null;
        }

        public static TypeTreeType FindTypeTreeTypeByScriptIndex(AssetsFileMetadata metadata, ushort scriptIndex)
        {
            foreach (TypeTreeType type in metadata.TypeTreeTypes)
            {
                if (type.ScriptTypeIndex == scriptIndex)
                    return type;
            }
            return null;
        }

        public static TypeTreeType FindTypeTreeTypeByName(AssetsFileMetadata metadata, string name)
        {
            foreach (TypeTreeType type in metadata.TypeTreeTypes)
            {
                if (type.Nodes[0].GetTypeString(type.StringBuffer) == name)
                    return type;
            }
            return null;
        }

        public static ushort GetScriptIndex(AssetsFile file, AssetFileInfo info)
        {
            if (file.Header.Version < 0x10)
                return info.ScriptTypeIndex;
            else
                return file.Metadata.TypeTreeTypes[info.TypeIdOrIndex].ScriptTypeIndex;
        }

        public static string GetAssetNameFast(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfo info)
        {
            ClassDatabaseType type = FindAssetClassByID(cldb, info.TypeId);
            AssetsFileReader reader = file.Reader;

            if (file.Metadata.TypeTreeNotStripped)
            {
                ushort scriptId = GetScriptIndex(file, info);

                TypeTreeType ttType = FindTypeTreeTypeByID(file.Metadata, info.TypeId, scriptId);

                string ttTypeName = ttType.Nodes[0].GetTypeString(ttType.StringBuffer);
                if (ttType.Nodes.Count == 0) return type.name.GetString(cldb); // fallback to cldb
                if (ttType.Nodes.Count > 1 && ttType.Nodes[1].GetNameString(ttType.StringBuffer) == "m_Name")
                {
                    reader.Position = info.AbsoluteByteStart;
                    return reader.ReadCountStringInt32();
                }
                // todo, use the typetree since we have it already, there could be extra fields
                else if (ttTypeName == "GameObject")
                {
                    reader.Position = info.AbsoluteByteStart;
                    int size = reader.ReadInt32();
                    int componentSize = file.Header.Version > 0x10 ? 0x0c : 0x10;
                    reader.Position += size * componentSize;
                    reader.Position += 0x04;
                    return reader.ReadCountStringInt32();
                }
                else if (ttTypeName == "MonoBehaviour")
                {
                    reader.Position = info.AbsoluteByteStart;
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
                reader.Position = info.AbsoluteByteStart;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "GameObject")
            {
                reader.Position = info.AbsoluteByteStart;
                int size = reader.ReadInt32();
                int componentSize = file.Header.Version > 0x10 ? 0x0c : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 0x04;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "MonoBehaviour")
            {
                reader.Position = info.AbsoluteByteStart;
                reader.Position += 0x1c;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return typeName;
        }
    }
}
