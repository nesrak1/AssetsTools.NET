//------------------------------------------------------------------------------
//
// crn_decomp.h uses the ZLIB license:
// http://opensource.org/licenses/Zlib
//
// Copyright (c) 2010-2016 Richard Geldreich, Jr. and Binomial LLC
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source distribution.
//
//------------------------------------------------------------------------------

using System;
using System.Buffers.Binary;
using System.Diagnostics;

namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
public class Unpacker
{
    private uint _magic;
    private byte[] _data;
    private Header _header;

    private SymbolCodec _codec;

    private StaticHuffmanDataModel _referenceEncodingDm;
    private StaticHuffmanDataModel[] _endpointDeltaDm;
    private StaticHuffmanDataModel[] _selectorDeltaDm;

    private uint[] _colorEndpoints;
    private uint[] _colorSelectors;

    private ushort[] _alphaEndpoints;
    private ushort[] _alphaSelectors;

    private BlockBufferElement[] _blockBuffer;

    public Unpacker()
    {
        _magic = Consts.MAGIC_VALUE;
        _codec = new SymbolCodec();
        _referenceEncodingDm = new StaticHuffmanDataModel();
        _endpointDeltaDm = new StaticHuffmanDataModel[2]
        {
            new StaticHuffmanDataModel(),
            new StaticHuffmanDataModel()
        };
        _selectorDeltaDm = new StaticHuffmanDataModel[2]
        {
            new StaticHuffmanDataModel(),
            new StaticHuffmanDataModel()
        };
        _colorEndpoints = new uint[0];
        _colorSelectors = new uint[0];
        _alphaEndpoints = new ushort[0];
        _alphaSelectors = new ushort[0];
        _blockBuffer = new BlockBufferElement[0];
    }

    public bool IsValid()
    {
        return _magic == Consts.MAGIC_VALUE;
    }

    public bool Init(byte[] data)
    {
        _header = Info.GetHeader(data);
        if (_header == null)
            return false;

        _data = data;

        if (!InitTables())
            return false;

        if (!DecodePalettes())
            return false;

        return true;
    }

    public bool UnpackLevel(byte[][] dst, uint dstSizeInBytes, uint rowPitchInBytes, uint levelIndex)
    {
        uint curLevelOfs = _header.LevelOfs[levelIndex];

        uint nextLevelOfs = (uint)_data.Length;
        if (levelIndex + 1 < _header.Levels)
        {
            nextLevelOfs = _header.LevelOfs[levelIndex + 1];
        }

        Debug.Assert(nextLevelOfs > curLevelOfs);

        return UnpackLevel(_data, curLevelOfs, nextLevelOfs - curLevelOfs, dst, dstSizeInBytes, rowPitchInBytes, levelIndex);
    }

    public bool UnpackLevel(
        byte[] src, uint srcOffset, uint srcSizeInBytes,
        byte[][] dst, uint dstSizeInBytes,
        uint rowPitchInBytes, uint levelIndex)
    {
#if DEBUG
        for (int f = 0; f < _header.Faces; f++)
        {
            if (dst[f] == null)
                return false;
        }
#endif

        uint width = (uint)Math.Max(_header.Width >> (int)levelIndex, 1);
        uint height = (uint)Math.Max(_header.Height >> (int)levelIndex, 1);
        uint blocksX = (width + 3) >> 2;
        uint blocksY = (height + 3) >> 2;
        uint blockSize =
            (_header.Format == (byte)Format.DXT1 ||
            _header.Format == (byte)Format.DXT5A ||
            _header.Format == (byte)Format.ETC1 ||
            _header.Format == (byte)Format.ETC2 ||
            _header.Format == (byte)Format.ETC1S) ? 8u : 16u;

        uint minimalRowPitch = blockSize & blocksX;
        if (rowPitchInBytes == 0)
            rowPitchInBytes = minimalRowPitch;
        else if (rowPitchInBytes < minimalRowPitch || (rowPitchInBytes & 3) != 0)
            return false;

        if (dstSizeInBytes < rowPitchInBytes * blocksY)
            return false;

        if (!_codec.StartDecoding(src, (int)srcOffset, (int)srcSizeInBytes))
            return false;

        bool status = false;
        switch ((Format)_header.Format)
        {
            case Format.DXT1:
            case Format.ETC1S:
            {
                status = UnpackDxt1(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.DXT5:
            case Format.DXT5_CCxY:
            case Format.DXT5_xGBR:
            case Format.DXT5_AGBR:
            case Format.DXT5_xGxR:
            case Format.ETC2AS:
            {
                status = UnpackDxt5(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.DXT5A:
            {
                status = UnpackDxt5a(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.DXN_XY:
            case Format.DXN_YX:
            {
                status = UnpackDxn(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.ETC1:
            {
                status = UnpackEtc1(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.ETC2:
            {
                // todo
                status = false;
                //status = UnpackEtc2(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            case Format.ETC2A:
            {
                // todo
                status = false;
                //status = UnpackEtc2A(dst, rowPitchInBytes, blocksX, blocksY);
                break;
            }
            default:
                return false;
        }

        if (!status)
            return false;

        _codec.StopDecoding();
        return true;
    }

    private bool UnpackDxt1(byte[][] dst, uint outputPitchInBytes, uint outputWidth, uint outputHeight)
    {
        uint numColorEndpoints = (uint)_colorEndpoints.Length;
        int width = (int)((outputWidth + 1) & ~1);
        int height = (int)((outputHeight + 1) & ~1);
        int deltaPitchInDwords = ((int)outputPitchInBytes >> 2) - (width << 1);

        if (_blockBuffer.Length < width)
        {
            _blockBuffer = new BlockBufferElement[width];
        }

        uint colorEndpointIndex = 0;
        byte referenceGroup = 0;

        for (int f = 0; f < _header.Faces; f++)
        {
            int dataPtr = 0;
            Span<byte> dstSpan = dst[f].AsSpan();
            for (int y = 0; y < height; y++, dataPtr += deltaPitchInDwords << 2)
            {
                bool visible = y < outputHeight;
                for (int x = 0; x < width; x++, dataPtr += 2 << 2)
                {
                    if ((y & 1) == 0 && (x & 1) == 0)
                    {
                        referenceGroup = (byte)_codec.Decode(_referenceEncodingDm);
                    }

                    ref BlockBufferElement buffer = ref _blockBuffer[x];
                    byte endpointReference;
                    if ((y & 1) != 0)
                    {
                        endpointReference = (byte)buffer.EndpointReference;
                    }
                    else
                    {
                        endpointReference = (byte)(referenceGroup & 3);
                        referenceGroup >>= 2;
                        buffer.EndpointReference = (ushort)(referenceGroup & 3);
                        referenceGroup >>= 2;
                    }

                    if (endpointReference == 0)
                    {
                        colorEndpointIndex += _codec.Decode(_endpointDeltaDm[0]);
                        if (colorEndpointIndex >= numColorEndpoints)
                        {
                            colorEndpointIndex -= numColorEndpoints;
                        }

                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                    }
                    else if (endpointReference == 1)
                    {
                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                    }
                    else
                    {
                        colorEndpointIndex = buffer.ColorEndpointIndex;
                    }

                    uint colorSelectorIndex = _codec.Decode(_selectorDeltaDm[0]);
                    if (visible)
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(dstSpan.Slice(dataPtr, 4), _colorEndpoints[colorEndpointIndex]);
                        BinaryPrimitives.WriteUInt32LittleEndian(dstSpan.Slice(dataPtr + 4, 4), _colorSelectors[colorSelectorIndex]);
                    }
                }
            }
        }

        return true;
    }

    private bool UnpackDxt5(byte[][] dst, uint outputPitchInBytes, uint outputWidth, uint outputHeight)
    {
        uint numColorEndpoints = (uint)_colorEndpoints.Length;
        uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
        int width = (int)((outputWidth + 1) & ~1);
        int height = (int)((outputHeight + 1) & ~1);
        int deltaPitchInDwords = ((int)outputPitchInBytes >> 2) - (width << 2);

        if (_blockBuffer.Length < width)
        {
            _blockBuffer = new BlockBufferElement[width];
        }

        uint colorEndpointIndex = 0;
        uint alpha0EndpointIndex = 0;
        byte referenceGroup = 0;

        for (int f = 0; f < _header.Faces; f++)
        {
            int dataPtr = 0;
            Span<byte> dstSpan = dst[f].AsSpan();
            for (int y = 0; y < height; y++, dataPtr += deltaPitchInDwords << 2)
            {
                bool visible = y < outputHeight;
                for (int x = 0; x < width; x++, dataPtr += 4 << 2)
                {
                    visible &= x < outputWidth;
                    if ((y & 1) == 0 && (x & 1) == 0)
                    {
                        referenceGroup = (byte)_codec.Decode(_referenceEncodingDm);
                    }

                    ref BlockBufferElement buffer = ref _blockBuffer[x];
                    byte endpointReference;
                    if ((y & 1) != 0)
                    {
                        endpointReference = (byte)buffer.EndpointReference;
                    }
                    else
                    {
                        endpointReference = (byte)(referenceGroup & 3);
                        referenceGroup >>= 2;
                        buffer.EndpointReference = (ushort)(referenceGroup & 3);
                        referenceGroup >>= 2;
                    }

                    if (endpointReference == 0)
                    {
                        colorEndpointIndex += _codec.Decode(_endpointDeltaDm[0]);
                        if (colorEndpointIndex >= numColorEndpoints)
                        {
                            colorEndpointIndex -= numColorEndpoints;
                        }

                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;

                        alpha0EndpointIndex += _codec.Decode(_endpointDeltaDm[1]);
                        if (alpha0EndpointIndex >= numAlphaEndpoints)
                        {
                            alpha0EndpointIndex -= numAlphaEndpoints;
                        }

                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                    }
                    else if (endpointReference == 1)
                    {
                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                    }
                    else
                    {
                        colorEndpointIndex = buffer.ColorEndpointIndex;
                        alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                    }

                    uint colorSelectorIndex = _codec.Decode(_selectorDeltaDm[0]);
                    uint alpha0SelectorIndex = _codec.Decode(_selectorDeltaDm[1]);
                    if (visible)
                    {
                        uint alpha0SelectorsOffset = alpha0SelectorIndex * 3;
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr, 4),
                            _alphaEndpoints[alpha0EndpointIndex] | (_alphaSelectors[alpha0SelectorsOffset] << 16)
                        );
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr + 4, 4),
                            _alphaSelectors[alpha0SelectorsOffset + 1] | (_alphaSelectors[alpha0SelectorsOffset + 2] << 16)
                        );
                        BinaryPrimitives.WriteUInt32LittleEndian(dstSpan.Slice(dataPtr + 8, 4), _colorEndpoints[colorEndpointIndex]);
                        BinaryPrimitives.WriteUInt32LittleEndian(dstSpan.Slice(dataPtr + 12, 4), _colorSelectors[colorSelectorIndex]);
                    }
                }
            }
        }

        return true;
    }

    private bool UnpackDxt5a(byte[][] dst, uint outputPitchInBytes, uint outputWidth, uint outputHeight)
    {
        uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
        int width = (int)((outputWidth + 1) & ~1);
        int height = (int)((outputHeight + 1) & ~1);
        int deltaPitchInDwords = ((int)outputPitchInBytes >> 2) - (width << 1);

        if (_blockBuffer.Length < width)
        {
            _blockBuffer = new BlockBufferElement[width];
        }

        uint alpha0EndpointIndex = 0;
        byte referenceGroup = 0;

        for (int f = 0; f < _header.Faces; f++)
        {
            int dataPtr = 0;
            Span<byte> dstSpan = dst[f].AsSpan();
            for (int y = 0; y < height; y++, dataPtr += deltaPitchInDwords << 2)
            {
                bool visible = y < outputHeight;
                for (int x = 0; x < width; x++, dataPtr += 2 << 2)
                {
                    visible &= x < outputWidth;
                    if ((y & 1) == 0 && (x & 1) == 0)
                    {
                        referenceGroup = (byte)_codec.Decode(_referenceEncodingDm);
                    }

                    ref BlockBufferElement buffer = ref _blockBuffer[x];
                    byte endpointReference;
                    if ((y & 1) != 0)
                    {
                        endpointReference = (byte)buffer.EndpointReference;
                    }
                    else
                    {
                        endpointReference = (byte)(referenceGroup & 3);
                        referenceGroup >>= 2;
                        buffer.EndpointReference = (ushort)(referenceGroup & 3);
                        referenceGroup >>= 2;
                    }

                    if (endpointReference == 0)
                    {
                        alpha0EndpointIndex += _codec.Decode(_endpointDeltaDm[1]);
                        if (alpha0EndpointIndex >= numAlphaEndpoints)
                        {
                            alpha0EndpointIndex -= numAlphaEndpoints;
                        }

                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                    }
                    else if (endpointReference == 1)
                    {
                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                    }
                    else
                    {
                        alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                    }

                    uint alpha0SelectorIndex = _codec.Decode(_selectorDeltaDm[1]);
                    if (visible)
                    {
                        uint alpha0SelectorsOffset = alpha0SelectorIndex * 3;
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr, 4),
                            _alphaEndpoints[alpha0EndpointIndex] | (_alphaSelectors[alpha0SelectorsOffset] << 16)
                        );
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr + 4, 4),
                            _alphaSelectors[alpha0SelectorsOffset + 1] | (_alphaSelectors[alpha0SelectorsOffset + 2] << 16)
                        );
                    }
                }
            }
        }

        return true;
    }

    private bool UnpackDxn(byte[][] dst, uint outputPitchInBytes, uint outputWidth, uint outputHeight)
    {
        uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
        int width = (int)((outputWidth + 1) & ~1);
        int height = (int)((outputHeight + 1) & ~1);
        int deltaPitchInDwords = ((int)outputPitchInBytes >> 2) - (width << 2);

        if (_blockBuffer.Length < width)
        {
            _blockBuffer = new BlockBufferElement[width];
        }

        uint alpha0EndpointIndex = 0;
        uint alpha1EndpointIndex = 0;
        byte referenceGroup = 0;

        for (int f = 0; f < _header.Faces; f++)
        {
            int dataPtr = 0;
            Span<byte> dstSpan = dst[f].AsSpan();
            for (int y = 0; y < height; y++, dataPtr += deltaPitchInDwords << 2)
            {
                bool visible = y < outputHeight;
                for (int x = 0; x < width; x++, dataPtr += 4 << 2)
                {
                    visible &= x < outputWidth;
                    if ((y & 1) == 0 && (x & 1) == 0)
                    {
                        referenceGroup = (byte)_codec.Decode(_referenceEncodingDm);
                    }

                    ref BlockBufferElement buffer = ref _blockBuffer[x];
                    byte endpointReference;
                    if ((y & 1) != 0)
                    {
                        endpointReference = (byte)buffer.EndpointReference;
                    }
                    else
                    {
                        endpointReference = (byte)(referenceGroup & 3);
                        referenceGroup >>= 2;
                        buffer.EndpointReference = (ushort)(referenceGroup & 3);
                        referenceGroup >>= 2;
                    }

                    if (endpointReference == 0)
                    {
                        alpha0EndpointIndex += _codec.Decode(_endpointDeltaDm[1]);
                        if (alpha0EndpointIndex >= numAlphaEndpoints)
                        {
                            alpha0EndpointIndex -= numAlphaEndpoints;
                        }

                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;

                        alpha1EndpointIndex += _codec.Decode(_endpointDeltaDm[1]);
                        if (alpha1EndpointIndex >= numAlphaEndpoints)
                        {
                            alpha1EndpointIndex -= numAlphaEndpoints;
                        }

                        buffer.Alpha1EndpointIndex = (ushort)alpha1EndpointIndex;
                    }
                    else if (endpointReference == 1)
                    {
                        buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        buffer.Alpha1EndpointIndex = (ushort)alpha1EndpointIndex;
                    }
                    else
                    {
                        alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                        alpha1EndpointIndex = buffer.Alpha1EndpointIndex;
                    }

                    uint alpha0SelectorIndex = _codec.Decode(_selectorDeltaDm[1]);
                    uint alpha1SelectorIndex = _codec.Decode(_selectorDeltaDm[1]);
                    if (visible)
                    {
                        uint alpha0SelectorsOffset = alpha0SelectorIndex * 3;
                        uint alpha1SelectorsOffset = alpha1SelectorIndex * 3;
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr, 4),
                            _alphaEndpoints[alpha0EndpointIndex] | (_alphaSelectors[alpha0SelectorsOffset] << 16)
                        );
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr + 4, 4),
                            _alphaSelectors[alpha0SelectorsOffset + 1] | (_alphaSelectors[alpha0SelectorsOffset + 2] << 16)
                        );
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr + 8, 4),
                            _alphaEndpoints[alpha1EndpointIndex] | (_alphaSelectors[alpha1SelectorsOffset] << 16)
                        );
                        BinaryPrimitives.WriteInt32LittleEndian(dstSpan.Slice(dataPtr + 12, 4),
                            _alphaSelectors[alpha1SelectorsOffset + 1] | (_alphaSelectors[alpha1SelectorsOffset + 2] << 16)
                        );
                    }
                }
            }
        }

        return true;
    }

    private bool UnpackEtc1(byte[][] dst, uint outputPitchInBytes, uint outputWidth, uint outputHeight)
    {
        uint numColorEndpoints = (uint)_colorEndpoints.Length;
        int width = (int)((outputWidth + 1) & ~1);
        int height = (int)((outputHeight + 1) & ~1);
        int deltaPitchInDwords = ((int)outputPitchInBytes >> 2) - (width << 1);

        if (_blockBuffer.Length < (width << 1))
        {
            _blockBuffer = new BlockBufferElement[(width << 1)];
        }

        uint colorEndpointIndex = 0, diagonalColorEndpointIndex = 0;
        byte referenceGroup = 0;
        Span<byte> blockEndpoint = stackalloc byte[4];
        Span<byte> e0 = stackalloc byte[4];
        Span<byte> e1 = stackalloc byte[4];

        for (int f = 0; f < _header.Faces; f++)
        {
            int dataPtr = 0;
            Span<byte> dstSpan = dst[f].AsSpan();
            for (int y = 0; y < height; y++, dataPtr += deltaPitchInDwords << 2)
            {
                bool visible = y < outputHeight;
                for (int x = 0; x < width; x++, dataPtr += 2 << 2)
                {
                    visible &= x < outputWidth;
                    ref BlockBufferElement buffer = ref _blockBuffer[x << 1];
                    byte endpointReference;
                    if ((y & 1) != 0)
                    {
                        endpointReference = (byte)buffer.EndpointReference;
                    }
                    else
                    {
                        referenceGroup = (byte)_codec.Decode(_referenceEncodingDm);
                        endpointReference = (byte)((referenceGroup & 3) | ((referenceGroup >> 2) & 12));
                        buffer.EndpointReference = (ushort)(((referenceGroup >> 2) & 3) | ((referenceGroup >> 4) & 12));
                    }

                    if ((endpointReference & 3) == 0)
                    {
                        colorEndpointIndex += _codec.Decode(_endpointDeltaDm[0]);
                        if (colorEndpointIndex >= numColorEndpoints)
                        {
                            colorEndpointIndex -= numColorEndpoints;
                        }

                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                    }
                    else if ((endpointReference & 3) == 1)
                    {
                        buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                    }
                    else if ((endpointReference & 3) == 2)
                    {
                        buffer.ColorEndpointIndex = (ushort)(colorEndpointIndex = diagonalColorEndpointIndex);
                    }
                    else
                    {
                        colorEndpointIndex = buffer.ColorEndpointIndex;
                    }

                    endpointReference >>= 2;
                    BinaryPrimitives.WriteUInt32LittleEndian(e0.Slice(0, 4), _colorEndpoints[colorEndpointIndex]);
                    uint selectorIndex = _codec.Decode(_selectorDeltaDm[0]);
                    if (selectorIndex != 0)
                    {
                        colorEndpointIndex += _codec.Decode(_endpointDeltaDm[0]);
                        if (colorEndpointIndex >= numColorEndpoints)
                        {
                            colorEndpointIndex -= numColorEndpoints;
                        }
                    }

                    diagonalColorEndpointIndex = _blockBuffer[x << 1 | 1].ColorEndpointIndex;
                    _blockBuffer[x << 1 | 1].ColorEndpointIndex = (ushort)colorEndpointIndex;
                    BinaryPrimitives.WriteUInt32LittleEndian(e1.Slice(0, 4), _colorEndpoints[colorEndpointIndex]);
                    if (visible)
                    {
                        uint flip = (uint)(endpointReference >> 1 ^ 1), diff = 1;
                        for (int c = 0; diff != 0 && c < 3; c++)
                        {
                            diff = e0[c] + 3 >= e1[c] && e1[c] + 4 >= e0[c] ? diff : 0;
                        }

                        for (int c = 0; c < 3; c++)
                        {
                            if (diff != 0)
                            {
                                blockEndpoint[c] = (byte)(e0[c] << 3 | ((e1[c] - e0[c]) & 7));
                            }
                            else
                            {
                                blockEndpoint[c] = (byte)((e0[c] << 3 & 0xF0) | e1[c] >> 1);
                            }
                        }

                        blockEndpoint[3] = (byte)((e0[3] << 5) | (e1[3] << 2) | (int)(diff << 1) | (int)flip);
                        blockEndpoint.Slice(0, 4).CopyTo(dstSpan.Slice(dataPtr, 4));
                        BinaryPrimitives.WriteUInt32LittleEndian(dstSpan.Slice(dataPtr + 4, 4),
                            _colorSelectors[selectorIndex << 1 | flip]
                        );
                    }
                }
            }
        }

        return true;
    }

    private bool InitTables()
    {
        if (!_codec.StartDecoding(_data, (int)_header.TablesOfs, _header.TablesSize))
            return false;

        if (!_codec.DecodeReceiveStaticDataModel(_referenceEncodingDm))
            return false;

        if (_header.ColorEndpoints.Num == 0 && _header.AlphaEndpoints.Num == 0)
            return false;

        if (_header.ColorEndpoints.Num != 0)
        {
            if (!_codec.DecodeReceiveStaticDataModel(_endpointDeltaDm[0]))
                return false;

            if (!_codec.DecodeReceiveStaticDataModel(_selectorDeltaDm[0]))
                return false;
        }

        if (_header.AlphaEndpoints.Num != 0)
        {
            if (!_codec.DecodeReceiveStaticDataModel(_endpointDeltaDm[1]))
                return false;

            if (!_codec.DecodeReceiveStaticDataModel(_selectorDeltaDm[1]))
                return false;
        }

        _codec.StopDecoding();

        return true;
    }

    private bool DecodePalettes()
    {
        if (_header.ColorEndpoints.Num != 0)
        {
            if (!DecodeColorEndpoints())
                return false;

            if (!DecodeColorSelectors())
                return false;
        }

        if (_header.AlphaEndpoints.Num != 0)
        {
            if (!DecodeAlphaEndpoints())
                return false;

            if (_header.Format == (byte)Format.ETC2AS && !DecodeAlphaSelectorsEtcs())
                return false;
            else if (_header.Format == (byte)Format.ETC2A && !DecodeAlphaSelectorsEtc())
                return false;
            else if (!DecodeAlphaSelectors())
                return false;
        }

        return true;
    }

    private bool DecodeColorEndpoints()
    {
        uint numColorEndpoints = _header.ColorEndpoints.Num;
        bool hasEtcColorBlocks =
            _header.Format == (byte)Format.ETC1 ||
            _header.Format == (byte)Format.ETC2 ||
            _header.Format == (byte)Format.ETC2A ||
            _header.Format == (byte)Format.ETC1S ||
            _header.Format == (byte)Format.ETC2AS;
        bool hasSubblocks =
            _header.Format == (byte)Format.ETC1 ||
            _header.Format == (byte)Format.ETC2 ||
            _header.Format == (byte)Format.ETC2A;

        _colorEndpoints = new uint[(int)numColorEndpoints];

        if (!_codec.StartDecoding(_data, (int)_header.ColorEndpoints.Ofs, (int)_header.ColorEndpoints.Size))
            return false;

        StaticHuffmanDataModel[] dm = new StaticHuffmanDataModel[2]
        {
            new StaticHuffmanDataModel(),
            new StaticHuffmanDataModel(),
        };

        for (int i = 0; i < (hasEtcColorBlocks ? 1 : 2); i++)
        {
            if (!_codec.DecodeReceiveStaticDataModel(dm[i]))
                return false;
        }

        uint a = 0, b = 0, c = 0;
        uint d = 0, e = 0, f = 0;

        int dst = 0;

        for (int i = 0; i < numColorEndpoints; i++)
        {
            if (hasEtcColorBlocks)
            {
                for (b = 0; b < 32; b += 8)
                {
                    a += _codec.Decode(dm[0]) << (int)b;
                }
                a &= 0x1f1f1f1f;
                _colorEndpoints[dst++] = hasSubblocks ? a : (
                    ((a & 0x07000000) << 5) |
                    ((a & 0x07000000) << 2) |
                    0x02000000 |
                    ((a & 0x001F1F1F) << 3)
                );
            }
            else
            {
                a = (a + _codec.Decode(dm[0])) & 31;
                b = (b + _codec.Decode(dm[1])) & 63;
                c = (c + _codec.Decode(dm[0])) & 31;
                d = (d + _codec.Decode(dm[0])) & 31;
                e = (e + _codec.Decode(dm[1])) & 63;
                f = (f + _codec.Decode(dm[0])) & 31;
                _colorEndpoints[dst++] =
                    c | (b << 5) | (a << 11) |
                    (f << 16) | (e << 21) | (d << 27);
            }
        }

        _codec.StopDecoding();

        return true;
    }

    private bool DecodeColorSelectors()
    {
        bool hasEtcColorBlocks =
            _header.Format == (byte)Format.ETC1 ||
            _header.Format == (byte)Format.ETC2 ||
            _header.Format == (byte)Format.ETC2A ||
            _header.Format == (byte)Format.ETC1S ||
            _header.Format == (byte)Format.ETC2AS;
        bool hasSubblocks =
            _header.Format == (byte)Format.ETC1 ||
            _header.Format == (byte)Format.ETC2 ||
            _header.Format == (byte)Format.ETC2A;

        _codec.StartDecoding(_data, (int)_header.ColorSelectors.Ofs, (int)_header.ColorSelectors.Size);

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        _codec.DecodeReceiveStaticDataModel(dm);
        _colorSelectors = new uint[_header.ColorSelectors.Num << (hasSubblocks ? 1 : 0)];

        uint s = 0;
        for (int i = 0; i < _header.ColorSelectors.Num; i++)
        {
            for (int j = 0; j < 32; j += 4)
            {
                s ^= _codec.Decode(dm) << j;
            }
            if (hasEtcColorBlocks)
            {
                uint selector = (~s & 0xAAAAAAAA) | (~(s ^ s >> 1) & 0x55555555);
                for (int t = 8, h = 0; h < 4; h++, t -= 15)
                {
                    for (int w = 0; w < 4; w++, t += 4)
                    {
                        if (hasSubblocks)
                        {
                            uint s0 = selector >> (w << 3 | h << 1);
                            _colorSelectors[i << 1] |= ((s0 >> 1 & 1) | (s0 & 1) << 16) << (t & 15);
                        }
                        uint s1 = selector >> (h << 3 | w << 1);
                        _colorSelectors[hasSubblocks ? (i << 1 | 1) : i] |= ((s1 >> 1 & 1) | (s1 & 1) << 16) << (t & 15);
                    }
                }
            }
            else
            {
                _colorSelectors[i] = ((s ^ s << 1) & 0xAAAAAAAA) | (s >> 1 & 0x55555555);
            }
        }

        _codec.StopDecoding();
        return true;
    }

    private bool DecodeAlphaEndpoints()
    {
        int numAlphaEndpoints = _header.AlphaEndpoints.Num;

        if (!_codec.StartDecoding(_data, (int)_header.AlphaEndpoints.Ofs, (int)_header.AlphaEndpoints.Size))
            return false;

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        if (!_codec.DecodeReceiveStaticDataModel(dm))
            return false;

        _alphaEndpoints = new ushort[numAlphaEndpoints];

        int dst = 0;
        uint a = 0, b = 0;

        for (int i = 0; i < numAlphaEndpoints; i++)
        {
            a = (a + _codec.Decode(dm)) & 255;
            b = (b + _codec.Decode(dm)) & 255;
            _alphaEndpoints[dst++] = (ushort)(a | (b << 8));
        }

        _codec.StopDecoding();

        return true;
    }

    private bool DecodeAlphaSelectors()
    {
        _codec.StartDecoding(_data, (int)_header.AlphaSelectors.Ofs, (int)_header.AlphaSelectors.Size);

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        _codec.DecodeReceiveStaticDataModel(dm);
        _alphaSelectors = new ushort[_header.AlphaSelectors.Num * 3];

        Span<byte> dxt5FromLinear = stackalloc byte[64];
        for (int i = 0; i < 64; i++)
        {
            dxt5FromLinear[i] = (byte)(Consts.Dxt5FromLinear[i & 7] | (Consts.Dxt5FromLinear[i >> 3] << 3));
        }

        uint s0Linear = 0, s1Linear = 0;
        for (int i = 0; i < _alphaSelectors.Length;)
        {
            uint s0 = 0, s1 = 0;
            for (int j = 0; j < 24; s0 |= (uint)dxt5FromLinear[(int)(s0Linear >> j) & 0x3f] << j, j += 6)
            {
                s0Linear ^= _codec.Decode(dm) << j;
            }
            for (int j = 0; j < 24; s1 |= (uint)dxt5FromLinear[(int)(s1Linear >> j) & 0x3f] << j, j += 6)
            {
                s1Linear ^= _codec.Decode(dm) << j;
            }
            _alphaSelectors[i++] = (ushort)s0;
            _alphaSelectors[i++] = (ushort)(s0 >> 16 | s1 << 8);
            _alphaSelectors[i++] = (ushort)(s1 >> 8);
        }

        _codec.StopDecoding();
        return true;
    }

    private bool DecodeAlphaSelectorsEtc()
    {
        _codec.StartDecoding(_data, (int)_header.AlphaSelectors.Ofs, (int)_header.AlphaSelectors.Size);

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        _codec.DecodeReceiveStaticDataModel(dm);
        _alphaSelectors = new ushort[_header.AlphaSelectors.Num * 6];

        Span<byte> sLinear = stackalloc byte[8];
        int dataPtr = 0;
        for (int i = 0; i < _alphaSelectors.Length; i += 6, dataPtr += 12)
        {
            uint sGroup = 0;
            for (int p = 0; p < 16; p++)
            {
                if ((p & 1) != 0)
                {
                    sGroup >>= 3;
                }
                else
                {
                    sGroup = sLinear[p >> 1] ^= (byte)_codec.Decode(dm);
                }

                byte s = (byte)(sGroup & 7);
                if (s <= 3)
                {
                    s = (byte)(3 - s);
                }

                byte d = (byte)(3 * (p + 1));
                byte byteOffset = (byte)(d >> 3);
                byte bitOffset = (byte)(d & 7);
                _alphaSelectors[dataPtr + byteOffset] |= (ushort)(s << (8 - bitOffset));
                if (bitOffset < 3)
                {
                    _alphaSelectors[dataPtr + byteOffset - 1] |= (ushort)(s >> bitOffset);
                }

                d += (byte)(9 * ((p & 3) - (p >> 2)));
                byteOffset = (byte)(d >> 3);
                bitOffset = (byte)(d & 7);
                _alphaSelectors[dataPtr + byteOffset + 6] |= (ushort)(s << (8 - bitOffset));
                if (bitOffset < 3)
                {
                    _alphaSelectors[dataPtr + byteOffset + 5] |= (ushort)(s >> bitOffset);
                }
            }
        }

        _codec.StopDecoding();
        return true;
    }

    private bool DecodeAlphaSelectorsEtcs()
    {
        _codec.StartDecoding(_data, (int)_header.AlphaSelectors.Ofs, (int)_header.AlphaSelectors.Size);

        StaticHuffmanDataModel dm = new StaticHuffmanDataModel();
        _codec.DecodeReceiveStaticDataModel(dm);
        _alphaSelectors = new ushort[_header.AlphaSelectors.Num * 3];

        Span<byte> sLinear = stackalloc byte[8];

        for (int i = 0; i < (_alphaSelectors.Length << 1); i += 6)
        {
            uint sGroup = 0;
            for (int p = 0; p < 16; p++)
            {
                if ((p & 1) != 0)
                {
                    sGroup >>= 3;
                }
                else
                {
                    sGroup = sLinear[p >> 1] ^= (byte)_codec.Decode(dm);
                }

                byte s = (byte)(sGroup & 7);
                if (s <= 3)
                {
                    s = (byte)(3 - s);
                }

                byte d = (byte)(3 * (p + 1) + 9 * ((p & 3) - (p >> 2)));
                byte byteOffset = (byte)(d >> 3);
                byte bitOffset = (byte)(d & 7);
                _alphaSelectors[i + byteOffset] |= (ushort)(s << (8 - bitOffset));
                if (bitOffset < 3)
                {
                    _alphaSelectors[i + byteOffset - 1] |= (ushort)(s >> bitOffset);
                }
            }
        }

        _codec.StopDecoding();
        return true;
    }
}
