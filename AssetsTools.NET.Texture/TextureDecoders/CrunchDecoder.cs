using AssetRipper.TextureDecoder.Dxt;
using AssetRipper.TextureDecoder.Etc;
using AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
using System;

namespace AssetsTools.NET.Texture.TextureDecoders
{
    public class CrunchDecoder
    {
        public static int Decompress(byte[] data, int width, int height, TextureFormat format, out byte[] output)
        {
            output = [];

            TextureInfo texInfo = Decomp.GetTextureInfo(data);
            if (texInfo == null)
            {
                return 0;
            }

            Unpacker context = Decomp.UnpackBegin(data);
            if (context == null)
            {
                return 0;
            }

            uint levelWidth = Math.Max(1, texInfo.Width);
            uint levelHeight = Math.Max(1, texInfo.Height);
            uint numBlocksX = (levelWidth + 3) >> 2;
            uint numBlocksY = (levelHeight + 3) >> 2;

            uint rowPitch = numBlocksX * texInfo.BytesPerBlock;
            uint faceSize = numBlocksY * rowPitch;

            byte[][] faceData = new byte[texInfo.Faces][];
            for (int i = 0; i < texInfo.Faces; i++)
            {
                faceData[i] = new byte[faceSize];
            }

            bool success = Decomp.UnpackLevel(context, faceData, rowPitch, 0);
            Decomp.UnpackEnd(context);

            if (!success || faceData.Length == 0)
            {
                return 0;
            }

            byte[] firstFace = faceData[0];
            return format switch
            {
                TextureFormat.DXT1Crunched => DxtDecoder.DecompressDXT1(firstFace, width, height, out output),
                TextureFormat.DXT5Crunched => DxtDecoder.DecompressDXT5(firstFace, width, height, out output),
                TextureFormat.ETC_RGB4Crunched => EtcDecoder.DecompressETC(firstFace, width, height, out output),
                TextureFormat.ETC2_RGBA8Crunched => EtcDecoder.DecompressETC2A8(firstFace, width, height, out output),
                _ => 0,
            };
        }
    }
}
