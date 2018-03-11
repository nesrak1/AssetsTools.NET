////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileReader : BinaryReader
    {
        public bool bigEndian = true;
        public AssetsFileReader(FileStream fileStream) : base(fileStream) { }
        public AssetsFileReader(MemoryStream memStream) : base(memStream) { }
        public AssetsFileReader(Stream stream) : base(stream) { }
        public override short ReadInt16()
        {
            unchecked
            {
                return bigEndian ? (short)ReverseShort((ushort)base.ReadInt16()) : base.ReadInt16();
                //return bigEndian ? BitConverter.ToInt16(ReadBytes(2).Reverse().ToArray(), 0) : base.ReadInt16();
            }
        }
        public override ushort ReadUInt16()
        {
            unchecked
            {
                return bigEndian ? ReverseShort(base.ReadUInt16()) : base.ReadUInt16();
                //return bigEndian ? BitConverter.ToUInt16(ReadBytes(2).Reverse().ToArray(), 0) : base.ReadUInt16();
            }
        }
        public override int ReadInt32()
        {
            unchecked
            {
                return bigEndian ? (int)ReverseInt((uint)base.ReadInt32()) : base.ReadInt32();
                //return bigEndian ? BitConverter.ToInt32(ReadBytes(4).Reverse().ToArray(), 0) : base.ReadInt32();
            }
        }
        public override uint ReadUInt32()
        {
            unchecked
            {
                return bigEndian ? ReverseInt(base.ReadUInt32()) : base.ReadUInt32();
                //return bigEndian ? BitConverter.ToUInt32(ReadBytes(4).Reverse().ToArray(), 0) : base.ReadUInt32();
            }
        }
        public override long ReadInt64()
        {
            unchecked
            {
                return bigEndian ? (long)ReverseLong((ulong)base.ReadInt64()) : base.ReadInt64();
                //return bigEndian ? BitConverter.ToInt64(ReadBytes(8).Reverse().ToArray(), 0) : base.ReadInt64();
            }
        }
        public override ulong ReadUInt64()
        {
            unchecked
            {
                return bigEndian ? ReverseLong(base.ReadUInt64()) : base.ReadUInt64();
                //return bigEndian ? BitConverter.ToUInt64(ReadBytes(8).Reverse().ToArray(), 0) : base.ReadUInt64();
            }
        }
        public ushort ReverseShort(ushort value)
        {
            return (ushort)(((value & 0xFF00) >> 8) | (value & 0x00FF) << 8);
        }
        public uint ReverseInt(uint value)
        {
            value = (value >> 16) | (value << 16);
            return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
        }
        public ulong ReverseLong(ulong value)
        {
            value = (value >> 32) | (value << 32);
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
        }
        public void Align()
        {
            long pad = 4 - (BaseStream.Position % 4);
            if (pad != 4) BaseStream.Position += pad;
        }
        public string ReadStringLength(int len)
        {
            return Encoding.ASCII.GetString(ReadBytes(len));
        }
        public string ReadNullTerminated()
        {
            string output = "";
            char curChar;
            while ((curChar = ReadChar()) != 0x00)
            {
                output += curChar;
            }
            return output;
        }
        public static string ReadNullTerminatedArray(byte[] bytes, uint pos)
        {
            string output = "";
            char curChar;
            while ((curChar = (char)bytes[pos]) != 0x00)
            {
                output += curChar;
                pos++;
            }
            return output;
        }
        public string ReadCountString()
        {
            byte length = ReadByte();
            return ReadStringLength(length);
        }
        public string ReadCountStringInt32()
        {
            int length = (int)ReadUInt32();
            return ReadStringLength(length);
        }
        public ulong Position
        {
            get { return (ulong)BaseStream.Position; }
            set { BaseStream.Position = (long)value; }
        }
    }

    public class AssetsFileWriter : BinaryWriter
    {
        public bool bigEndian = true;
        public AssetsFileWriter(FileStream fileStream) : base(fileStream) { }
        public AssetsFileWriter(MemoryStream memoryStream) : base(memoryStream) { }
        public AssetsFileWriter(Stream stream) : base(stream) { }
        public override void Write(short val)
        {
            unchecked
            {
                if (bigEndian) base.Write((short)ReverseShort((ushort)val));
                else base.Write(val);
            }
        }
        public override void Write(ushort val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseShort(val));
                else base.Write(val);
            }
        }
        public override void Write(int val)
        {
            unchecked
            {
                if (bigEndian) base.Write((int)ReverseInt((uint)val));
                else base.Write(val);
            }
        }
        public override void Write(uint val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseInt(val));
                else base.Write(val);
            }
        }
        public override void Write(long val)
        {
            unchecked
            {
                if (bigEndian) base.Write((long)ReverseLong((ulong)val));
                else base.Write(val);
            }
        }
        public override void Write(ulong val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseLong(val));
                else base.Write(val);
            }
        }
        public override void Write(string val)
        {
            base.Write(Encoding.ASCII.GetBytes(val));
        }
        public ushort ReverseShort(ushort value)
        {
            return (ushort)(((value & 0xFF00) >> 8) | (value & 0x00FF) << 8);
        }
        public uint ReverseInt(uint value)
        {
            value = (value >> 16) | (value << 16);
            return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
        }
        public ulong ReverseLong(ulong value)
        {
            value = (value >> 32) | (value << 32);
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
        }
        public void Align()
        {
            while (BaseStream.Position % 4 != 0) Write((byte)0x00);
        }
        public void WriteNullTerminated(string text)
        {
            Write(text);
            Write((byte)0x00);
        }
        public void WriteCountString(string text)
        {
            if (text.Length > 0xFF)
                new Exception("String is longer than 255! Use the Int32 variant instead!");
            Write((byte)text.Length);
            Write(text);
        }
        public void WriteCountStringInt32(string text)
        {
            Write(text.Length);
            Write(text);
        }
        public ulong Position
        {
            get { return (ulong)BaseStream.Position; }
            set { BaseStream.Position = (long)value; }
        }
    }

    //for assets that begin with a m_Name field
    public struct AssetFile
    {
        public uint filenameSize;            //0x00 //little-endian
        public byte data;                    //0x04

        ///public string GetFileName(int classId);
        ///public byte GetFileData();
        ///public uint GetFileDataIndex();
    }

    public class AssetFileInfo
    {
        public ulong index;                  //0x00 //little-endian //version < 0x0E : only DWORD
        public uint offs_curFile;            //0x08 //little-endian
        public uint curFileSize;             //0x0C //little-endian
        public uint curFileTypeOrIndex;      //0x10 //little-endian //starting with version 0x10, this is an index into the type tree
        //inheritedUnityClass                //for Unity classes, this is curFileType; for MonoBehaviours, this is 114
        //version < 0x0B                     //inheritedUnityClass is DWORD, no scriptIndex exists
        public ushort inheritedUnityClass;   //0x14 //little-endian (MonoScript)//only version < 0x10
        //scriptIndex                        //for Unity classes, this is 0xFFFF; for MonoBehaviours, this is the index of the script in the MonoManager asset
        public ushort scriptIndex;           //0x16 //little-endian//only version <= 0x10
        public byte unknown1;                //0x18 //only 0x0F <= version <= 0x10 //with alignment always a DWORD
        public static uint GetSize(uint version)
        {
            uint size = 0;
            size += 4;
            if (version >= 0x0E) size += 4;
            size += 12;
            if (version < 0x10) size += 2;
            if (version <= 0x10) size += 2;
            if (0x0F <= version && version <= 0x10) size += 4;
            return size;
        }
        ///public ulong Read(uint version, ulong pos, AssetsFileReader reader, FileStream readerPar, bool bigEndian);
        ///public ulong Write(uint version, ulong pos, BinaryWriter writer, FileStream writerPar);
    }

    public struct AssetFileList
    {
        public uint sizeFiles;                      //0x00 //little-endian
        public AssetFileInfo fileInfs;              //0x04

        ///public uint GetSizeBytes(uint version);
        ///public ulong Read(ulong version, uint pos, AssetsFileReader reader, FileStream readerPar, bool bigEndian);
        ///public ulong Write(ulong version, uint pos, BinaryWriter writer, FileStream writerPar);
    }

    public struct AssetsFileHeader
    {
        public uint metadataSize;                   //0x00
        public uint fileSize;                       //0x04 //big-endian
        public uint format;                         //0x08
        public uint offs_firstFile;                 //0x0C //big-endian
        //0 == little-endian (default, haven't seen anything else); 1 == big-endian, in theory
        public uint endianness;                     //0x10, for format < 9 at (fileSize - metadataSize), right before TypeTree
        public byte[] unknown;                      //0x11, for format >= 9

        ///public uint GetSizeBytes();
        public ulong Read(ulong absFilePos, AssetsFileReader reader)
        {
            metadataSize = reader.ReadUInt32();
            fileSize = reader.ReadUInt32();
            format = reader.ReadUInt32();
            offs_firstFile = reader.ReadUInt32();
            endianness = reader.ReadByte();
            reader.bigEndian = endianness == 1 ? true : false;
            unknown = reader.ReadBytes(3);
            return reader.Position;
        }
        //does NOT write the endianness byte for format < 9!
        public ulong Write(ulong pos, AssetsFileWriter writer)
        {
            writer.bigEndian = true;
            writer.Write(metadataSize);
            writer.Write(fileSize);
            writer.Write(format);
            writer.Write(offs_firstFile);
            writer.Write(endianness);
            writer.bigEndian = endianness == 1 ? true : false;
            writer.Write(unknown);
            return writer.Position;
        }
    }

    public struct AssetsFileDependency
    {
        //version < 6 : no bufferedPath
        //version < 5 : no bufferedPath, guid, type
        public struct GUID128
        {
            public long mostSignificant; //64-127 //big-endian
            public long leastSignificant; //0-63  //big-endian
            public ulong Read(ulong absFilePos, AssetsFileReader reader)
            {
                mostSignificant = reader.ReadInt64();
                leastSignificant = reader.ReadInt64();
                return reader.Position;
            }
            public ulong Write(ulong absFilePos, AssetsFileWriter writer)
            {
                writer.Write(mostSignificant);
                writer.Write(leastSignificant);
                return writer.Position;
            }
        }
        public GUID128 guid;
        public int type;
        public string assetPath; //path to the .assets file
        public byte[] bufferedPath; //for buffered (type=1)
        public ulong Read(ulong absFilePos, AssetsFileReader reader, bool bigEndian)
        {
            bufferedPath = new byte[] { reader.ReadByte() }; //-Dunno why it's here but it is
            guid = new GUID128();
            guid.Read(reader.Position, reader);
            type = reader.ReadInt32();
            assetPath = reader.ReadNullTerminated();
            //todo: the switchero was here for testing purposes, should be handled by application for full control
            if (assetPath.StartsWith("library/"))
            {
                assetPath = "Resources\\" + assetPath.Substring(8);
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer)
        {
            writer.Write(bufferedPath);
            guid.Write(writer.Position, writer);
            writer.Write(type);
            string assetPathTemp = assetPath;
            if (assetPathTemp.StartsWith("Resources\\"))
            {
                assetPathTemp = "library/" + assetPath.Substring(10);
            }
            writer.WriteNullTerminated(assetPathTemp);
            return writer.Position;
        }
    }

    public struct AssetsFileDependencyList
    {
        public uint dependencyCount;
        //BYTE unknown; //seemingly always 0
        public AssetsFileDependency[] pDependencies;
	    public ulong Read(ulong absFilePos, AssetsFileReader reader, uint format, bool bigEndian)
        {
            dependencyCount = reader.ReadUInt32();
            pDependencies = new AssetsFileDependency[dependencyCount];
            for (int i = 0; i < dependencyCount; i++)
            {
                AssetsFileDependency dependency = new AssetsFileDependency();
                dependency.Read(reader.Position, reader, bigEndian);
                pDependencies[i] = dependency;
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint format)
        {
            writer.Write(dependencyCount);
            for (int i = 0; i < dependencyCount; i++)
            {
                pDependencies[i].Write(writer.Position, writer);
            }
            return writer.Position;
        }
    }

    public struct TypeField_0D
    {
        public ushort version;                      //0x00
        public byte depth;                          //0x02 //specifies the amount of parents
        public bool isArray;                        //0x03
        public uint typeStringOffset;               //0x04 //-was this previously different? if so, then when?
        public uint nameStringOffset;               //0x08 //-same here
        public uint size;                           //0x0C //size in bytes; if not static (if it contains an array), set to -1
        public uint index;                          //0x10
        public uint flags;                          //0x14
        public ulong Read(ulong absFilePos, AssetsFileReader reader, bool bigEndian)
        {
            version = reader.ReadUInt16();
            depth = reader.ReadByte();
            isArray = reader.ReadBoolean();
            typeStringOffset = reader.ReadUInt16();
            reader.ReadUInt16();
            nameStringOffset = reader.ReadUInt16();
            reader.ReadUInt16();
            size = reader.ReadUInt32();
            index = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            return reader.Position;
        }
        public ulong Write(ulong curFilePos, AssetsFileWriter writer)
        {
            writer.Write(version);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(typeStringOffset);
            writer.Write(nameStringOffset);
            writer.Write(size);
            writer.Write(index);
            writer.Write(flags);
            return writer.Position;
        }
        ///public string GetTypeString(string stringTable, ulong stringTableLen);
	    ///public string GetNameString(string stringTable, ulong stringTableLen);
    }

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
            //-The below may or may not be big endian. UABE's API headers and disunity's docs claim so, but doesn't appear to be
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
                //im gonna regret this someday
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

    public struct TypeField_07 //everything big endian
    {
        public string type; //null-terminated
        public string name; //null-terminated
        public uint size;
        public uint index;
        public uint arrayFlag;
        public uint flags1;
        public uint flags2;
        public uint childrenCount;
        public TypeField_07[] children;

        ///public ulong Read(bool hasTypeTree, ulong absFilePos, AssetsFileReader reader, FileStream readerPar, uint version, uint typeVersion, bool bigEndian);
        ///public ulong Write(bool hasTypeTree, ulong absFilePos, BinaryWriter writer, FileStream writerPar);
    }

    public struct Type_07
    {
        public int classId; //big endian
        public TypeField_07 @base;

	    ///public ulong Read(bool hasTypeTree, ulong absFilePos, AssetsFileReader reader, FileStream readerPar, uint version, uint typeVersion, bool bigEndian);
        ///public ulong Write(bool hasTypeTree, ulong absFilePos, BinaryWriter writer, FileStream writerPar);
    }

    public struct TypeTree
    {
        public string unityVersion; //null-terminated; stored for .assets format > 6
        public uint version; //big endian; stored for .assets format > 6
        public bool hasTypeTree; //stored for .assets format >= 13; Unity 5 only stores some metadata if it's set to false
        public uint fieldCount; //big endian;
        
		public Type_0D[] pTypes_Unity5;
        public Type_07[] pTypes_Unity4;

        public uint dwUnknown; //actually belongs to the asset list; stored for .assets format < 14
        public uint _fmt; //not stored here in the .assets file, the variable is just to remember the .assets file version

        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint version, bool bigEndian)
        {
            unityVersion = reader.ReadNullTerminated();
            this.version = reader.ReadUInt32();
            hasTypeTree = reader.ReadBoolean();
            fieldCount = reader.ReadUInt32();
            pTypes_Unity5 = new Type_0D[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                Type_0D type0d = new Type_0D();
                type0d.Read(hasTypeTree, reader.Position, reader, version, version, bigEndian);
                pTypes_Unity5[i] = type0d;
            }
            if (version < 0x0E) dwUnknown = reader.ReadUInt32();
            _fmt = version; //-todo: figure out what the heck this is for. if ver = -1 on write does it set it to default or something?
            return reader.Position;
        }//Minimum AssetsFile format : 6
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint version)
        {
            writer.WriteNullTerminated(unityVersion);
            writer.Write(this.version);
            writer.Write(hasTypeTree);
            writer.Write(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                pTypes_Unity5[i].Write(hasTypeTree, writer.Position, writer, version);
            }
            if (version < 0x0E) writer.Write(dwUnknown);
            return writer.Position;
        }

        ///public void Clear();
    }

    public class AssetPPtr
    {
        public AssetPPtr(uint fileID, ulong pathID)
        {
            this.fileID = fileID;
            this.pathID = pathID;
        }
        public uint fileID;
        public ulong pathID;
    }

    public struct PreloadList
    {
        public uint len;
        public AssetPPtr[] items;

        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint format, bool bigEndian)
        {
            len = reader.ReadUInt32();
            items = new AssetPPtr[len];
            for (int i = 0; i < len; i++)
            {
                items[i] = new AssetPPtr(reader.ReadUInt32(), reader.ReadUInt64());
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint format)
        {
            writer.Write(len);
            for (int i = 0; i < len; i++)
            {
                writer.Write(items[i].fileID);
                writer.Write(items[i].pathID);
            }
            return writer.Position;
        }
    }

    public class AssetsFile
    {
        public AssetsFileHeader header;
        public TypeTree typeTree;

        public PreloadList preloadTable;
        public AssetsFileDependencyList dependencies;

        public uint AssetTablePos;
        public uint AssetCount;

        public AssetsFileReader reader;
        public Stream readerPar;

        public AssetsFile(AssetsFileReader reader)
        {
            this.reader = reader;
            readerPar = reader.BaseStream;
            header = new AssetsFileHeader();
            header.Read(0, reader);
            typeTree = new TypeTree();
            typeTree.Read(reader.Position, reader, header.format, reader.bigEndian);
            AssetCount = reader.ReadUInt32();
            reader.Align();
            AssetTablePos = Convert.ToUInt32(reader.BaseStream.Position);
            reader.BaseStream.Position += AssetFileInfo.GetSize(header.format) * AssetCount;
            preloadTable = new PreloadList();
            preloadTable.Read(reader.Position, reader, header.format, reader.bigEndian);
            dependencies = new AssetsFileDependencyList();
            dependencies.Read(reader.Position, reader, header.format, reader.bigEndian);
        }

        //set fileID to -1 if all replacers are for this .assets file but don't have the fileID set to the same one
        //typeMeta is used to add the type information (hash and type fields) for format >= 0x10 if necessary
        public ulong Write(AssetsFileWriter writer, ulong filePos, /*AssetsReplacer[] pReplacers, ulong replacerCount, */uint fileID, ClassDatabaseFile typeMeta = null) {
            header.Write(writer.Position, writer);
            typeTree.Write(writer.Position, writer, header.format);
            writer.Write(AssetCount);
            writer.Align();
            uint size = AssetFileInfo.GetSize(header.format) * AssetCount;
            for (int i = 0; i < size; i++)
            {
                writer.Write((byte)0x00);
            }
            preloadTable.Write(writer.Position, writer, header.format);
            dependencies.Write(writer.Position, writer, header.format);
            ulong endMarker = writer.Position;
            header.metadataSize = (uint)writer.Position - 0x14;
            writer.Position = 0;
            header.Write(writer.Position, writer);
            writer.Position = endMarker;
            return writer.Position;
        }

        ///public bool GetAssetFile(ulong fileInfoOffset, AssetsFileReader reader, AssetFile pBuf, FileStream readerPar);
        ///public ulong GetAssetFileOffs(ulong fileInfoOffset, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByIndex(ulong fileIndex, AssetFile pBuf, uint pSize, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByIndex(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public bool GetAssetFileByName(string name, AssetFile pBuf, uint pSize, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffs(ulong fileIndex, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetAssetFileInfoOffsByName(string name, AssetsFileReader reader, FileStream readerPar);
        ///public ulong GetFileList(AssetsFileReader reader, FileStream readerPar);
        ///public bool VerifyAssetsFile(AssetsFileVerifyLogger logger = null);
    }
}
