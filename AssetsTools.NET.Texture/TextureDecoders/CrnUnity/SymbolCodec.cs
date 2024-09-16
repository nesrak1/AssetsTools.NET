using System.Diagnostics;

namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
internal class SymbolCodec
{
    public byte[] DecodeBuf;
    public int DecodeBufNext;
    public int DecodeBufEnd;

    public uint BitBuf;
    public int BitCount;

    public bool StartDecoding(byte[] buf, int offset, int size)
    {
        if (buf.Length == 0)
            return false;

        DecodeBuf = buf;
        DecodeBufNext = offset;
        DecodeBufEnd = offset + size;

        GetBitsInit();

        return true;
    }

    public bool DecodeReceiveStaticDataModel(StaticHuffmanDataModel model)
    {
        uint totalUsedSyms = DecodeBits(CrnMath.TotalBits(Consts.MAX_SUPPORTED_SYMS));

        if (totalUsedSyms == 0)
        {
            model.Clear();
            return false;
        }

        model.CodeSizes = new byte[totalUsedSyms];

        uint numCodelengthCodesToSend = DecodeBits(5);
        if (numCodelengthCodesToSend < 1 || numCodelengthCodesToSend > Consts.MAX_CODELENGTH_CODES)
            return false;

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        dm.CodeSizes = new byte[Consts.MAX_CODELENGTH_CODES];

        for (int i = 0; i < numCodelengthCodesToSend; i++)
        {
            dm.CodeSizes[Consts.MostProbableCodelengthCodes[i]] = (byte)DecodeBits(3);
        }

        if (!dm.PrepareDecoderTables())
            return false;

        uint ofs = 0;
        while (ofs < totalUsedSyms)
        {
            uint numRemaining = totalUsedSyms - ofs;

            uint code = Decode(dm);
            if (code <= 16)
            {
                model.CodeSizes[(int)ofs++] = (byte)code;
            }
            else if (code == Consts.SMALL_ZERO_RUN_CODE)
            {
                uint len = DecodeBits(Consts.SMALL_ZERO_RUN_EXTRA_BITS) + Consts.MIN_SMALL_ZERO_RUN_SIZE;
                if (len > numRemaining)
                    return false;

                ofs += len;
            }
            else if (code == Consts.LARGE_ZERO_RUN_CODE)
            {
                uint len = DecodeBits(Consts.LARGE_ZERO_RUN_EXTRA_BITS) + Consts.MIN_LARGE_ZERO_RUN_SIZE;
                if (len > numRemaining)
                    return false;

                ofs += len;
            }
            else if (code == Consts.SMALL_REPEAT_CODE || code == Consts.LARGE_REPEAT_CODE)
            {
                uint len;
                if (code == Consts.SMALL_REPEAT_CODE)
                {
                    len = DecodeBits(Consts.SMALL_NON_ZERO_RUN_EXTRA_BITS) + Consts.SMALL_MIN_NON_ZERO_RUN_SIZE;
                }
                else
                {
                    len = DecodeBits(Consts.LARGE_NON_ZERO_RUN_EXTRA_BITS) + Consts.LARGE_MIN_NON_ZERO_RUN_SIZE;
                }

                if (ofs == 0 || len > numRemaining)
                    return false;

                uint prev = model.CodeSizes[(int)ofs - 1];
                if (prev == 0)
                    return false;

                uint end = ofs + len;
                while (ofs < end)
                {
                    model.CodeSizes[(int)ofs++] = (byte)prev;
                }
            }
            else
            {
                Debug.Assert(false);
                return false;
            }
        }

        if (ofs != totalUsedSyms)
            return false;

        return model.PrepareDecoderTables();
    }

    public uint DecodeBits(uint numBits)
    {
        if (numBits == 0)
            return 0;

        if (numBits > 16)
        {
            uint a = GetBits(numBits - 16);
            uint b = GetBits(16);

            return (a << 16) | b;
        }
        else
        {
            return GetBits(numBits);
        }
    }

    public uint Decode(StaticHuffmanDataModel model)
    {
        DecoderTables tables = model.DecodeTables;

        if (BitCount < 24)
        {
            if (BitCount < 16)
            {
                int c0 = 0, c1 = 0;
                int p = DecodeBufNext;
                if (p < DecodeBufEnd)
                {
                    c0 = DecodeBuf[p++];
                }
                if (p < DecodeBufEnd)
                {
                    c1 = DecodeBuf[p++];
                }
                DecodeBufNext = p;
                BitCount += 16;
                uint c = (uint)((c0 << 8) | c1);
                BitBuf |= c << (32 - BitCount);
            }
            else
            {
                uint c = (uint)((DecodeBufNext < DecodeBufEnd) ? DecodeBuf[DecodeBufNext++] : 0);
                BitCount += 8;
                BitBuf |= c << (32 - BitCount);
            }
        }

        uint k = (BitBuf >> 16) + 1;
        uint sym, len;

        if (k <= tables.TableMaxCode)
        {
            uint t = tables.Lookup[BitBuf >> (int)(32 - tables.TableBits)];

            Debug.Assert(t != uint.MaxValue);
            sym = t & ushort.MaxValue;
            len = t >> 16;

            Debug.Assert(model.CodeSizes[(int)sym] == len);
        }
        else
        {
            len = tables.DecodeStartCodeSize;

            while (true)
            {
                if (k <= tables.MaxCodes[len - 1])
                    break;

                len++;
            }

            int valPtr = tables.ValPtrs[len - 1] + (int)(BitBuf >> (32 - (int)len));

            if ((uint)valPtr >= model.TotalSyms)
            {
                Debug.Assert(false);
                return 0;
            }

            sym = tables.SortedSymbolOrder[valPtr];
        }

        BitBuf <<= (int)len;
        BitCount -= (int)len;

        return sym;
    }

    public ulong StopDecoding()
    {
        return (ulong)DecodeBufNext;
    }

    private void GetBitsInit()
    {
        BitBuf = 0;
        BitCount = 0;
    }

    private uint GetBits(uint numBits)
    {
        Debug.Assert(numBits <= 32);

        while (BitCount < numBits)
        {
            uint c = 0;
            if (DecodeBufNext != DecodeBufEnd)
            {
                c = DecodeBuf[DecodeBufNext++];
            }

            BitCount += 8;
            Debug.Assert(BitCount <= Consts.BIT_BUF_SIZE);

            BitBuf |= c << (int)(Consts.BIT_BUF_SIZE - BitCount);
        }

        uint result = BitBuf >> (int)(Consts.BIT_BUF_SIZE - numBits);

        BitBuf <<= (int)numBits;
        BitCount -= (int)numBits;

        return result;
    }
}
