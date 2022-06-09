using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageHeader
    {
        public string Magic { get; set; }
        public byte FileVersion { get; set; }
        public byte CompressionType { get; set; } //todo enum
        public byte DataType { get; set; } //todo enum
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }

        public void Read(AssetsFileReader reader)
        {
            reader.BigEndian = false;
            Magic = reader.ReadStringLength(4);

            if (Magic != "TPK*")
            {
                if (Magic == "CLPK")
                    throw new NotSupportedException("Old CLPK style class packages are no longer supported.");
                else
                    throw new NotSupportedException("TPK* magic not found. Is this really a tpk file?");
            }

            FileVersion = reader.ReadByte();

            if (FileVersion > 1)
                throw new Exception($"Unsupported or invalid file version {FileVersion}.");

            CompressionType = reader.ReadByte();
            DataType = reader.ReadByte();
            reader.ReadByte();   // reserved
            reader.ReadUInt32(); // ...
            CompressedSize = reader.ReadUInt32();
            DecompressedSize = reader.ReadUInt32();
        }
        /*
        public void Write(AssetsFileWriter writer)
        {
            writer.BigEndian = false;
            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(FileVersion);
            writer.Write(CompressionType);
            writer.Write(StringTableOffset);
            writer.Write(StringTableDecompressedSize);
            writer.Write(StringTableCompressedSize);
            if (FileVersion == 1)
                writer.Write(FileBlockSize);
            writer.Write(Files.Count);
            for (int i = 0; i < Files.Count; i++)
            {
                ClassDatabaseFileRef fileRef = Files[i];
                writer.Write(fileRef.offset);
                writer.Write(fileRef.length);

                string fixedFileName = fileRef.name.PadRight(15, '\0').Substring(0, 15);
                writer.Write(Encoding.ASCII.GetBytes(fixedFileName));
            }
        }
        */
    }

    public class ClassDatabaseFileRef
    {
        public uint offset;
        public uint length;
        public string name;
    }
}
