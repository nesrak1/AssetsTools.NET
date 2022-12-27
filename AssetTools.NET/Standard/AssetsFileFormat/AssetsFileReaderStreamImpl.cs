using System;
using System.IO;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileReaderStreamImpl : BinaryReader, IAssetsFileReaderImpl, IAssetsFileReaderStreamExtra
    {
        public AssetsFileReaderStreamImpl(Stream stream)
            : base(stream)
        {
        }

        public long InitialSize
        {
            get { return BaseStream.Length; }
        }
        public string BackingFile
        {
            get { return (BaseStream as FileStream)?.Name; }
        }
        public byte ReadByte(long position)
        {
            Check(position);
            return base.ReadByte();
        }
        public sbyte ReadSByte(long position)
        {
            Check(position);
            return base.ReadSByte();
        }
        public short ReadInt16(long position)
        {
            Check(position);
            return base.ReadInt16();
        }
        public ushort ReadUInt16(long position)
        {
            Check(position);
            return base.ReadUInt16();
        }
        public int ReadInt32(long position)
        {
            Check(position);
            return base.ReadInt32();
        }
        public uint ReadUInt32(long position)
        {
            Check(position);
            return base.ReadUInt32();
        }
        public long ReadInt64(long position)
        {
            Check(position);
            return base.ReadInt64();
        }
        public ulong ReadUInt64(long position)
        {
            Check(position);
            return base.ReadUInt64();
        }
        public float ReadSingle(long position)
        {
            Check(position);
            return base.ReadSingle();
        }
        public double ReadDouble(long position)
        {
            Check(position);
            return base.ReadDouble();
        }
        public int Read(long position, byte[] buffer, int count)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
            return base.Read(buffer, 0, count);
        }
        public byte[] ReadBytes(long position, int count)
        {
            Check(position);
            return base.ReadBytes(count);
        }
        public string ReadStringLength(long position, int len)
        {
            Check(position);
            return Encoding.UTF8.GetString(ReadBytes(len));
        }
        public string ReadNullTerminated(long position, out int rlen)
        {
            Check(position);
            long savedPosition = BaseStream.Position;
            string output = "";
            char curChar;
            while ((curChar = ReadChar()) != 0x00)
            {
                output += curChar;
            }
            rlen = (int)(BaseStream.Position - savedPosition);
            return output;
        }

        public long Length
        {
            get { return BaseStream.Length; }
        }
        public void UpdatePosition(long position)
        {
            BaseStream.Position = position;
        }
        public void Check(long position)
        {
            if (position != BaseStream.Position)
                throw new InvalidOperationException();
        }
    }
}
