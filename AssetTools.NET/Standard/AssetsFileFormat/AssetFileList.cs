using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetFileList
    {
        public uint sizeFiles;
        public AssetFileInfo[] fileInfs;

        public uint GetSizeBytes(uint version)
        {
            if (fileInfs == null)
            {
                return 0;
            } else
            {
                return (uint)AssetFileInfo.GetSize(version) * sizeFiles + 4;
            }
        }
        public void Read(uint version, AssetsFileReader reader)
        {
            sizeFiles = reader.ReadUInt32();
            fileInfs = new AssetFileInfo[sizeFiles];
            for (int i = 0; i < sizeFiles; i++)
            {
                fileInfs[i] = new AssetFileInfo();
                fileInfs[i].Read(version, reader);
            }
        }
        public void Write(uint version, AssetsFileWriter writer)
        {
            writer.Write(sizeFiles);
            for (int i = 0; i < sizeFiles; i++)
            {
                fileInfs[i].Write(version, writer);
            }
        }
    }
}
