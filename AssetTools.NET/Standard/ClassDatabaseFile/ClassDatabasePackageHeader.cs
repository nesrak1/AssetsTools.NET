using System.Collections.Generic;

namespace AssetsTools.NET
{
    public struct ClassDatabasePackageHeader
    {
        public string magic;
        public byte fileVersion;
        public byte compressionType;
        public uint stringTableOffset, stringTableLenUncompressed, stringTableLenCompressed;
        public uint fileBlockSize;
        public uint fileCount;
        public List<ClassDatabaseFileRef> files;
        public void Read(AssetsFileReader reader)
        {
            reader.bigEndian = false;
            magic = reader.ReadStringLength(4);
            fileVersion = reader.ReadByte();
            compressionType = reader.ReadByte();
            stringTableOffset = reader.ReadUInt32();
            stringTableLenUncompressed = reader.ReadUInt32();
            stringTableLenCompressed = reader.ReadUInt32();
            fileBlockSize = reader.ReadUInt32();
            fileCount = reader.ReadUInt32();
            files = new List<ClassDatabaseFileRef>();
            for (int i = 0; i < fileCount; i++)
            {
                files.Add(new ClassDatabaseFileRef()
                {
                    offset = reader.ReadUInt32(),
                    length = reader.ReadUInt32(),
                    name = reader.ReadStringLength(15)
                });
            }
        }
    }
    public struct ClassDatabaseFileRef
    {
        public uint offset;
        public uint length;
        public string name;
    }
}
