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
            for (int j = 0; j < fieldCount; j++)
            {
                ClassDatabaseTypeField cdtf = new ClassDatabaseTypeField();
                cdtf.Read(reader, filePos, version);
                fields.Add(cdtf);
            }
            return reader.Position;
        }
    }
}
