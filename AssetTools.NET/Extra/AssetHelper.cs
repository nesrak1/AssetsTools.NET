////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;

namespace AssetsTools.NET.Extra
{
    public static class AssetHelper
    {
        public static ClassDatabaseType FindAssetClassByID(ClassDatabaseFile cldb, uint id)
        {
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

        public static string GetAssetNameFast(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfoEx info)
        {
            ulong pos = info.absoluteFilePos;
            ClassDatabaseType type = FindAssetClassByID(cldb, info.curFileType);

            AssetsFileReader reader = file.reader;

            if (type.fields.Count == 0) return type.name.GetString(cldb);
            if (type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = info.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "GameObject")
            {
                reader.Position = info.absoluteFilePos;
                int size = reader.ReadInt32();
                reader.Position += (ulong)(size * 12);
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
    }
}
