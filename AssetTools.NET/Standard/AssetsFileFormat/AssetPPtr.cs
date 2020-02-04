namespace AssetsTools.NET
{
    public class AssetPPtr
    {
        public AssetPPtr(int fileID, long pathID)
        {
            this.fileID = fileID;
            this.pathID = pathID;
        }
        public int fileID;
        public long pathID;
    }
}
