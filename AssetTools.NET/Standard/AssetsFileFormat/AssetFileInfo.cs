using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetFileInfo
    {
        public long index;
        public long curFileOffset;
        public uint curFileSize;
        public int curFileTypeOrIndex;
        public ushort inheritedUnityClass;
        public ushort scriptIndex;
        public byte unknown1;
        public static int GetSize(uint version)
        {
            int size = 0;
            size += 4;
            if (version >= 0x0E) size += 4;
            size += 12;
            if (version >= 0x16) size += 4;
            if (version < 0x10) size += 2;
            if (version <= 0x10) size += 2;
            if (0x0F <= version && version <= 0x10) size += 1;
            return size;
        }
        public void Read(uint version, AssetsFileReader reader)
        {
            reader.Align();
            if (version >= 0x0E)
            {
                index = reader.ReadInt64();
            }
            else
            {
                index = reader.ReadUInt32();
            }
            if (version >= 0x16)
            {
                curFileOffset = reader.ReadInt64();
            }
            else
            {
                curFileOffset = reader.ReadUInt32();
            }
            curFileSize = reader.ReadUInt32();
            curFileTypeOrIndex = reader.ReadInt32();
            if (version < 0x10)
            {
                inheritedUnityClass = reader.ReadUInt16();
            }
            if (version <= 0x10)
            {
                scriptIndex = reader.ReadUInt16();
            }
            if (0x0F <= version && version <= 0x10)
            {
                unknown1 = reader.ReadByte();
            }
        }
        public void Write(uint version, AssetsFileWriter writer)
        {
            writer.Align();
            if (version >= 0x0E)
            {
                writer.Write(index);
            }
            else
            {
                writer.Write((uint)index);
            }
            if (version >= 0x16)
            {
                writer.Write(curFileOffset);
            }
            else
            {
                writer.Write((uint)curFileOffset);
            }
            writer.Write(curFileSize);
            writer.Write(curFileTypeOrIndex);
            if (version < 0x10)
            {
                writer.Write(inheritedUnityClass);
            }
            if (version <= 0x10)
            {
                writer.Write(scriptIndex);
            }
            if (0x0F <= version && version <= 0x10)
            {
                writer.Write(unknown1);
            }
        }
    }
}
