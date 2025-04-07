namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
internal static class CrnMath
{
    public static bool IsPowerOf2(uint n)
    {
        return n > 0 && ((n & (n - 1)) == 0);
    }

    public static uint NextPowerOf2(uint val)
    {
        val--;
        val |= val >> 16;
        val |= val >> 8;
        val |= val >> 4;
        val |= val >> 2;
        val |= val >> 1;
        return val + 1;
    }

    public static uint TotalBits(uint v)
    {
        uint l = 0;
        while (v > 0)
        {
            v >>= 1;
            l++;
        }
        return l;
    }

    public static uint FloorLog2i(uint v)
    {
        uint l = 0;
        while (v > 1)
        {
            v >>= 1;
            l++;
        }
        return l;
    }

    public static uint CeilLog2i(uint v)
    {
        uint l = FloorLog2i(v);
        if (l != 32 && v > (1 << (int)l))
        {
            return l + 1;
        }
        return l;
    }
}
