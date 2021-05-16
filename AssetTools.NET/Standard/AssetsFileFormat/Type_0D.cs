using System;
using System.Text;

namespace AssetsTools.NET
{
    public class Type_0D
    {
        public int classId;

        public byte unknown16_1;
        public ushort scriptIndex;

        public uint scriptHash1;
        public uint scriptHash2;
        public uint scriptHash3;
        public uint scriptHash4;

        public uint typeHash1;
        public uint typeHash2;
        public uint typeHash3;
        public uint typeHash4;
        public uint typeFieldsExCount;
        public TypeField_0D[] typeFieldsEx;

        public uint stringTableLen;
        public string stringTable;

        public int dependenciesCount;
        public int[] dependencies;

        public void Read(bool hasTypeTree, AssetsFileReader reader, uint version)
        {
            classId = reader.ReadInt32();
            if (version >= 0x10) unknown16_1 = reader.ReadByte();
            if (version >= 0x11) scriptIndex = reader.ReadUInt16();
            if ((version < 0x11 && classId < 0) || (version >= 0x11 && classId == 0x72))
            {
                scriptHash1 = reader.ReadUInt32();
                scriptHash2 = reader.ReadUInt32();
                scriptHash3 = reader.ReadUInt32();
                scriptHash4 = reader.ReadUInt32();
            }
            typeHash1 = reader.ReadUInt32();
            typeHash2 = reader.ReadUInt32();
            typeHash3 = reader.ReadUInt32();
            typeHash4 = reader.ReadUInt32();
            if (hasTypeTree)
            {
                typeFieldsExCount = reader.ReadUInt32();
                stringTableLen = reader.ReadUInt32();
                typeFieldsEx = new TypeField_0D[typeFieldsExCount];
                for (int i = 0; i < typeFieldsExCount; i++)
                {
                    TypeField_0D typefield0d = new TypeField_0D();
                    typefield0d.Read(reader, version);
                    typeFieldsEx[i] = typefield0d;
                }
                stringTable = Encoding.UTF8.GetString(reader.ReadBytes((int)stringTableLen));
                if (version >= 0x15)
                {
                    dependenciesCount = reader.ReadInt32();
                    dependencies = new int[dependenciesCount];
                    for (int i = 0; i < dependenciesCount; i++)
                    {
                        dependencies[i] = reader.ReadInt32();
                    }
                }
            }
        }
        public void Write(bool hasTypeTree, AssetsFileWriter writer, uint version)
        {
            writer.Write(classId);
            if (version >= 0x10) writer.Write(unknown16_1);
            if (version >= 0x11) writer.Write(scriptIndex);
            if (classId == 0x72)
            {
                writer.Write(scriptHash1);
                writer.Write(scriptHash2);
                writer.Write(scriptHash3);
                writer.Write(scriptHash4);
            }
            writer.Write(typeHash1);
            writer.Write(typeHash2);
            writer.Write(typeHash3);
            writer.Write(typeHash4);
            if (hasTypeTree)
            {
                writer.Write(typeFieldsExCount);
                writer.Write(stringTable.Length);
                for (int i = 0; i < typeFieldsExCount; i++)
                {
                    typeFieldsEx[i].Write(writer, version);
                }
                writer.Write(stringTable);
                if (version >= 0x15)
                {
                    writer.Write(dependenciesCount);
                    for (int i = 0; i < dependenciesCount; i++)
                    {
                        writer.Write(dependencies[i]);
                    }
                }
            }
        }

        public static readonly string strTable = "AABB\0AnimationClip\0AnimationCurve\0AnimationState\0Array\0Base\0BitField\0bitset\0bool\0char\0ColorRGBA\0Component\0data\0deque\0double\0dynamic_array\0FastPropertyName\0first\0float\0Font\0GameObject\0Generic Mono\0GradientNEW\0GUID\0GUIStyle\0int\0list\0long long\0map\0Matrix4x4f\0MdFour\0MonoBehaviour\0MonoScript\0m_ByteSize\0m_Curve\0m_EditorClassIdentifier\0m_EditorHideFlags\0m_Enabled\0m_ExtensionPtr\0m_GameObject\0m_Index\0m_IsArray\0m_IsStatic\0m_MetaFlag\0m_Name\0m_ObjectHideFlags\0m_PrefabInternal\0m_PrefabParentObject\0m_Script\0m_StaticEditorFlags\0m_Type\0m_Version\0Object\0pair\0PPtr<Component>\0PPtr<GameObject>\0PPtr<Material>\0PPtr<MonoBehaviour>\0PPtr<MonoScript>\0PPtr<Object>\0PPtr<Prefab>\0PPtr<Sprite>\0PPtr<TextAsset>\0PPtr<Texture>\0PPtr<Texture2D>\0PPtr<Transform>\0Prefab\0Quaternionf\0Rectf\0RectInt\0RectOffset\0second\0set\0short\0size\0SInt16\0SInt32\0SInt64\0SInt8\0staticvector\0string\0TextAsset\0TextMesh\0Texture\0Texture2D\0Transform\0TypelessData\0UInt16\0UInt32\0UInt64\0UInt8\0unsigned int\0unsigned long long\0unsigned short\0vector\0Vector2f\0Vector3f\0Vector4f\0m_ScriptingClassIdentifier\0Gradient\0Type*\0int2_storage\0int3_storage\0BoundsInt\0m_CorrespondingSourceObject\0m_PrefabInstance\0m_PrefabAsset\0FileSize\0Hash128";
    }
}
