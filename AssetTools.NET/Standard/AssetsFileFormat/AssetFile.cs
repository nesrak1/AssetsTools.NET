using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools.NET
{
    //for assets that begin with a m_Name field
    public struct AssetFile
    {
        public uint filenameSize;            //0x00 //little-endian
        public byte data;                    //0x04

        ///public string GetFileName(int classId);
        ///public byte GetFileData();
        ///public uint GetFileDataIndex();
    }
}
