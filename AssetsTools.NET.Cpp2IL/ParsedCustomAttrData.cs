using System.IO;

namespace AssetsTools.NET.Cpp2IL
{
    public class ParsedCustomAttrData
    {
        public uint[] attributeIndices;

        public ParsedCustomAttrData(byte[] data)
        {
            using AssetsFileReader br = new AssetsFileReader(new MemoryStream(data));
            br.BigEndian = false;

            uint attributeCount = ReadCompressedUint(br);
            attributeIndices = new uint[attributeCount];
            for (int i = 0; i < attributeCount; i++)
            {
                attributeIndices[i] = br.ReadUInt32();
            }

            // todo: read the rest
        }

        // no idea what happens in big-endian, but is there anything still in big-endian?
        private uint ReadCompressedUint(AssetsFileReader r)
        {
            uint ReadFullUint(AssetsFileReader r)
            {
                return r.ReadUInt32();
            }

            uint ReadPartialUint(AssetsFileReader r, byte b)
            {
                if ((b & 192) == 192)
                    return (b & ~192U) << 24 | (uint)(r.ReadByte() << 16) | (uint)(r.ReadByte() << 8) | (uint)r.ReadByte();
                else if ((b & 128) == 128)
                    return (b & ~128U) << 8 | (uint)r.ReadByte();
                else
                    throw new InvalidDataException($"Can't read {b}-type compressed int");
            }

            var b = r.ReadByte();
            var result = b switch
            {
                255 => uint.MaxValue,
                254 => uint.MaxValue - 1,
                >= 0 and <= 127 => b,
                240 => ReadFullUint(r),
                _ => ReadPartialUint(r, b),
            };
            return result;
        }
    }
}
