using AssetsTools.NET.Extra;

namespace AssetsTools.NET
{
    public class AssetPPtr
    {
        /// <summary>
        /// File path of the pointer. If empty or null, FileId will be used.
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// File ID of the pointer.
        /// </summary>
        public int FileId { get; set; }
        /// <summary>
        /// Path ID of the pointer.
        /// </summary>
        public long PathId { get; set; }

        public AssetPPtr()
        {
            FilePath = string.Empty;
            FileId = 0;
            PathId = 0;
        }

        public AssetPPtr(int fileId, long pathId)
        {
            FilePath = string.Empty;
            FileId = fileId;
            PathId = pathId;
        }

        public AssetPPtr(string fileName, long pathId)
        {
            FilePath = fileName;
            FileId = 0;
            PathId = pathId;
        }

        public AssetPPtr(string fileName, int fileId, long pathId)
        {
            FilePath = fileName;
            FileId = fileId;
            PathId = pathId;
        }

        public bool HasFilePath()
        {
            return FilePath != string.Empty && FilePath != null;
        }

        // it may be good enough to just check PathId == 0, not sure yet
        public bool IsNull()
        {
            if (HasFilePath())
                return PathId == 0;
            else
                return FileId == 0 && PathId == 0;
        }

        public void SetFilePathFromFile(AssetsFile file)
        {
            int depIndex = FileId - 1;
            if (FileId > 0 && file.Metadata.Externals.Count < depIndex)
                FilePath = file.Metadata.Externals[depIndex].PathName;
        }

        public void SetFilePathFromFile(AssetsManager am, AssetsFileInstance fileInst)
        {
            if (FileId == 0)
            {
                FilePath = fileInst.path;
                return;
            }

            int depIndex = FileId - 1;
            AssetsFileInstance depInst = fileInst.GetDependency(am, depIndex);
            if (depInst != null)
            {
                FilePath = depInst.path;
            }
        }

        public static AssetPPtr FromField(AssetTypeValueField field)
        {
            return new AssetPPtr(field["m_FileID"].AsInt, field["m_PathID"].AsLong);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AssetPPtr))
            {
                return false;
            }

            AssetPPtr assetPPtr = (AssetPPtr)obj;
            if (assetPPtr.HasFilePath() && HasFilePath())
            {
                return assetPPtr.PathId == PathId && assetPPtr.FilePath == FilePath;
            }
            else if (!assetPPtr.HasFilePath() && !HasFilePath())
            {
                return assetPPtr.PathId == PathId && assetPPtr.FileId == FileId;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (HasFilePath() ? FilePath.GetHashCode() : FileId.GetHashCode());
            hash = hash * 23 + PathId.GetHashCode();
            return hash;
        }
    }
}
