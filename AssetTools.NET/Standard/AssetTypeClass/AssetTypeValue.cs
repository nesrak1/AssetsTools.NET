using System;

namespace AssetsTools.NET
{
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
                        value.asBool = Convert.ToByte(valueContainer) == 1 ? true : false;
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
                case EnumValueTypes.ValueType_UInt8:
                    return (uint)value.asUInt8;
                case EnumValueTypes.ValueType_UInt16:
                    return (uint)value.asUInt16;
                case EnumValueTypes.ValueType_UInt64:
                    return (uint)value.asUInt64;
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
}
