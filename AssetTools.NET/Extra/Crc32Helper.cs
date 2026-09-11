namespace AssetsTools.NET.Extra
{
    public static class Crc32Helper
    {
        public const uint InitialValue = 0xFFFFFFFF;
        public const int ChunkSize = 0x8000;

        private static readonly uint[] Table = BuildTable();

        public static uint Feed(uint crc, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                crc = Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }
            return crc;
        }

        public static uint Finalize(uint crc)
        {
            return ~crc;
        }

        private static uint[] BuildTable()
        {
            const uint poly = 0xEDB88320;
            uint[] table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                uint c = i;
                for (int j = 0; j < 8; j++)
                {
                    c = ((c & 1) != 0) ? ((c >> 1) ^ poly) : (c >> 1);
                }
                table[i] = c;
            }
            return table;
        }
    }
}
