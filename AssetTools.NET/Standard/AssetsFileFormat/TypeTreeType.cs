﻿using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class TypeTreeType
    {
        /// <summary>
        /// ID for this type.
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// Marks whether the type is stripped or not. Stripped types do not have any fields.
        /// </summary>
        public bool IsStrippedType { get; set; }
        /// <summary>
        /// Script index for this type. Only used in MonoBehaviours, and MonoBehaviours of the same
        /// script have the same index.
        /// </summary>
        public ushort ScriptTypeIndex { get; set; }
        /// <summary>
        /// Hash of the script's fields. Two different scripts with the same fields can have the same hash.
        /// </summary>
        public Hash128 ScriptIdHash { get; set; }
        /// <summary>
        /// Hash of the type's fields.
        /// </summary>
        public Hash128 TypeHash { get; set; }
        /// <summary>
        /// Nodes for this type. This list will be empty if the type is stripped.
        /// </summary>
        public List<TypeTreeNode> Nodes { get; set; }
        /// <summary>
        /// String table bytes for this type.
        /// </summary>
        public byte[] StringBufferBytes { get; set; }
        /// <summary>
        /// Is the type a reference type?
        /// </summary>
        public bool IsRefType { get; set; }
        /// <summary>
        /// Type dependencies for this type. Used by MonoBehaviours referencing ref types. Only used
        /// when IsRefType is false.
        /// </summary>
        public int[] TypeDependencies { get; set; }
        /// <summary>
        /// Type reference information. Only used when IsRefType is true.
        /// </summary>
        public AssetTypeReference TypeReference { get; set; }

        public string StringBuffer
        {
            get => StringBufferBytes != null ? Encoding.UTF8.GetString(StringBufferBytes) : null;
            set => StringBufferBytes = Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Read the <see cref="TypeTreeType"/> with the provided reader and format version.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="version">The version of the file.</param>
        /// <param name="hasTypeTree">Is type tree enabled for this file?</param>
        /// <param name="isRefType">Is this type part of the ref type list?</param>
        public void Read(AssetsFileReader reader, uint version, bool hasTypeTree, bool isRefType)
        {
            TypeId = reader.ReadInt32();
            if (version >= 16)
            {
                IsStrippedType = reader.ReadBoolean();
            }

            if (version >= 17)
            {
                ScriptTypeIndex = reader.ReadUInt16();
            }
            else
            {
                ScriptTypeIndex = 0xffff;
            }

            if ((version < 17 && TypeId < 0) ||
                (version >= 17 && TypeId == (int)AssetClassID.MonoBehaviour) ||
                (isRefType && ScriptTypeIndex != 0xffff))
            {
                ScriptIdHash = new Hash128(reader);
            }

            TypeHash = new Hash128(reader);
            IsRefType = isRefType;

            if (hasTypeTree)
            {
                int typeTreeNodeCount = reader.ReadInt32();
                int stringBufferLen = reader.ReadInt32();
                Nodes = new List<TypeTreeNode>(typeTreeNodeCount);
                for (int i = 0; i < typeTreeNodeCount; i++)
                {
                    TypeTreeNode typeField = new TypeTreeNode();
                    typeField.Read(reader, version);
                    Nodes.Add(typeField);
                }
                StringBufferBytes = reader.ReadBytes(stringBufferLen);
                if (version >= 21)
                {
                    if (!isRefType)
                    {
                        int dependenciesCount = reader.ReadInt32();
                        TypeDependencies = new int[dependenciesCount];
                        for (int i = 0; i < dependenciesCount; i++)
                        {
                            TypeDependencies[i] = reader.ReadInt32();
                        }
                    }
                    else
                    {
                        TypeReference = new AssetTypeReference();
                        TypeReference.ReadMetadata(reader);
                    }
                }
            }
        }

        /// <summary>
        /// Write the <see cref="TypeTreeType"/> with the provided writer and format version.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="version">The version of the file.</param>
        /// <param name="hasTypeTree">Is type tree enabled for this file?</param>
        public void Write(AssetsFileWriter writer, uint version, bool hasTypeTree)
        {
            writer.Write(TypeId);
            if (version >= 16)
                writer.Write(IsStrippedType);

            if (version >= 17)
                writer.Write(ScriptTypeIndex);

            if ((version < 17 && TypeId < 0) ||
                (version >= 17 && TypeId == (int)AssetClassID.MonoBehaviour) ||
                (IsRefType && ScriptTypeIndex != 0xffff))
            {
                writer.Write(ScriptIdHash.data);
            }

            writer.Write(TypeHash.data);

            if (hasTypeTree)
            {
                writer.Write(Nodes.Count);
                writer.Write(StringBufferBytes.Length);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    Nodes[i].Write(writer, version);
                }
                writer.Write(StringBufferBytes);
                if (version >= 21)
                {
                    if (!IsRefType)
                    {
                        writer.Write(TypeDependencies.Length);
                        for (int i = 0; i < TypeDependencies.Length; i++)
                        {
                            writer.Write(TypeDependencies[i]);
                        }
                    }
                    else
                    {
                        TypeReference.WriteMetadata(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Get the maximum size of this type tree type.
        /// </summary>
        /// <param name="version">The version of the file.</param>
        /// <param name="hasTypeTree">Is type tree enabled for this file?</param>
        public long GetSize(uint version, bool hasTypeTree)
        {
            long size = 0;
            size += 4;
            if (version >= 16)
                size += 1;

            if (version >= 17)
                size += 2;

            if ((version < 17 && TypeId < 0) ||
                (version >= 17 && TypeId == (int)AssetClassID.MonoBehaviour) ||
                (IsRefType && ScriptTypeIndex != 0xffff))
            {
                size += 16;
            }

            size += 16;

            if (hasTypeTree)
            {
                size += 4;
                size += 4;
                size += TypeTreeNode.GetSize(version) * Nodes.Count;
                size += StringBufferBytes.Length;
                if (version >= 21)
                {
                    if (!IsRefType)
                    {
                        size += 4;
                        size += TypeDependencies.Length * 4;
                    }
                    else
                    {
                        size += TypeReference.ClassName.Length + 1;
                        size += TypeReference.Namespace.Length + 1;
                        size += TypeReference.AsmName.Length + 1;
                    }
                }
            }

            return size;
        }

        /// <summary>
        /// The string table used for commonly occuring strings in type trees.
        /// </summary>
        public static readonly byte[] COMMON_STRING_TABLE = Encoding.UTF8.GetBytes(
            "AABB\0AnimationClip\0AnimationCurve\0AnimationState\0Array\0Base\0BitField\0bitset\0bool\0char\0" +
            "ColorRGBA\0Component\0data\0deque\0double\0dynamic_array\0FastPropertyName\0first\0float\0Font\0" +
            "GameObject\0Generic Mono\0GradientNEW\0GUID\0GUIStyle\0int\0list\0long long\0map\0Matrix4x4f\0MdFour\0" +
            "MonoBehaviour\0MonoScript\0m_ByteSize\0m_Curve\0m_EditorClassIdentifier\0m_EditorHideFlags\0m_Enabled\0" +
            "m_ExtensionPtr\0m_GameObject\0m_Index\0m_IsArray\0m_IsStatic\0m_MetaFlag\0m_Name\0m_ObjectHideFlags\0" +
            "m_PrefabInternal\0m_PrefabParentObject\0m_Script\0m_StaticEditorFlags\0m_Type\0m_Version\0Object\0" +
            "pair\0PPtr<Component>\0PPtr<GameObject>\0PPtr<Material>\0PPtr<MonoBehaviour>\0PPtr<MonoScript>\0" +
            "PPtr<Object>\0PPtr<Prefab>\0PPtr<Sprite>\0PPtr<TextAsset>\0PPtr<Texture>\0PPtr<Texture2D>\0PPtr<Transform>\0" +
            "Prefab\0Quaternionf\0Rectf\0RectInt\0RectOffset\0second\0set\0short\0size\0SInt16\0SInt32\0SInt64\0" +
            "SInt8\0staticvector\0string\0TextAsset\0TextMesh\0Texture\0Texture2D\0Transform\0TypelessData\0UInt16\0" +
            "UInt32\0UInt64\0UInt8\0unsigned int\0unsigned long long\0unsigned short\0vector\0Vector2f\0Vector3f\0" +
            "Vector4f\0m_ScriptingClassIdentifier\0Gradient\0Type*\0int2_storage\0int3_storage\0BoundsInt\0m_CorrespondingSourceObject\0" +
            "m_PrefabInstance\0m_PrefabAsset\0FileSize\0Hash128\0RenderingLayerMask\0");
    }
}
