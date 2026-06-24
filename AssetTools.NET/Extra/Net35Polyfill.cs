using System;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public static class Net35Polyfill
    {
        //https://stackoverflow.com/a/13022108
        public static void CopyToCompat(this Stream input, Stream output, long bytes = -1, int bufferSize = 80 * 1024)
        {
            byte[] buffer = new byte[bufferSize];
            int read;

            // set to largest value so we always go over buffer (hopefully)
            if (bytes == -1)
                bytes = long.MaxValue;

            // bufferSize will always be an int so if bytes is larger, it's also under the size of an int
            while (bytes > 0 && (read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        //https://stackoverflow.com/a/4108907
        public static bool HasFlag(Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }

        public static bool TryParseHexString(string str, out byte[] data)
        {
            data = new byte[0];
            if ((str.Length % 2) != 0)
                return false;

            data = new byte[str.Length / 2];
            for (int srcIdx = 0, dstIdx = 0; srcIdx < str.Length; srcIdx += 2, dstIdx++)
            {
                uint charA = HexCharacterValue(str[srcIdx]);
                uint charB = HexCharacterValue(str[srcIdx + 1]);
                if (charA == uint.MaxValue || charB == uint.MaxValue)
                    return false;

                data[dstIdx] = (byte)((charA << 4) | charB);
            }

            return true;
        }

        private static uint HexCharacterValue(char c)
        {
            return c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'a' => 10,
                'b' => 11,
                'c' => 12,
                'd' => 13,
                'e' => 14,
                'f' => 15,
                _ => uint.MaxValue
            };
        }
    }
}
