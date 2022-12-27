using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using AssetsTools.NET.Extra;
using Mono.Cecil;

namespace AssetsTools.NET
{
    public class AssetsFileStatefulReader : IDisposable
    {
        //todo, this should default to bigEndian = false
        //since it's more likely little endian than big endian
        public bool bigEndian = true;
        public readonly bool shared;
        public bool threadSafe;
        public IAssetsFileReaderImpl impl;
        public IAssetsFileReaderStreamExtra streamImpl;
        internal long position = -1;

        public AssetsFileStatefulReader(IAssetsFileReaderImpl impl, bool shared)
        {
            this.impl = impl;
            this.shared = shared;
            streamImpl = impl as IAssetsFileReaderStreamExtra;
            threadSafe = streamImpl == null;
            position = 0;
        }

        internal AssetsFileStatefulReader(AssetsFileStatefulReader other)
        {
            if (!other.shared) throw new InvalidOperationException();
            impl = other.impl;
            shared = true;
            streamImpl = other.streamImpl;
            threadSafe = other.threadSafe;
        }

        public void Close()
        {
            if (!shared)
                impl.Dispose();
        }
        public void Dispose() { Close(); }

        public byte ReadByte()
        {
            byte value = impl.ReadByte(position);
            position++;
            return value;
        }
        public sbyte ReadSByte()
        {
            sbyte value = impl.ReadSByte(position);
            position++;
            return value;
        }
        public bool ReadBoolean() { return ReadByte() != 0; }
        public short ReadInt16()
        {
            unchecked
            {
                short value = impl.ReadInt16(position);
                position += 2;
                return bigEndian ? (short)ReverseShort((ushort)value) : value;
            }
        }
        public ushort ReadUInt16()
        {
            unchecked
            {
                ushort value = impl.ReadUInt16(position);
                position += 2;
                return bigEndian ? ReverseShort(value) : value;
            }
        }
        public int ReadInt24()
        {
            unchecked
            {
                return bigEndian ? (int)ReverseInt((uint)System.BitConverter.ToInt32(ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 0)) :
                    System.BitConverter.ToInt32(ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 0);
            }
        }
        public uint ReadUInt24()
        {
            unchecked
            {
                return bigEndian ? ReverseInt(System.BitConverter.ToUInt32(ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 0)) :
                    System.BitConverter.ToUInt32(ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 0);
            }
        }
        public int ReadInt32()
        {
            unchecked
            {
                int value = impl.ReadInt32(position);
                position += 4;
                return bigEndian ? (int)ReverseInt((uint)value) : value;
            }
        }
        public uint ReadUInt32()
        {
            unchecked
            {
                uint value = impl.ReadUInt32(position);
                position += 4;
                return bigEndian ? ReverseInt(value) : value;
            }
        }
        public long ReadInt64()
        {
            unchecked
            {
                long value = impl.ReadInt64(position);
                position += 8;
                return bigEndian ? (long)ReverseLong((ulong)value) : value;
            }
        }
        public ulong ReadUInt64()
        {
            unchecked
            {
                ulong value = impl.ReadUInt64(position);
                position += 8;
                return bigEndian ? ReverseLong(value) : value;
            }
        }
        public float ReadSingle()
        {
            float value = impl.ReadSingle(position);
            position += 4;
            return value;
        }
        public double ReadDouble()
        {
            double value = impl.ReadDouble(position);
            position += 8;
            return value;
        }
        public ushort ReverseShort(ushort value)
        {
            return (ushort)(((value & 0xFF00) >> 8) | (value & 0x00FF) << 8);
        }
        public uint ReverseInt(uint value)
        {
            value = (value >> 16) | (value << 16);
            return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
        }
        public ulong ReverseLong(ulong value)
        {
            value = (value >> 32) | (value << 32);
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
        }
        public void Align()
        {
            long pad = 4 - (position % 4);
            if (pad != 4) position += pad;
            streamImpl?.UpdatePosition(position);
        }
        public void Align8()
        {
            long pad = 8 - (position % 8);
            if (pad != 8) position += pad;
            streamImpl?.UpdatePosition(position);
        }
        public void Align16()
        {
            long pad = 16 - (position % 16);
            if (pad != 16) position += pad;
            streamImpl?.UpdatePosition(position);
        }
        public int Read(byte[] buffer, int offset, int count)
        {
            int readCount = impl.Read(position + offset, buffer, count);
            position = position + offset + readCount;
            return readCount;
        }
        public byte[] ReadBytes(int count)
        {
            byte[] bytes = impl.ReadBytes(position, count);
            position += count;
            return bytes;
        }
        public string ReadStringLength(int len)
        {
            string value = impl.ReadStringLength(position, len);
            position += len;
            return value;
        }
        public string ReadNullTerminated()
        {
            string value = impl.ReadNullTerminated(position, out int rlen);
            position += rlen;
            return value;
        }
        public static string ReadNullTerminatedArray(byte[] bytes, uint pos)
        {
            return AssetsFileReaderHelper.ReadNullTerminatedArray(bytes, pos);
        }
        public string ReadCountString()
        {
            byte length = ReadByte();
            return ReadStringLength(length);
        }
        public string ReadCountStringInt16()
        {
            ushort length = ReadUInt16();
            return ReadStringLength(length);
        }
        public string ReadCountStringInt32()
        {
            int length = ReadInt32();
            return ReadStringLength(length);
        }

        public long InitialSize
        {
            get { return impl.InitialSize; }
        }
        public string BackingFile
        {
            get { return impl.BackingFile; }
        }
        public long Position
        {
            get { return position; }
            set {
                position = value;
                streamImpl?.UpdatePosition(value);
            }
        }

        public AssetsFileStatefulReader clone()
        {
            var readerClone = new AssetsFileStatefulReader(this);
            readerClone.position = position;
            return readerClone;
        }

        public AssetsFileStatefulReader derive(long newPosition = -1)
        {
            var readerClone = new AssetsFileStatefulReader(this);
            readerClone.position = newPosition;
            return readerClone;
        }
    }

    public static partial class AssetsFileReaderHelper
    {
        public static AssetsFileStatefulReader createReader(string filePath, bool shared = true)
        {
            return new AssetsFileStatefulReader(createReaderImpl(filePath), shared);
        }
        public static AssetsFileStatefulReader createStreamReader(string filePath, bool shared = true)
        {
            return new AssetsFileStatefulReader(createStreamReaderImpl(filePath), shared);
        }
        public static AssetsFileStatefulReader createReader(Stream stream, bool useThreadSafeReader = true, bool shared = true)
        {
            return new AssetsFileStatefulReader(createReaderImpl(stream, useThreadSafeReader), shared);
        }
    }
}
