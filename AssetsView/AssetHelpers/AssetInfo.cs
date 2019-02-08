using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.AssetHelpers
{
    public class AssetInfo
    {
        public static string GetAssetNameFast(AssetFileInfoEx afi, ClassDatabaseFile cldb, ClassDatabaseType type, AssetsFileInstance inst)
        {
            AssetsFileReader reader = inst.file.reader;
            if (type.fields.Count == 0)
            {
                return type.name.GetString(cldb);
            }
            else if (type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = afi.absoluteFilePos;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "GameObject")
            {
                reader.Position = afi.absoluteFilePos;
                int size = reader.ReadInt32();
                reader.Position += (ulong)size * 12;
                reader.Position += 4UL;
                return reader.ReadCountStringInt32();
            }
            else if (type.name.GetString(cldb) == "MonoBehaviour")
            {
                reader.Position = afi.absoluteFilePos;
                reader.Position += 28UL;
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
