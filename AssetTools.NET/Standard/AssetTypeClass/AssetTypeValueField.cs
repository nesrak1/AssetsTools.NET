using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetTypeValueField : IEnumerable<AssetTypeValueField>
    {
        /// <summary>
        /// Template field corresponding to this value field.
        /// </summary>
        public AssetTypeTemplateField TemplateField { get; set; }
        /// <summary>
        /// Value of this field.
        /// </summary>
        public AssetTypeValue Value { get; set; }
        /// <summary>
        /// Children of this field.
        /// </summary>
        public List<AssetTypeValueField> Children { get; set; }

        public bool IsDummy { get; set; }

        public static readonly AssetTypeValueField DUMMY_FIELD = new AssetTypeValueField()
        {
            TemplateField = new AssetTypeTemplateField
            {
                Name = "DUMMY",
                HasValue = false,
                IsAligned = false,
                IsArray = false,
                Type = "DUMMY",
                ValueType = AssetValueType.None,
                Children = new List<AssetTypeTemplateField>(0)
            },
            Value = null,
            IsDummy = true,
            Children = new List<AssetTypeValueField>(0)
        };

        public void Read(AssetTypeValue value, AssetTypeTemplateField templateField, List<AssetTypeValueField> children)
        {
            Value = value;
            TemplateField = templateField;
            Children = children;
            IsDummy = false;
        }

        public AssetTypeValueField this[string name]
        {
            get
            {
                if (IsDummy)
                {
                    throw new DummyFieldAccessException("Cannot access fields of a dummy field!");
                }

                if (name.Contains("."))
                {
                    string[] splitNames = name.Split('.');
                    AssetTypeValueField field = this;
                    foreach (string splitName in splitNames)
                    {
                        bool foundChild = false;

                        foreach (AssetTypeValueField child in field.Children)
                        {
                            if (child.TemplateField.Name == splitName)
                            {
                                foundChild = true;
                                field = child;
                                break;
                            }
                        }

                        if (!foundChild)
                        {
                            return DUMMY_FIELD;
                        }
                    }
                    return field;
                }
                else
                {
                    foreach (AssetTypeValueField child in Children)
                    {
                        if (child.TemplateField.Name == name)
                        {
                            return child;
                        }
                    }
                    return DUMMY_FIELD;
                }
            }
        }

        public AssetTypeValueField this[int index]
        {
            get
            {
                if (IsDummy)
                {
                    throw new DummyFieldAccessException("Cannot access fields of a dummy field!");
                }

                return Children[index];
            }
        }

        public AssetTypeValueField Get(string name) => this[name];
        public AssetTypeValueField Get(int index) => this[index];

        public static AssetValueType GetValueTypeByTypeName(string type)
        {
            switch (type)
            {
                case "string":
                    return AssetValueType.String;
                case "SInt8":
                case "char":
                    return AssetValueType.Int8;
                case "UInt8":
                case "unsigned char":
                    return AssetValueType.UInt8;
                case "SInt16":
                case "short":
                    return AssetValueType.Int16;
                case "UInt16":
                case "unsigned short":
                    return AssetValueType.UInt16;
                case "SInt32":
                case "int":
                case "Type*":
                    return AssetValueType.Int32;
                case "UInt32":
                case "unsigned int":
                    return AssetValueType.UInt32;
                case "SInt64":
                case "long":
                    return AssetValueType.Int64;
                case "UInt64":
                case "unsigned long long":
                case "FileSize":
                    return AssetValueType.UInt64;
                case "float":
                    return AssetValueType.Float;
                case "double":
                    return AssetValueType.Double;
                case "bool":
                    return AssetValueType.Bool;
                case "Array":
                    return AssetValueType.Array;
                case "TypelessData":
                    return AssetValueType.ByteArray;
                case "ManagedReferencesRegistry":
                    return AssetValueType.ManagedReferencesRegistry;
                default:
                    return AssetValueType.None;
            }
        }

        public void Write(AssetsFileWriter writer, int depth = 0)
        {
            if (TemplateField.IsArray)
            {
                if (TemplateField.ValueType == AssetValueType.ByteArray)
                {
                    byte[] byteArray = AsByteArray;

                    writer.Write(byteArray.Length);
                    writer.Write(byteArray);

                    if (TemplateField.IsAligned)
                    {
                        writer.Align();
                    }
                }
                else
                {
                    int arraySize = Children.Count;

                    writer.Write(arraySize);
                    for (int i = 0; i < arraySize; i++)
                    {
                        this[i].Write(writer, depth + 1);
                    }

                    if (TemplateField.IsAligned)
                    {
                        writer.Align();
                    }
                }
            }
            else
            {
                if (Children.Count == 0)
                {
                    switch (TemplateField.ValueType)
                    {
                        case AssetValueType.Int8:
                            writer.Write(AsSByte);

                            if (TemplateField.IsAligned)
                            {
                                writer.Align();
                            }
                            break;
                        case AssetValueType.UInt8:
                            writer.Write(AsByte);

                            if (TemplateField.IsAligned)
                            {
                                writer.Align();
                            }
                            break;
                        case AssetValueType.Bool:
                            writer.Write(AsBool);

                            if (TemplateField.IsAligned)
                            {
                                writer.Align();
                            }
                            break;
                        case AssetValueType.Int16:
                            writer.Write(AsShort);

                            if (TemplateField.IsAligned)
                            {
                                writer.Align();
                            }
                            break;
                        case AssetValueType.UInt16:
                            writer.Write(AsUShort);

                            if (TemplateField.IsAligned)
                            {
                                writer.Align();
                            }
                            break;
                        case AssetValueType.Int32:
                            writer.Write(AsInt);
                            break;
                        case AssetValueType.UInt32:
                            writer.Write(AsUInt);
                            break;
                        case AssetValueType.Int64:
                            writer.Write(AsLong);
                            break;
                        case AssetValueType.UInt64:
                            writer.Write(AsULong);
                            break;
                        case AssetValueType.Float:
                            writer.Write(AsFloat);
                            break;
                        case AssetValueType.Double:
                            writer.Write(AsDouble);
                            break;
                        case AssetValueType.String:
                            writer.Write(AsByteArray.Length);
                            writer.Write(AsByteArray);
                            writer.Align();
                            break;
                        case AssetValueType.ManagedReferencesRegistry:
                            writer.Write(AsManagedReferencesRegistry.version);
                            int childCount = AsManagedReferencesRegistry.references.Count;
                            
                            for (int i = 0; i < childCount; i++)
                            {
                                AssetTypeReferencedObject refdObject = AsManagedReferencesRegistry.references[i];
                                if (AsManagedReferencesRegistry.version != 1)
                                {
                                    writer.Write(refdObject.rid);
                                }
                                refdObject.type.WriteAsset(writer);
                                refdObject.data.Write(writer);
                            }
                            if (AsManagedReferencesRegistry.version == 1)
                            {
                                AssetTypeReference.TERMINUS.WriteAsset(writer);
                            }
                            break;
                    }
                }
                else
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        this[i].Write(writer, depth + 1);
                    }

                    if (TemplateField.IsAligned)
                    {
                        writer.Align();
                    }
                }
            }
        }

        public byte[] WriteToByteArray(bool bigEndian = false)
        {
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                w.BigEndian = bigEndian;
                Write(w);
                data = ms.ToArray();
            }
            return data;
        }

        public IEnumerator<AssetTypeValueField> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        // for convenience
        public bool AsBool { get => Value.AsBool; set => Value.AsBool = value; }
        public sbyte AsSByte { get => Value.AsSByte; set => Value.AsSByte = value; }
        public byte AsByte { get => Value.AsByte; set => Value.AsByte = value; }
        public short AsShort { get => Value.AsShort; set => Value.AsShort = value; }
        public ushort AsUShort { get => Value.AsUShort; set => Value.AsUShort = value; }
        public int AsInt { get => Value.AsInt; set => Value.AsInt = value; }
        public uint AsUInt { get => Value.AsUInt; set => Value.AsUInt = value; }
        public long AsLong { get => Value.AsLong; set => Value.AsLong = value; }
        public ulong AsULong { get => Value.AsULong; set => Value.AsULong = value; }
        public float AsFloat { get => Value.AsFloat; set => Value.AsFloat = value; }
        public double AsDouble { get => Value.AsDouble; set => Value.AsDouble = value; }
        public string AsString { get => Value.AsString; set => Value.AsString = value; }
        public object AsObject { get => Value.AsObject; set => Value.AsObject = value; }
        public AssetTypeArrayInfo AsArray { get => Value.AsArray; set => Value.AsArray = value; }
        public byte[] AsByteArray { get => Value.AsByteArray; set => Value.AsByteArray = value; }
        public ManagedReferencesRegistry AsManagedReferencesRegistry { get => Value.AsManagedReferencesRegistry; set => Value.AsManagedReferencesRegistry = value; }

        public string TypeName { get => TemplateField.Type; }
        public string FieldName { get => TemplateField.Name; }
    }
}
