using System;

namespace AssetsTools.NET
{
    public class AssetBundleHeader
    {
        /// <summary>
        /// Magic appearing at the beginning of all bundles. Possible options are:
        /// UnityFS, UnityWeb, UnityRaw, UnityArchive
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// Version of this file.
        /// </summary>
        public uint Version { get; set; }
        /// <summary>
        /// Generation version string. For Unity 5 bundles this is always "5.x.x"
        /// </summary>
        public string GenerationVersion { get; set; }
        /// <summary>
        /// Engine version. This is the specific version string being used. For example, "2019.4.2f1"
        /// </summary>
        public string EngineVersion { get; set; }

        /// <summary>
        /// Header for bundles with a UnityFS Signature.
        /// </summary>
        public AssetBundleFSHeader FileStreamHeader { get; set; }

        public void Read(AssetsFileReader reader)
        {
            reader.BigEndian = true;
            Signature = reader.ReadNullTerminated();
            Version = reader.ReadUInt32();
            GenerationVersion = reader.ReadNullTerminated();
            EngineVersion = reader.ReadNullTerminated();
            if (Signature == "UnityFS")
            {
                FileStreamHeader = new AssetBundleFSHeader();
                FileStreamHeader.Read(reader);
            }
            else
            {
                throw new NotSupportedException($"{Signature} signature not supported!");
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.BigEndian = true;
            writer.WriteNullTerminated(Signature);
            writer.Write(Version);
            writer.WriteNullTerminated(GenerationVersion);
            writer.WriteNullTerminated(EngineVersion);
            if (Signature == "UnityFS")
            {
                FileStreamHeader.Write(writer);
            }
            else
            {
                throw new NotSupportedException($"{Signature} signature not supported!");
            }
        }
        
        public long GetBundleInfoOffset()
        {
            if (Signature != "UnityFS")
                throw new NotSupportedException($"{Signature} signature not supported!");

            AssetBundleFSHeaderFlags flags = FileStreamHeader.Flags;
            long totalFileSize = FileStreamHeader.TotalFileSize;
            long compressedSize = FileStreamHeader.CompressedSize;

            if ((flags & AssetBundleFSHeaderFlags.BlockAndDirAtEnd) != 0)
            {
                if (totalFileSize == 0)
                    return -1;
                return totalFileSize - compressedSize;
            }
            else
            {
                long ret = GenerationVersion.Length + EngineVersion.Length + 0x1a;
                if (Version >= 7)
                {
                    if ((flags & AssetBundleFSHeaderFlags.OldWebPluginCompatibility) != 0)
                        return ((ret + 0x0a) + 15) & ~15;
                    else
                        return ((ret + Signature.Length + 1) + 15) & ~15;
                }
                else
                {
                    if ((flags & AssetBundleFSHeaderFlags.OldWebPluginCompatibility) != 0)
                        return ret + 0x0a;
                    else
                        return ret + Signature.Length + 1;
                }
            }
        }

        public long GetFileDataOffset()
        {
            if (Signature != "UnityFS")
                throw new NotSupportedException($"{Signature} signature not supported!");

            AssetBundleFSHeaderFlags flags = FileStreamHeader.Flags;
            long compressedSize = FileStreamHeader.CompressedSize;

            long ret = GenerationVersion.Length + EngineVersion.Length + 0x1a;
            if ((flags & AssetBundleFSHeaderFlags.OldWebPluginCompatibility) != 0)
                ret += 0x0a;
            else
                ret += Signature.Length + 1;

            if (Version >= 7)
                ret = (ret + 15) & ~15;
            if ((flags & AssetBundleFSHeaderFlags.BlockAndDirAtEnd) == 0)
                ret += compressedSize;
            if ((flags & AssetBundleFSHeaderFlags.BlockInfoNeedPaddingAtStart) != 0)
                ret = (ret + 15) & ~15;

            return ret;
        }

        // todo: enum
        public byte GetCompressionType()
        {
            if (Signature != "UnityFS")
                throw new NotSupportedException($"{Signature} signature not supported!");

            return (byte)(FileStreamHeader.Flags & AssetBundleFSHeaderFlags.CompressionMask);
        }
    }
}
