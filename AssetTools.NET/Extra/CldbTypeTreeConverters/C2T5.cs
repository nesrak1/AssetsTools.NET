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
            List<TypeField_0D> field0ds = new List<TypeField_0D>();
            for (int i = 0; i < type.fields.Count; i++)
            {
                ClassDatabaseTypeField field = type.fields[i];
                string fieldName = field.fieldName.GetString(classes) + '\0';
                string typeName = field.typeName.GetString(classes) + '\0';
                uint fieldNamePos = 0xFFFFFFFF;
                uint typeNamePos = 0xFFFFFFFF;

                if (stringTable.Contains(fieldName))
                {
                    fieldNamePos = (uint)stringTable.IndexOf(fieldName);
                } else if (Type_0D.strTable.Contains(fieldName))
                {
                    fieldNamePos = (uint)Type_0D.strTable.IndexOf(fieldName) + 0x80000000;
                } else
                {
                    fieldNamePos = (uint)stringTable.Length;
                    stringTable += fieldName;
                }

                if (stringTable.Contains(typeName))
                {
                    typeNamePos = (uint)stringTable.IndexOf(typeName);
                }
                else if (Type_0D.strTable.Contains(typeName))
                {
                    typeNamePos = (uint)Type_0D.strTable.IndexOf(typeName) + 0x80000000;
                }
                else
                {
                    typeNamePos = (uint)stringTable.Length;
                    stringTable += typeName;
                }
                
                field0ds.Add(new TypeField_0D()
                {
                    depth = field.depth,
                    flags = field.flags2,
                    index = (uint)i,
                    isArray = field.isArray == 1 ? true : false,
                    nameStringOffset = fieldNamePos,
                    size = field.size,
                    typeStringOffset = typeNamePos,
                    version = 1
                });
            }

            type0d.pStringTable = stringTable;
            type0d.stringTableLen = (uint)stringTable.Length;
            type0d.pTypeFieldsEx = field0ds.ToArray();
            return type0d;
        }
    }
}
