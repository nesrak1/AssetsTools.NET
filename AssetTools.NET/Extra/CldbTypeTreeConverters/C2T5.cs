using System.Collections.Generic;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public static class ClassDatabaseToTypeTree
    {
        public static TypeTreeType Convert(ClassDatabaseFile classes, string name)
        {
            ClassDatabaseType type = AssetHelper.FindAssetClassByName(classes, name);
            return Convert(classes, type);
        }

        public static TypeTreeType Convert(ClassDatabaseFile classes, int id)
        {
            ClassDatabaseType type = AssetHelper.FindAssetClassByID(classes, id);
            return Convert(classes, type);
        }

        public static TypeTreeType Convert(ClassDatabaseFile classes, ClassDatabaseType type)
        {
            TypeTreeType type0d = new TypeTreeType()
            {
                TypeId = type.classId,
                ScriptTypeIndex = 0xFFFF,
                IsStrippedType = false,
                ScriptIdHash = new Hash128(),
                TypeHash = new Hash128()
            };
            string stringTable = "";
            Dictionary<string, uint> strTableList = new Dictionary<string, uint>();
            Dictionary<string, uint> defTableList = new Dictionary<string, uint>();

            uint strTablePos = 0;
            uint defTablePos = 0;

            string[] defaultTable = TypeTreeType.COMMON_STRING_TABLE.Substring(TypeTreeType.COMMON_STRING_TABLE.Length - 1).Split('\0');
            foreach (string entry in defaultTable)
            {
                if (entry != "")
                {
                    defTableList.Add(entry, defTablePos);
                    defTablePos += (uint)entry.Length + 1;
                }
            }

            List<TypeTreeNode> nodes = new List<TypeTreeNode>();
            for (int i = 0; i < type.fields.Count; i++)
            {
                ClassDatabaseTypeField field = type.fields[i];
                string fieldName = field.fieldName.GetString(classes);
                string typeName = field.typeName.GetString(classes);
                uint fieldNamePos = 0xFFFFFFFF;
                uint typeNamePos = 0xFFFFFFFF;

                if (strTableList.ContainsKey(fieldName))
                {
                    fieldNamePos = strTableList[fieldName];
                }
                else if (defTableList.ContainsKey(fieldName))
                {
                    fieldNamePos = defTableList[fieldName] + 0x80000000;
                }
                else
                {
                    fieldNamePos = strTablePos;
                    strTableList.Add(fieldName, strTablePos);
                    strTablePos += (uint)fieldName.Length + 1;
                }

                if (strTableList.ContainsKey(typeName))
                {
                    typeNamePos = strTableList[typeName];
                }
                else if (defTableList.ContainsKey(typeName))
                {
                    typeNamePos = defTableList[typeName] + 0x80000000;
                }
                else
                {
                    typeNamePos = strTablePos;
                    strTableList.Add(typeName, strTablePos);
                    strTablePos += (uint)typeName.Length + 1;
                }

                nodes.Add(new TypeTreeNode()
                {
                    Level = field.depth,
                    MetaFlags = field.flags2,
                    Index = (uint)i,
                    TypeFlags = field.isArray,
                    NameStrOffset = fieldNamePos,
                    ByteSize = field.size,
                    TypeStrOffset = typeNamePos,
                    Version = field.version
                });
            }

            List<KeyValuePair<string, uint>> sortedStrTableList = strTableList.OrderBy(n => n.Value).ToList();
            foreach (KeyValuePair<string, uint> entry in sortedStrTableList)
            {
                stringTable += entry.Key + '\0';
            }

            type0d.StringBuffer = stringTable;
            type0d.Nodes = nodes;
            return type0d;
        }
    }
}
