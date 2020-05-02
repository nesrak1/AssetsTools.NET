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

namespace AssetsTools.NET.Extra
{
    public class ETCDecoders
    {
        public static byte[] ReadETC(byte[] data, int width, int height)
        {
            int blockCountX = (width + 3) >> 2;
            int blockCountY = (height + 3) >> 2;

            int len = blockCountX * blockCountY * 16 * 4;
            byte[] bytes = new byte[len];

            int pos = 0;

            int[] baseColorSubblock1 = new int[3];
            int[] baseColorSubblock2 = new int[3];
            int[][] baseColorSubblocks = new int[2][];

            uint[] tableCodewords = new uint[2];

            byte b0, b1, b2, b3;
            uint pixelIndexword;

            int pixelIndex, modifier;
            uint tableCodeword;
            int[] baseColorSubblock;

            int x, y, i;

            for (y = 0; y < blockCountY; y++)
            {
                for (x = 0; x < blockCountX; x++)
                {
                    b0 = data[pos];
                    b1 = data[pos + 1];
                    b2 = data[pos + 2];
                    b3 = data[pos + 3];

                    //differential mode
                    if ((b3 & 2) > 0)
                    {
                        baseColorSubblock1[0] = b0 & 0xF8;
                        baseColorSubblock1[0] |= (baseColorSubblock1[0] & 224) >> 5;
                        baseColorSubblock1[1] = b1 & 0xF8;
                        baseColorSubblock1[1] |= (baseColorSubblock1[1] & 224) >> 5;
                        baseColorSubblock1[2] = b2 & 0xF8;
                        baseColorSubblock1[2] |= (baseColorSubblock1[2] & 224) >> 5;
                        baseColorSubblock2[0] = b0 & 0xF8;
                        baseColorSubblock2[0] += ETCTables.Comp3BitShiftedTable[b0 & 7];
                        baseColorSubblock2[0] |= (baseColorSubblock2[0] & 224) >> 5;
                        baseColorSubblock2[1] = b1 & 0xF8;
                        baseColorSubblock2[1] += ETCTables.Comp3BitShiftedTable[b1 & 7];
                        baseColorSubblock2[1] |= (baseColorSubblock2[1] & 224) >> 5;
                        baseColorSubblock2[2] = b2 & 0xF8;
                        baseColorSubblock2[2] += ETCTables.Comp3BitShiftedTable[b2 & 7];
                        baseColorSubblock2[2] |= (baseColorSubblock2[2] & 224) >> 5;
                    }
                    else
                    {
                        baseColorSubblock1[0] = b0 & 0xF0;
                        baseColorSubblock1[0] |= baseColorSubblock1[0] >> 4;
                        baseColorSubblock1[1] = b1 & 0xF0;
                        baseColorSubblock1[1] |= baseColorSubblock1[1] >> 4;
                        baseColorSubblock1[2] = b2 & 0xF0;
                        baseColorSubblock1[2] |= baseColorSubblock1[2] >> 4;
                        baseColorSubblock2[0] = b0 & 0x0F;
                        baseColorSubblock2[0] |= baseColorSubblock2[0] << 4;
                        baseColorSubblock2[1] = b1 & 0x0F;
                        baseColorSubblock2[1] |= baseColorSubblock2[1] << 4;
                        baseColorSubblock2[2] = b2 & 0x0F;
                        baseColorSubblock2[2] |= baseColorSubblock2[2] << 4;
                    }

                    baseColorSubblocks[0] = baseColorSubblock1;
                    baseColorSubblocks[1] = baseColorSubblock2;
                    tableCodewords[0] = (uint)((b3 & 224) >> 5);
                    tableCodewords[1] = (uint)((b3 & 28) >> 2);
                    pixelIndexword = ((uint)data[pos + 4] << 24) | ((uint)data[pos + 5] << 16) | ((uint)data[pos + 6] << 8) | data[pos + 7];

                    //flipbit (horiz/vert)
                    if ((b3 & 1) == 0)
                    {
                        for (i = 0; i < 16; i++)
                        {
                            tableCodeword = tableCodewords[i >> 3];
                            baseColorSubblock = baseColorSubblocks[i >> 3];

                            pixelIndex = (int)(((pixelIndexword & (1 << i)) >> i)
                                | ((pixelIndexword & ((long)0x10000 << i)) >> (16 + i - 1)));
                            modifier = ETCTables.ModifierTable[tableCodeword][pixelIndex];

                            int dataPos = (x * 4 * 4) + ((i >> 2) * 4) + (y * width * 4 * 4) + (i % 4 * width * 4);
                            bytes[dataPos] = ETCTables.Clamp0To255Table[baseColorSubblock[2] + modifier + 255];
                            bytes[dataPos + 1] = ETCTables.Clamp0To255Table[baseColorSubblock[1] + modifier + 255];
                            bytes[dataPos + 2] = ETCTables.Clamp0To255Table[baseColorSubblock[0] + modifier + 255];
                            bytes[dataPos + 3] = 0xFF;
                        }
                    }
                    else
                    {
                        for (i = 0; i < 16; i++)
                        {
                            tableCodeword = tableCodewords[(i & 2) >> 1];
                            baseColorSubblock = baseColorSubblocks[(i & 2) >> 1];

                            pixelIndex = (int)(((pixelIndexword & (1 << i)) >> i)
                                | ((pixelIndexword & ((long)0x10000 << i)) >> (16 + i - 1)));
                            modifier = ETCTables.ModifierTable[tableCodeword][pixelIndex];

                            int dataPos = (x * 4 * 4) + ((i >> 2) * 4) + (y * width * 4 * 4) + (i % 4 * width * 4);
                            bytes[dataPos] = ETCTables.Clamp0To255Table[baseColorSubblock[2] + modifier + 255];
                            bytes[dataPos + 1] = ETCTables.Clamp0To255Table[baseColorSubblock[1] + modifier + 255];
                            bytes[dataPos + 2] = ETCTables.Clamp0To255Table[baseColorSubblock[0] + modifier + 255];
                            bytes[dataPos + 3] = 0xFF;
                        }
                    }
                    pos += 8;
                }
            }
            return bytes;
        }
    }
}
