/*
Copyright (c) 2015 Harm Hanemaaijer <fgenfb@yahoo.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public static class BC7Decoder
    {
        public static byte[] ReadBC7(byte[] data, int width, int height)
        {
            int blockCountX = (width + 3) >> 2;
            int blockCountY = (height + 3) >> 2;

            int len = blockCountX * blockCountY * 16 * 4;
            byte[] bytes = new byte[len];

            int pos = 0;

            BitReader bitReader = new BitReader();

            int modeData;
            ulong data0, data1;

            byte[] m1Endpoints = new byte[2 * 2 * 3];
            byte[] mxEndpoints = new byte[3 * 2 * 4];
            byte[] subsetIndex = new byte[16];
            byte[] anchorIndex = new byte[4];
            byte[] colorIndex = new byte[16];
            byte[] alphaIndex = new byte[16];

            byte[] endpointStart = new byte[4];
            byte[] endpointEnd = new byte[4];

            int mode;

            int numSubsets;
            int partitionSetId;
            int rotation;
            int indexSelectionBit;
            int alphaIndexBitCount;
            int colorIndexBitCount;

            int colorPrec;
            int alphaPrec;
            int colorPrecPlusP;
            int alphaPrecPlusP;

            int componentsInData0;
            long blockData;
            int mask;
            int totalBitsPerComponent;
            uint bits;

            ulong dataSec;

            uint mask1;
            uint mask2;

            byte pbitZero;
            byte pbitOne;

            int x, y, i, j, k;

            for (y = 0; y < blockCountY; y++)
            {
                for (x = 0; x < blockCountX; x++)
                {
                    modeData = data[pos];
                    data0 = BitConverter.ToUInt64(data, pos);
                    data1 = BitConverter.ToUInt64(data, pos + 8);
                    bitReader.Reset(data0, data1);

                    if ((modeData & 1) == 1)
                        mode = 0;
                    else if (((modeData >> 1) & 1) == 1)
                        mode = 1;
                    else if (((modeData >> 2) & 1) == 1)
                        mode = 2;
                    else if (((modeData >> 3) & 1) == 1)
                        mode = 3;
                    else if (((modeData >> 4) & 1) == 1)
                        mode = 4;
                    else if (((modeData >> 5) & 1) == 1)
                        mode = 5;
                    else if (((modeData >> 6) & 1) == 1)
                        mode = 6;
                    else if (((modeData >> 7) & 1) == 1)
                        mode = 7;
                    else
                        mode = -1;

                    bitReader.index += mode + 1;

                    if (mode == 1)
                    {
                        partitionSetId = (byte)GetBits64(data0, 2, 7);
                        m1Endpoints[0] = (byte)GetBits64(data0, 8, 13);
                        m1Endpoints[3] = (byte)GetBits64(data0, 14, 19);
                        m1Endpoints[6] = (byte)GetBits64(data0, 20, 25);
                        m1Endpoints[9] = (byte)GetBits64(data0, 26, 31);

                        m1Endpoints[1] = (byte)GetBits64(data0, 32, 37);
                        m1Endpoints[4] = (byte)GetBits64(data0, 38, 43);
                        m1Endpoints[7] = (byte)GetBits64(data0, 44, 49);
                        m1Endpoints[10] = (byte)GetBits64(data0, 50, 55);

                        m1Endpoints[2] = (byte)GetBits64(data0, 56, 61);
                        m1Endpoints[5] = (byte)(GetBits64(data0, 62, 63) | (GetBits64(data1, 0, 3) << 2));
                        m1Endpoints[8] = (byte)GetBits64(data1, 4, 9);
                        m1Endpoints[11] = (byte)GetBits64(data1, 10, 15);

                        for (i = 0; i < 2 * 2; i++)
                        {
                            m1Endpoints[i * 3 + 0] <<= 2;
                            m1Endpoints[i * 3 + 1] <<= 2;
                            m1Endpoints[i * 3 + 2] <<= 2;
                        }

                        pbitZero = (byte)((byte)GetBits64(data1, 16, 16) << 1);
                        pbitOne = (byte)((byte)GetBits64(data1, 17, 17) << 1);

                        for (i = 0; i < 3; i++)
                        {
                            m1Endpoints[0 * 3 + i] |= pbitZero;
                            m1Endpoints[1 * 3 + i] |= pbitZero;
                            m1Endpoints[2 * 3 + i] |= pbitOne;
                            m1Endpoints[3 * 3 + i] |= pbitOne;
                        }

                        for (i = 0; i < 2 * 2; i++)
                        {
                            m1Endpoints[i * 3 + 0] |= (byte)(m1Endpoints[i * 3 + 0] >> 7);
                            m1Endpoints[i * 3 + 1] |= (byte)(m1Endpoints[i * 3 + 1] >> 7);
                            m1Endpoints[i * 3 + 2] |= (byte)(m1Endpoints[i * 3 + 2] >> 7);
                        }

                        for (i = 0; i < 16; i++)
                        {
                            subsetIndex[i] = BPTCTables.P2[partitionSetId * 16 + i];
                        }

                        anchorIndex[0] = 0;
                        anchorIndex[1] = BPTCTables.AnchorIndexSecondSubset[partitionSetId];

                        data1 >>= 18;

                        for (i = 0; i < 16; i++)
                        {
                            colorIndex[i] = 0;
                        }

                        for (i = 0; i < 16; i++)
                        {
                            if (i == anchorIndex[subsetIndex[i]])
                            {
                                colorIndex[i] = (byte)(data1 & 3);
                                data1 >>= 2;
                            }
                            else
                            {
                                colorIndex[i] = (byte)(data1 & 7);
                                data1 >>= 3;
                            }
                        }

                        for (i = 0; i < 16; i++)
                        {
                            for (j = 0; j < 3; j++)
                            {
                                endpointStart[j] = m1Endpoints[2 * subsetIndex[i] * 3 + j];
                                endpointEnd[j] = m1Endpoints[(2 * subsetIndex[i] + 1) * 3 + j];
                            }

                            int dataPos = (x * 4 * 4) + (i % 4 * 4) + (y * width * 4 * 4) + ((i >> 2) * width * 4);
                            bytes[dataPos] = Interpolate(endpointStart[2], endpointEnd[2], colorIndex[i], 3);
                            bytes[dataPos + 1] = Interpolate(endpointStart[1], endpointEnd[1], colorIndex[i], 3);
                            bytes[dataPos + 2] = Interpolate(endpointStart[0], endpointEnd[0], colorIndex[i], 3);
                            bytes[dataPos + 3] = 0xFF;
                        }
                    }
                    else
                    {
                        numSubsets = 1;
                        partitionSetId = 0;
                        if (BPTCTables.ModeHasPartitionBits[mode])
                        {
                            numSubsets = BPTCTables.NumberOfSubsets[mode];
                            partitionSetId = bitReader.ReadNumber(BPTCTables.NumberOfPartitionBits[mode]);
                        }
                        rotation = bitReader.ReadNumber(BPTCTables.NumberOfRotationBits[mode]);
                        indexSelectionBit = 0;
                        if (mode == 4)
                        {
                            indexSelectionBit = bitReader.ReadNumber(1);
                        }
                        alphaIndexBitCount = BPTCTables.AlphaIndexBitCount[mode] - indexSelectionBit;
                        colorIndexBitCount = BPTCTables.ColorIndexBitCount[mode] + indexSelectionBit;

                        //extract endpoints
                        componentsInData0 = BPTCTables.ComponentsInData0[mode];
                        blockData = (long)(data0 >> bitReader.index);
                        colorPrec = BPTCTables.ColorPrecision[mode];
                        mask = (1 << colorPrec) - 1;
                        totalBitsPerComponent = numSubsets * 2 * colorPrec;

                        for (i = 0; i < componentsInData0; i++)
                        {
                            for (j = 0; j < numSubsets; j++)
                            {
                                for (k = 0; k < 2; k++)
                                {
                                    mxEndpoints[j * 8 + k * 4 + i] = (byte)(blockData & mask);
                                    blockData >>= colorPrec;
                                }
                            }
                        }
                        bitReader.index += componentsInData0 * totalBitsPerComponent;
                        if (componentsInData0 < 3)
                        {
                            blockData = (long)(data0 >> bitReader.index);
                            blockData |= (long)(data1 << (64 - bitReader.index));
                            i = componentsInData0;
                            for (j = 0; j < numSubsets; j++)
                            {
                                for (k = 0; k < 2; k++)
                                {
                                    mxEndpoints[j * 8 + k * 4 + i] = (byte)(blockData & mask);
                                    blockData >>= colorPrec;
                                }
                            }
                            bitReader.index += totalBitsPerComponent;
                        }
                        if (componentsInData0 < 2)
                        {
                            blockData = (long)(data1 >> (bitReader.index - 64));
                            i = 2;
                            for (j = 0; j < numSubsets; j++)
                            {
                                for (k = 0; k < 2; k++)
                                {
                                    mxEndpoints[j * 8 + k * 4 + i] = (byte)(blockData & mask);
                                    blockData >>= colorPrec;
                                }
                            }
                            bitReader.index += totalBitsPerComponent;
                        }
                        alphaPrec = BPTCTables.AlphaPrecision[mode];
                        if (alphaPrec > 0)
                        {
                            if (mode == 7)
                                blockData = (long)(data1 >> (bitReader.index - 64));
                            else if (mode == 5)
                                blockData = (long)((data0 >> bitReader.index) | ((data1 & 0x3) << 14));
                            else
                                blockData = (long)(data0 >> bitReader.index);

                            mask = (1 << alphaPrec) - 1;
                            for (j = 0; j < numSubsets; j++)
                            {
                                for (k = 0; k < 2; k++)
                                {
                                    mxEndpoints[j * 8 + k * 4 + 3] = (byte)(blockData & mask);
                                    blockData >>= alphaPrec;
                                }
                            }
                            bitReader.index += numSubsets * 2 * alphaPrec;
                        }

                        //fully extract endpoints
                        if (BPTCTables.ModeHasPBits[mode])
                        {
                            if (bitReader.index < 64)
                            {
                                bits = (uint)(data0 >> bitReader.index);
                                if ((bitReader.index + numSubsets * 2) > 64)
                                {
                                    bits |= (uint)(data1 << (64 - bitReader.index));
                                }
                            }
                            else
                            {
                                bits = (uint)(data1 >> (bitReader.index - 64));
                            }

                            for (i = 0; i < numSubsets * 2; i++)
                            {
                                mxEndpoints[i * 4 + 0] <<= 1;
                                mxEndpoints[i * 4 + 1] <<= 1;
                                mxEndpoints[i * 4 + 2] <<= 1;
                                mxEndpoints[i * 4 + 3] <<= 1;

                                mxEndpoints[i * 4 + 0] |= (byte)(bits & 1);
                                mxEndpoints[i * 4 + 1] |= (byte)(bits & 1);
                                mxEndpoints[i * 4 + 2] |= (byte)(bits & 1);
                                mxEndpoints[i * 4 + 3] |= (byte)(bits & 1);
                                bits >>= 1;
                            }
                            bitReader.index += numSubsets * 2;
                        }
                        colorPrecPlusP = BPTCTables.ColorPrecisionPlusPBit[mode];
                        alphaPrecPlusP = BPTCTables.AlphaPrecisionPlusPBit[mode];
                        for (i = 0; i < numSubsets * 2; i++)
                        {
                            mxEndpoints[i * 4 + 0] <<= 8 - colorPrecPlusP;
                            mxEndpoints[i * 4 + 1] <<= 8 - colorPrecPlusP;
                            mxEndpoints[i * 4 + 2] <<= 8 - colorPrecPlusP;
                            mxEndpoints[i * 4 + 3] <<= 8 - alphaPrecPlusP;

                            mxEndpoints[i * 4 + 0] |= (byte)(mxEndpoints[i * 4 + 0] >> colorPrecPlusP);
                            mxEndpoints[i * 4 + 1] |= (byte)(mxEndpoints[i * 4 + 1] >> colorPrecPlusP);
                            mxEndpoints[i * 4 + 2] |= (byte)(mxEndpoints[i * 4 + 2] >> colorPrecPlusP);
                            mxEndpoints[i * 4 + 3] |= (byte)(mxEndpoints[i * 4 + 3] >> alphaPrecPlusP);
                        }
                        if (mode <= 3)
                        {
                            for (i = 0; i < numSubsets * 2; i++)
                            {
                                mxEndpoints[i * 4 + 3] = 0xFF;
                            }
                        }

                        for (i = 0; i < 16; i++)
                        {
                            if (numSubsets == 1)
                            {
                                subsetIndex[i] = 0;
                            }
                            else if (numSubsets == 2)
                            {
                                subsetIndex[i] = BPTCTables.P2[partitionSetId * 16 + i];
                            }
                            else
                            {
                                subsetIndex[i] = BPTCTables.P3[partitionSetId * 16 + i];
                            }
                        }
                        for (i = 0; i < numSubsets; i++)
                        {
                            if (partitionSetId == 0)
                            {
                                anchorIndex[i] = 0;
                            }
                            else if (numSubsets == 2)
                            {
                                anchorIndex[i] = BPTCTables.AnchorIndexSecondSubset[partitionSetId];
                            }
                            else if (partitionSetId == 1)
                            {
                                anchorIndex[i] = BPTCTables.AnchorIndexSecondSubsetOfThree[partitionSetId];
                            }
                            else
                            {
                                anchorIndex[i] = BPTCTables.AnchorIndexThirdSubset[partitionSetId];
                            }
                        }

                        for (i = 0; i < 16; i++)
                        {
                            colorIndex[i] = 0;
                            alphaIndex[i] = 0;
                        }

                        if (bitReader.index >= 64)
                        {
                            dataSec = data1 >> (bitReader.index - 64);
                            mask1 = (uint)((1 << BPTCTables.IndexBits[mode]) - 1);
                            mask2 = (uint)((1 << (BPTCTables.IndexBits[mode] - 1)) - 1);
                            for (i = 0; i < 16; i++)
                            {
                                if (i == anchorIndex[subsetIndex[i]])
                                {
                                    colorIndex[i] = (byte)(dataSec & mask2);
                                    dataSec >>= BPTCTables.IndexBits[mode] - 1;
                                    alphaIndex[i] = colorIndex[i];
                                }
                                else
                                {
                                    colorIndex[i] = (byte)(dataSec & mask1);
                                    dataSec >>= BPTCTables.IndexBits[mode];
                                    alphaIndex[i] = colorIndex[i];
                                }
                            }
                        }
                        else
                        {
                            dataSec = data0 >> 50;
                            dataSec |= data1 << 14;
                            for (i = 0; i < 16; i++)
                            {
                                if (i == anchorIndex[subsetIndex[i]])
                                {
                                    if (indexSelectionBit == 1)
                                    {
                                        alphaIndex[i] = (byte)(dataSec & 0x1);
                                        dataSec >>= 1;
                                    }
                                    else
                                    {
                                        colorIndex[i] = (byte)(dataSec & 0x1);
                                        dataSec >>= 1;
                                    }
                                }
                                else
                                {
                                    if (indexSelectionBit == 1)
                                    {
                                        alphaIndex[i] = (byte)(dataSec & 0x3);
                                        dataSec >>= 2;
                                    }
                                    else
                                    {
                                        colorIndex[i] = (byte)(dataSec & 0x3);
                                        dataSec >>= 2;
                                    }
                                }
                            }
                            dataSec = data1 >> (81 - 64);
                        }

                        if (BPTCTables.IndexBits2[mode] > 0)
                        {
                            mask1 = (uint)((1 << BPTCTables.IndexBits2[mode]) - 1);
                            mask2 = (uint)((1 << (BPTCTables.IndexBits2[mode] - 1)) - 1);
                            for (i = 0; i < 16; i++)
                            {
                                if (i == anchorIndex[subsetIndex[i]])
                                {
                                    if (indexSelectionBit == 1)
                                    {
                                        colorIndex[i] = (byte)(dataSec & 0x3);
                                        dataSec >>= 2;
                                    }
                                    else
                                    {
                                        alphaIndex[i] = (byte)(dataSec & mask2);
                                        dataSec >>= BPTCTables.IndexBits2[mode] - 1;
                                    }
                                }
                                else
                                {
                                    if (indexSelectionBit == 1)
                                    {
                                        colorIndex[i] = (byte)(dataSec & 0x7);
                                        dataSec >>= 3;
                                    }
                                    else
                                    {
                                        alphaIndex[i] = (byte)(dataSec & mask1);
                                        dataSec >>= BPTCTables.IndexBits2[mode];
                                    }
                                }
                            }
                        }

                        for (i = 0; i < 16; i++)
                        {
                            for (j = 0; j < 4; j++)
                            {
                                endpointStart[j] = mxEndpoints[2 * subsetIndex[i] * 4 + j];
                                endpointEnd[j] = mxEndpoints[(2 * subsetIndex[i] + 1) * 4 + j];
                            }

                            int dataPos = (x * 4 * 4) + (i % 4 * 4) + (y * width * 4 * 4) + ((i >> 2) * width * 4);
                            if (rotation == 0)
                            {
                                //rgba -> bgra
                                bytes[dataPos] = Interpolate(endpointStart[2], endpointEnd[2], colorIndex[i], colorIndexBitCount);
                                bytes[dataPos + 1] = Interpolate(endpointStart[1], endpointEnd[1], colorIndex[i], colorIndexBitCount);
                                bytes[dataPos + 2] = Interpolate(endpointStart[0], endpointEnd[0], colorIndex[i], colorIndexBitCount);
                                bytes[dataPos + 3] = Interpolate(endpointStart[3], endpointEnd[3], alphaIndex[i], alphaIndexBitCount);
                            }
                            else
                            {
                                if (rotation == 1)
                                {
                                    //agbr -> bgar
                                    bytes[dataPos] = Interpolate(endpointStart[2], endpointEnd[2], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 1] = Interpolate(endpointStart[1], endpointEnd[1], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 2] = Interpolate(endpointStart[3], endpointEnd[3], alphaIndex[i], alphaIndexBitCount);
                                    bytes[dataPos + 3] = Interpolate(endpointStart[0], endpointEnd[0], colorIndex[i], colorIndexBitCount);
                                }
                                else if (rotation == 2)
                                {
                                    //rabg -> barg
                                    bytes[dataPos] = Interpolate(endpointStart[2], endpointEnd[2], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 1] = Interpolate(endpointStart[3], endpointEnd[3], alphaIndex[i], alphaIndexBitCount);
                                    bytes[dataPos + 2] = Interpolate(endpointStart[0], endpointEnd[0], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 3] = Interpolate(endpointStart[1], endpointEnd[1], colorIndex[i], colorIndexBitCount);
                                }
                                else
                                {
                                    //rgab -> agrb
                                    bytes[dataPos] = Interpolate(endpointStart[3], endpointEnd[3], alphaIndex[i], alphaIndexBitCount);
                                    bytes[dataPos + 1] = Interpolate(endpointStart[1], endpointEnd[1], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 2] = Interpolate(endpointStart[0], endpointEnd[0], colorIndex[i], colorIndexBitCount);
                                    bytes[dataPos + 3] = Interpolate(endpointStart[2], endpointEnd[2], colorIndex[i], colorIndexBitCount);
                                }
                            }
                        }
                    }
                    pos += 16;
                }
            }
            return bytes;
        }

        private static uint GetBits64(ulong data, int bit0, int bit1)
        {
            ulong mask;
            if (bit1 == 63)
                mask = ulong.MaxValue;
            else
                mask = ((ulong)1 << (bit1 + 1)) - 1;

            return (uint)((data & mask) >> bit0);
        }

        private static byte Interpolate(byte e0, byte e1, byte index, int indexPrecision)
        {
            if (indexPrecision == 2)
                return (byte)(((64 - BPTCTables.AlphaWeight2[index]) * (uint)e0 + BPTCTables.AlphaWeight2[index] * (uint)e1 + 32) >> 6);
            else if (indexPrecision == 3)
                return (byte)(((64 - BPTCTables.AlphaWeight3[index]) * (uint)e0 + BPTCTables.AlphaWeight3[index] * (uint)e1 + 32) >> 6);
            else
                return (byte)(((64 - BPTCTables.AlphaWeight4[index]) * (uint)e0 + BPTCTables.AlphaWeight4[index] * (uint)e1 + 32) >> 6);
        }
    }
}
