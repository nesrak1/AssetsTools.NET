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
            if (str.stringTableOffset != 0xFFFFFFFF) //total guess haha, havent messed with inline strings much
            {
                fromStringTable = true;
            }
            else
            {
                //untested, probably wrong
                fromStringTable = false;
                str.@string = reader.ReadCountString(); //this may be different
            }
            return reader.Position;
        }
    }
}
