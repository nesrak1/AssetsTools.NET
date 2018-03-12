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
        public AssetFileInfo fileInfs;              //0x04

        ///public uint GetSizeBytes(uint version);
        ///public ulong Read(ulong version, uint pos, AssetsFileReader reader, FileStream readerPar, bool bigEndian);
        ///public ulong Write(ulong version, uint pos, BinaryWriter writer, FileStream writerPar);
    }
}
