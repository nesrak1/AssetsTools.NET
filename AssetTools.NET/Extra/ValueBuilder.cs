using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class ValueBuilder
    {
        public static AssetTypeValueField DefaultValueFieldFromArrayTemplate(AssetTypeValueField arrayField)
        {
            return DefaultValueFieldFromArrayTemplate(arrayField.templateField);
        }
        public static AssetTypeValueField DefaultValueFieldFromArrayTemplate(AssetTypeTemplateField arrayField)
        {
            if (!arrayField.isArray)
                return null;

            AssetTypeTemplateField templateField = arrayField.children[1];
            return DefaultValueFieldFromTemplate(templateField);
        }

        public static AssetTypeValueField DefaultValueFieldFromTemplate(AssetTypeTemplateField templateField)
        {
            AssetTypeTemplateField[] templateChildren = templateField.children;
            AssetTypeValueField[] valueChildren;
            if (templateField.isArray ||
                templateField.valueType == EnumValueTypes.String)
            {
                valueChildren = new AssetTypeValueField[0];
            }
            else
            {
                valueChildren = new AssetTypeValueField[templateChildren.Length];
                for (int i = 0; i < templateChildren.Length; i++)
                {
                    valueChildren[i] = DefaultValueFieldFromTemplate(templateChildren[i]);
                }
            }

            AssetTypeValue defaultValue = DefaultValueFromTemplate(templateField);

            AssetTypeValueField root = new AssetTypeValueField()
            {
                children = valueChildren,
                childrenCount = valueChildren.Length,
                templateField = templateField,
                value = defaultValue
            };
            return root;
        }

        public static AssetTypeValue DefaultValueFromTemplate(AssetTypeTemplateField templateField)
        {
            object obj;
            switch (templateField.valueType)
            {
                case EnumValueTypes.Int8:
                    obj = (sbyte)0; break;
                case EnumValueTypes.UInt8:
                    obj = (byte)0; break;
                case EnumValueTypes.Bool:
                    obj = false; break;
                case EnumValueTypes.Int16:
                    obj = (short)0; break;
                case EnumValueTypes.UInt16:
                    obj = (ushort)0; break;
                case EnumValueTypes.Int32:
                    obj = 0; break;
                case EnumValueTypes.UInt32:
                    obj = 0u; break;
                case EnumValueTypes.Int64:
                    obj = 0L; break;
                case EnumValueTypes.UInt64:
                    obj = 0uL; break;
                case EnumValueTypes.Float:
                    obj = 0f; break;
                case EnumValueTypes.Double:
                    obj = 0d; break;
                case EnumValueTypes.String:
                    obj = string.Empty; break;
                case EnumValueTypes.Array:
                    obj = new AssetTypeArray(); break;
                case EnumValueTypes.ByteArray:
                    obj = new AssetTypeByteArray(); break;
                default:
                    obj = null; break;
            }
            if (obj == null && templateField.isArray)
            {
                //arrays don't usually have their type set,
                //so we have to check .isArray instead
                obj = new AssetTypeArray();
                return new AssetTypeValue(EnumValueTypes.Array, obj);
            }
            else
            {
                return new AssetTypeValue(templateField.valueType, obj);
            }
        }
    }
}
