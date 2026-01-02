using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class AssetTypeValueIterator
    {
        private AssetTypeTemplateField baseTempField;
        private AssetsFileReader reader;
        private RefTypeManager refMan;
        private readonly Stack<IEnumerator<AssetTypeTemplateField>> tempFieldStack;

        private AssetTypeValueField valueFieldCache;
        private readonly long basePosition;

        public AssetTypeTemplateField TempField;

        public AssetTypeValueField ReadValueField()
        {
            if (valueFieldCache == null)
            {
                valueFieldCache = new AssetTypeValueField()
                {
                    TemplateField = TempField
                };

                // rewind since we already read the length
                if (TempField.IsArray && TempField.ValueType != AssetValueType.ByteArray)
                {
                    reader.Position -= 4;
                }

                TempField.ReadType(reader, valueFieldCache, refMan);

                // prevent iterating the children now that we've read it
                if (TempField.Children.Count > 0 && TempField.ValueType != AssetValueType.String)
                {
                    IEnumerator<AssetTypeTemplateField> topItem = tempFieldStack.Peek();
                    do
                    {
                        // special case for arrays: we need to align here rather than below
                        // because we create child iterators for arrays rather than depend
                        // on the template field.
                        if (topItem.Current != null
                            && topItem.Current.IsArray
                            && topItem.Current.ValueType != AssetValueType.ByteArray
                            && topItem.Current.IsAligned)
                        {
                            reader.Align();
                        }
                    } while (topItem.MoveNext());
                }
            }
            return valueFieldCache;
        }

        public List<AssetTypeTemplateField> TempFieldStack
        {
            get
            {
                List<AssetTypeTemplateField> stack = new List<AssetTypeTemplateField>(tempFieldStack.Count);
                foreach (IEnumerator<AssetTypeTemplateField> stackItem in tempFieldStack)
                {
                    stack.Add(stackItem.Current);
                }

                return stack;
            }
        }

        public int Depth
        {
            get
            {
                bool canHaveChildren = TempField.ValueType switch
                {
                    AssetValueType.None => true,
                    AssetValueType.Bool => false,
                    AssetValueType.Int8 => false,
                    AssetValueType.UInt8 => false,
                    AssetValueType.Int16 => false,
                    AssetValueType.UInt16 => false,
                    AssetValueType.Int32 => false,
                    AssetValueType.UInt32 => false,
                    AssetValueType.Int64 => false,
                    AssetValueType.UInt64 => false,
                    AssetValueType.Float => false,
                    AssetValueType.Double => false,
                    AssetValueType.String => false,
                    AssetValueType.Array => true,
                    AssetValueType.ByteArray => false,
                    AssetValueType.ManagedReferencesRegistry => false,
                    _ => throw new Exception("Invalid value type")
                };
                return tempFieldStack.Count - (canHaveChildren ? 1 : 0);
            }
        }

        public int ReadPosition
        {
            get
            {
                return (int)(reader.Position - basePosition);
            }
        }

        /// <summary>
        /// Create an AssetTypeValueIterator, which allows walking an asset's field tree without
        /// allocating unnecessary values. If <paramref name="reader"/> is locked behind a lock, you
        /// should manually take the lock while using the iterator. The current iterating position
        /// is not remembered by this iterator. Instead, the reader's position is relied on.
        /// </summary>
        /// <param name="templateField">The template base field to use.</param>
        /// <param name="reader">The reader to use, set the correct position.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        public AssetTypeValueIterator(AssetTypeTemplateField templateField, AssetsFileReader reader, RefTypeManager refMan = null)
        {
            baseTempField = templateField;
            this.reader = reader;
            this.refMan = refMan;

            basePosition = reader.Position;

            tempFieldStack = new Stack<IEnumerator<AssetTypeTemplateField>>();
            Reset();
        }

        /// <summary>
        /// Create an AssetTypeValueIterator, which allows walking an asset's field tree without
        /// allocating unnecessary values. If <paramref name="reader"/> is locked behind a lock, you
        /// should manually take the lock while using the iterator. The current iterating position
        /// is not remembered by this iterator. Instead, the reader's position is relied on.
        /// </summary>
        /// <param name="templateField">The template base field to use.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="position">The position to start reading from.</param>
        /// <param name="refMan">The ref type manager to use, if reading a MonoBehaviour using a ref type.</param>
        public AssetTypeValueIterator(AssetTypeTemplateField templateField, AssetsFileReader reader, long position, RefTypeManager refMan = null)
            : this(templateField, ReaderWithSetPos(reader, position), refMan)
        {
        }

        private static AssetsFileReader ReaderWithSetPos(AssetsFileReader reader, long position)
        {
            reader.Position = position;
            return reader;
        }

        public void Reset()
        {
            tempFieldStack.Clear();
            tempFieldStack.Push(baseTempField.Children.GetEnumerator());

            TempField = baseTempField;
            valueFieldCache = null;
        }

        public void Reset(AssetTypeTemplateField templateField, AssetsFileReader reader, RefTypeManager refMan = null)
        {
            baseTempField = templateField;
            this.reader = reader;
            this.refMan = refMan;

            tempFieldStack.Clear();
            tempFieldStack.Push(baseTempField.Children.GetEnumerator());

            TempField = baseTempField;
            valueFieldCache = null;
        }

        public bool ReadNext()
        {
            if (tempFieldStack.Count == 0)
            {
                return false;
            }

            // move reader to next field if we didn't read it
            if (valueFieldCache == null)
            {
                if (TempField.IsArray)
                {
                    if (TempField.ValueType == AssetValueType.ByteArray)
                    {
                        int byteLen = reader.ReadInt32();
                        reader.Position += byteLen;

                        if (TempField.IsAligned)
                        {
                            reader.Align();
                        }
                    }
                }
                else
                {
                    if (TempField.ValueType != AssetValueType.None)
                    {
                        bool aligned = TempField.IsAligned;
                        switch (TempField.ValueType)
                        {
                            case AssetValueType.Bool:
                            case AssetValueType.Int8:
                            case AssetValueType.UInt8:
                                reader.Position += 1;
                                break;
                            case AssetValueType.Int16:
                            case AssetValueType.UInt16:
                                reader.Position += 2;
                                break;
                            case AssetValueType.Int32:
                            case AssetValueType.UInt32:
                            case AssetValueType.Float:
                                reader.Position += 4;
                                break;
                            case AssetValueType.Int64:
                            case AssetValueType.UInt64:
                            case AssetValueType.Double:
                                reader.Position += 8;
                                break;
                            case AssetValueType.String:
                                int stringLen = reader.ReadInt32();
                                reader.Position += stringLen;

                                if (TempField.Children.Count > 0)
                                    aligned = TempField.Children[0].IsAligned;
                                else
                                    aligned = true;

                                break;
                            case AssetValueType.ManagedReferencesRegistry:
                                // this allocates, but I'm skipping writing code to do this the "right" way
                                AssetTypeValueField ignored = new AssetTypeValueField();
                                TempField.ReadManagedReferencesRegistryType(reader, ignored, refMan);
                                break;
                        }

                        if (aligned)
                        {
                            reader.Align();
                        }
                    }
                }
            }

            IEnumerator<AssetTypeTemplateField> topField = tempFieldStack.Peek();
            while (true)
            {
                // special case for arrays: we need to align here rather than below
                // because we create child iterators for arrays rather than depend
                // on the template field.
                if (topField.Current != null
                    && topField.Current.IsArray
                    && topField.Current.ValueType != AssetValueType.ByteArray
                    && topField.Current.IsAligned)
                {
                    reader.Align();
                }

                if (topField.MoveNext())
                {
                    break;
                }

                tempFieldStack.Pop();
                if (tempFieldStack.Count == 0)
                {
                    return false;
                }

                topField = tempFieldStack.Peek();
            }

            AssetTypeTemplateField childField = topField.Current;
            if (childField.IsArray && childField.ValueType != AssetValueType.ByteArray)
            {
                int arrChildCount = reader.ReadInt32();
                tempFieldStack.Push(GetArrayEnumerator(childField.Children[1], arrChildCount));
            }
            else if (childField.ValueType == AssetValueType.None)
            {
                if (childField.Children.Count > 0)
                {
                    tempFieldStack.Push(childField.Children.GetEnumerator());
                }
            }

            TempField = childField;
            valueFieldCache = null;
            return true;
        }

        private IEnumerator<AssetTypeTemplateField> GetArrayEnumerator(AssetTypeTemplateField templateField, int times)
        {
            for (int i = 0; i < times; i++)
            {
                yield return templateField;
            }
        }
    }
}
