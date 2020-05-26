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
        public static byte[] ReadETC(byte[] data, int width, int height, bool etc2 = false)
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

            int baseColor1R, baseColor1G, baseColor1B;
            int baseColor2R, baseColor2G, baseColor2B;
            byte[] paintColorR = new byte[4];
            byte[] paintColorG = new byte[4];
            byte[] paintColorB = new byte[4];
            int distance;
            int baseColor1Value, baseColor2Value;

            int r, g, b;
            int ro, go, bo;
            int rh, gh, bh;
            int rv, gv, bv;
            
            int etcMode;

            int x, y, i, j, k;

            for (y = 0; y < blockCountY; y++)
            {
                for (x = 0; x < blockCountX; x++)
                {
                    b0 = data[pos];
                    b1 = data[pos + 1];
                    b2 = data[pos + 2];
                    b3 = data[pos + 3];

                    etcMode = 0;
                    if (etc2 && (b3 & 2) != 0)
                    {
                        r = b0 & 0xF8;
                        r += ETCTables.Comp3BitShiftedTable[b0 & 7];
                        g = b1 & 0xF8;
                        g += ETCTables.Comp3BitShiftedTable[b1 & 7];
                        b = b2 & 0xF8;
                        b += ETCTables.Comp3BitShiftedTable[b2 & 7];
                        if ((r & 0xFF07) != 0)
                        {
                            etcMode = 1;
                        }
                        else if ((g & 0xFF07) != 0)
                        {
                            etcMode = 2;
                        }
                        else if ((b & 0xFF07) != 0)
                        {
                            etcMode = 3;
                        }
                    }

                    //etc1
                    if (!etc2 || (etc2 && (b3 & 2) == 0) || etcMode == 0)
                    {
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
                    }
                    //t/h mode
                    else if (etcMode == 1 || etcMode == 2)
                    {
                        //t mode
                        if (etcMode == 1)
                        {
                            baseColor1R = ((b0 & 0x18) >> 1) | (b0 & 0x3);
                            baseColor1R |= baseColor1R << 4;
                            baseColor1G = b1 & 0xF0;
                            baseColor1G |= baseColor1G >> 4;
                            baseColor1B = b1 & 0x0F;
                            baseColor1B |= baseColor1B << 4;
                            baseColor2R = b2 & 0xF0;
                            baseColor2R |= baseColor2R >> 4;
                            baseColor2G = b2 & 0x0F;
                            baseColor2G |= baseColor2G << 4;
                            baseColor2B = b3 & 0xF0;
                            baseColor2B |= baseColor2B >> 4;

                            distance = ETCTables.Etc2DistanceTable[((b3 & 0x0C) >> 1) | (b3 & 0x1)];
                            paintColorR[0] = (byte)baseColor1R;
                            paintColorG[0] = (byte)baseColor1G;
                            paintColorB[0] = (byte)baseColor1B;
                            paintColorR[2] = (byte)baseColor2R;
                            paintColorG[2] = (byte)baseColor2G;
                            paintColorB[2] = (byte)baseColor2B;
                            paintColorR[1] = ETCTables.Clamp0To255Table[baseColor2R + distance + 255];
                            paintColorG[1] = ETCTables.Clamp0To255Table[baseColor2G + distance + 255];
                            paintColorB[1] = ETCTables.Clamp0To255Table[baseColor2B + distance + 255];
                            paintColorR[3] = ETCTables.Clamp0To255Table[baseColor2R - distance + 255];
                            paintColorG[3] = ETCTables.Clamp0To255Table[baseColor2G - distance + 255];
                            paintColorB[3] = ETCTables.Clamp0To255Table[baseColor2B - distance + 255];
                        }
                        //h mode
                        else
                        {

                            baseColor1R = (b0 & 0x78) >> 3;
                            baseColor1R |= baseColor1R << 4;
                            baseColor1G = ((b0 & 0x07) << 1) | ((b1 & 0x10) >> 4);
                            baseColor1G |= baseColor1G << 4;
                            baseColor1B = (b1 & 0x08) | ((b1 & 0x03) << 1) | ((b2 & 0x80) >> 7);
                            baseColor1B |= baseColor1B << 4;
                            baseColor2R = (b2 & 0x78) >> 3;
                            baseColor2R |= baseColor2R << 4;
                            baseColor2G = ((b2 & 0x07) << 1) | ((b3 & 0x80) >> 7);
                            baseColor2G |= baseColor2G << 4;
                            baseColor2B = (b3 & 0x78) >> 3;
                            baseColor2B |= baseColor2B << 4;

                            baseColor1Value = (baseColor1R << 16) + (baseColor1G << 8) + baseColor1B;
                            baseColor2Value = (baseColor2R << 16) + (baseColor2G << 8) + baseColor2B;

                            distance = ETCTables.Etc2DistanceTable[(b3 & 0x04) | ((b3 & 0x01) << 1) | ((baseColor1Value >= baseColor2Value) ? 1 : 0)];
                            paintColorR[0] = ETCTables.Clamp0To255Table[baseColor1R + distance + 255];
                            paintColorG[0] = ETCTables.Clamp0To255Table[baseColor1G + distance + 255];
                            paintColorB[0] = ETCTables.Clamp0To255Table[baseColor1B + distance + 255];
                            paintColorR[1] = ETCTables.Clamp0To255Table[baseColor1R - distance + 255];
                            paintColorG[1] = ETCTables.Clamp0To255Table[baseColor1G - distance + 255];
                            paintColorB[1] = ETCTables.Clamp0To255Table[baseColor1B - distance + 255];
                            paintColorR[2] = ETCTables.Clamp0To255Table[baseColor2R + distance + 255];
                            paintColorG[2] = ETCTables.Clamp0To255Table[baseColor2G + distance + 255];
                            paintColorB[2] = ETCTables.Clamp0To255Table[baseColor2B + distance + 255];
                            paintColorR[3] = ETCTables.Clamp0To255Table[baseColor2R - distance + 255];
                            paintColorG[3] = ETCTables.Clamp0To255Table[baseColor2G - distance + 255];
                            paintColorB[3] = ETCTables.Clamp0To255Table[baseColor2B - distance + 255];
                        }
                        pixelIndexword = ((uint)data[pos + 4] << 24) | ((uint)data[pos + 5] << 16) | ((uint)data[pos + 6] << 8) | data[pos + 7];
                        for (i = 0; i < 16; i++)
                        {
                            pixelIndex = (int)(((pixelIndexword & (1 << i)) >> i)
                                | ((pixelIndexword & ((long)0x10000 << i)) >> (16 + i - 1)));
                            int dataPos = (x * 4 * 4) + ((i >> 2) * 4) + (y * width * 4 * 4) + (i % 4 * width * 4);
                            bytes[dataPos] = paintColorB[pixelIndex];
                            bytes[dataPos + 1] = paintColorG[pixelIndex];
                            bytes[dataPos + 2] = paintColorR[pixelIndex];
                            bytes[dataPos + 3] = 0xFF;
                        }
                    }
                    //planar mode
                    else if (etcMode == 3)
                    {
                        ro = (b0 & 0x7E) >> 1;
                        go = ((b0 & 0x1) << 6) | ((b1 & 0x7E) >> 1);
                        bo = ((b1 & 0x1) << 5) | (b2 & 0x18) | ((b2 & 0x03) << 1) | ((b3 & 0x80) >> 7);
                        rh = ((b3 & 0x7C) >> 1) | (b3 & 0x1);
                        gh = (data[pos + 4] & 0xFE) >> 1;
                        bh = ((data[pos + 4] & 0x1) << 5) | ((data[pos + 5] & 0xF8) >> 3);
                        rv = ((data[pos + 5] & 0x7) << 3) | ((data[pos + 6] & 0xE0) >> 5);
                        gv = ((data[pos + 6] & 0x1F) << 2) | ((data[pos + 7] & 0xC0) >> 6);
                        bv = data[pos + 7] & 0x3F;

                        ro = (ro << 2) | ((ro & 0x30) >> 4);
                        go = (go << 1) | ((go & 0x40) >> 6);
                        bo = (bo << 2) | ((bo & 0x30) >> 4);
                        rh = (rh << 2) | ((rh & 0x30) >> 4);
                        gh = (gh << 1) | ((gh & 0x40) >> 6);
                        bh = (bh << 2) | ((bh & 0x30) >> 4);
                        rv = (rv << 2) | ((rv & 0x30) >> 4);
                        gv = (gv << 1) | ((gv & 0x40) >> 6);
                        bv = (bv << 2) | ((bv & 0x30) >> 4);

                        for (k = 0; k < 4; k++)
                        {
                            for (j = 0; j < 4; j++)
                            {
                                int dataPos = (x * 4 * 4) + (j * 4) + (y * width * 4 * 4) + (k * width * 4);
                                bytes[dataPos] = ETCTables.Clamp0To255Table[((j * (bh - bo) + k * (bv - bo) + 4 * bo + 2) >> 2) + 255];
                                bytes[dataPos + 1] = ETCTables.Clamp0To255Table[((j * (gh - go) + k * (gv - go) + 4 * go + 2) >> 2) + 255];
                                bytes[dataPos + 2] = ETCTables.Clamp0To255Table[((j * (rh - ro) + k * (rv - ro) + 4 * ro + 2) >> 2) + 255];
                                bytes[dataPos + 3] = 0xFF;
                            }
                        }
                    }
                    pos += 8;
                }
            }
            return bytes;
        }
    }
}
