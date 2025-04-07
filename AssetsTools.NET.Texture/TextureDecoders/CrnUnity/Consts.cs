namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
public class Consts
{
    public const uint HEADER_MIN_SIZE = 62;
    public const uint MAGIC_VALUE = 0x1EF9CABD;
    public const ushort SIG_VALUE = ('H' << 8) | 'x';

    public const uint MAX_EXPECTED_CODE_SIZE = 16;
    public const uint MAX_SUPPORTED_SYMS = 8192;
    public const uint MAX_TABLE_BITS = 11;

    public const uint BIT_BUF_SIZE = 32;

    public const int MAX_LEVEL_RESOLUTION = 4096;
    public const int MIN_PALETTE_SIZE = 8;
    public const int MAX_PALETTE_SIZE = 8192;
    public const int MAX_FACES = 6;
    public const int MAX_LEVELS = 16;
    public const int MAX_HELPER_THREADS = 15;
    public const int MIN_QUALITY_LEVEL = 0;
    public const int MAX_QUALITY_LEVEL = 255;

    public const uint MAX_CODELENGTH_CODES = 21;

    public const uint SMALL_ZERO_RUN_CODE = 17;
    public const uint LARGE_ZERO_RUN_CODE = 18;
    public const uint SMALL_REPEAT_CODE = 19;
    public const uint LARGE_REPEAT_CODE = 20;

    public const uint MIN_SMALL_ZERO_RUN_SIZE = 3;
    public const uint MAX_SMALL_ZERO_RUN_SIZE = 10;
    public const uint MIN_LARGE_ZERO_RUN_SIZE = 11;
    public const uint MAX_LARGE_ZERO_RUN_SIZE = 138;

    public const uint SMALL_MIN_NON_ZERO_RUN_SIZE = 3;
    public const uint SMALL_MAX_NON_ZERO_RUN_SIZE = 6;
    public const uint LARGE_MIN_NON_ZERO_RUN_SIZE = 7;
    public const uint LARGE_MAX_NON_ZERO_RUN_SIZE = 70;

    public const uint SMALL_ZERO_RUN_EXTRA_BITS = 3;
    public const uint LARGE_ZERO_RUN_EXTRA_BITS = 7;
    public const uint SMALL_NON_ZERO_RUN_EXTRA_BITS = 2;
    public const uint LARGE_NON_ZERO_RUN_EXTRA_BITS = 6;

    public static readonly byte[] MostProbableCodelengthCodes =
    {
        (byte)SMALL_ZERO_RUN_CODE, (byte)LARGE_ZERO_RUN_CODE,
        (byte)SMALL_REPEAT_CODE, (byte)LARGE_REPEAT_CODE,
        0, 8,
        7, 9,
        6, 10,
        5, 11,
        4, 12,
        3, 13,
        2, 14,
        1, 15,
        16
    };

    public static readonly byte[] Dxt1ToLinear = { 0, 3, 1, 2 };
    public static readonly byte[] Dxt1FromLinear = { 0, 2, 3, 1 };
    public static readonly byte[] Etc1FromLinear = { 3, 2, 0, 1 };

    public static readonly byte[] Dxt5ToLinear = { 0, 7, 1, 2, 3, 4, 5, 6 };
    public static readonly byte[] Dxt5FromLinear = { 0, 2, 3, 4, 5, 6, 7, 1 };

    public static readonly byte[] SixAlphaInvertTable = { 1, 0, 5, 4, 3, 2, 6, 7 };
    public static readonly byte[] EightAlphaInvertTable = { 1, 0, 7, 6, 5, 4, 3, 2 };
}
