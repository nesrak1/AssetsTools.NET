using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AssetsTools.NET
{
    public class AssetTypeTemplateField
    {
        public string name;
        public string type;
        public EnumValueTypes valueType;
        public bool isArray;
        public bool align;
        public bool hasValue;
        public int childrenCount;
        public AssetTypeTemplateField[] children;

        public bool From0D(Type_0D u5Type, int fieldIndex)
        {
            TypeField_0D field = u5Type.typeFieldsEx[fieldIndex];
            name = field.GetNameString(u5Type.stringTable);
            type = field.GetTypeString(u5Type.stringTable);
            valueType = AssetTypeValueField.GetValueTypeByTypeName(type);
            isArray = field.isArray == 1 ? true : false;
            align = (field.flags & 0x4000) != 0x00 ? true : false;
            hasValue = (valueType == EnumValueTypes.None) ? false : true;

            List<int> childrenIndexes = new List<int>();
            int thisDepth = u5Type.typeFieldsEx[fieldIndex].depth;
            for (int i = fieldIndex + 1; i < u5Type.typeFieldsExCount; i++)
            {
                if (u5Type.typeFieldsEx[i].depth == thisDepth + 1)
                {
                    childrenCount++;
                    childrenIndexes.Add(i);
                }
                if (u5Type.typeFieldsEx[i].depth <= thisDepth) break;
            }
            children = new AssetTypeTemplateField[childrenCount];
            int child = 0;
            for (int i = fieldIndex + 1; i < u5Type.typeFieldsExCount; i++)
            {
                if (u5Type.typeFieldsEx[i].depth == thisDepth + 1)
                {
                    children[child] = new AssetTypeTemplateField();
                    children[child].From0D(u5Type, childrenIndexes[child]);
                    child++;
                }
                if (u5Type.typeFieldsEx[i].depth <= thisDepth) break;
            }
            return true;
        }
        public bool FromClassDatabase(ClassDatabaseFile file, ClassDatabaseType type, uint fieldIndex)
        {
            ClassDatabaseTypeField field = type.fields[(int)fieldIndex];
            name = field.fieldName.GetString(file);
            this.type = field.typeName.GetString(file);
            valueType = AssetTypeValueField.GetValueTypeByTypeName(this.type);
            isArray = field.isArray == 1 ? true : false;
            align = (field.flags2 & 0x4000) != 0x00 ? true : false;
            hasValue = (valueType == EnumValueTypes.None) ? false : true;

            List<int> childrenIndexes = new List<int>();
            int thisDepth = type.fields[(int)fieldIndex].depth;
            for (int i = (int)fieldIndex + 1; i < type.fields.Count; i++)
            {
                if (type.fields[i].depth == thisDepth + 1)
                {
                    childrenCount++;
                    childrenIndexes.Add(i);
                }
                if (type.fields[i].depth <= thisDepth) break;
            }
            children = new AssetTypeTemplateField[childrenCount];
            int child = 0;
            for (int i = (int)fieldIndex + 1; i < type.fields.Count; i++)
            {
                if (type.fields[i].depth == thisDepth + 1)
                {
                    children[child] = new AssetTypeTemplateField();
                    children[child].FromClassDatabase(file, type, (uint)childrenIndexes[child]);
                    child++;
                }
                if (type.fields[i].depth <= thisDepth) break;
            }
            return true;
        }
        public AssetTypeValueField MakeValue(AssetsFileReader reader)
        {
            AssetTypeValueField valueField = new AssetTypeValueField();
            valueField.templateField = this;
            valueField = ReadType(reader, valueField);
            return valueField;
        }

        public AssetTypeValueField ReadType(AssetsFileReader reader, AssetTypeValueField valueField)
        {
            if (valueField.templateField.isArray)
            {
                if (valueField.templateField.childrenCount == 2)
                {
                    EnumValueTypes sizeType = valueField.templateField.children[0].valueType;
                    if (sizeType == EnumValueTypes.Int32 ||
                        sizeType == EnumValueTypes.UInt32)
                    {
                        if (valueField.templateField.valueType == EnumValueTypes.ByteArray)
                        {
                            valueField.childrenCount = 0;
                            valueField.children = new AssetTypeValueField[0];
                            int size = reader.ReadInt32();
                            byte[] data = reader.ReadBytes(size);
                            if (valueField.templateField.align) reader.Align();
                            AssetTypeByteArray atba = new AssetTypeByteArray();
                            atba.size = (uint)size;
                            atba.data = data;
                            valueField.value = new AssetTypeValue(EnumValueTypes.ByteArray, atba);
                        }
                        else
                        {
                            valueField.childrenCount = reader.ReadInt32();
                            valueField.children = new AssetTypeValueField[valueField.childrenCount];
                            for (int i = 0; i < valueField.childrenCount; i++)
                            {
                                valueField.children[i] = new AssetTypeValueField();
                                valueField.children[i].templateField = valueField.templateField.children[1];
                                valueField.children[i] = ReadType(reader, valueField.children[i]);
                            }
                            if (valueField.templateField.align) reader.Align();
                            AssetTypeArray ata = new AssetTypeArray();
                            ata.size = valueField.childrenCount;
                            valueField.value = new AssetTypeValue(EnumValueTypes.Array, ata);
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid array value type! Found an unexpected " + sizeType.ToString() + " type instead!");
                    }
                }
                else
                {
                    throw new Exception("Invalid array!");
                }
            }
            else
            {
                EnumValueTypes type = valueField.templateField.valueType;
                if (type != 0) valueField.value = new AssetTypeValue(type, null);
                if (type == EnumValueTypes.String)
                {
                    int length = reader.ReadInt32();
                    valueField.value.Set(reader.ReadBytes(length));
                    reader.Align();
                }
                else
                {
                    valueField.childrenCount = valueField.templateField.childrenCount;
                    if (valueField.childrenCount == 0)
                    {
                        valueField.children = new AssetTypeValueField[0];
                        switch (valueField.templateField.valueType)
                        {
                            case EnumValueTypes.Int8:
                                valueField.value.Set(reader.ReadSByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.UInt8:
                            case EnumValueTypes.Bool:
                                valueField.value.Set(reader.ReadByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.Int16:
                                valueField.value.Set(reader.ReadInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.UInt16:
                                valueField.value.Set(reader.ReadUInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.Int32:
                                valueField.value.Set(reader.ReadInt32());
                                break;
                            case EnumValueTypes.UInt32:
                                valueField.value.Set(reader.ReadUInt32());
                                break;
                            case EnumValueTypes.Int64:
                                valueField.value.Set(reader.ReadInt64());
                                break;
                            case EnumValueTypes.UInt64:
                                valueField.value.Set(reader.ReadUInt64());
                                break;
                            case EnumValueTypes.Float:
                                valueField.value.Set(reader.ReadSingle());
                                break;
                            case EnumValueTypes.Double:
                                valueField.value.Set(reader.ReadDouble());
                                break;
                        }
                    }
                    else
                    {
                        valueField.children = new AssetTypeValueField[valueField.childrenCount];
                        for (int i = 0; i < valueField.childrenCount; i++)
                        {
                            valueField.children[i] = new AssetTypeValueField();
                            valueField.children[i].templateField = valueField.templateField.children[i];
                            valueField.children[i] = ReadType(reader, valueField.children[i]);
                        }
                        if (valueField.templateField.align) reader.Align();
                    }
                }
            }
            return valueField;
        }
    }
}
