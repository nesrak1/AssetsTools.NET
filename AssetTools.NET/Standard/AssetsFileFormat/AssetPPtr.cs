namespace AssetsTools.NET
{
    public class AssetPPtr
    {
        public AssetPPtr(uint fileID, ulong pathID)
        {
            this.fileID = fileID;
            this.pathID = pathID;
        }
        public uint fileID;
        public ulong pathID;
    }
}
