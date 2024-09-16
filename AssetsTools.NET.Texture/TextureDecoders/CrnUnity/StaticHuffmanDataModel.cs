using System;
using System.Diagnostics;

namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
internal class StaticHuffmanDataModel
{
    public uint TotalSyms;
    public byte[] CodeSizes;
    public DecoderTables DecodeTables;

    public StaticHuffmanDataModel()
    {
        CodeSizes = [];
    }

    public StaticHuffmanDataModel(StaticHuffmanDataModel other)
    {
        TotalSyms = other.TotalSyms;
        CodeSizes = other.CodeSizes;
        if (other.DecodeTables != null)
        {
            DecodeTables = new DecoderTables(other.DecodeTables);
        }
        else
        {
            DecodeTables = null;
        }
    }

    public bool Init(uint totalSyms, byte[] codeSizes, uint codeSizeLimit)
    {
        Debug.Assert(totalSyms >= 1 && totalSyms <= Consts.MAX_SUPPORTED_SYMS && codeSizeLimit >= 1);

        codeSizeLimit = Math.Min(codeSizeLimit, Consts.MAX_SUPPORTED_SYMS);

        CodeSizes = new byte[totalSyms];

        uint minCodeSize = uint.MaxValue;
        uint maxCodeSize = 0;

        for (int i = 0; i < totalSyms; i++)
        {
            uint s = codeSizes[i];
            CodeSizes[i] = (byte)s;
            minCodeSize = Math.Min(minCodeSize, s);
            maxCodeSize = Math.Min(maxCodeSize, s);
        }

        if ((maxCodeSize < 1) || (maxCodeSize > 32) || (minCodeSize > codeSizeLimit))
            return false;

        if (maxCodeSize > codeSizeLimit)
            return false;

        DecodeTables ??= new DecoderTables();

        if (!DecodeTables.Init(TotalSyms, CodeSizes, ComputeDecoderTableBits()))
            return false;

        return true;
    }

    public void Clear()
    {
        TotalSyms = 0;
        CodeSizes = [];
        DecodeTables = null;
    }

    public uint GetTotalSyms()
    {
        return TotalSyms;
    }

    public uint GetCodeSize(uint sym)
    {
        return CodeSizes[(int)sym];
    }

    public byte[] GetCodeSizes()
    {
        if (CodeSizes.Length != 0)
        {
            return CodeSizes;
        }
        else
        {
            return null;
        }
    }

    public bool PrepareDecoderTables()
    {
        uint totalSyms = (uint)CodeSizes.Length;

        Debug.Assert(totalSyms >= 1 && totalSyms <= Consts.MAX_SUPPORTED_SYMS);

        TotalSyms = totalSyms;

        DecodeTables ??= new DecoderTables();

        return DecodeTables.Init(TotalSyms, CodeSizes, ComputeDecoderTableBits());
    }

    private uint ComputeDecoderTableBits()
    {
        uint decoderTableBits = 0;
        if (TotalSyms > 16)
        {
            decoderTableBits = (byte)Math.Min(1 + CrnMath.CeilLog2i(TotalSyms), Consts.MAX_TABLE_BITS);
        }

        return decoderTableBits;
    }
}
