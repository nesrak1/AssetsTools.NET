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

        public void SetFilePathFromFile(AssetsFile file)
        {
            FilePath = file.Metadata.Externals[FileId].PathName;
        }
    }
}
