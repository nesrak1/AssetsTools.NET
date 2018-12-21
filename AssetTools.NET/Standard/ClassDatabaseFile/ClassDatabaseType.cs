using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassDatabaseType
    {
        public int classId;
        public int baseClass;
        public ClassDatabaseFileString name;

        public List<ClassDatabaseTypeField> fields;
        public ulong Read(AssetsFileReader reader, ulong filePos, int version)
        {
            classId = reader.ReadInt32();
            baseClass = reader.ReadInt32();
            name = new ClassDatabaseFileString();
            name.Read(reader, reader.Position);
            uint fieldCount = reader.ReadUInt32();
            fields = new List<ClassDatabaseTypeField>();
            for (int i = 0; i < fieldCount; i++)
            {
                ClassDatabaseTypeField cdtf = new ClassDatabaseTypeField();
                cdtf.Read(reader, filePos, version);
                fields.Add(cdtf);
            }
            return reader.Position;
        }
        public ulong Write(AssetsFileWriter writer, ulong filePos, int version)
        {
            writer.Write(classId);
            writer.Write(baseClass);
            name.Write(writer, filePos);
            writer.Write(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].Write(writer, filePos, version);
            }
            return writer.Position;
        }
    }
}
