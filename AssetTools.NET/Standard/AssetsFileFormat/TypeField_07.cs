namespace AssetsTools.NET
{
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
}
