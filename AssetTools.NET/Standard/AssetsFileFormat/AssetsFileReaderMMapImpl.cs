#if !NET35
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileReaderMMapImpl : IAssetsFileReaderImpl
    {
        public MemoryMappedViewAccessor mmapView;
        internal IntPtr ptr;
        public readonly long initialSize;
        public readonly string filePath;

        public AssetsFileReaderMMapImpl(MemoryMappedViewAccessor mmapView, long size, string filePath = null)
        {
            this.filePath = filePath;
            initialSize = (size < 0) ? mmapView.Capacity : size;
            Init(mmapView);
        }

        public AssetsFileReaderMMapImpl(FileStream fileStream)
        {
            filePath = fileStream.Name;
            initialSize = fileStream.Length;
            // MemoryMappedFile.CreateFromFile(FileStream, ...) requires .NET 4.6+.
            var mmapFile = MemoryMappedFile.CreateFromFile(filePath);
            Init(mmapFile.CreateViewAccessor());
        }

        internal void Init(MemoryMappedViewAccessor mmapView)
        {
            this.mmapView = mmapView;
            unsafe {
                byte* bytes = null;
                mmapView.SafeMemoryMappedViewHandle.AcquirePointer(ref bytes);
                ptr = (IntPtr)bytes;
            }
        }
        public void Dispose()
        {
            mmapView.SafeMemoryMappedViewHandle.ReleasePointer();
            mmapView.Dispose();
        }

        public long InitialSize
        {
            get { return initialSize; }
        }
        public string BackingFile
        {
            get { return filePath; }
        }
        public byte ReadByte(long position)
        {
            return mmapView.ReadByte(position);
        }
        public sbyte ReadSByte(long position)
        {
            return mmapView.ReadSByte(position);
        }
        public short ReadInt16(long position)
        {
            return mmapView.ReadInt16(position);
        }
        public ushort ReadUInt16(long position)
        {
            return mmapView.ReadUInt16(position);
        }
        public int ReadInt32(long position)
        {
            return mmapView.ReadInt32(position);
        }
        public uint ReadUInt32(long position)
        {
            return mmapView.ReadUInt32(position);
        }
        public long ReadInt64(long position)
        {
            return mmapView.ReadInt64(position);
        }
        public ulong ReadUInt64(long position)
        {
            return mmapView.ReadUInt64(position);
        }
        public float ReadSingle(long position)
        {
            return mmapView.ReadSingle(position);
        }
        public double ReadDouble(long position)
        {
            return mmapView.ReadDouble(position);
        }
        public int Read(long position, byte[] buffer, int count)
        {
            int readCount = (int)Math.Max(0, Math.Min(initialSize - position, count));
            unsafe {
                Marshal.Copy((IntPtr)((byte*)ptr + position), buffer, 0, readCount);
            }
            return readCount;
        }
        public byte[] ReadBytes(long position, int count)
        {
            var bytes = new byte[count];
            unsafe {
                Marshal.Copy((IntPtr)((byte*)ptr + position), bytes, 0, count);
            }
            return bytes;
        }
        public string ReadStringLength(long position, int len)
        {
            unsafe {
                return ReadUTF8String(position, len);
            }
        }
        public string ReadNullTerminated(long position, out int rlen)
        {
            unsafe {
                byte* bytes = (byte*)ptr;
                rlen = AssetsFileReaderHelper.strlen(bytes + position) + 1;
                return ReadUTF8String(position, rlen - 1);
            }
        }
        // For .NET 4.0 compatibility.
        public unsafe string ReadUTF8String(long position, int len)
        {
#if NET46_OR_GREATER
            byte* bytes = (byte*)ptr;
            return Encoding.UTF8.GetString(bytes + position, len);
#else
            return Encoding.UTF8.GetString(ReadBytes(position, len), 0, len);
#endif
        }
    }

    public static partial class AssetsFileReaderHelper
    {
        public static IAssetsFileReaderImpl createMMapReaderImpl(string filePath)
        {
            var mmapFile = MemoryMappedFile.CreateFromFile(filePath);
            long size = (new FileInfo(filePath)).Length;
            return new AssetsFileReaderMMapImpl(mmapFile.CreateViewAccessor(), size, filePath);
        }
    }
}
#endif
