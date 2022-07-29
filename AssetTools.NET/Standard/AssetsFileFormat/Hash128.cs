using System.Text;

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

        public bool IsZero()
        {
            if (data == null)
                return true;
            
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);

            foreach (byte b in data)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        public static Hash128 NewBlankHash()
        {
            return new Hash128() { data = new byte[16] };
        }
    }
}
