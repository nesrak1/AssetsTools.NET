//Very lightweight half remake using magic

using System;
using System.Collections;

namespace Half
{
    public static class Half
    {
        public static float HalfToSingle(ushort half)
        {
            bool[] bits = new bool[32];
            new BitArray(new int[] { half }).CopyTo(bits, 0);
            int sign = ClampBits(Range(bits, 15, 1));
            int exponent = ClampBits(Range(bits, 10, 5));
            int mantissa = ClampBits(Range(bits, 0, 5)) + (ClampBits(Range(bits, 5, 5)) << 5);
            return (1-sign*2f)*(((float)Math.Pow(2,(exponent-15)))*(1+mantissa/1023f));
        }

        static bool[] Range(bool[] bits, int start, int length)
        {
            bool[] result = new bool[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = bits[start+i];
            }
            return result;
        }

        static int ClampBits(bool[] bits)
        {
            byte result = 0x00;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i]) result |= (byte)(1 << (i));
            }
            return result;
        }
    }
}
