using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsFileTable
    {
        public AssetsFile file;
        public AssetsFileReader reader;
        public Stream readerPar;

        public AssetFileInfoEx[] assetFileInfo;
        public uint assetFileInfoCount;

        //assets tools uses a binary search tree, but we'll just use a dictionary instead
        private Dictionary<long, int> lookupBase;
        
        public AssetsFileTable(AssetsFile file)
        {
            this.file = file;
            reader = file.reader;
            readerPar = file.readerPar;
            reader.bigEndian = file.header.endianness == 1 ? true : false;
            reader.BaseStream.Position = file.assetTablePos;
            assetFileInfoCount = file.assetCount;
            assetFileInfo = new AssetFileInfoEx[assetFileInfoCount];
            for (int i = 0; i < assetFileInfoCount; i++)
            {
                AssetFileInfoEx assetFileInfoSet = new AssetFileInfoEx();
                assetFileInfoSet.Read(file.header.format, reader);
                assetFileInfoSet.absoluteFilePos = file.header.firstFileOffset + assetFileInfoSet.curFileOffset;
                if (file.header.format < 0x10)
                {
                    if (assetFileInfoSet.curFileTypeOrIndex < 0)
                    {
                        assetFileInfoSet.curFileType = 0x72;
                    } else
                    {
                        assetFileInfoSet.curFileType = (uint)assetFileInfoSet.curFileTypeOrIndex;
                    }
                } else
                {
                    assetFileInfoSet.curFileType = (uint)file.typeTree.unity5Types[assetFileInfoSet.curFileTypeOrIndex].classId;
                }
                
                assetFileInfo[i] = assetFileInfoSet;
            }
        }
        
        public AssetFileInfoEx GetAssetInfo(long pathId)
        {
            if (lookupBase != null)
            {
                if (lookupBase.ContainsKey(pathId))
                {
                    return assetFileInfo[lookupBase[pathId]];
                }
            }
            else
            {
                for (int i = 0; i < assetFileInfo.Length; i++)
                {
                    AssetFileInfoEx info = assetFileInfo[i];
                    if (info.index == pathId)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        public void GenerateQuickLookupTree()
        {
            lookupBase = new Dictionary<long, int>();
            for (int i = 0; i < assetFileInfo.Length; i++)
            {
                AssetFileInfoEx info = assetFileInfo[i];
                lookupBase[info.index] = i;
            }
        }
    }
}
