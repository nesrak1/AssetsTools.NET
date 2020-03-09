namespace AssetsTools.NET
{
    public struct ClassDatabaseTypeField
    {
        public ClassDatabaseFileString typeName;
        public ClassDatabaseFileString fieldName;
        public byte depth;
        public byte isArray;
        public int size;
        public ushort version;
        public uint flags2;

        public void Read(AssetsFileReader reader, int version)
        {
            typeName = new ClassDatabaseFileString();
            typeName.Read(reader);
            fieldName = new ClassDatabaseFileString();
            fieldName.Read(reader);
            depth = reader.ReadByte();
            isArray = reader.ReadByte();
            size = reader.ReadInt32();
            switch (version)
            {
                case 1:
                    flags2 = reader.ReadUInt32();
                    break;
                case 3:
                case 4:
                    this.version = reader.ReadUInt16();
                    flags2 = reader.ReadUInt32();
                    break;
            }
        }
        public void Write(AssetsFileWriter writer, int version)
        {
            typeName.Write(writer);
            fieldName.Write(writer);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(size);
            switch (version)
            {
                case 1:
                    writer.Write(flags2);
                    break;
                case 3:
                case 4:
                    writer.Write(this.version);
                    writer.Write(flags2);
                    break;
            }
        }
    }
}
