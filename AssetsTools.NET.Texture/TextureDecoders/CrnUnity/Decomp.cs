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

namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
public class Decomp
{
    public static Unpacker UnpackBegin(byte[] data)
    {
        if (data == null || data.Length < Consts.HEADER_MIN_SIZE)
            return null;

        Unpacker unpacker = new Unpacker();
        if (!unpacker.Init(data))
            return null;

        return unpacker;
    }

    public static bool UnpackEnd(Unpacker unpacker)
    {
        if (unpacker == null)
            return false;

        if (!unpacker.IsValid())
            return false;

        // obviously we can't delete anything here

        return true;
    }

    public static TextureInfo GetTextureInfo(byte[] data)
    {
        if (data == null || data.Length < Consts.HEADER_MIN_SIZE)
            return null;

        Header header = Info.GetHeader(data);
        if (header == null)
            return null;

        TextureInfo info = new TextureInfo();
        info.Width = header.Width;
        info.Height = header.Height;
        info.Levels = header.Levels;
        info.Faces = header.Faces;
        info.Format = (Format)header.Format;
        info.BytesPerBlock =
            (header.Format == (byte)Format.DXT1 ||
            header.Format == (byte)Format.DXT5A ||
            header.Format == (byte)Format.ETC1 ||
            header.Format == (byte)Format.ETC2 ||
            header.Format == (byte)Format.ETC1S) ? 8u : 16u;

        info.Userdata0 = header.Userdata0;
        info.Userdata1 = header.Userdata1;
        return info;
    }

    public static bool UnpackLevel(Unpacker unpacker, byte[][] dst, uint rowPitchInBytes, uint levelIndex)
    {
        uint dstSizeInBytes = 0;
        for (int f = 0; f < dst.Length; f++)
        {
            dstSizeInBytes += (uint)dst[f].Length;
        }

        if (unpacker == null || dst == null || dstSizeInBytes < 8 || levelIndex >= Consts.MAX_LEVELS)
            return false;

        // not really necessary for safe language like c#
        if (!unpacker.IsValid())
            return false;

        return unpacker.UnpackLevel(dst, dstSizeInBytes, rowPitchInBytes, levelIndex);
    }
}
