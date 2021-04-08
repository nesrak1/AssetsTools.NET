using System;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileWriter : BinaryWriter
    {
        public bool bigEndian = true;
        public AssetsFileWriter(FileStream fileStream) : base(fileStream) { }
        public AssetsFileWriter(MemoryStream memoryStream) : base(memoryStream) { }
        public AssetsFileWriter(Stream stream) : base(stream) { }
        public override void Write(short val)
        {
            unchecked
            {
                if (bigEndian) base.Write((short)ReverseShort((ushort)val));
                else base.Write(val);
            }
        }
        public override void Write(ushort val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseShort(val));
                else base.Write(val);
            }
        }
        public override void Write(int val)
        {
            unchecked
            {
                if (bigEndian) base.Write((int)ReverseInt((uint)val));
                else base.Write(val);
            }
        }
        public override void Write(uint val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseInt(val));
                else base.Write(val);
            }
        }
        public override void Write(long val)
        {
            unchecked
            {
                if (bigEndian) base.Write((long)ReverseLong((ulong)val));
                else base.Write(val);
            }
        }
        public override void Write(ulong val)
        {
            unchecked
            {
                if (bigEndian) base.Write(ReverseLong(val));
                else base.Write(val);
            }
        }
        public override void Write(string val)
        {
            base.Write(Encoding.UTF8.GetBytes(val));
        }
        public void WriteUInt24(uint val)
        {
            unchecked
            {
                if (bigEndian) base.Write(BitConverter.GetBytes(ReverseInt(val)), 1, 3);
                else base.Write(BitConverter.GetBytes(val), 0, 3);
            }
        }
        public void WriteInt24(int val)
        {
            unchecked
            {
                if (bigEndian) base.Write(BitConverter.GetBytes((int)ReverseInt((uint)val)), 1, 3);
                else base.Write(BitConverter.GetBytes(val), 0, 3);
            }
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
            while (BaseStream.Position % 4 != 0) Write((byte)0x00);
        }
        public void Align8()
        {
            while (BaseStream.Position % 8 != 0) Write((byte)0x00);
        }
        public void Align16()
        {
            while (BaseStream.Position % 16 != 0) Write((byte)0x00);
        }
        public void WriteNullTerminated(string text)
        {
            Write(text);
            Write((byte)0x00);
        }
        public void WriteCountString(string text)
        {
            if (Encoding.UTF8.GetByteCount(text) > 0xFF)
                new Exception("String is longer than 255! Use the Int32 variant instead!");
            Write((byte)Encoding.UTF8.GetByteCount(text));
            Write(text);
        }
        public void WriteCountStringInt16(string text)
        {
            if (Encoding.UTF8.GetByteCount(text) > 0xFFFF)
                new Exception("String is longer than 65535! Use the Int32 variant instead!");
            Write((ushort)Encoding.UTF8.GetByteCount(text));
            Write(text);
        }
        public void WriteCountStringInt32(string text)
        {
            Write(Encoding.UTF8.GetByteCount(text));
            Write(text);
        }
        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

    }
}
