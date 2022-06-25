namespace AssetsTools.NET
{
    public class AssetPPtr
    {
        public string FilePath { get; set; }
        public int FileId { get; set; }
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

        public static AssetPPtr FromField(AssetTypeValueField field)
        {
            return new AssetPPtr(field["m_FileID"].AsInt, field["m_PathID"].AsLong);
        }
    }
}
