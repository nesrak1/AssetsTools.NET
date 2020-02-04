using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetFileInfo
    {
        public long index;                   //0x00 //little-endian //version < 0x0E : only DWORD
        public uint curFileOffset;           //0x08 //little-endian
        public uint curFileSize;             //0x0C //little-endian
        public int curFileTypeOrIndex;       //0x10 //little-endian //starting with version 0x10, this is an index into the type tree
        //inheritedUnityClass                //for Unity classes, this is curFileType; for MonoBehaviours, this is 114
        //version < 0x0B                     //inheritedUnityClass is DWORD, no scriptIndex exists
        public ushort inheritedUnityClass;   //0x14 //little-endian (MonoScript)//only version < 0x10
        //scriptIndex                        //for Unity classes, this is 0xFFFF; for MonoBehaviours, this is the index of the script in the MonoManager asset
        public ushort scriptIndex;           //0x16 //little-endian//only version <= 0x10
        public byte unknown1;                //0x18 //only 0x0F <= version <= 0x10 //with alignment always a DWORD
        public static int GetSize(uint version)
        {
            int size = 0;
            size += 4;
            if (version >= 0x0E) size += 4;
            size += 12;
            if (version < 0x10) size += 2;
            if (version <= 0x10) size += 2;
            if (0x0F <= version && version <= 0x10) size += 4;
            return size;
        }
        public void Read(uint version, AssetsFileReader reader)
        {
            if (version >= 0x0E)
            {
                index = reader.ReadInt64();
            }
            else
            {
                index = reader.ReadUInt32();
            }
            curFileOffset = reader.ReadUInt32();
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
                reader.ReadBytes(3);
            }
        }
        public void Write(uint version, AssetsFileWriter writer)
        {
            if (version >= 0x0E)
            {
                writer.Write(index);
            }
            else
            {
                writer.Write((uint)index);
            }
            writer.Write(curFileOffset);
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
                writer.Write(new byte[] { 0x00, 0x00, 0x00 });
            }
        }
    }
}
