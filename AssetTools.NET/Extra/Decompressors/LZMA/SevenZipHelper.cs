using System;
using System.IO;


namespace SevenZip.Compression.LZMA
{
    public static class SevenZipHelper
    {
        static CoderPropID[] propIDs =
        {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        // these are the default properties, keeping it simple for now:
        static object[] properties =
        {
            1 << 21, // DictionarySize
            2, // PosStateBits
            3, // LitContextBits
            0, // LitPosBits
            2, // Algorithm
            32, // NumFastBytes
            "bt4", // MatchFinder
            false // EndMarker
        };

        public static byte[] Compress(byte[] inputBytes, ICodeProgress progress = null)
        {
            MemoryStream inStream = new MemoryStream(inputBytes);
            MemoryStream outStream = new MemoryStream();
            Compress(inStream, outStream, progress);
            return outStream.ToArray();
        }

        public static void Compress(Stream inStream, Stream outStream, ICodeProgress progress = null)
        {
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            //don't write size
            //long fileSize = inStream.Length;
            //for (int i = 0; i < 8; i++)
            //    outStream.WriteByte((Byte)(fileSize >> (8 * i)));
            encoder.Code(inStream, outStream, -1, -1, progress);
        }

        public static byte[] Decompress(byte[] inputBytes)
        {
            MemoryStream newInStream = new MemoryStream(inputBytes);

            Decoder decoder = new Decoder();

            newInStream.Seek(0, 0);
            MemoryStream newOutStream = new MemoryStream();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            byte[] b = newOutStream.ToArray();

            return b;
        }


        public static MemoryStream StreamDecompress(MemoryStream newInStream)
        {
            Decoder decoder = new Decoder();

            newInStream.Seek(0, 0);
            MemoryStream newOutStream = new MemoryStream();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            newOutStream.Position = 0;
            return newOutStream;
        }

        public static MemoryStream StreamDecompress(MemoryStream newInStream, long outSize)
        {
            Decoder decoder = new Decoder();

            newInStream.Seek(0, 0);
            MemoryStream newOutStream = new MemoryStream();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            newOutStream.Position = 0;
            return newOutStream;
        }

        public static void StreamDecompress(Stream compressedStream, Stream decompressedStream, long compressedSize, long decompressedSize)
        {
            long basePosition = compressedStream.Position;
            Decoder decoder = new Decoder();

            byte[] properties = new byte[5];
            if (compressedStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);

            decoder.Code(compressedStream, decompressedStream, compressedSize - 5, decompressedSize, null);
            compressedStream.Position = basePosition + compressedSize;
        }
    }
}
