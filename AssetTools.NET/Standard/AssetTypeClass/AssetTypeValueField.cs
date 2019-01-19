namespace AssetsTools.NET
{
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
            get
            {
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
                case "char":
                case "byte":
                    return EnumValueTypes.ValueType_UInt8;
                case "sint16":
                case "short":
                    return EnumValueTypes.ValueType_Int16;
                case "uint16":
                case "unsigned short":
                case "ushort":
                    return EnumValueTypes.ValueType_UInt16;
                case "sint32":
                case "int":
                    return EnumValueTypes.ValueType_Int32;
                case "type*":
                    return EnumValueTypes.ValueType_Int32;
                case "uint32":
                case "unsigned int":
                case "uint":
                    return EnumValueTypes.ValueType_UInt32;
                case "sint64":
                case "long":
                    return EnumValueTypes.ValueType_Int64;
                case "uint64":
                case "unsigned long":
                case "ulong":
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
}
