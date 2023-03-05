using System;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetTypeValue
    {
        public AssetValueType ValueType { get; set; }
        private object Value { get; set; }

        public AssetTypeValue(bool value)
        {
            ValueType = AssetValueType.Bool;
            Value = value;
        }
        public AssetTypeValue(sbyte value)
        {
            ValueType = AssetValueType.Int8;
            Value = value;
        }
        public AssetTypeValue(byte value)
        {
            ValueType = AssetValueType.UInt8;
            Value = value;
        }
        public AssetTypeValue(short value)
        {
            ValueType = AssetValueType.Int16;
            Value = value;
        }
        public AssetTypeValue(ushort value)
        {
            ValueType = AssetValueType.UInt16;
            Value = value;
        }
        public AssetTypeValue(int value)
        {
            ValueType = AssetValueType.Int32;
            Value = value;
        }
        public AssetTypeValue(uint value)
        {
            ValueType = AssetValueType.UInt32;
            Value = value;
        }
        public AssetTypeValue(long value)
        {
            ValueType = AssetValueType.Int64;
            Value = value;
        }
        public AssetTypeValue(ulong value)
        {
            ValueType = AssetValueType.UInt64;
            Value = value;
        }
        public AssetTypeValue(float value)
        {
            ValueType = AssetValueType.Float;
            Value = value;
        }
        public AssetTypeValue(double value)
        {
            ValueType = AssetValueType.Double;
            Value = value;
        }
        public AssetTypeValue(string value)
        {
            ValueType = AssetValueType.String;
            Value = Encoding.UTF8.GetBytes(value);
        }
        public AssetTypeValue(byte[] value, bool asString)
        {
            ValueType = asString ? AssetValueType.String : AssetValueType.ByteArray;
            Value = value;
        }
        public AssetTypeValue(ManagedReferencesRegistry value)
        {
            ValueType = AssetValueType.ManagedReferencesRegistry;
            Value = value;
        }
        public AssetTypeValue(AssetValueType valueType, object value = null)
        {
            ValueType = valueType;

            if (value is string stringValue)
                Value = Encoding.UTF8.GetBytes(stringValue);
            else
                Value = value;
        }

        public bool AsBool
        {
            get
            {
                if (Value is bool boolValue)
                {
                    return boolValue;
                }
                if (Value is byte byteValue)
                {
                    return byteValue == 1;
                }
                return false;
            }
            set => Value = value;
        }
        public sbyte AsSByte
        {
            get
            {
                if (Value is sbyte sbyteValue)
                    return sbyteValue;
                else
                    return (sbyte)Convert.ChangeType(Value, typeof(sbyte));
            }
            set => Value = value;
        }
        public byte AsByte
        {
            get
            {
                if (Value is byte byteValue)
                    return byteValue;
                else
                    return (byte)Convert.ChangeType(Value, typeof(byte));
            }
            set => Value = value;
        }
        public short AsShort
        {
            get
            {
                if (Value is short shortValue)
                    return shortValue;
                else
                    return (short)Convert.ChangeType(Value, typeof(short));
            }
            set => Value = value;
        }
        public ushort AsUShort
        {
            get
            {
                if (Value is ushort ushortValue)
                    return ushortValue;
                else
                    return (ushort)Convert.ChangeType(Value, typeof(ushort));
            }
            set => Value = value;
        }
        public int AsInt
        {
            get
            {
                if (Value is int intValue)
                    return intValue;
                else
                    return (int)Convert.ChangeType(Value, typeof(int));
            }
            set => Value = value;
        }
        public uint AsUInt
        {
            get
            {
                if (Value is uint uintValue)
                    return uintValue;
                else
                    return (uint)Convert.ChangeType(Value, typeof(uint));
            }
            set => Value = value;
        }
        public long AsLong
        {
            get
            {
                if (Value is long longValue)
                    return longValue;
                else
                    return (long)Convert.ChangeType(Value, typeof(long));
            }
            set => Value = value;
        }
        public ulong AsULong
        {
            get
            {
                if (Value is ulong uintValue)
                    return uintValue;
                else
                    return (ulong)Convert.ChangeType(Value, typeof(ulong));
            }
            set => Value = value;
        }
        public float AsFloat
        {
            get
            {
                if (Value is float floatValue)
                    return floatValue;
                else
                    return (float)Convert.ChangeType(Value, typeof(float));
            }
            set => Value = value;
        }
        public double AsDouble
        {
            get
            {
                if (Value is double doubleValue)
                    return doubleValue;
                else
                    return (double)Convert.ChangeType(Value, typeof(double));
            }
            set => Value = value;
        }
        public string AsString
        {
            get
            {
                if (ValueType == AssetValueType.String)
                    return Encoding.UTF8.GetString((byte[])Value);
                else if (ValueType == AssetValueType.Bool)
                    return (bool)Value ? "true" : "false";
                else if (ValueType == AssetValueType.ByteArray)
                    return SimpleHexDump((byte[])Value);
                else
                    return Value.ToString();
            }
            set => Value = Encoding.UTF8.GetBytes(value);
        }

        // probably will get removed soon
        public AssetTypeArrayInfo AsArray
        {
            get => (AssetTypeArrayInfo)Value;
            set => Value = value;
        }
        public byte[] AsByteArray
        {
            get => (byte[])Value;
            set => Value = value;
        }
        public ManagedReferencesRegistry AsManagedReferencesRegistry
        {
            get => (ManagedReferencesRegistry)Value;
            set => Value = value;
        }

        // use this only if you have to!
        public object AsObject
        {
            get => Value;
            set
            {
                if (value is string stringValue)
                    Value = Encoding.UTF8.GetBytes(stringValue);
                else
                    Value = value;
            }
        }

        public override string ToString()
        {
            return AsString;
        }

        private string SimpleHexDump(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder();
            if (byteArray.Length <= 0)
                return string.Empty;

            int i;
            for (i = 0; i < byteArray.Length - 1; i++)
            {
                sb.Append(byteArray[i].ToString("x2"));
                sb.Append(" ");
            }
            sb.Append(byteArray[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
