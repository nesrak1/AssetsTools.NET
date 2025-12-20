using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AssetsTools.NET
{
    public class AssetTypeTemplateField
    {
        /// <summary>
        /// Name of the field.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type name of the field.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Type of the field (as an enum).
        /// </summary>
        public AssetValueType ValueType { get; set; }
        /// <summary>
        /// Is the field an array?
        /// </summary>
        public bool IsArray { get; set; }
        /// <summary>
        /// Is the field aligned? This aligns four bytes after all children have been read/written.
        /// </summary>
        public bool IsAligned { get; set; }
        /// <summary>
        /// Does the field have value? (i.e. is the field a numeric / string / array type?)
        /// </summary>
        public bool HasValue { get; set; }
        /// <summary>
        /// Version of the field. This value is updated when the type changes across engine versions.
        /// </summary>
        public ushort Version { get; set; }
        /// <summary>
        /// Children of the field.
        /// </summary>
        public List<AssetTypeTemplateField> Children { get; set; }

        /// <summary>
        /// Read the template field from a type tree type.
        /// </summary>
        /// <param name="typeTreeType">The type tree type to read from.</param>
        public void FromTypeTree(TypeTreeType typeTreeType)
        {
            int fieldIndex = 0;
            FromTypeTree(typeTreeType, ref fieldIndex);
        }

        private void FromTypeTree(TypeTreeType typeTreeType, ref int fieldIndex)
        {
            TypeTreeNode field = typeTreeType.Nodes[fieldIndex];
            Name = field.GetNameString(typeTreeType.StringBufferBytes);
            Type = field.GetTypeString(typeTreeType.StringBufferBytes);
            ValueType = AssetTypeValueField.GetValueTypeByTypeName(Type);
            IsArray = Net35Polyfill.HasFlag(field.TypeFlags, TypeTreeNodeFlags.Array);
            IsAligned = (field.MetaFlags & 0x4000) != 0;
            HasValue = ValueType != AssetValueType.None;
            Version = field.Version;

            Children = new List<AssetTypeTemplateField>();

            for (fieldIndex++; fieldIndex < typeTreeType.Nodes.Count; fieldIndex++)
            {
                TypeTreeNode typeTreeField = typeTreeType.Nodes[fieldIndex];
                if (typeTreeField.Level <= field.Level)
                {
                    fieldIndex--;
                    break;
                }

                AssetTypeTemplateField assetField = new AssetTypeTemplateField();
                assetField.FromTypeTree(typeTreeType, ref fieldIndex);
                Children.Add(assetField);
            }

            // there can be a case where string child is not an array but an int
            // (ExposedReferenceTable field in PlayableDirector class before 2018.4.25)
            if (ValueType == AssetValueType.String && !Children[0].IsArray && Children[0].ValueType != AssetValueType.None)
            {
                Type = "_string";
                ValueType = AssetValueType.None;
            }

            if (IsArray)
            {
                ValueType = Children[1].ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array;
            }

            Children.TrimExcess();
        }

        /// <summary>
        /// Read the template field from a class database type.
        /// </summary>
        /// <param name="cldbFile">The class database file to read from.</param>
        /// <param name="cldbType">The class database type to read.</param>
        /// <param name="preferEditor">Read from the editor version of this type if available?</param>
        public void FromClassDatabase(ClassDatabaseFile cldbFile, ClassDatabaseType cldbType, bool preferEditor = false)
        {
            if (cldbType.EditorRootNode == null && cldbType.ReleaseRootNode == null)
                throw new Exception("No root nodes were found!");

            ClassDatabaseTypeNode node = cldbType.GetPreferredNode(preferEditor);

            FromClassDatabase(cldbFile.StringTable, node);
        }

        private void FromClassDatabase(ClassDatabaseStringTable strTable, ClassDatabaseTypeNode node)
        {
            Name = strTable.GetString(node.FieldName);
            Type = strTable.GetString(node.TypeName);

            // temporary hack for tpk
            if (Type == "SInt32")
                Type = "int";
            else if (Type == "UInt32")
                Type = "unsigned int";

            ValueType = AssetTypeValueField.GetValueTypeByTypeName(Type);
            IsArray = node.TypeFlags == 1;
            IsAligned = (node.MetaFlag & 0x4000) != 0;
            HasValue = ValueType != AssetValueType.None;
            Version = node.Version;

            Children = new List<AssetTypeTemplateField>(node.Children.Count);
            foreach (ClassDatabaseTypeNode childNode in node.Children)
            {
                AssetTypeTemplateField childField = new AssetTypeTemplateField();
                childField.FromClassDatabase(strTable, childNode);
                Children.Add(childField);
            }

            // there can be a case where string child is not an array but an int
            // (ExposedReferenceTable field in PlayableDirector class before 2018.4.25)
            if (ValueType == AssetValueType.String && !Children[0].IsArray && Children[0].ValueType != AssetValueType.None)
            {
                Type = "_string";
                ValueType = AssetValueType.None;
            }

            if (IsArray)
            {
                ValueType = Children[1].ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array;
            }
        }

        /// <summary>
        /// Deserialize an asset into a value field.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        /// <returns>The deserialized base field.</returns>
        public AssetTypeValueField MakeValue(AssetsFileReader reader, RefTypeManager refMan = null)
        {
            AssetTypeValueField valueField = new AssetTypeValueField
            {
                TemplateField = this
            };
            valueField = ReadType(reader, valueField, refMan);
            return valueField;
        }

        /// <summary>
        /// Deserialize an asset into a value field.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="position">The position to start reading from.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        /// <returns>The deserialized value field.</returns>
        public AssetTypeValueField MakeValue(AssetsFileReader reader, long position, RefTypeManager refMan = null)
        {
            reader.Position = position;
            return MakeValue(reader, refMan);
        }

        /// <summary>
        /// Deserialize a single field and its children.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="valueField">The empty base value field to use.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        /// <returns>The deserialized base field.</returns>
        public AssetTypeValueField ReadType(AssetsFileReader reader, AssetTypeValueField valueField, RefTypeManager refMan)
        {
            if (valueField.TemplateField.IsArray)
            {
                int arrayChildCount = valueField.TemplateField.Children.Count;
                if (arrayChildCount != 2)
                    throw new Exception($"Expected array to have two children, found {arrayChildCount} instead!");

                AssetValueType sizeType = valueField.TemplateField.Children[0].ValueType;

                if (sizeType != AssetValueType.Int32 && sizeType != AssetValueType.UInt32)
                    throw new Exception($"Expected int array size type, found {sizeType} instead!");

                if (valueField.TemplateField.ValueType == AssetValueType.ByteArray)
                {
                    valueField.Children = new List<AssetTypeValueField>(0);

                    int size = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(size);

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();

                    valueField.Value = new AssetTypeValue(AssetValueType.ByteArray, data);
                }
                else
                {
                    int size = reader.ReadInt32();
                    valueField.Children = new List<AssetTypeValueField>(size);

                    for (int i = 0; i < size; i++)
                    {
                        AssetTypeValueField childField = new AssetTypeValueField();
                        childField.TemplateField = valueField.TemplateField.Children[1];
                        valueField.Children.Add(ReadType(reader, childField, refMan));
                    }

                    valueField.Children.TrimExcess();

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();

                    AssetTypeArrayInfo arrayTypeInfo = new AssetTypeArrayInfo
                    {
                        size = size
                    };

                    valueField.Value = new AssetTypeValue(AssetValueType.Array, arrayTypeInfo);
                }
            }
            else
            {
                AssetValueType type = valueField.TemplateField.ValueType;
                if (type == AssetValueType.None)
                {
                    int childCount = valueField.TemplateField.Children.Count;
                    valueField.Children = new List<AssetTypeValueField>(childCount);
                    for (int i = 0; i < childCount; i++)
                    {
                        AssetTypeValueField childField = new AssetTypeValueField();
                        childField.TemplateField = valueField.TemplateField.Children[i];
                        valueField.Children.Add(ReadType(reader, childField, refMan));
                    }
                    valueField.Children.TrimExcess();
                    valueField.Value = null;

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();
                }
                else
                {
                    ReadPrimitiveType(reader, valueField, type, refMan);
                }

            }
            return valueField;
        }

        /// <summary>
        /// Deserialize a single primtive field and its children.
        /// This method only works for strings, numbers, and ManagedReferencesRegistry.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="valueField">The empty base value field to use.</param>
        /// <param name="type">The value type of the template field.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        /// <returns>The deserialized base field.</returns>
#if NETSTANDARD2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ReadPrimitiveType(AssetsFileReader reader, AssetTypeValueField valueField, AssetValueType type, RefTypeManager refMan)
        {
            if (type == AssetValueType.String)
            {
                valueField.Children = new List<AssetTypeValueField>(0);
                int length = reader.ReadInt32();
                valueField.Value = new AssetTypeValue(reader.ReadBytes(length), true);
                reader.Align();
            }
            else if (type == AssetValueType.ManagedReferencesRegistry)
            {
                ReadManagedReferencesRegistryType(reader, valueField, refMan);
            }
            else
            {
                int childCount = valueField.TemplateField.Children.Count;
                if (childCount == 0)
                {
                    valueField.Children = new List<AssetTypeValueField>(0);
                    switch (type)
                    {
                        case AssetValueType.Int8:
                            valueField.Value = new AssetTypeValue(reader.ReadSByte());
                            break;
                        case AssetValueType.UInt8:
                            valueField.Value = new AssetTypeValue(reader.ReadByte());
                            break;
                        case AssetValueType.Bool:
                            valueField.Value = new AssetTypeValue(reader.ReadBoolean());
                            break;
                        case AssetValueType.Int16:
                            valueField.Value = new AssetTypeValue(reader.ReadInt16());
                            break;
                        case AssetValueType.UInt16:
                            valueField.Value = new AssetTypeValue(reader.ReadUInt16());
                            break;
                        case AssetValueType.Int32:
                            valueField.Value = new AssetTypeValue(reader.ReadInt32());
                            break;
                        case AssetValueType.UInt32:
                            valueField.Value = new AssetTypeValue(reader.ReadUInt32());
                            break;
                        case AssetValueType.Int64:
                            valueField.Value = new AssetTypeValue(reader.ReadInt64());
                            break;
                        case AssetValueType.UInt64:
                            valueField.Value = new AssetTypeValue(reader.ReadUInt64());
                            break;
                        case AssetValueType.Float:
                            valueField.Value = new AssetTypeValue(reader.ReadSingle());
                            break;
                        case AssetValueType.Double:
                            valueField.Value = new AssetTypeValue(reader.ReadDouble());
                            break;
                    }

                    if (valueField.TemplateField.IsAligned)
                        reader.Align();
                }
                else if (type != AssetValueType.None)
                {
                    throw new Exception("Cannot read value of field with children!");
                }
            }
        }

        /// <summary>
        /// Deserialize a ManagedReferencesRegistry field.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="valueField">The empty base value field to use.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        /// <returns>The deserialized base field.</returns>
        public void ReadManagedReferencesRegistryType(AssetsFileReader reader, AssetTypeValueField valueField, RefTypeManager refMan)
        {
            if (refMan == null)
                throw new Exception($"{nameof(refMan)} must be non-null to deserialize objects with ref types.");

            valueField.Children = new List<AssetTypeValueField>(0);
            ManagedReferencesRegistry registry = new ManagedReferencesRegistry();
            valueField.Value = new AssetTypeValue(registry);
            int registryChildCount = valueField.TemplateField.Children.Count;
            if (registryChildCount != 2)
                throw new Exception($"Expected ManagedReferencesRegistry to have two children, found {registryChildCount} instead!");

            registry.version = reader.ReadInt32();
            registry.references = new List<AssetTypeReferencedObject>();

            if (registry.version == 1)
            {
                while (true)
                {
                    // rid is consecutive starting at 0
                    var refdObject = MakeReferencedObject(reader, registry.version, registry.references.Count, refMan);
                    if (refdObject.type.Equals(AssetTypeReference.TERMINUS))
                    {
                        break;
                    }
                    registry.references.Add(refdObject);
                }
            }
            else
            {
                int childCount = reader.ReadInt32();
                for (int i = 0; i < childCount; i++)
                {
                    // rid is read from data
                    var refdObject = MakeReferencedObject(reader, registry.version, -1, refMan);
                    registry.references.Add(refdObject);
                }
            }
        }

        public AssetTypeTemplateField this[string name]
        {
            get
            {
                if (name.Contains("."))
                {
                    string[] splitNames = name.Split('.');
                    AssetTypeTemplateField field = this;
                    foreach (string splitName in splitNames)
                    {
                        bool foundChild = false;

                        foreach (AssetTypeTemplateField child in field.Children)
                        {
                            if (child.Name == splitName)
                            {
                                foundChild = true;
                                field = child;
                                break;
                            }
                        }

                        if (!foundChild)
                        {
                            return null;
                        }
                    }
                    return field;
                }
                else
                {
                    foreach (AssetTypeTemplateField child in Children)
                    {
                        if (child.Name == name)
                        {
                            return child;
                        }
                    }
                    return null;
                }
            }
        }

        public AssetTypeTemplateField this[int index]
        {
            get
            {
                return Children[index];
            }
        }

        /// <summary>
        /// Perform a deep clone of the <see cref="AssetTypeTemplateField"/>.
        /// </summary>
        /// <returns>The cloned field.</returns>
        public AssetTypeTemplateField Clone()
        {
            return new AssetTypeTemplateField
            {
                Name = Name,
                Type = Type,
                ValueType = ValueType,
                IsArray = IsArray,
                IsAligned = IsAligned,
                HasValue = HasValue,
                Children = Children.Select(c => c.Clone()).ToList()
            };
        }

        private AssetTypeReferencedObject MakeReferencedObject(AssetsFileReader reader, int registryVersion, int referenceIndex, RefTypeManager refMan)
        {
            AssetTypeReferencedObject refdObject = new AssetTypeReferencedObject();

            if (registryVersion == 1)
            {
                refdObject.rid = referenceIndex;
            }
            else
            {
                refdObject.rid = reader.ReadInt64();
            }

            AssetTypeReference refType = new AssetTypeReference();
            refType.ReadAsset(reader);
            refdObject.type = refType;

            AssetTypeTemplateField objectTempField = refMan.GetTemplateField(refType);
            if (objectTempField != null)
            {
                AssetTypeValueField tempField = new AssetTypeValueField()
                {
                    TemplateField = objectTempField
                };
                refdObject.data = ReadType(reader, tempField, refMan);
            }
            else
            {
                refdObject.data = AssetTypeValueField.DUMMY_FIELD;
            }

            return refdObject;
        }

        public override string ToString()
        {
            return Type + " " + Name;
        }
    }
}
