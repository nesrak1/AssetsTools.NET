namespace AssetsTools.NET
{
    public class AssetBundleHeader06
    {
        public string signature;
        public uint fileVersion;
        public string minPlayerVersion;
        public string fileEngineVersion;
        public long totalFileSize;
        public uint compressedSize;
        public uint decompressedSize;
        public uint flags;

        public void Read(AssetsFileReader reader)
        {
            reader.bigEndian = true;
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
            writer.bigEndian = true;
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
                if (fileVersion >= 7)
                {
                    if ((flags & 0x100) != 0)
                        return ((ret + 0x0A) + 15) >> 4 << 4;
                    else
                        return ((ret + signature.Length + 1) + 15) >> 4 << 4;
                }
                else
                {
                    if ((flags & 0x100) != 0)
                        return ret + 0x0A;
                    else
                        return ret + signature.Length + 1;
                }
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
            if (fileVersion >= 7)
                ret = (ret + 15) >> 4 << 4;
            if ((flags & 0x80) == 0)
                ret += compressedSize;
            return ret;
        }
        public byte GetCompressionType()
        {
            return (byte)(flags & 0x3F);
        }
    }
}
