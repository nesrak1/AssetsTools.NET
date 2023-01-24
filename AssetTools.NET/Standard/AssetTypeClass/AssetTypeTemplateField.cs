using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AssetsTools.NET
{
    public class AssetTypeTemplateField
    {
        /// <summary>
        /// Name of the field.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type name of the field.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Type of the field (as an enum).
        /// </summary>
        public AssetValueType ValueType { get; set; }
        /// <summary>
        /// Is the field an array?
        /// </summary>
        public bool IsArray { get; set; }
        /// <summary>
        /// Is the field aligned? This aligns four bytes after all children have been read/written.
        /// </summary>
        public bool IsAligned { get; set; }
        /// <summary>
        /// Does the field have value? (i.e. is the field a numeric / string / array type?)
        /// </summary>
        public bool HasValue { get; set; }
        /// <summary>
        /// Children of the field.
        /// </summary>
        public List<AssetTypeTemplateField> Children { get; set; }

        public void FromTypeTree(TypeTreeType typeTreeType)
        {
            int fieldIndex = 0;
            FromTypeTree(typeTreeType, ref fieldIndex);
        }

        private void FromTypeTree(TypeTreeType typeTreeType, ref int fieldIndex)
        {
            TypeTreeNode field = typeTreeType.Nodes[fieldIndex];
            Name = field.GetNameString(typeTreeType.StringBuffer);
            Type = field.GetTypeString(typeTreeType.StringBuffer);
            ValueType = AssetTypeValueField.GetValueTypeByTypeName(Type);
            IsArray = field.TypeFlags == 1;
            IsAligned = (field.MetaFlags & 0x4000) != 0;
            HasValue = ValueType != AssetValueType.None;

            Children = new List<AssetTypeTemplateField>();

            for (fieldIndex++; fieldIndex < typeTreeType.Nodes.Count; fieldIndex++)
            {
                TypeTreeNode typeTreeField = typeTreeType.Nodes[fieldIndex];
                if (typeTreeField.Level <= field.Level)
                {
                    fieldIndex--;
                    break;
                }

                AssetTypeTemplateField assetField = new AssetTypeTemplateField();
                assetField.FromTypeTree(typeTreeType, ref fieldIndex);
                Children.Add(assetField);
            }

            //There can be a case where string child is not an array but an int
            //(ExposedReferenceTable field in PlayableDirector class before 2018.4.25)
            if (ValueType == AssetValueType.String && !Children[0].IsArray && Children[0].ValueType != AssetValueType.None)
            {
                Type = Children[0].Type;
                ValueType = Children[0].ValueType;

                Children.Clear();
            }

            if (IsArray)
            {
                ValueType = Children[1].ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array;
            }


            Children.TrimExcess();
        }

        public void FromClassDatabase(ClassDatabaseFile cldbFile, ClassDatabaseType cldbType, bool preferEditor = false)
        {
            if (cldbType.EditorRootNode == null && cldbType.ReleaseRootNode == null)
                throw new Exception("No root nodes were found!");

            ClassDatabaseTypeNode node = cldbType.GetPreferredNode(preferEditor);

            FromClassDatabase(cldbFile.StringTable, node);
        }

        private void FromClassDatabase(ClassDatabaseStringTable strTable, ClassDatabaseTypeNode node)
        {
            Name = strTable.GetString(node.FieldName);
            Type = strTable.GetString(node.TypeName);

            // temporary hack for tpk
            if (Type == "SInt32")
                Type = "int";
            else if (Type == "UInt32")
                Type = "unsigned int";

            ValueType = AssetTypeValueField.GetValueTypeByTypeName(Type);
            IsArray = node.TypeFlags == 1;
            IsAligned = (node.MetaFlag & 0x4000) != 0;
            HasValue = ValueType != AssetValueType.None;

            Children = new List<AssetTypeTemplateField>(node.Children.Count);
            foreach (ClassDatabaseTypeNode childNode in node.Children)
            {
                AssetTypeTemplateField childField = new AssetTypeTemplateField();
                childField.FromClassDatabase(strTable, childNode);
                Children.Add(childField);
            }

            //There can be a case where string child is not an array but an int
            //(ExposedReferenceTable field in PlayableDirector class before 2018.4.25)
            if (ValueType == AssetValueType.String && !Children[0].IsArray && Children[0].ValueType != AssetValueType.None)
            {
                Type = Children[0].Type;
                ValueType = Children[0].ValueType;

                Children.Clear();
                Children.TrimExcess();
            }

            if (IsArray)
            {
                ValueType = Children[1].ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array;
            }
        }

        public AssetTypeValueField MakeValue(AssetsFileReader reader)
        {
            AssetTypeValueField valueField = new AssetTypeValueField
            {
                TemplateField = this
            };
            valueField = ReadType(reader, valueField);
            return valueField;
        }

        public AssetTypeValueField MakeValue(AssetsFileReader reader, long position)
        {
            reader.Position = position;
            return MakeValue(reader);
        }

        public AssetTypeValueField ReadType(AssetsFileReader reader, AssetTypeValueField valueField)
        {
            if (valueField.TemplateField.IsArray)
            {
                int arrayChildCount = valueField.TemplateField.Children.Count;
                if (arrayChildCount != 2)
                    throw new Exception($"Expected array to have two children, found {arrayChildCount} instead!");

                AssetValueType sizeType = valueField.TemplateField.Children[0].ValueType;

                if (sizeType != AssetValueType.Int32 && sizeType != AssetValueType.UInt32)
                    throw new Exception($"Expected int array size type, found {sizeType} instead!");

                if (valueField.TemplateField.ValueType == AssetValueType.ByteArray)
                {
                    valueField.Children = new List<AssetTypeValueField>(0);

                    int size = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(size);

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();

                    valueField.Value = new AssetTypeValue(AssetValueType.ByteArray, data);
                }
                else
                {
                    int size = reader.ReadInt32();
                    valueField.Children = new List<AssetTypeValueField>(size);

                    for (int i = 0; i < size; i++)
                    {
                        AssetTypeValueField childField = new AssetTypeValueField();
                        childField.TemplateField = valueField.TemplateField.Children[1];
                        valueField.Children.Add(ReadType(reader, childField));
                    }

                    valueField.Children.TrimExcess();

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();

                    AssetTypeArrayInfo arrayTypeInfo = new AssetTypeArrayInfo
                    {
                        size = size
                    };

                    valueField.Value = new AssetTypeValue(AssetValueType.Array, arrayTypeInfo);
                }
            }
            else
            {
                AssetValueType type = valueField.TemplateField.ValueType;
                if (type == AssetValueType.None)
                {
                    int childCount = valueField.TemplateField.Children.Count;
                    valueField.Children = new List<AssetTypeValueField>(childCount);
                    for (int i = 0; i < childCount; i++)
                    {
                        AssetTypeValueField childField = new AssetTypeValueField();
                        childField.TemplateField = valueField.TemplateField.Children[i];
                        valueField.Children.Add(ReadType(reader, childField));
                    }
                    valueField.Children.TrimExcess();
                    valueField.Value = null;

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();
                }
                else
                {
                    if (type == AssetValueType.String)
                    {
                        valueField.Children = new List<AssetTypeValueField>(0);
                        int length = reader.ReadInt32();
                        valueField.Value = new AssetTypeValue(reader.ReadBytes(length), true);
                        reader.Align();
                    }
                    else
                    {
                        int childCount = valueField.TemplateField.Children.Count;
                        if (childCount == 0)
                        {
                            valueField.Children = new List<AssetTypeValueField>(0);
                            switch (valueField.TemplateField.ValueType)
                            {
                                case AssetValueType.Int8:
                                    valueField.Value = new AssetTypeValue(reader.ReadSByte());
                                    break;
                                case AssetValueType.UInt8:
                                    valueField.Value = new AssetTypeValue(reader.ReadByte());
                                    break;
                                case AssetValueType.Bool:
                                    valueField.Value = new AssetTypeValue(reader.ReadBoolean());
                                    break;
                                case AssetValueType.Int16:
                                    valueField.Value = new AssetTypeValue(reader.ReadInt16());
                                    break;
                                case AssetValueType.UInt16:
                                    valueField.Value = new AssetTypeValue(reader.ReadUInt16());
                                    break;
                                case AssetValueType.Int32:
                                    valueField.Value = new AssetTypeValue(reader.ReadInt32());
                                    break;
                                case AssetValueType.UInt32:
                                    valueField.Value = new AssetTypeValue(reader.ReadUInt32());
                                    break;
                                case AssetValueType.Int64:
                                    valueField.Value = new AssetTypeValue(reader.ReadInt64());
                                    break;
                                case AssetValueType.UInt64:
                                    valueField.Value = new AssetTypeValue(reader.ReadUInt64());
                                    break;
                                case AssetValueType.Float:
                                    valueField.Value = new AssetTypeValue(reader.ReadSingle());
                                    break;
                                case AssetValueType.Double:
                                    valueField.Value = new AssetTypeValue(reader.ReadDouble());
                                    break;
                            }

                            if (valueField.TemplateField.IsAligned)
                                reader.Align();
                        }
                        else if (valueField.TemplateField.ValueType != AssetValueType.None)
                        {
                            throw new Exception("Cannot read value of field with children!");
                        }
                    }
                }

            }
            return valueField;
        }
    }
}
