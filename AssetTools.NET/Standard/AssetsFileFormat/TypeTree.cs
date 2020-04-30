using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class TypeTree
    {
        public string unityVersion; //null-terminated; stored for .assets format > 6
        public uint version; //big endian; stored for .assets format > 6
        public bool hasTypeTree; //stored for .assets format >= 13; Unity 5 only stores some metadata if it's set to false
        public int fieldCount; //big endian;

        public List<Type_0D> unity5Types;
        public List<Type_07> unity4Types; //todo, unity 4 types not implemented yet

        public uint dwUnknown; //actually belongs to the asset list; stored for .assets format < 14

        public void Read(AssetsFileReader reader, uint version)
        {
            unityVersion = reader.ReadNullTerminated();
            this.version = reader.ReadUInt32();
            hasTypeTree = reader.ReadBoolean();
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
        }//Minimum AssetsFile format : 6
        public void Write(AssetsFileWriter writer, uint version)
        {
            writer.WriteNullTerminated(unityVersion);
            writer.Write(this.version);
            writer.Write(hasTypeTree);
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

        ///public void Clear();
    }
}
