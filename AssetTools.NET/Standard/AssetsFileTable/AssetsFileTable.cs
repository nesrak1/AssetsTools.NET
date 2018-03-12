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
            reader = new AssetsFileReader(pFile.readerPar); //-todo, look back and see why I made a new reader here
            readerPar = pFile.readerPar;
            reader.bigEndian = pFile.header.endianness == 1 ? true : false;
            reader.BaseStream.Position = pFile.AssetTablePos;
            assetFileInfoCount = pFile.AssetCount;
            pAssetFileInfo = new AssetFileInfoEx[assetFileInfoCount];
            for (int i = 0; i < assetFileInfoCount; i++)
            {
                AssetFileInfoEx assetFileInfoSet = new AssetFileInfoEx();
                if (pFile.header.format >= 0x0E)
                {
                    assetFileInfoSet.index = reader.ReadUInt64();
                }
                else
                {
                    assetFileInfoSet.index = reader.ReadUInt32();
                }
                assetFileInfoSet.offs_curFile = reader.ReadUInt32();
                assetFileInfoSet.curFileSize = reader.ReadUInt32();
                assetFileInfoSet.curFileTypeOrIndex = reader.ReadUInt32();
                if (pFile.header.format < 0x10)
                {
                    assetFileInfoSet.inheritedUnityClass = reader.ReadUInt16();
                }
                if (pFile.header.format <= 0x10)
                {
                    assetFileInfoSet.scriptIndex = reader.ReadUInt16();
                }
                if (0x0F <= pFile.header.format && pFile.header.format <= 0x10)
                {
                    assetFileInfoSet.unknown1 = reader.ReadByte();
                    reader.ReadBytes(3);
                }
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
