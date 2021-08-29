using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class TypeTree
    {
        public string unityVersion;
        public uint version;
        public bool hasTypeTree;
        public int fieldCount;

        public List<Type_0D> unity5Types;
        public List<Type_07> unity4Types; //todo, unity 4 types not implemented yet

        public uint dwUnknown;

        public void Read(AssetsFileReader reader, uint version)
        {
            unityVersion = reader.ReadNullTerminated();
            this.version = reader.ReadUInt32();
            if (version >= 0x0D)
            {
                hasTypeTree = reader.ReadBoolean();
            }
            fieldCount = reader.ReadInt32();
            unity5Types = new List<Type_0D>();
            for (int i = 0; i < fieldCount; i++)
            {
                Type_0D type0d = new Type_0D();
                type0d.Read(hasTypeTree, reader, version);
                unity5Types.Add(type0d);
            }
            if (version < 0x0E)
            {
                dwUnknown = reader.ReadUInt24();
            }
        }
        public void Write(AssetsFileWriter writer, uint version)
        {
            writer.WriteNullTerminated(unityVersion);
            writer.Write(this.version);
            if (version >= 0x0D)
            {
                writer.Write(hasTypeTree);
            }
            fieldCount = unity5Types.Count;
            writer.Write(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                unity5Types[i].Write(hasTypeTree, writer, version);
            }
            if (version < 0x0E)
            {
                writer.WriteUInt24(dwUnknown);
            }
        }
    }
}
