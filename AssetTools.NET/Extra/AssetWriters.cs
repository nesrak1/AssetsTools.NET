using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    //To stay close to the original API, these methods will stay apart from their classes
    //If the original API ever adds these methods into their classes, these will be moved back
    public static class AssetWriters
    {
        //AssetTypeInstance
        public static void Write(this AssetTypeInstance instance, AssetsFileWriter writer)
        {
            for (int i = 0; i < instance.baseFieldCount; i++)
            {
                instance.baseFields[i].Write(writer);
            }
        }
        public static byte[] WriteToByteArray(this AssetTypeInstance instance, bool bigEndian = false)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                w.bigEndian = bigEndian;
                instance.Write(w);
                data = ms.ToArray();
            }
            return data;
        }
        //AssetTypeValueField
        public static void Write(this AssetTypeValueField valueField, AssetsFileWriter writer, int depth = 0)
        {
            if (valueField.templateField.isArray)
            {
                if (valueField.templateField.valueType == EnumValueTypes.ByteArray)
                {
                    AssetTypeByteArray byteArray = valueField.value.value.asByteArray;

                    byteArray.size = (uint)byteArray.data.Length;
                    writer.Write(byteArray.size);
                    writer.Write(byteArray.data);
                    if (valueField.templateField.align) writer.Align();
                }
                else
                {
                    AssetTypeArray array = valueField.value.value.asArray;

                    array.size = valueField.childrenCount;
                    writer.Write(array.size);
                    for (int i = 0; i < array.size; i++)
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
                        case EnumValueTypes.Int8:
                            writer.Write(valueField.value.value.asInt8);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.UInt8:
                            writer.Write(valueField.value.value.asUInt8);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.Bool:
                            writer.Write(valueField.value.value.asBool);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.Int16:
                            writer.Write(valueField.value.value.asInt16);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.UInt16:
                            writer.Write(valueField.value.value.asUInt16);
                            if (valueField.templateField.align) writer.Align();
                            break;
                        case EnumValueTypes.Int32:
                            writer.Write(valueField.value.value.asInt32);
                            break;
                        case EnumValueTypes.UInt32:
                            writer.Write(valueField.value.value.asUInt32);
                            break;
                        case EnumValueTypes.Int64:
                            writer.Write(valueField.value.value.asInt64);
                            break;
                        case EnumValueTypes.UInt64:
                            writer.Write(valueField.value.value.asUInt64);
                            break;
                        case EnumValueTypes.Float:
                            writer.Write(valueField.value.value.asFloat);
                            break;
                        case EnumValueTypes.Double:
                            writer.Write(valueField.value.value.asDouble);
                            break;
                        case EnumValueTypes.String:
                            writer.Write(valueField.value.value.asString.Length);
                            writer.Write(valueField.value.value.asString);
                            writer.Align();
                            break;
                    }
                }
                else
                {
                    for (int i = 0; i < valueField.childrenCount; i++)
                    {
                        valueField[i].Write(writer, depth + 1);
                    }
                    if (valueField.templateField.align) writer.Align();
                }
            }
        }
        public static byte[] WriteToByteArray(this AssetTypeValueField valueField, bool bigEndian = false)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                w.bigEndian = bigEndian;
                valueField.Write(w);
                data = ms.ToArray();
            }
            return data;
        }
    }
}
