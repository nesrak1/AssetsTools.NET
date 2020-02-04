namespace AssetsTools.NET
{
    //Unity 5.3+
    public struct AssetBundleHeader06
    {
        //no alignment in this struct!
        public string signature; //0-terminated; UnityFS, UnityRaw, UnityWeb or UnityArchive
        public uint fileVersion; //big-endian, = 6
        public string minPlayerVersion; //0-terminated; 5.x.x
        public string fileEngineVersion; //0-terminated; exact unity engine version
        public long totalFileSize;
        //sizes for the blocks info :
        public uint compressedSize;
        public uint decompressedSize;
        //(flags & 0x3F) is the compression mode (0 = none; 1 = LZMA; 2-3 = LZ4)
        //(flags & 0x40) says whether the bundle has directory info
        //(flags & 0x80) says whether the block and directory list is at the end
        public uint flags;

        ///public bool ReadInitial(AssetsFileReader reader, LPARAM lPar, AssetsFileVerifyLogger errorLogger = NULL);
        public void Read(AssetsFileReader reader)
        {
            signature = reader.ReadNullTerminated();
            fileVersion = reader.ReadUInt32();
            minPlayerVersion = reader.ReadNullTerminated();
            fileEngineVersion = reader.ReadNullTerminated();
            totalFileSize = reader.ReadInt64();
            compressedSize = reader.ReadUInt32();
            decompressedSize = reader.ReadUInt32();
            flags = reader.ReadUInt32();
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(signature);
            writer.Write(fileVersion);
            writer.WriteNullTerminated(minPlayerVersion);
            writer.WriteNullTerminated(fileEngineVersion);
            writer.Write(totalFileSize);
            writer.Write(compressedSize);
            writer.Write(decompressedSize);
            writer.Write(flags);
        }
        public long GetBundleInfoOffset()
        {
            if ((flags & 0x80) != 0)
            {
                if (totalFileSize == 0)
                    return -1;
                return totalFileSize - compressedSize;
            }
            else
            {
                //if (!strcmp(this->signature, "UnityWeb") || !strcmp(this->signature, "UnityRaw"))
                //	return 9;
                long ret = minPlayerVersion.Length + fileEngineVersion.Length + 0x1A;
                if ((flags & 0x100) != 0)
                    return ret + 0x0A;
                else
                    return ret + signature.Length + 1;
            }
        }
        public long GetFileDataOffset()
        {
            long ret = 0;
            if (signature == "UnityArchive")
                return compressedSize;
            else if (signature == "UnityFS" || signature == "UnityWeb")
            {
                ret = minPlayerVersion.Length + fileEngineVersion.Length + 0x1A;
                if ((flags & 0x100) != 0)
                    ret += 0x0A;
                else
                    ret += signature.Length + 1;
            }
            if ((flags & 0x80) == 0)
                ret += compressedSize;
            return ret;
        }
    }
}
