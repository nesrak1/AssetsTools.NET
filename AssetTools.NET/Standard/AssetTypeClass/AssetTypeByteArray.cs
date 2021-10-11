namespace AssetsTools.NET
{
    public struct AssetTypeByteArray
    {
        public AssetTypeByteArray(byte[] data)
        {
            this.size = (uint)data.Length;
            this.data = data;
        }

        public uint size;
        public byte[] data;
    }
}
