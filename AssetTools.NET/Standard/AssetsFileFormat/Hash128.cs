namespace AssetsTools.NET
{
    public struct Hash128
    {
        public byte[] data; //16 bytes
        public Hash128(byte[] data)
        {
            this.data = data;
        }
        public Hash128(AssetsFileReader reader)
        {
            data = reader.ReadBytes(16);
        }
    }
}
