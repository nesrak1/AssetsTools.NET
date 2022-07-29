using System.Collections.Generic;

namespace AssetsTools.NET.Extra
{
    public class ValueBuilder
    {
        public static AssetTypeValueField DefaultValueFieldFromArrayTemplate(AssetTypeValueField arrayField)
        {
            return DefaultValueFieldFromArrayTemplate(arrayField.TemplateField);
        }
        public static AssetTypeValueField DefaultValueFieldFromArrayTemplate(AssetTypeTemplateField arrayField)
        {
            if (!arrayField.IsArray)
                return null;

            AssetTypeTemplateField templateField = arrayField.Children[1];
            return DefaultValueFieldFromTemplate(templateField);
        }

        public static AssetTypeValueField DefaultValueFieldFromTemplate(AssetTypeTemplateField templateField)
        {
            List<AssetTypeTemplateField> templateChildren = templateField.Children;
            List<AssetTypeValueField> valueChildren;

            if (templateField.IsArray || templateField.ValueType == AssetValueType.String)
            {
                valueChildren = new List<AssetTypeValueField>(0);
            }
            else
            {
                valueChildren = new List<AssetTypeValueField>(templateChildren.Count);
                for (int i = 0; i < templateChildren.Count; i++)
                {
                    valueChildren.Add(DefaultValueFieldFromTemplate(templateChildren[i]));
                }
            }

            AssetTypeValue defaultValue = DefaultValueFromTemplate(templateField);

            AssetTypeValueField root = new AssetTypeValueField()
            {
                Children = valueChildren,
                TemplateField = templateField,
                Value = defaultValue
            };
            return root;
        }

        public static AssetTypeValue DefaultValueFromTemplate(AssetTypeTemplateField templateField)
        {
            object obj;
            switch (templateField.ValueType)
            {
                case AssetValueType.Int8:
                    obj = (sbyte)0; break;
                case AssetValueType.UInt8:
                    obj = (byte)0; break;
                case AssetValueType.Bool:
                    obj = false; break;
                case AssetValueType.Int16:
                    obj = (short)0; break;
                case AssetValueType.UInt16:
                    obj = (ushort)0; break;
                case AssetValueType.Int32:
                    obj = 0; break;
                case AssetValueType.UInt32:
                    obj = 0u; break;
                case AssetValueType.Int64:
                    obj = 0L; break;
                case AssetValueType.UInt64:
                    obj = 0uL; break;
                case AssetValueType.Float:
                    obj = 0f; break;
                case AssetValueType.Double:
                    obj = 0d; break;
                case AssetValueType.String:
                case AssetValueType.ByteArray:
                    obj = new byte[0]; break;
                case AssetValueType.Array:
                    obj = new AssetTypeArrayInfo(); break;
                default:
                    obj = null; break;
            }
            if (obj == null && templateField.IsArray)
            {
                // arrays don't usually have their type set,
                // so we have to check .IsArray instead
                obj = new AssetTypeArrayInfo();
                return new AssetTypeValue(AssetValueType.Array, obj);
            }
            else
            {
                return new AssetTypeValue(templateField.ValueType, obj);
            }
        }
    }
}
