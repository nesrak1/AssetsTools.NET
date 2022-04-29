using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetBundleFSHeader
    {
        /// <summary>
        /// Size of entire file.
        /// </summary>
        public long TotalFileSize { get; set; }
        /// <summary>
        /// Size of the compressed data. This is the same as DecompressedSize if not compressed.
        /// </summary>
        public uint CompressedSize { get; set; }
        /// <summary>
        /// Size of the decompressed data.
        /// </summary>
        public uint DecompressedSize { get; set; }
        /// <summary>
        /// Flags of this bundle. <br/>
        /// First 6 bits (0x3f mask): Compression mode. 0 for uncompressed, 1 for LZ4, 2 for LZMA. <br/>
        /// 0x40: Has directory info. Should always be true for 5.2+. <br/>
        /// 0x80: Block and directory info is at end. The Unity editor does not usually use this.
        /// </summary>
        public uint Flags { get; set; } // todo enum

        public void Read(AssetsFileReader reader)
        {
            TotalFileSize = reader.ReadInt64();
            CompressedSize = reader.ReadUInt32();
            DecompressedSize = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(TotalFileSize);
            writer.Write(CompressedSize);
            writer.Write(DecompressedSize);
            writer.Write(Flags);
        }
    }
}
