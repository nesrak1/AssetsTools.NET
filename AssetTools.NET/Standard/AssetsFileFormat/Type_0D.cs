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
        public string[] pStringTable;

        public ulong Read(bool hasTypeTree, ulong absFilePos, AssetsFileReader reader, uint version, uint typeVersion, bool bigEndian)
        {
            classId = reader.ReadInt32();
            if (version >= 0x10) unknown16_1 = reader.ReadByte();
            if (version >= 0x11) scriptIndex = reader.ReadUInt16();
            if ((version < 0x11 && classId < 0) || (version >= 0x11 && scriptIndex != 0xFFFF))
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
                string rawStringTable = Encoding.UTF8.GetString(reader.ReadBytes((int)stringTableLen));
                pStringTable = rawStringTable.Split('\0');
                Array.Resize(ref pStringTable, pStringTable.Length - 1);
                //Debug.WriteLine(pStringTable);
            }
            return reader.Position;
        }
        public ulong Write(bool hasTypeTree, ulong absFilePos, AssetsFileWriter writer, uint version)
        {
            writer.Write(classId);
            if (version >= 0x10) writer.Write(unknown16_1);
            if (version >= 0x11) writer.Write(scriptIndex);
            if ((version < 0x11 && classId < 0) || (version >= 0x11 && scriptIndex != 0xFFFF))
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
                writer.Write(stringTableLen);
                for (int i = 0; i < typeFieldsExCount; i++)
                {
                    pTypeFieldsEx[i].Write(writer.Position, writer);
                }
                //-im gonna regret this someday
                stringTableLen = 0;
                for (int i = 0; i < pStringTable.Length; i++)
                {
                    stringTableLen += (uint)pStringTable[i].Length + 1;
                }
                writer.Write(stringTableLen);
                for (int i = 0; i < pStringTable.Length; i++)
                {
                    writer.WriteNullTerminated(pStringTable[i]);
                }
            }
            return writer.Position;
        }
        //0x28212B0
        public static readonly string strTable = "AABB.AnimationClip.AnimationCurve.AnimationState.Array.Base.BitField.bitset.bool.char.ColorRGBA.Component.data.deque.double.dynamic_array.FastPropertyName.first.float.Font.GameObject.Generic Mono.GradientNEW.GUID.GUIStyle.int.list.long long.map.Matrix4x4f.MdFour.MonoBehaviour.MonoScript.m_ByteSize.m_Curve.m_EditorClassIdentifier.m_EditorHideFlags.m_Enabled.m_ExtensionPtr.m_GameObject.m_Index.m_IsArray.m_IsStatic.m_MetaFlag.m_Name.m_ObjectHideFlags.m_PrefabInternal.m_PrefabParentObject.m_Script.m_StaticEditorFlags.m_Type.m_Version.Object.pair.PPtr<Component>.PPtr<GameObject>.PPtr<Material>.PPtr<MonoBehaviour>.PPtr<MonoScript>.PPtr<Object>.PPtr<Prefab>.PPtr<Sprite>.PPtr<TextAsset>.PPtr<Texture>.PPtr<Texture2D>.PPtr<Transform>.Prefab.Quaternionf.Rectf.RectInt.RectOffset.second.set.short.size.SInt16.SInt32.SInt64.SInt8.staticvector.string.TextAsset.TextMesh.Texture.Texture2D.Transform.TypelessData.UInt16.UInt32.UInt64.UInt8.unsigned int.unsigned long long.unsigned short.vector.Vector2f.Vector3f.Vector4f.m_ScriptingClassIdentifier.Gradient.Type*";
    }
}
