using System;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetTypeValue
    {
        public EnumValueTypes type;

        public struct ValueTypes
        {
            private object value;
            public AssetTypeArray asArray
            {
                get { return (AssetTypeArray)value; }
                set { this.value = value; }
            }
            public AssetTypeByteArray asByteArray
            {
                get { return (AssetTypeByteArray)value; }
                set { this.value = value; }
            }
            public bool asBool
            {
                get { return (bool)value; }
                set { this.value = value; }
            }
            public sbyte asInt8
            {
                get { return (sbyte)value; }
                set { this.value = value; }
            }
            public byte asUInt8
            {
                get { return (byte)value; }
                set { this.value = value; }
            }
            public short asInt16
            {
                get { return (short)value; }
                set { this.value = value; }
            }
            public ushort asUInt16
            {
                get { return (ushort)value; }
                set { this.value = value; }
            }
            public int asInt32
            {
                get { return (int)value; }
                set { this.value = value; }
            }
            public uint asUInt32
            {
                get { return (uint)value; }
                set { this.value = value; }
            }
            public long asInt64
            {
                get { return (long)value; }
                set { this.value = value; }
            }
            public ulong asUInt64
            {
                get { return (ulong)value; }
                set { this.value = value; }
            }
            public float asFloat
            {
                get { return (float)value; }
                set { this.value = value; }
            }
            public double asDouble
            {
                get { return (double)value; }
                set { this.value = value; }
            }
            public byte[] asString
            {
                get { return (byte[])value; }
                set { this.value = value; }
            }
        }
        public ValueTypes value;

        public AssetTypeValue(EnumValueTypes type, object valueContainer)
        {
            this.type = type;
            if (valueContainer != null)
                Set(valueContainer);
        }
        public EnumValueTypes GetValueType()
        {
            return type;
        }
        public void Set(object valueContainer)
        {
            unchecked
            {
                switch (type)
                {
                    case EnumValueTypes.Bool:
                        value.asBool = Convert.ToByte(valueContainer) == 1 ? true : false;
                        break;
                    case EnumValueTypes.Int8:
                        value.asInt8 = Convert.ToSByte(valueContainer);
                        break;
                    case EnumValueTypes.UInt8:
                        value.asUInt8 = Convert.ToByte(valueContainer);
                        break;
                    case EnumValueTypes.Int16:
                        value.asInt16 = Convert.ToInt16(valueContainer);
                        break;
                    case EnumValueTypes.UInt16:
                        value.asUInt16 = Convert.ToUInt16(valueContainer);
                        break;
                    case EnumValueTypes.Int32:
                        value.asInt32 = Convert.ToInt32(valueContainer);
                        break;
                    case EnumValueTypes.UInt32:
                        value.asUInt32 = Convert.ToUInt32(valueContainer);
                        break;
                    case EnumValueTypes.Int64:
                        value.asInt64 = Convert.ToInt64(valueContainer);
                        break;
                    case EnumValueTypes.UInt64:
                        value.asUInt64 = Convert.ToUInt64(valueContainer);
                        break;
                    case EnumValueTypes.Float:
                        value.asFloat = Convert.ToSingle(valueContainer);
                        break;
                    case EnumValueTypes.Double:
                        value.asDouble = Convert.ToDouble(valueContainer);
                        break;
                    case EnumValueTypes.String:
                    {
                        value.asString = valueContainer switch
                                         {
                                             byte[] byteArray => byteArray,
                                             string str       => Encoding.UTF8.GetBytes(str),
                                             _                => new byte[0]
                                         };
                        break;
                    }
                    case EnumValueTypes.Array:
                        value.asArray = (AssetTypeArray)valueContainer;
                        break;
                    case EnumValueTypes.ByteArray:
                    {
                        value.asByteArray = valueContainer switch
                                            {
                                                AssetTypeByteArray byteArrayStruct  => byteArrayStruct,
                                                byte[] byteArray                    => new AssetTypeByteArray(byteArray),
                                                _                                   => new AssetTypeByteArray(new byte[0])
                                            };
                        break;
                    }
                }
            }
        }
        public AssetTypeArray AsArray()
        {
            return (type == EnumValueTypes.Array) ? value.asArray : new AssetTypeArray { size = 0xFFFF };
        }
        public AssetTypeByteArray AsByteArray()
        {
            return (type == EnumValueTypes.ByteArray) ? value.asByteArray : new AssetTypeByteArray { size = 0xFFFF };
        }
        public string AsString()
        {
            switch (type)
            {
                case EnumValueTypes.Bool:
                    return value.asBool ? "true" : "false";
                case EnumValueTypes.Int8:
                    return value.asInt8.ToString();
                case EnumValueTypes.UInt8:
                    return value.asUInt8.ToString();
                case EnumValueTypes.Int16:
                    return value.asInt16.ToString();
                case EnumValueTypes.UInt16:
                    return value.asUInt16.ToString();
                case EnumValueTypes.Int32:
                    return value.asInt32.ToString();
                case EnumValueTypes.UInt32:
                    return value.asUInt32.ToString();
                case EnumValueTypes.Int64:
                    return value.asInt64.ToString();
                case EnumValueTypes.UInt64:
                    return value.asUInt64.ToString();
                case EnumValueTypes.Float:
                    return value.asFloat.ToString();
                case EnumValueTypes.Double:
                    return value.asDouble.ToString();
                case EnumValueTypes.String:
                    return Encoding.UTF8.GetString(value.asString);
                default:
                    return "";
            }
        }
        public byte[] AsStringBytes()
        {
            return (type == EnumValueTypes.String) ? value.asString : null;
        }
        public bool AsBool()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                case EnumValueTypes.Double:
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return false;
                //new casts
                case EnumValueTypes.Int8:
                    return value.asInt8 == 1;
                case EnumValueTypes.Int16:
                    return value.asInt16 == 1;
                case EnumValueTypes.Int32:
                    return value.asInt32 == 1;
                case EnumValueTypes.Int64:
                    return value.asInt64 == 1;
                case EnumValueTypes.UInt8:
                    return value.asUInt8 == 1;
                case EnumValueTypes.UInt16:
                    return value.asUInt16 == 1;
                case EnumValueTypes.UInt32:
                    return value.asUInt32 == 1;
                case EnumValueTypes.UInt64:
                    return value.asUInt64 == 1;
                default:
                    return value.asBool;
            }
        }
        public int AsInt()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return (int)value.asFloat;
                case EnumValueTypes.Double:
                    return (int)value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.Int8:
                    return (int)value.asInt8;
                case EnumValueTypes.Int16:
                    return (int)value.asInt16;
                case EnumValueTypes.Int64:
                    return (int)value.asInt64;
                //new casts
                case EnumValueTypes.UInt8:
                    return (int)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (int)value.asUInt16;
                case EnumValueTypes.UInt32:
                    return (int)value.asUInt32;
                case EnumValueTypes.UInt64:
                    return (int)value.asUInt64;
                default:
                    return value.asInt32;
            }
        }
        public uint AsUInt()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return (uint)value.asFloat;
                case EnumValueTypes.Double:
                    return (uint)value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.UInt8:
                    return (uint)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (uint)value.asUInt16;
                case EnumValueTypes.UInt64:
                    return (uint)value.asUInt64;
                //new casts
                case EnumValueTypes.Int8:
                    return (uint)value.asUInt8;
                case EnumValueTypes.Int16:
                    return (uint)value.asUInt16;
                case EnumValueTypes.Int32:
                    return (uint)value.asUInt32;
                case EnumValueTypes.Int64:
                    return (uint)value.asUInt64;
                default:
                    return value.asUInt32;
            }
        }
        public long AsInt64()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return (long)value.asFloat;
                case EnumValueTypes.Double:
                    return (long)value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.Int8:
                    return (long)value.asInt8;
                case EnumValueTypes.Int16:
                    return (long)value.asInt16;
                case EnumValueTypes.Int32:
                    return (long)value.asInt32;
                //new casts
                case EnumValueTypes.UInt8:
                    return (long)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (long)value.asUInt16;
                case EnumValueTypes.UInt32:
                    return (long)value.asUInt32;
                case EnumValueTypes.UInt64:
                    return (long)value.asUInt64;
                default:
                    return value.asInt64;
            }
        }
        public ulong AsUInt64()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return (ulong)value.asFloat;
                case EnumValueTypes.Double:
                    return (ulong)value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.UInt8:
                    return (ulong)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (ulong)value.asUInt16;
                case EnumValueTypes.UInt32:
                    return (ulong)value.asUInt32;
                //new casts
                case EnumValueTypes.Int8:
                    return (ulong)value.asUInt8;
                case EnumValueTypes.Int16:
                    return (ulong)value.asUInt16;
                case EnumValueTypes.Int32:
                    return (ulong)value.asUInt32;
                case EnumValueTypes.Int64:
                    return (ulong)value.asUInt64;
                default:
                    return value.asUInt64;
            }
        }
        public float AsFloat()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return value.asFloat;
                case EnumValueTypes.Double:
                    return (float)value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.Int8:
                    return (float)value.asInt8;
                case EnumValueTypes.Int16:
                    return (float)value.asInt16;
                case EnumValueTypes.Int32:
                    return (float)value.asInt32;
                //new casts
                case EnumValueTypes.UInt8:
                    return (float)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (float)value.asUInt16;
                case EnumValueTypes.UInt32:
                    return (float)value.asUInt32;
                default:
                    return (float)value.asUInt64;
            }
        }
        public double AsDouble()
        {
            switch (type)
            {
                case EnumValueTypes.Float:
                    return (double)value.asFloat;
                case EnumValueTypes.Double:
                    return value.asDouble;
                case EnumValueTypes.String:
                case EnumValueTypes.ByteArray:
                case EnumValueTypes.Array:
                    return 0;
                case EnumValueTypes.Int8:
                    return (double)value.asInt8;
                case EnumValueTypes.Int16:
                    return (double)value.asInt16;
                case EnumValueTypes.Int32:
                    return (double)value.asInt32;
                //new casts
                case EnumValueTypes.UInt8:
                    return (double)value.asUInt8;
                case EnumValueTypes.UInt16:
                    return (double)value.asUInt16;
                case EnumValueTypes.UInt32:
                    return (double)value.asUInt32;
                default:
                    return (double)value.asUInt64;
            }
        }
    }
}
