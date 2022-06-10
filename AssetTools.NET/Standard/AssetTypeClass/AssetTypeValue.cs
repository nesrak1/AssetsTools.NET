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
        public AssetTypeValue(AssetValueType valueType, object value = null)
        {
            ValueType = valueType;
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
            get => (sbyte)Value;
            set => Value = value;
        }
        public byte AsByte
        {
            get => (byte)Value;
            set => Value = value;
        }
        public short AsShort
        {
            get => (short)Value;
            set => Value = value;
        }
        public ushort AsUShort
        {
            get => (ushort)Value;
            set => Value = value;
        }
        public int AsInt
        {
            get => (int)Value;
            set => Value = value;
        }
        public uint AsUInt
        {
            get => (uint)Value;
            set => Value = value;
        }
        public long AsLong
        {
            get => (long)Value;
            set => Value = value;
        }
        public ulong AsULong
        {
            get => (ulong)Value;
            set => Value = value;
        }
        public float AsFloat
        {
            get => (float)Value;
            set => Value = value;
        }
        public double AsDouble
        {
            get => (double)Value;
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

        public override string ToString()
        {
            return Value.ToString();
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
