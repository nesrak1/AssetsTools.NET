using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools.NET
{
    public struct AssetFileList
    {
        public uint sizeFiles;                      //0x00 //little-endian
        public AssetFileInfo[] fileInfs;            //0x04

        public uint GetSizeBytes(uint version)
        {
            if (fileInfs == null)
            {
                return 0;
            } else
            {
                return AssetFileInfo.GetSize(version) * sizeFiles + 4;
            }
        }
        public ulong Read(uint version, ulong pos, AssetsFileReader reader, bool bigEndian)
        {
            sizeFiles = reader.ReadUInt32();
            fileInfs = new AssetFileInfo[sizeFiles];
            for (int i = 0; i < sizeFiles; i++)
            {
                fileInfs[i] = new AssetFileInfo();
                fileInfs[i].Read(version, pos, reader, bigEndian);
            }
            return reader.Position;
        }
        public ulong Write(uint version, ulong pos, AssetsFileWriter writer)
        {
            writer.Write(sizeFiles);
            for (int i = 0; i < sizeFiles; i++)
            {
                fileInfs[i].Write(version, pos, writer);
            }
            return writer.Position;
        }
    }
}
