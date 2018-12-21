namespace AssetsTools.NET
{
    public struct ClassDatabaseTypeField
    {
        public ClassDatabaseFileString typeName;
        public ClassDatabaseFileString fieldName;
        public byte depth;
        public byte isArray;
        public uint size;
        public ushort version;
        public uint flags2;

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
        }
        public ulong Write(AssetsFileWriter writer, ulong filePos, int version)
        {
            typeName.Write(writer, filePos);
            fieldName.Write(writer, filePos);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(size);
            writer.Write(this.version);
            writer.Write(flags2);
            return writer.Position;
        }
    }
}
