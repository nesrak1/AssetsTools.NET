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
        public uint childrenCount;
        public AssetTypeTemplateField[] children;

        ///public AssetTypeTemplateField()
        ///public void Clear()
        public bool From0D(Type_0D pU5Type, uint fieldIndex)
        {
            return true;
        }
        public bool FromClassDatabase(ClassDatabaseFile pFile, ClassDatabaseType pType, uint fieldIndex)
        {
            ClassDatabaseTypeField field = pType.fields[(int)fieldIndex];
            name = field.fieldName.GetString(pFile);
            type = field.typeName.GetString(pFile);
            valueType = AssetTypeValueField.GetValueTypeByTypeName(type);
            isArray = field.isArray == 1 ? true : false;
            align = (field.flags2 & 0x4000) != 0x00 ? true : false;
            hasValue = (valueType == EnumValueTypes.ValueType_None) ? false : true;

            List<int> childrenIndexes = new List<int>();
            int thisDepth = pType.fields[(int)fieldIndex].depth;
            for (int i = (int)fieldIndex + 1; i < pType.fields.Count; i++)
            {
                if (pType.fields[i].depth == thisDepth + 1)
                {
                    childrenCount++;
                    childrenIndexes.Add(i);
                }
                if (pType.fields[i].depth <= thisDepth) break;
            }
            children = new AssetTypeTemplateField[childrenCount];
            int child = 0;
            for (int i = (int)fieldIndex + 1; i < pType.fields.Count; i++)
            {
                if (pType.fields[i].depth == thisDepth + 1)
                {
                    children[child] = new AssetTypeTemplateField();
                    children[child].FromClassDatabase(pFile, pType, (uint)childrenIndexes[child]);
                    child++;
                }
                if (pType.fields[i].depth <= thisDepth) break;
            }
            return true;
        }
        ///public bool From07(TypeField_07 pTypeField)
        public ulong MakeValue(AssetsFileReader reader, ulong filePos, out AssetTypeValueField ppValueField, bool bigEndian)
        {
            ppValueField = new AssetTypeValueField();
            ppValueField.templateField = this;
            ppValueField = ReadType(reader, filePos, ppValueField, bigEndian);
            return reader.Position;
        }

        public AssetTypeValueField ReadType(AssetsFileReader reader, ulong filePos, AssetTypeValueField valueField, bool bigEndian)
        {
            if (valueField.templateField.isArray)
            {
                if (valueField.templateField.childrenCount == 2)
                {
                    EnumValueTypes sizeType = valueField.templateField.children[0].valueType;
                    if (sizeType == EnumValueTypes.ValueType_Int32 ||
                        sizeType == EnumValueTypes.ValueType_UInt32)
                    {
                        valueField.childrenCount = reader.ReadUInt32();
                        valueField.pChildren = new AssetTypeValueField[valueField.childrenCount];
                        for (int i = 0; i < valueField.childrenCount; i++)
                        {
                            valueField.pChildren[i] = new AssetTypeValueField();
                            valueField.pChildren[i].templateField = valueField.templateField.children[1];
                            valueField.pChildren[i] = ReadType(reader, reader.Position, valueField.pChildren[i], bigEndian);
                        }
                        if (valueField.templateField.align) reader.Align();
                        AssetTypeArray ata = new AssetTypeArray();
                        ata.size = valueField.childrenCount;
                        valueField.value = new AssetTypeValue(EnumValueTypes.ValueType_Array, 0);
                        valueField.value.Set(ata);
                    }
                    else
                    {
                        Debug.WriteLine("Invalid array value type! Found an unexpected " + sizeType.ToString() + " type instead!");
                    }
                }
                else
                {
                    Debug.WriteLine("Invalid array!");
                }
            }
            else
            {
                EnumValueTypes type = valueField.templateField.valueType;
                if (type != 0) valueField.value = new AssetTypeValue(type, 0);
                if (type == EnumValueTypes.ValueType_String)
                {
                    valueField.value.Set(reader.ReadCountStringInt32());
                    reader.Align();
                }
                else
                {
                    valueField.childrenCount = valueField.templateField.childrenCount;
                    if (valueField.childrenCount == 0)
                    {
                        valueField.pChildren = new AssetTypeValueField[0];
                        switch (valueField.templateField.valueType)
                        {
                            case EnumValueTypes.ValueType_Int8:
                                valueField.value.Set(reader.ReadSByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.ValueType_UInt8:
                            case EnumValueTypes.ValueType_Bool:
                                valueField.value.Set(reader.ReadByte());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.ValueType_Int16:
                                valueField.value.Set(reader.ReadInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.ValueType_UInt16:
                                valueField.value.Set(reader.ReadUInt16());
                                if (valueField.templateField.align) reader.Align();
                                break;
                            case EnumValueTypes.ValueType_Int32:
                                valueField.value.Set(reader.ReadInt32());
                                break;
                            case EnumValueTypes.ValueType_UInt32:
                                valueField.value.Set(reader.ReadUInt32());
                                break;
                            case EnumValueTypes.ValueType_Int64:
                                valueField.value.Set(reader.ReadInt64());
                                break;
                            case EnumValueTypes.ValueType_UInt64:
                                valueField.value.Set(reader.ReadUInt64());
                                break;
                            case EnumValueTypes.ValueType_Float:
                                valueField.value.Set(reader.ReadSingle());
                                break;
                            case EnumValueTypes.ValueType_Double:
                                valueField.value.Set(reader.ReadDouble());
                                break;
                        }
                    }
                    else
                    {
                        valueField.pChildren = new AssetTypeValueField[valueField.childrenCount];
                        for (int i = 0; i < valueField.childrenCount; i++)
                        {
                            valueField.pChildren[i] = new AssetTypeValueField();
                            valueField.pChildren[i].templateField = valueField.templateField.children[i];
                            valueField.pChildren[i] = ReadType(reader, reader.Position, valueField.pChildren[i], bigEndian);
                        }
                        if (valueField.templateField.align) reader.Align();
                    }
                }
            }
            return valueField;
        }
        ///public AssetTypeTemplateField SearchChild(string name)
    }
}
