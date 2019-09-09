using System.Collections.Generic;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public static class C2T5
    {
        public static Type_0D Cldb2TypeTree(ClassDatabaseFile classes, string name)
        {
            int id = classes.classes.Where(c => c.name.GetString(classes) == name).First().classId;
            return Cldb2TypeTree(classes, id);
        }

        public static Type_0D Cldb2TypeTree(ClassDatabaseFile classes, int id)
        {
            ClassDatabaseType type = classes.classes.Where(c => c.classId == id).First();
            Type_0D type0d = new Type_0D()
            {
                classId = type.classId,
                typeFieldsExCount = (uint)type.fields.Count,
                scriptIndex = 0xFFFF,
                unknown16_1 = 0,
                unknown1 = 0,
                unknown2 = 0,
                unknown3 = 0,
                unknown4 = 0,
                unknown5 = 0,
                unknown6 = 0,
                unknown7 = 0,
                unknown8 = 0
            };
            string stringTable = "";
            Dictionary<string, uint> strTableList = new Dictionary<string, uint>();
            Dictionary<string, uint> defTableList = new Dictionary<string, uint>();

            uint strTablePos = 0;
            uint defTablePos = 0;

            string[] defaultTable = Type_0D.strTable.Split('\0');
            foreach (string entry in defaultTable)
            {
                if (entry != "")
                {
                    defTableList.Add(entry, defTablePos);
                    defTablePos += (uint)entry.Length + 1;
                }
            }

            List<TypeField_0D> field0ds = new List<TypeField_0D>();
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

                field0ds.Add(new TypeField_0D()
                {
                    depth = field.depth,
                    flags = field.flags2,
                    index = (uint)i,
                    isArray = field.isArray,
                    nameStringOffset = fieldNamePos,
                    size = field.size,
                    typeStringOffset = typeNamePos,
                    version = field.version
                });
            }

            List<KeyValuePair<string, uint>> sortedStrTableList = strTableList.OrderBy(n => n.Value).ToList();
            foreach (KeyValuePair<string, uint> entry in sortedStrTableList)
            {
                stringTable += entry.Key + '\0';
            }

            type0d.pStringTable = stringTable;
            type0d.stringTableLen = (uint)stringTable.Length;
            type0d.pTypeFieldsEx = field0ds.ToArray();
            return type0d;
        }
    }
}
