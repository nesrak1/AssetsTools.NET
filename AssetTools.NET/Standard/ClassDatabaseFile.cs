////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public struct ClassDatabaseFileString
    {
        public struct TableString
        {
            public uint stringTableOffset;
            public string @string;
        }
        public TableString str;

        public bool fromStringTable;
        public string GetString(ClassDatabaseFile pFile)
        {
            if (fromStringTable)
            {
                return AssetsFileReader.ReadNullTerminatedArray(pFile.stringTable, str.stringTableOffset);
            }
            else
            {
                return str.@string;
            }
        }
        public ulong Read(AssetsFileReader reader, ulong filePos)
        {
            fromStringTable = true;
            str.stringTableOffset = reader.ReadUInt32();
            if (str.stringTableOffset != 0xFFFFFFFF) //-total guess haha, havent messed with inline strings much
            {
                fromStringTable = true;
            }
            else
            {
                //-untested, probably wrong
                fromStringTable = false;
                str.@string = reader.ReadCountString(); //-this may be different
            }
            return reader.Position;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos);
    }
    public struct ClassDatabaseTypeField
    {
        public ClassDatabaseFileString typeName;
        public ClassDatabaseFileString fieldName;
        public byte depth;
        public byte isArray;
        public uint size;
        //DWORD index;
        public ushort version;
        public uint flags2;

        ///public ClassDatabaseTypeField();
        ///public ClassDatabaseTypeField(ClassDatabaseTypeField other);
        public ulong Read(AssetsFileReader reader, ulong filePos, int version)
        {
            typeName = new ClassDatabaseFileString();
            typeName.Read(reader, reader.Position);
            fieldName = new ClassDatabaseFileString();
            fieldName.Read(reader, reader.Position);
            depth = reader.ReadByte();
            isArray = reader.ReadByte();
            size = reader.ReadUInt32();
            version = reader.ReadUInt16();
            flags2 = reader.ReadUInt32();
            return reader.Position;
        }//reads version 0,1,2,3
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos, int version); //writes version 1,2,3
    }
    public class ClassDatabaseType
    {
    	public int classId;
        public int baseClass;
        public ClassDatabaseFileString name;

        public List<ClassDatabaseTypeField> fields;
        //DWORD fieldCount;
        //ClassDatabaseTypeField *fields;
        ///public ClassDatabaseType();
        ///public ClassDatabaseType(ClassDatabaseType other);
        public ulong Read(AssetsFileReader reader, ulong filePos, int version)
        {
            classId = reader.ReadInt32();
            baseClass = reader.ReadInt32();
            name = new ClassDatabaseFileString();
            name.Read(reader, reader.Position);
            uint fieldCount = reader.ReadUInt32();
            fields = new List<ClassDatabaseTypeField>();
            for (int j = 0; j < fieldCount; j++)
            {
                ClassDatabaseTypeField cdtf = new ClassDatabaseTypeField();
                cdtf.Read(reader, filePos, version);
                fields.Add(cdtf);
            }
            return reader.Position;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos, int version);
        ///public Hash128 MakeTypeHash(ClassDatabaseFile pDatabaseFile);
    }
    public struct ClassDatabaseFileHeader
    {
        public string header;
        public byte fileVersion;

        public byte compressionType; //version 2; 0 = none, 1 = LZ4
        public uint compressedSize, uncompressedSize;  //version 2
                                                       //BYTE assetsVersionCount; //version 0 only
                                                       //BYTE *assetsVersions; //version 0 only

        public byte unityVersionCount;
        public string[] pUnityVersions;


        public uint stringTableLen;
        public uint stringTablePos;
        public ulong Read(AssetsFileReader reader, ulong filePos)
        {
            reader.bigEndian = false;
            header = reader.ReadStringLength(4);
            if (header != "cldb") return reader.Position;
            fileVersion = reader.ReadByte();
            if (fileVersion != 3) return reader.Position;
            compressionType = reader.ReadByte();
            if (compressionType != 0) return reader.Position;
            compressedSize = reader.ReadUInt32();
            uncompressedSize = reader.ReadUInt32();
            unityVersionCount = reader.ReadByte();
            pUnityVersions = new string[unityVersionCount];
            for (int i = 0; i < unityVersionCount; i++)
            {
                pUnityVersions[i] = reader.ReadCountString();
            }
            stringTableLen = reader.ReadUInt32();
            stringTablePos = reader.ReadUInt32();
            return reader.Position;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos);
        //DWORD _tmp; //used only if assetsVersions == NULL; version 0 only
    }
    public class ClassDatabaseFile
    {
        public bool valid;
        //Only for internal use, otherwise this could create a memory leak!
        public bool dontFreeStringTable;
        public ClassDatabaseFileHeader header;

        public List<ClassDatabaseType> classes;
        //DWORD classCount;
        //ClassDatabaseType *classes;

        public byte[] stringTable;
        ///public ulong Read(AssetsFileReader reader, FileStream readerPar, ulong filePos);
        public bool Read(AssetsFileReader reader)
        {
            header = new ClassDatabaseFileHeader();
            header.Read(reader, 0);
            if (header.header != "cldb" || header.fileVersion != 3 || header.compressionType != 0) {
                valid = false;
                return valid;
            }
            classes = new List<ClassDatabaseType>();
            long classTablePos = reader.BaseStream.Position;
            reader.BaseStream.Position = header.stringTablePos;
            stringTable = reader.ReadBytes((int)header.stringTableLen);
            reader.BaseStream.Position = classTablePos;
            uint size = reader.ReadUInt32();
            for (int i = 0; i < size; i++)
            {
                ClassDatabaseType cdt = new ClassDatabaseType();
                cdt.Read(reader, reader.Position, header.fileVersion);
                classes.Add(cdt);
            }
            valid = true;
            return valid;
        }
        ///public ulong Write(AssetsFileWriter writer, FileStream writerPar, ulong filePos, int optimizeStringTable = 1, uint compress = 1, bool writeStringTable = true);
        public bool IsValid()
        {
            return valid;
        }

        ///public bool InsertFrom(ClassDatabaseFile pOther, ClassDatabaseType pType);
        ///public void Clear();

        public ClassDatabaseFile() { }
        ///public ClassDatabaseFile(ClassDatabaseFile other);
    }
}
