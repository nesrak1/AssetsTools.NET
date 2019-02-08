using System;
using System.Text;

namespace AssetsTools.NET
{
    public struct Type_0D //everything big endian
    {
        //Starting with U5.5, all MonoBehaviour types have MonoBehaviour's classId (114)
        //Before, the different MonoBehaviours had different negative classIds, starting with -1
        public int classId; //0x00

        public byte unknown16_1; //format >= 0x10, wild guess : bool "has MonoBehaviour type id" (usually 0)
        public ushort scriptIndex; //format >= 0x11 U5.5+, index to the MonoManager  0xFFFF)

        //Script ID (md4 hash)
        public uint unknown1; //if classId < 0 //0x04
        public uint unknown2; //if classId < 0 //0x08
        public uint unknown3; //if classId < 0 //0x0C
        public uint unknown4; //if classId < 0 //0x10

        //Type hash / properties hash (md4)
        public uint unknown5; //0x04 or 0x14
        public uint unknown6; //0x08 or 0x18
        public uint unknown7; //0x0C or 0x1C
        public uint unknown8; //0x10 or 0x20
        public uint typeFieldsExCount; //if (TypeTree.enabled) //0x14 or 0x24
        public TypeField_0D[] pTypeFieldsEx;

        public uint stringTableLen; //if (TypeTree.enabled) //0x18 or 0x28
        public string pStringTable;

        public ulong Read(bool hasTypeTree, ulong absFilePos, AssetsFileReader reader, uint version, uint typeVersion, bool bigEndian)
        {
            classId = reader.ReadInt32();
            if (version >= 0x10) unknown16_1 = reader.ReadByte();
            if (version >= 0x11) scriptIndex = reader.ReadUInt16();
            //if ((version < 0x11 && classId < 0) || (version >= 0x11 && scriptIndex != 0xFFFF)) //original is if (classId == 114)
            //if ((version < 0x11 && classId < 0) || (version >= 0x11 && scriptIndex != 0xFFFF))
            if ((version < 0x11 && classId < 0) || (version >= 0x11 && classId == 114))
            {
                unknown1 = reader.ReadUInt32();
                unknown2 = reader.ReadUInt32();
                unknown3 = reader.ReadUInt32();
                unknown4 = reader.ReadUInt32();
            }
            unknown5 = reader.ReadUInt32();
            unknown6 = reader.ReadUInt32();
            unknown7 = reader.ReadUInt32();
            unknown8 = reader.ReadUInt32();
            if (hasTypeTree)
            {
                typeFieldsExCount = reader.ReadUInt32();
                stringTableLen = reader.ReadUInt32();
                pTypeFieldsEx = new TypeField_0D[typeFieldsExCount];
                for (int i = 0; i < typeFieldsExCount; i++)
                {
                    TypeField_0D typefield0d = new TypeField_0D();
                    typefield0d.Read(reader.Position, reader, bigEndian);
                    pTypeFieldsEx[i] = typefield0d;
                }
                pStringTable = Encoding.UTF8.GetString(reader.ReadBytes((int)stringTableLen));
            }
            return reader.Position;
        }
        public ulong Write(bool hasTypeTree, ulong absFilePos, AssetsFileWriter writer, uint version)
        {
            writer.Write(classId);
            if (version >= 0x10) writer.Write(unknown16_1);
            if (version >= 0x11) writer.Write(scriptIndex);
            //if ((version < 0x11 && classId < 0) || (version >= 0x11 && scriptIndex != 0xFFFF))
            if (classId == 114)
            {
                writer.Write(unknown1);
                writer.Write(unknown2);
                writer.Write(unknown3);
                writer.Write(unknown4);
            }
            writer.Write(unknown5);
            writer.Write(unknown6);
            writer.Write(unknown7);
            writer.Write(unknown8);
            if (hasTypeTree)
            {
                writer.Write(typeFieldsExCount);
                writer.Write(pStringTable.Length);
                for (int i = 0; i < typeFieldsExCount; i++)
                {
                    pTypeFieldsEx[i].Write(writer.Position, writer);
                }
                writer.Write(pStringTable);
            }
            return writer.Position;
        }
        //?
        public static readonly string strTable = "AABB\0AnimationClip\0AnimationCurve\0AnimationState\0Array\0Base\0BitField\0bitset\0bool\0char\0ColorRGBA\0Component\0data\0deque\0double\0dynamic_array\0FastPropertyName\0first\0float\0Font\0GameObject\0Generic Mono\0GradientNEW\0GUID\0GUIStyle\0int\0list\0long long\0map\0Matrix4x4f\0MdFour\0MonoBehaviour\0MonoScript\0m_ByteSize\0m_Curve\0m_EditorClassIdentifier\0m_EditorHideFlags\0m_Enabled\0m_ExtensionPtr\0m_GameObject\0m_Index\0m_IsArray\0m_IsStatic\0m_MetaFlag\0m_Name\0m_ObjectHideFlags\0m_PrefabInternal\0m_PrefabParentObject\0m_Script\0m_StaticEditorFlags\0m_Type\0m_Version\0Object\0pair\0PPtr<Component>\0PPtr<GameObject>\0PPtr<Material>\0PPtr<MonoBehaviour>\0PPtr<MonoScript>\0PPtr<Object>\0PPtr<Prefab>\0PPtr<Sprite>\0PPtr<TextAsset>\0PPtr<Texture>\0PPtr<Texture2D>\0PPtr<Transform>\0Prefab\0Quaternionf\0Rectf\0RectInt\0RectOffset\0second\0set\0short\0size\0SInt16\0SInt32\0SInt64\0SInt8\0staticvector\0string\0TextAsset\0TextMesh\0Texture\0Texture2D\0Transform\0TypelessData\0UInt16\0UInt32\0UInt64\0UInt8\0unsigned int\0unsigned long long\0unsigned short\0vector\0Vector2f\0Vector3f\0Vector4f\0m_ScriptingClassIdentifier\0Gradient\0Type*\0";
    }
}
