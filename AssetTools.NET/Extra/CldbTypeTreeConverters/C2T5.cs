using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class ClassDatabaseToTypeTree
    {
        private ClassDatabaseFile cldbFile;
        private uint stringTablePos;
        private Dictionary<string, uint> stringTableLookup;
        private Dictionary<string, uint> commonStringTableLookup;
        private List<TypeTreeNode> typeTreeNodes;

        public static TypeTreeType Convert(ClassDatabaseFile classes, string name, bool preferEditor = false)
        {
            ClassDatabaseType type = classes.FindAssetClassByName(name);
            return Convert(classes, type, preferEditor);
        }

        public static TypeTreeType Convert(ClassDatabaseFile classes, int id, bool preferEditor = false)
        {
            ClassDatabaseType type = classes.FindAssetClassByID(id);
            return Convert(classes, type, preferEditor);
        }

        public static TypeTreeType Convert(ClassDatabaseFile classes, ClassDatabaseType type, bool preferEditor = false)
        {
            ClassDatabaseToTypeTree converter = new ClassDatabaseToTypeTree();
            return converter.ConvertInternal(classes, type, preferEditor);
        }

        private TypeTreeType ConvertInternal(ClassDatabaseFile classes, ClassDatabaseType type, bool preferEditor = false)
        {
            TypeTreeType type0d = new TypeTreeType()
            {
                TypeId = type.ClassId,
                ScriptTypeIndex = 0xffff,
                IsStrippedType = false,
                ScriptIdHash = Hash128.NewBlankHash(),
                TypeHash = Hash128.NewBlankHash(),
                TypeDependencies = new int[0]
            };

            cldbFile = classes;

            stringTablePos = 0;
            stringTableLookup = new Dictionary<string, uint>();
            commonStringTableLookup = new Dictionary<string, uint>();
            typeTreeNodes = new List<TypeTreeNode>();
            InitializeDefaultStringTableIndices();

            ClassDatabaseTypeNode node = type.GetPreferredNode(preferEditor);
            ConvertFields(node, 0);

            StringBuilder stringTableBuilder = new StringBuilder();
            List<KeyValuePair<string, uint>> sortedStringTable = stringTableLookup.OrderBy(n => n.Value).ToList();
            foreach (KeyValuePair<string, uint> entry in sortedStringTable)
            {
                stringTableBuilder.Append(entry.Key + '\0');
            }

            type0d.StringBuffer = stringTableBuilder.ToString();
            type0d.Nodes = typeTreeNodes;
            return type0d;
        }

        private void InitializeDefaultStringTableIndices()
        {
            int commonStringTablePos = 0;
            List<ushort> commonStringIndices = cldbFile.CommonStringBufferIndices;
            foreach (ushort entry in commonStringIndices)
            {
                string strEntry = cldbFile.StringTable.GetString(entry);
                if (strEntry != string.Empty)
                {
                    commonStringTableLookup.Add(strEntry, (uint)commonStringTablePos);
                    commonStringTablePos += strEntry.Length + 1;
                }
            }
        }

        private void ConvertFields(ClassDatabaseTypeNode node, int depth)
        {
            string fieldName = cldbFile.GetString(node.FieldName);
            string typeName = cldbFile.GetString(node.TypeName);
            uint fieldNamePos;
            uint typeNamePos;

            if (stringTableLookup.ContainsKey(fieldName))
            {
                fieldNamePos = stringTableLookup[fieldName];
            }
            else if (commonStringTableLookup.ContainsKey(fieldName))
            {
                fieldNamePos = commonStringTableLookup[fieldName] + 0x80000000;
            }
            else
            {
                fieldNamePos = stringTablePos;
                stringTableLookup.Add(fieldName, stringTablePos);
                stringTablePos += (uint)fieldName.Length + 1;
            }

            if (stringTableLookup.ContainsKey(typeName))
            {
                typeNamePos = stringTableLookup[typeName];
            }
            else if (commonStringTableLookup.ContainsKey(typeName))
            {
                typeNamePos = commonStringTableLookup[typeName] + 0x80000000;
            }
            else
            {
                typeNamePos = stringTablePos;
                stringTableLookup.Add(typeName, stringTablePos);
                stringTablePos += (uint)typeName.Length + 1;
            }

            typeTreeNodes.Add(new TypeTreeNode()
            {
                Level = (byte)depth,
                MetaFlags = node.MetaFlag,
                Index = (uint)typeTreeNodes.Count,
                TypeFlags = (TypeTreeNodeFlags)node.TypeFlags,
                NameStrOffset = fieldNamePos,
                ByteSize = node.ByteSize,
                TypeStrOffset = typeNamePos,
                Version = node.Version
            });

            foreach (ClassDatabaseTypeNode child in node.Children)
            {
                ConvertFields(child, depth + 1);
            }
        }
    }
}
