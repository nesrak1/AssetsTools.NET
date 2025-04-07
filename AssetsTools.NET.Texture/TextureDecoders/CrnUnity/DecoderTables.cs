using System;
using System.Diagnostics;
using System.Linq;

namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
internal class DecoderTables
{
    public uint NumSyms;
    public uint TotalUsedSyms;
    public uint TableBits;
    public uint TableShift;
    public uint TableMaxCode;
    public uint DecodeStartCodeSize;

    public byte MinCodeSize;
    public byte MaxCodeSize;

    public uint[] MaxCodes;
    public int[] ValPtrs;

    public uint[] Lookup;
    public ushort[] SortedSymbolOrder;

    public DecoderTables()
    {
        MaxCodes = new uint[Consts.MAX_EXPECTED_CODE_SIZE + 1];
        ValPtrs = new int[Consts.MAX_EXPECTED_CODE_SIZE + 1];

        Lookup = [];
        SortedSymbolOrder = [];
    }

    public DecoderTables(DecoderTables other)
    {
        Clear();

        NumSyms = other.NumSyms;
        TotalUsedSyms = other.TotalUsedSyms;
        TableBits = other.TableBits;
        TableShift = other.TableShift;
        TableMaxCode = other.TableMaxCode;
        DecodeStartCodeSize = other.DecodeStartCodeSize;

        MinCodeSize = other.MinCodeSize;
        MaxCodeSize = other.MaxCodeSize;

        MaxCodes = other.MaxCodes.ToArray();
        ValPtrs = other.ValPtrs.ToArray();

        if (other.Lookup != null)
        {
            Lookup = other.Lookup.ToArray();
        }
        if (other.SortedSymbolOrder != null)
        {
            SortedSymbolOrder = other.SortedSymbolOrder.ToArray();
        }
    }

    public void Clear()
    {
        if (Lookup != null)
        {
            Lookup = null;
        }

        if (SortedSymbolOrder != null)
        {
            SortedSymbolOrder = null;
        }
    }

    public bool Init(uint numSyms, byte[] codesizes, uint tableBits)
    {
        Span<uint> minCodes = stackalloc uint[(int)Consts.MAX_EXPECTED_CODE_SIZE];
        if (numSyms == 0 || tableBits > Consts.MAX_TABLE_BITS)
            return false;

        NumSyms = numSyms;

        Span<uint> numCodes = stackalloc uint[(int)Consts.MAX_EXPECTED_CODE_SIZE + 1];
        numCodes.Clear();

        for (int i = 0; i < numSyms; i++)
        {
            uint c = codesizes[i];
            if (c != 0)
            {
                numCodes[(int)c]++;
            }
        }

        Span<uint> sortedPositions = stackalloc uint[(int)Consts.MAX_EXPECTED_CODE_SIZE + 1];

        uint curCode = 0;

        uint totalUsedSyms = 0;
        uint maxCodeSize = 0;
        uint minCodeSize = uint.MaxValue;
        for (int i = 1; i <= Consts.MAX_EXPECTED_CODE_SIZE; i++)
        {
            uint n = numCodes[i];
            if (n == 0)
            {
                MaxCodes[i - 1] = 0;
            }
            else
            {
                minCodeSize = Math.Min(minCodeSize, (uint)i);
                maxCodeSize = Math.Max(maxCodeSize, (uint)i);

                minCodes[i - 1] = curCode;

                MaxCodes[i - 1] = curCode + n - 1;
                MaxCodes[i - 1] = 1 + ((MaxCodes[i - 1] << (16 - i)) | (uint)((1 << (16 - i)) - 1));

                ValPtrs[i - 1] = (int)totalUsedSyms;

                sortedPositions[i] = totalUsedSyms;

                curCode += n;
                totalUsedSyms += n;
            }

            curCode <<= 1;
        }

        TotalUsedSyms = totalUsedSyms;

        if (totalUsedSyms > SortedSymbolOrder.Length)
        {
            uint curSortedSymbolOrderSize = totalUsedSyms;
            if (!CrnMath.IsPowerOf2(totalUsedSyms))
            {
                curSortedSymbolOrderSize = Math.Min(numSyms, CrnMath.NextPowerOf2(totalUsedSyms));
            }

            SortedSymbolOrder = new ushort[curSortedSymbolOrderSize];
        }

        MinCodeSize = (byte)minCodeSize;
        MaxCodeSize = (byte)maxCodeSize;

        for (int i = 0; i < numSyms; i++)
        {
            uint c = codesizes[i];
            if (c != 0)
            {
                Debug.Assert(numCodes[(int)c] != 0);

                uint sortedPos = sortedPositions[(int)c]++;

                Debug.Assert(sortedPos < totalUsedSyms);

                SortedSymbolOrder[(int)sortedPos] = (ushort)i;
            }
        }

        if (tableBits <= minCodeSize)
        {
            tableBits = 0;
        }
        TableBits = tableBits;

        if (tableBits != 0)
        {
            uint tableSize = 1u << (int)tableBits;
            if (tableSize > Lookup.Length)
            {
                Lookup = new uint[tableSize];
            }

            for (int i = 0; i < tableSize; i++)
            {
                Lookup[i] = uint.MaxValue;
            }

            for (int codesize = 1; codesize <= tableBits; codesize++)
            {
                if (numCodes[codesize] == 0)
                    continue;

                uint fillsize = (uint)(tableBits - codesize);
                uint fillnum = 1u << (int)fillsize;

                uint minCode = minCodes[codesize - 1];
                uint maxCode = GetUnshiftedMaxCode((uint)codesize);
                uint valPtr = (uint)ValPtrs[codesize - 1];

                for (uint code = minCode; code <= maxCode; code++)
                {
                    uint symIndex = SortedSymbolOrder[valPtr + code - minCode];
                    Debug.Assert(codesizes[(int)symIndex] == codesize);

                    for (uint j = 0; j < fillnum; j++)
                    {
                        uint t = j + (code << (int)fillsize);

                        Debug.Assert(t < (1 << (int)tableBits));

                        Debug.Assert(Lookup[(int)t] == uint.MaxValue);

                        Lookup[(int)t] = symIndex | ((uint)codesize << 16);
                    }
                }
            }
        }

        for (int i = 0; i < Consts.MAX_EXPECTED_CODE_SIZE; i++)
        {
            ValPtrs[i] -= (int)minCodes[i];
        }

        TableMaxCode = 0;
        DecodeStartCodeSize = MinCodeSize;

        if (tableBits != 0)
        {
            int i;
            for (i = (int)tableBits; i >= 1; i--)
            {
                if (numCodes[i] != 0)
                {
                    TableMaxCode = MaxCodes[i - 1];
                    break;
                }
            }
            if (i >= 1)
            {
                DecodeStartCodeSize = tableBits + 1;
                for (int j = (int)(tableBits + 1); j <= maxCodeSize; j++)
                {
                    if (numCodes[j] != 0)
                    {
                        DecodeStartCodeSize = (uint)j;
                        break;
                    }
                }
            }
        }

        MaxCodes[Consts.MAX_EXPECTED_CODE_SIZE] = uint.MaxValue;
        ValPtrs[Consts.MAX_EXPECTED_CODE_SIZE] = 0xfffff;

        TableShift = 32 - tableBits;
        return true;
    }

    public uint GetUnshiftedMaxCode(uint len)
    {
        Debug.Assert(len >= 1 && len <= Consts.MAX_EXPECTED_CODE_SIZE);
        uint k = MaxCodes[len - 1];
        if (k == 0)
        {
            return uint.MaxValue;
        }

        return (k - 1) >> (int)(16 - len);
    }
}
