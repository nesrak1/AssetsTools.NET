////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AssetsTools.NET
{
    public struct AssetTypeArray
    {
        public uint size;
        //AssetTypeValueField *dataField;
    }

    public struct AssetTypeByteArray
    {
        public uint size;
        public byte[] data;
    }

    public enum EnumValueTypes
    {
        ValueType_None,
        ValueType_Bool,
        ValueType_Int8,
        ValueType_UInt8,
        ValueType_Int16,
        ValueType_UInt16,
        ValueType_Int32,
        ValueType_UInt32,
        ValueType_Int64,
        ValueType_UInt64,
        ValueType_Float,
        ValueType_Double,
        ValueType_String,
        ValueType_Array,
        ValueType_ByteArray
    }

    public class AssetTypeValue
    {
        //bool freeValue;
        public EnumValueTypes type;
        
        public struct ValueTypes
        {
            public AssetTypeArray asArray;
            public AssetTypeByteArray asByteArray;

            public bool asBool;

            public sbyte asInt8;
            public byte asUInt8;

            public short asInt16;
            public ushort asUInt16;

            public int asInt32;
            public uint asUInt32;

            public long asInt64;
            public ulong asUInt64;

            public float asFloat;
            public double asDouble;

            public string asString;
        }
        public ValueTypes value = new ValueTypes();

	    //Creates an AssetTypeValue.
	    //type : the value type which valueContainer stores
	    //valueContainer : the buffer for the value type
	    //freeIfPointer : should the value get freed if value type is Array/String
	    public AssetTypeValue(EnumValueTypes type, object valueContainer)
        {
            this.type = type;
        }
        public EnumValueTypes GetValueType()
        {
            return type;
        }
        //-What is this monstrosity
        //-UABE uses unions, which I could use a special struct but decided against
        //-Obviously this doesn't cover everything, eg casting a uint8 to string
        public void Set(object valueContainer)
        {
            unchecked
            {
                switch (type)
                {
                    case EnumValueTypes.ValueType_Bool:
                        value.asBool = Convert.ToBoolean(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Int8:
                        value.asInt8 = Convert.ToSByte(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_UInt8:
                        value.asUInt8 = Convert.ToByte(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Int16:
                        value.asInt16 = Convert.ToInt16(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_UInt16:
                        value.asUInt16 = Convert.ToUInt16(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Int32:
                        value.asInt32 = Convert.ToInt32(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_UInt32:
                        value.asUInt32 = Convert.ToUInt32(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Int64:
                        value.asInt64 = Convert.ToInt64(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_UInt64:
                        value.asUInt64 = Convert.ToUInt64(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Float:
                        value.asFloat = Convert.ToSingle(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Double:
                        value.asDouble = Convert.ToDouble(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_String:
                        value.asString = Convert.ToString(valueContainer);
                        break;
                    case EnumValueTypes.ValueType_Array:
                        value.asArray = (AssetTypeArray)valueContainer;
                        break;
                    case EnumValueTypes.ValueType_ByteArray:
                        value.asByteArray = (AssetTypeByteArray)valueContainer;
                        break;
                    default:
                        break;
                }
            }
        }
        public AssetTypeArray AsArray()
        {
            return (type == EnumValueTypes.ValueType_Array) ? value.asArray : new AssetTypeArray() { size = 0xFFFF };
        }
        public AssetTypeByteArray AsByteArray()
        {
            return (type == EnumValueTypes.ValueType_ByteArray) ? value.asByteArray : new AssetTypeByteArray() { size = 0xFFFF };
        }
        public string AsString()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Bool:
                    return value.asBool ? "true" : "false";
                case EnumValueTypes.ValueType_Int8:
                    return value.asInt8.ToString();
                case EnumValueTypes.ValueType_UInt8:
                    return value.asUInt8.ToString();
                case EnumValueTypes.ValueType_Int16:
                    return value.asInt16.ToString();
                case EnumValueTypes.ValueType_UInt16:
                    return value.asUInt16.ToString();
                case EnumValueTypes.ValueType_Int32:
                    return value.asInt32.ToString();
                case EnumValueTypes.ValueType_UInt32:
                    return value.asUInt32.ToString();
                case EnumValueTypes.ValueType_Int64:
                    return value.asInt64.ToString();
                case EnumValueTypes.ValueType_UInt64:
                    return value.asUInt64.ToString();
                case EnumValueTypes.ValueType_Float:
                    return value.asFloat.ToString();
                case EnumValueTypes.ValueType_Double:
                    return value.asDouble.ToString();
                case EnumValueTypes.ValueType_String:
                    return value.asString;
                case EnumValueTypes.ValueType_None:
                case EnumValueTypes.ValueType_Array:
                case EnumValueTypes.ValueType_ByteArray:
                default:
                    return "";
            }
        }
        public bool AsBool()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                case EnumValueTypes.ValueType_Double:
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return false;
                default:
                    return value.asBool;
            }
        }
        public int AsInt()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return (int)value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return (int)value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                case EnumValueTypes.ValueType_Int8:
                    return (int)value.asInt8;
                case EnumValueTypes.ValueType_Int16:
                    return (int)value.asInt16;
                case EnumValueTypes.ValueType_Int64:
                    return (int)value.asInt64;
                default:
                    return value.asInt32;
            }
        }
        public uint AsUInt()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return (uint)value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return (uint)value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                default:
                    return value.asUInt32;
            }
        }
        public long AsInt64()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return (long)value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return (long)value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                case EnumValueTypes.ValueType_Int8:
                    return (long)value.asInt8;
                case EnumValueTypes.ValueType_Int16:
                    return (long)value.asInt16;
                case EnumValueTypes.ValueType_Int32:
                    return (long)value.asInt32;
                default:
                    return value.asInt64;
            }
        }
        public ulong AsUInt64()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return (uint)value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return (ulong)value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                default:
                    return value.asUInt64;
            }
        }
        public float AsFloat()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return (float)value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                case EnumValueTypes.ValueType_Int8:
                    return (float)value.asInt8;
                case EnumValueTypes.ValueType_Int16:
                    return (float)value.asInt16;
                case EnumValueTypes.ValueType_Int32:
                    return (float)value.asInt32;
                default:
                    return (float)value.asUInt64;
            }
        }
        public double AsDouble()
        {
            switch (type)
            {
                case EnumValueTypes.ValueType_Float:
                    return (double)value.asFloat;
                case EnumValueTypes.ValueType_Double:
                    return value.asDouble;
                case EnumValueTypes.ValueType_String:
                case EnumValueTypes.ValueType_ByteArray:
                case EnumValueTypes.ValueType_Array:
                    return 0;
                case EnumValueTypes.ValueType_Int8:
                    return (double)value.asInt8;
                case EnumValueTypes.ValueType_Int16:
                    return (double)value.asInt16;
                case EnumValueTypes.ValueType_Int32:
                    return (double)value.asInt32;
                default:
                    return (double)value.asUInt64;
            }
        }
    }
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
        ///public bool From0D(Type_0D pU5Type, uint fieldIndex)
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
            //root++;
            //Debug.WriteLine("f @" + reader.Position + " " + new string(' ', root) + valueField.templateField.type + "/" + valueField.templateField.name);
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
                            //Debug.WriteLine("|AC -> " + reader.Position + "CNT: " + valueField.childrenCount);
                            valueField.pChildren[i] = new AssetTypeValueField();
                            valueField.pChildren[i].templateField = valueField.templateField.children[1];
                            valueField.pChildren[i] = ReadType(reader, reader.Position, valueField.pChildren[i], bigEndian);
                        }
                        if (valueField.templateField.align) reader.Align();
                        AssetTypeArray ata = new AssetTypeArray();
                        ata.size = valueField.childrenCount;
                        valueField.value = new AssetTypeValue(EnumValueTypes.ValueType_Array, 0);
                        valueField.value.Set(ata);
                    } else
                    {
                        Debug.WriteLine("Invalid array value type! Found an unexpected " + sizeType.ToString() + " type instead!");
                    }
                } else
                {
                    Debug.WriteLine("Invalid array!");
                }
            } else
            {
                EnumValueTypes type = valueField.templateField.valueType;
                if (type != 0) valueField.value = new AssetTypeValue(type, 0);
                if (type == EnumValueTypes.ValueType_String)
                {
                    //Debug.WriteLine("1S -> " + reader.Position);
                    valueField.value.Set(reader.ReadCountStringInt32());
                    reader.Align();
                    //Debug.WriteLine("2S -> " + reader.Position);
                } else
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
                    } else
                    {
                        valueField.pChildren = new AssetTypeValueField[valueField.childrenCount];
                        for (int i = 0; i < valueField.childrenCount; i++)
                        {
                            //Debug.WriteLine("|RC -> " + reader.Position);
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
    public class AssetTypeValueField
    {
	    public AssetTypeTemplateField templateField;

        public uint childrenCount;
        public AssetTypeValueField[] pChildren;
        public AssetTypeValue value;

        public void Read(AssetTypeValue pValue, AssetTypeTemplateField pTemplate, uint childrenCount, AssetTypeValueField[] pChildren)
        {
            templateField = pTemplate;
            this.childrenCount = childrenCount;
            this.pChildren = pChildren;
            value = pValue;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos);

        //ASSETSTOOLS_API void Clear();

        //get a child field by its name
        public AssetTypeValueField this[string name]
        {
            get {
                foreach (AssetTypeValueField atvf in pChildren)
                {
                    if (atvf.templateField.name == name)
                    {
                        return atvf;
                    }
                }
                return AssetTypeInstance.GetDummyAssetTypeField();
            }
            set { }
        }
        //get a child field by its index
        public AssetTypeValueField this[uint index]
        {
            get { return pChildren[index]; }
            set { }
        }

        public AssetTypeValueField Get(string name) { return (this)[name]; }
        public AssetTypeValueField Get(uint index) { return (this)[index]; }

        public string GetName() { return templateField.name; }
        public string GetFieldType() { return templateField.type; }
        public AssetTypeValue GetValue() { return value; }
        public AssetTypeTemplateField GetTemplateField() { return templateField; }
        public AssetTypeValueField[] GetChildrenList() { return pChildren; }
        public void SetChildrenList(AssetTypeValueField[] pChildren, uint childrenCount) { this.pChildren = pChildren; this.childrenCount = childrenCount; }

        public uint GetChildrenCount() { return childrenCount; }

        public bool IsDummy()
        {
            return childrenCount == 0xFF;
        }

        ///public ulong GetByteSize(ulong filePos = 0);
        
        public static EnumValueTypes GetValueTypeByTypeName(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case "string":
                    return EnumValueTypes.ValueType_String;
                case "sint8":
                case "sbyte":
                    return EnumValueTypes.ValueType_Int8;
                case "uint8":
                    return EnumValueTypes.ValueType_UInt8;
                case "sint16":
                case "short":
                    return EnumValueTypes.ValueType_Int16;
                case "uint16":
                case "unsigned short":
                case "char":
                    return EnumValueTypes.ValueType_UInt16;
                case "sint32":
                case "int":
                    return EnumValueTypes.ValueType_Int32;
                case "type*":
                    return EnumValueTypes.ValueType_Int32;
                case "uint32":
                case "unsigned int":
                    return EnumValueTypes.ValueType_UInt32;
                case "sint64":
                case "long":
                    return EnumValueTypes.ValueType_Int64;
                case "uint64":
                case "unsigned long":
                    return EnumValueTypes.ValueType_UInt64;
                case "single":
                case "float":
                    return EnumValueTypes.ValueType_Float;
                case "double":
                    return EnumValueTypes.ValueType_Double;
                case "bool":
                    return EnumValueTypes.ValueType_Bool;
                //in the original function, array and byte array are actually returned as none, how strange
                //case "array":
                //    return EnumValueTypes.ValueType_Array;
                //case "bytearray":
                //    return EnumValueTypes.ValueType_ByteArray;
                default:
                    return EnumValueTypes.ValueType_None;
            }
        }
    }
    public class AssetTypeInstance
    {
        public uint baseFieldCount;
        public AssetTypeValueField[] baseFields;
        public uint allocationCount; public uint allocationBufLen;
        public byte[] memoryToClear;
	    public AssetTypeInstance(uint baseFieldCount, AssetTypeTemplateField[] ppBaseFields, AssetsFileReader reader, bool bigEndian, ulong filePos = 0)
        {
            this.baseFieldCount = baseFieldCount;
            reader.bigEndian = false;
            reader.BaseStream.Position = (long)filePos;
            baseFields = new AssetTypeValueField[this.baseFieldCount];
            for (int i = 0; i < baseFieldCount; i++)
            {
                //Debug.WriteLine(reader.BaseStream.Position);
                AssetTypeTemplateField templateBaseField = ppBaseFields[i];
                AssetTypeValueField atvf;
                templateBaseField.MakeValue(reader, reader.Position, out atvf, reader.bigEndian);
                //atvf.Read(atvf.value, templateBaseField, atvf.childrenCount, atvf.pChildren);
                baseFields[i] = atvf;
            }
        }
        ///public bool SetChildList(AssetTypeValueField pValueField, AssetTypeValueField[] pChildrenList, uint childrenCount, bool freeMemory = true);
        ///public bool AddTempMemory(byte[] pMemory);

        public static AssetTypeValueField GetDummyAssetTypeField()
        {
            AssetTypeValueField atvf = new AssetTypeValueField();
            atvf.childrenCount = 0xFF;
            return atvf;
        }

        public AssetTypeValueField GetBaseField(uint index = 0)
        {
            if (index >= baseFieldCount)
                return GetDummyAssetTypeField();
            return baseFields[index];
        }
    }
}
