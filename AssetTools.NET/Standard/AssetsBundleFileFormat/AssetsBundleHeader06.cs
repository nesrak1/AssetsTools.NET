namespace AssetsTools.NET
{
    //Unity 5.3+
    public struct AssetsBundleHeader06
    {
        //no alignment in this struct!
        public string signature; //0-terminated; UnityFS, UnityRaw, UnityWeb or UnityArchive
        public uint fileVersion; //big-endian, = 6
        public string minPlayerVersion; //0-terminated; 5.x.x
        public string fileEngineVersion; //0-terminated; exact unity engine version
        public ulong totalFileSize;
        //sizes for the blocks info :
        public uint compressedSize;
        public uint decompressedSize;
        //(flags & 0x3F) is the compression mode (0 = none; 1 = LZMA; 2-3 = LZ4)
        //(flags & 0x40) says whether the bundle has directory info
        //(flags & 0x80) says whether the block and directory list is at the end
        public uint flags;

        ///public bool ReadInitial(AssetsFileReader reader, LPARAM lPar, AssetsFileVerifyLogger errorLogger = NULL);
        public bool Read(AssetsFileReader reader/*, AssetsFileVerifyLogger errorLogger = null*/)
        {
            signature = reader.ReadNullTerminated();
            fileVersion = reader.ReadUInt32();
            minPlayerVersion = reader.ReadNullTerminated();
            fileEngineVersion = reader.ReadNullTerminated();
            totalFileSize = reader.ReadUInt64();
            compressedSize = reader.ReadUInt32();
            decompressedSize = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            return true;
        }
        public bool Write(AssetsFileWriter writer, ulong curFilePos/*, AssetsFileVerifyLogger errorLogger = NULL*/)
        {
            writer.Position = curFilePos;
            writer.WriteNullTerminated(signature);
            writer.Write(fileVersion);
            writer.WriteNullTerminated(minPlayerVersion);
            writer.WriteNullTerminated(fileEngineVersion);
            writer.Write(totalFileSize);
            writer.Write(compressedSize);
            writer.Write(decompressedSize);
            writer.Write(flags);
            return true;
        }
        public ulong GetBundleInfoOffset()
        {
            if ((this.flags & 0x80) != 0)
            {
                if (this.totalFileSize == 0)
                    return unchecked((ulong)-1);
                return this.totalFileSize - this.compressedSize;
            }
            else
            {
                //if (!strcmp(this->signature, "UnityWeb") || !strcmp(this->signature, "UnityRaw"))
                //	return 9;
                ulong ret = (ulong)(minPlayerVersion.Length + fileEngineVersion.Length + 0x1A);
                if ((this.flags & 0x100) != 0)
                    return (ret + 0x0A);
                else
                    return (ret + (ulong)signature.Length + 1);
            }
        }
        public uint GetFileDataOffset()
        {
            uint ret = 0;
            if (this.signature == "UnityArchive")
                return this.compressedSize;
            else if (this.signature == "UnityFS" || this.signature == "UnityWeb")
            {
                ret = (uint)minPlayerVersion.Length + (uint)fileEngineVersion.Length + 0x1A;
                if ((this.flags & 0x100) != 0)
                    ret += 0x0A;
                else
                    ret += (uint)signature.Length + 1;
            }
            if ((this.flags & 0x80) == 0)
                ret += this.compressedSize;
            return ret;
        }
    }
}
