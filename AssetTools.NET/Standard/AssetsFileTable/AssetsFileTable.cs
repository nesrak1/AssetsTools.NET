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

        //Reading names requires a high random access, set readNames to false if you don't need them
        public AssetsFileTable(AssetsFile pFile, bool readNames = true)
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
                assetFileInfoSet.curFileType = (uint)pFile.typeTree.pTypes_Unity5[assetFileInfoSet.curFileTypeOrIndex].classId; //todo: fix this variable (it can be from a different variable (index))
                pAssetFileInfo[i] = assetFileInfoSet;
                //-Check 0x111FB for readNames stuff
            }
        }

        ///public AssetFileInfoEx getAssetInfo(string name);
        ///public AssetFileInfoEx getAssetInfo(string name, uint type);
        public AssetFileInfoEx getAssetInfo(ulong pathId)
        {
            foreach (AssetFileInfoEx info in pAssetFileInfo)
            {
                if (info.index == pathId)
                {
                    return info;
                }
            }
            return null;
        }

        ///public AssetsFileReader getReader();
        ///public FileStream getReaderPar();
    }
}
