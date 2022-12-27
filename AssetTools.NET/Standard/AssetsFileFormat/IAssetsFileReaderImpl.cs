using System;
using System.IO;

namespace AssetsTools.NET
{
    // Native endian (=little-endian) stateless reader interface. Big endian is swapped in StatefulReader.
    public interface IAssetsFileReaderImpl : IDisposable
    {
        long InitialSize { get; }
        string BackingFile { get; }
        byte ReadByte(long position);
        sbyte ReadSByte(long position);
        short ReadInt16(long position);
        ushort ReadUInt16(long position);
        int ReadInt32(long position);
        uint ReadUInt32(long position);
        long ReadInt64(long position);
        ulong ReadUInt64(long position);
        float ReadSingle(long position);
        double ReadDouble(long position);
        int Read(long position, byte[] buffer, int count);
        byte[] ReadBytes(long position, int count);
        string ReadStringLength(long position, int len);
        string ReadNullTerminated(long position, out int len);  // len: including null-terminator.
    }

    public interface IAssetsFileReaderStreamExtra
    {
        long Length { get; }
        void UpdatePosition(long position);
    }

    public static partial class AssetsFileReaderHelper
    {
        public static IAssetsFileReaderImpl createReaderImpl(string filePath)
        {
#if NET35
            return createStreamReaderImpl(filePath);
#else
            return createMMapReaderImpl(filePath);
#endif
        }
        public static IAssetsFileReaderImpl createReaderImpl(Stream stream, bool useThreadSafeReader = true)
        {
            if (!useThreadSafeReader) {
                return createStreamReaderImpl(stream);
            } else {
                if (stream is FileStream fs) {
                    return createReaderImpl(fs.Name);
                } else {
                    throw new InvalidOperationException();
                }
            }
        }

        public static AssetsFileReaderStreamImpl createStreamReaderImpl(string filePath)
        {
            return new AssetsFileReaderStreamImpl(File.OpenRead(filePath));
        }
        public static AssetsFileReaderStreamImpl createStreamReaderImpl(Stream stream)
        {
            return new AssetsFileReaderStreamImpl(stream);
        }

        public static unsafe int strlen(byte[] bytes, long pos)
        {
            fixed (byte* s = bytes)
                return strlen(s + pos);
        }
        public static unsafe int strlen(byte* s)
        {
            byte* p = s;
            while(*p++ != '\0'); 
            return (int)(p - s - 1);
        }
        public static string ReadNullTerminatedArray(byte[] bytes, long pos)
        {
            unsafe {
                fixed (byte* pb = bytes)
                    return ReadNullTerminatedArray(pb, pos);
            }
        }
        public static unsafe string ReadNullTerminatedArray(byte* bytes, long pos)
        {
            // Unlike Encoding.ASCII.GetString:
            // String(sbyte*) (w/o Encoding) variants accept 8-bit value. (or Encoding=Latin1, N/A in .NET framework)
            // String(sbyte*) (w/o length) handles null-termination; String(sbyte*,int,int) does not.
            return new String((sbyte*)bytes + pos);
        }
    }
}
