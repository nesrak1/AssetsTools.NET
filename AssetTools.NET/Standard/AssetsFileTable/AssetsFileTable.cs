using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsFileTable
    {
        public AssetsFile pFile;
        public AssetsFileReader reader;
        public Stream readerPar;

        public AssetFileInfoEx[] pAssetFileInfo;
        public uint assetFileInfoCount;

        //assets tools uses a binary search tree, but we'll just use a dictionary instead
        private Dictionary<ulong, int> pLookupBase;
        
        public AssetsFileTable(AssetsFile pFile)
        {
            this.pFile = pFile;
            reader = pFile.reader;
            readerPar = pFile.readerPar;
            reader.bigEndian = pFile.header.endianness == 1 ? true : false;
            reader.BaseStream.Position = pFile.AssetTablePos;
            assetFileInfoCount = pFile.AssetCount;
            pAssetFileInfo = new AssetFileInfoEx[assetFileInfoCount];
            for (int i = 0; i < assetFileInfoCount; i++)
            {
                AssetFileInfoEx assetFileInfoSet = new AssetFileInfoEx();
                assetFileInfoSet.Read(pFile.header.format, reader.Position, reader, pFile.header.endianness == 1 ? true : false);
                assetFileInfoSet.absoluteFilePos = pFile.header.offs_firstFile + assetFileInfoSet.offs_curFile;
                assetFileInfoSet.curFileType = (uint)pFile.typeTree.pTypes_Unity5[assetFileInfoSet.curFileTypeOrIndex].classId;
                pAssetFileInfo[i] = assetFileInfoSet;
            }
        }

        ///public AssetFileInfoEx getAssetInfo(string name);
        ///public AssetFileInfoEx getAssetInfo(string name, uint type);
        public AssetFileInfoEx getAssetInfo(ulong pathId)
        {
            if (pLookupBase != null)
            {
                if (pLookupBase.ContainsKey(pathId))
                {
                    return pAssetFileInfo[pLookupBase[pathId]];
                }
            }
            else
            {
                for (int i = 0; i < pAssetFileInfo.Length; i++)
                {
                    AssetFileInfoEx info = pAssetFileInfo[i];
                    if (info.index == pathId)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        public bool GenerateQuickLookupTree()
        {
            pLookupBase = new Dictionary<ulong, int>();
            for (int i = 0; i < pAssetFileInfo.Length; i++)
            {
                AssetFileInfoEx info = pAssetFileInfo[i];
                pLookupBase[info.index] = i;
            }
            return true;
        }

        ///public AssetsFileReader getReader();
        ///public FileStream getReaderPar();
    }
}
