using System;
using System.IO;

namespace AssetsTools.NET.Texture
{
    public static class SwitchPlatformBlob
    {
        private const int MaxMipCount = 14; // assuming max size 16384
        private const int MipOffTableOff = 0x10;

        public static int GetBlockHeightIndex(byte[] platformBlob)
        {
            return BitConverter.ToInt32(platformBlob, 8);
        }

        public static int[] GetMipOffsets(byte[] platformBlob)
        {
            int mipCount;
            for (mipCount = 1; mipCount < MaxMipCount; mipCount++)
            {
                int mipOff = BitConverter.ToInt32(platformBlob, MipOffTableOff + (mipCount << 2));
                if (mipOff == 0)
                {
                    break;
                }
            }

            int[] mipOffsets = new int[mipCount];
            for (int i = 0; i < mipCount; i++)
            {
                mipOffsets[i] = BitConverter.ToInt32(platformBlob, MipOffTableOff + (i << 2));
            }

            return mipOffsets;
        }

        public static uint GetCompleteImageSize1(byte[] platformBlob)
        {
            return BitConverter.ToUInt32(platformBlob, 0x54);
        }

        public static uint GetCompleteImageSize2(byte[] platformBlob)
        {
            return BitConverter.ToUInt32(platformBlob, 0x58);
        }

        public static byte[] MakePlatformBlob(int blockHeightIndex, int[] mipOffsets, uint completeImageSize)
        {
            MemoryStream ms = new MemoryStream(0x5c);
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write(0); // unk, only seen as 0
            writer.Write(0x20); // unk, only seen as 0x20
            writer.Write(blockHeightIndex); // log2(blockHeight)
            writer.Write((ushort)0x07); // unk, only seen as 0x07
            writer.Write((ushort)0x01); // unk, only seen as 0x01 (could be image count?)
            for (int i = 0; i < MaxMipCount; i++)
            {
                writer.Write(i < mipOffsets.Length ? mipOffsets[i] : 0);
            }

            writer.Write(0); // unk, only seen as 0
            writer.Write(0); // unk, only seen as 0
            writer.Write(0); // unk, only seen as 0
            writer.Write(completeImageSize);
            writer.Write(completeImageSize); // duplicated for whatever reason

            return ms.ToArray();
        }
    }
}