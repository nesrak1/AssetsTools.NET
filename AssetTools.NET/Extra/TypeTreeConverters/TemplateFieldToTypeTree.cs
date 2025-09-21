using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class TemplateFieldToTypeTree
    {
        private uint stringTablePos;
        private Dictionary<string, uint> stringTableLookup;
        private Dictionary<string, uint> commonStringTableLookup;
        private List<TypeTreeNode> typeTreeNodes;

        public static TypeTreeType Convert(AssetTypeTemplateField baseField, int typeId, ushort scriptIndex = 0xffff, string[] commonStrings = null)
        {
            TemplateFieldToTypeTree converter = new TemplateFieldToTypeTree();
            return converter.ConvertInternal(baseField, typeId, scriptIndex, commonStrings);
        }

        public TypeTreeType ConvertInternal(AssetTypeTemplateField baseField, int typeId, ushort scriptIndex = 0xffff, string[] commonStrings = null)
        {
            TypeTreeType typeTreeType = new TypeTreeType()
            {
                TypeId = typeId,
                ScriptTypeIndex = scriptIndex,
                IsStrippedType = false,
                ScriptIdHash = Hash128.NewBlankHash(),
                TypeHash = Hash128.NewBlankHash(),
                TypeDependencies = new int[0]
            };

            stringTablePos = 0;
            stringTableLookup = new Dictionary<string, uint>();
            commonStringTableLookup = new Dictionary<string, uint>();
            typeTreeNodes = new List<TypeTreeNode>();
            InitializeDefaultStringTableIndices(commonStrings);

            ConvertFields(baseField, 0);

            StringBuilder stringTableBuilder = new StringBuilder();
            List<KeyValuePair<string, uint>> sortedStringTable = stringTableLookup.OrderBy(n => n.Value).ToList();
            foreach (KeyValuePair<string, uint> entry in sortedStringTable)
            {
                stringTableBuilder.Append(entry.Key + '\0');
            }

            typeTreeType.StringBuffer = stringTableBuilder.ToString();
            typeTreeType.Nodes = typeTreeNodes;
            typeTreeType.TypeHash = ComputeHash(typeTreeType);
            return typeTreeType;
        }

        private void InitializeDefaultStringTableIndices(string[] commonStrings)
        {
            int commonStringTablePos = 0;
            if (commonStrings == null || commonStrings.Length == 0)
            {
                return;
            }

            foreach (string strEntry in commonStrings)
            {
                if (strEntry != string.Empty)
                {
                    commonStringTableLookup.Add(strEntry, (uint)commonStringTablePos);
                    commonStringTablePos += strEntry.Length + 1;
                }
            }
        }

        private void ConvertFields(AssetTypeTemplateField templateField, int depth)
        {
            string fieldName = templateField.Name;
            string typeName = templateField.Type;
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
                MetaFlags = GetMetaFlags(templateField),
                Index = (uint)typeTreeNodes.Count,
                TypeFlags = GetTypeFlags(templateField),
                NameStrOffset = fieldNamePos,
                ByteSize = GetByteSize(templateField),
                TypeStrOffset = typeNamePos,
                Version = templateField.Version
            });

            foreach (AssetTypeTemplateField child in templateField.Children)
            {
                ConvertFields(child, depth + 1);
            }
        }

        private uint GetMetaFlags(AssetTypeTemplateField templateField)
        {
            // todo: all flags
            return templateField.IsAligned ? 0x4000u : 0u;
        }

        private TypeTreeNodeFlags GetTypeFlags(AssetTypeTemplateField templateField)
        {
            // todo: all flags
            var flags = TypeTreeNodeFlags.None;
            if (templateField.IsArray)
            {
                flags |= TypeTreeNodeFlags.Array;
            }
            return flags;
        }

        private int GetByteSize(AssetTypeTemplateField templateField)
        {
            // todo: need to handle arrays with elements of constant size
            return templateField.ValueType switch
            {
                AssetValueType.Bool => 1,
                AssetValueType.Int8 => 1,
                AssetValueType.UInt8 => 1,
                AssetValueType.Int16 => 2,
                AssetValueType.UInt16 => 2,
                AssetValueType.Int32 => 4,
                AssetValueType.UInt32 => 4,
                AssetValueType.Int64 => 8,
                AssetValueType.UInt64 => 8,
                AssetValueType.Float => 4,
                AssetValueType.Double => 8,
                _ => -1
            };
        }

        private static Hash128 ComputeHash(TypeTreeType typeTree)
        {
            var md4 = new MD4();
            Update(typeTree, md4);
            return new Hash128(md4.Digest());
        }

        private static void Update(TypeTreeType typeTree, MD4 md4)
        {
            var nodes = typeTree.Nodes.GetEnumerator();
            while (nodes.MoveNext())
            {
                var node = nodes.Current;
                md4.Update(Encoding.UTF8.GetBytes(node.GetTypeString(typeTree.StringBufferBytes)));
                md4.Update(Encoding.UTF8.GetBytes(node.GetNameString(typeTree.StringBufferBytes)));
                md4.Update(BitConverter.GetBytes(node.ByteSize));
                md4.Update(BitConverter.GetBytes(System.Convert.ToInt32(node.TypeFlags)));
                md4.Update(BitConverter.GetBytes(System.Convert.ToInt32(node.Version)));
                md4.Update(BitConverter.GetBytes(System.Convert.ToInt32(node.MetaFlags & 0x4000)));
            }
        }
    }
}
