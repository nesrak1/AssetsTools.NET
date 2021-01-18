using System.Collections.Generic;
using System.Linq;

namespace AssetsTools.NET
{
    public class AssetTypeValueField
    {
        public AssetTypeTemplateField templateField;

        public int childrenCount;
        public AssetTypeValueField[] children;
        public AssetTypeValue value;

        public void Read(AssetTypeValue value, AssetTypeTemplateField template, AssetTypeValueField[] children)
        {
            templateField = template;
            this.childrenCount = children.Length;
            this.children = children;
            this.value = value;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos);

        //ASSETSTOOLS_API void Clear();

        public AssetTypeValueField this[string name]
        {
            get
            {
                foreach (AssetTypeValueField atvf in children)
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

        public AssetTypeValueField this[int index]
        {
            get { return children[index]; }
            set { }
        }

        public AssetTypeValueField Get(string name) { return (this)[name]; }
        public AssetTypeValueField Get(int index) { return (this)[index]; }

        public string GetName() { return templateField.name; }
        public string GetFieldType() { return templateField.type; }
        public AssetTypeValue GetValue() { return value; }
        public AssetTypeTemplateField GetTemplateField() { return templateField; }
        public AssetTypeValueField[] GetChildrenList() { return children; }
        public void SetChildrenList(AssetTypeValueField[] children) { this.children = children; this.childrenCount = children.Length; }
        public void AddChildren(AssetTypeValueField[] children) { this.children = this.children.Concat(children).ToArray(); }
        public void AddChildren(AssetTypeValueField children) { this.children = this.children.Append(children).ToArray(); }
        
        public void RemoveChildren(AssetTypeValueField[] children)
        {
            // Removes an array of similar children
            this.children = this.children.Except(children).ToArray();
        }
        
        public void RemoveChildren(AssetTypeValueField children) { this.children = this.children.Where(i => i != children).ToArray(); }
        
        public int GetChildrenCount() { return childrenCount; }

        public bool IsDummy()
        {
            return childrenCount == -1;
        }

        ///public ulong GetByteSize(ulong filePos = 0);

        public static EnumValueTypes GetValueTypeByTypeName(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case "string":
                    return EnumValueTypes.String;
                case "sint8":
                case "sbyte":
                    return EnumValueTypes.Int8;
                case "uint8":
                case "char":
                case "byte":
                    return EnumValueTypes.UInt8;
                case "sint16":
                case "short":
                    return EnumValueTypes.Int16;
                case "uint16":
                case "unsigned short":
                case "ushort":
                    return EnumValueTypes.UInt16;
                case "sint32":
                case "int":
                    return EnumValueTypes.Int32;
                case "type*":
                    return EnumValueTypes.Int32;
                case "uint32":
                case "unsigned int":
                case "uint":
                    return EnumValueTypes.UInt32;
                case "sint64":
                case "long":
                    return EnumValueTypes.Int64;
                case "uint64":
                case "unsigned long":
                case "ulong":
                    return EnumValueTypes.UInt64;
                case "single":
                case "float":
                    return EnumValueTypes.Float;
                case "double":
                    return EnumValueTypes.Double;
                case "bool":
                    return EnumValueTypes.Bool;
                default:
                    return EnumValueTypes.None;
            }
        }
    }
}
