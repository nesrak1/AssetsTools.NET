namespace AssetsTools.NET
{
    public struct Type_07
    {
        public int classId; //big endian
        public TypeField_07 @base;

        ///public ulong Read(bool hasTypeTree, ulong absFilePos, AssetsFileReader reader, FileStream readerPar, uint version, uint typeVersion, bool bigEndian);
        ///public ulong Write(bool hasTypeTree, ulong absFilePos, BinaryWriter writer, FileStream writerPar);
    }
}
