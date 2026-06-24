using AssetsTools.NET.Extra;
using System.Text;

namespace AssetsTools.NET
{
    public struct Hash128
    {
        public byte[] data; // 16 bytes

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

        public static bool TryParse(string str, out Hash128 guid)
        {
            if (str.Length != 32 || !Net35Polyfill.TryParseHexString(str, out byte[] data))
            {
                guid = NewBlankHash();
                return false;
            }

            guid = new Hash128(data);
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Hash128))
                return false;

            Hash128 cobj = (Hash128)obj;

            if (cobj.data.Length != 16 || data.Length != 16)
                return false;

            byte[] a = cobj.data;
            byte[] b = data;

            // slightly unrolled loop (best we can do with this .NET version & no unsafe)
            for (int i = 0; i < 16; i += 4)
            {
                if (a[i + 0] != b[i + 0] ||
                    a[i + 1] != b[i + 1] ||
                    a[i + 2] != b[i + 2] ||
                    a[i + 3] != b[i + 3])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (data.Length != 16)
                return 0;

            int hashA = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            int hashB = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7];
            int hashC = (data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11];
            int hashD = (data[12] << 24) | (data[13] << 16) | (data[14] << 8) | data[15];

            return hashA ^ hashB ^ hashC ^ hashD;
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
