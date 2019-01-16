using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    //To stay close to the original API, these methods will stay apart from their classes
    //If the original API ever adds these methods into their classes, these will be moved back
    public static class AssetWriters
    {
        //AssetsBundleFile
        public static void Write(this AssetsBundleFile file, AssetsFileWriter writer/*AssetsFile assetsFile, AssetsReplacer[] replacers*/)
        {
            file.bundleHeader6.Write(writer, 0);
            file.bundleInf6.Write(writer, writer.Position);
            //assetsFile.Write(writer, writer.Position, replacers, 0);
        }
        //AssetTypeInstance
        public static void Write(this AssetTypeInstance instance, AssetsFileWriter writer)
        {
            for (int i = 0; i < instance.baseFieldCount; i++)
            {
                instance.baseFields[i].Write(writer);
            }
        }
        //AssetTypeValueField
        public static void Write(this AssetTypeValueField valueField, AssetsFileWriter writer, int depth = 0)
        {
            if (valueField.templateField.isArray)
            {
                if (valueField.templateField.valueType == EnumValueTypes.ValueType_ByteArray)
                {
                    uint size = valueField.value.value.asByteArray.size;

                    writer.Write(valueField.value.value.asByteArray.size);
                    writer.Write(valueField.value.value.asByteArray.data);
                    if (valueField.templateField.align) writer.Align();
                }
                else
                {
                    uint size = valueField.value.value.asArray.size;

                    writer.Write(size);
                    for (uint i = 0; i < size; i++)
                    {
                        valueField[i].Write(writer, depth + 1);
                    }
                    if (valueField.templateField.align) writer.Align();
                }
            }
            else
            {
                if (valueField.childrenCount == 0)
                {
                    switch (valueField.templateField.valueType)
                    {
                        case EnumValueTypes.ValueType_Int8:
                            writer.Write(valueField.value.value.asInt8);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.ValueType_UInt8:
                            writer.Write(valueField.value.value.asUInt8);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.ValueType_Bool:
                            writer.Write(valueField.value.value.asBool);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.ValueType_Int16:
                            writer.Write(valueField.value.value.asInt16);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.ValueType_UInt16:
                            writer.Write(valueField.value.value.asUInt16);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.ValueType_Int32:
                            writer.Write(valueField.value.value.asInt32);
                            break;
                        case EnumValueTypes.ValueType_UInt32:
                            writer.Write(valueField.value.value.asUInt32);
                            break;
                        case EnumValueTypes.ValueType_Int64:
                            writer.Write(valueField.value.value.asInt64);
                            break;
                        case EnumValueTypes.ValueType_UInt64:
                            writer.Write(valueField.value.value.asUInt64);
                            break;
                        case EnumValueTypes.ValueType_Float:
                            writer.Write(valueField.value.value.asFloat);
                            break;
                        case EnumValueTypes.ValueType_Double:
                            writer.Write(valueField.value.value.asDouble);
                            break;
                        case EnumValueTypes.ValueType_String:
                            writer.WriteCountStringInt32(valueField.value.value.asString);
                            writer.Align();
                            break;
                    }
                }
                else
                {
                    for (uint i = 0; i < valueField.childrenCount; i++)
                    {
                        valueField[i].Write(writer, depth + 1);
                    }
                    if (valueField.templateField.align) writer.Align();
                }
            }
        }
    }
}
