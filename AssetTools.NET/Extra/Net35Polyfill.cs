using System;
using System.IO;

#if NETSTANDARD2_1_OR_GREATER
using System.Buffers;
#endif

namespace AssetsTools.NET.Extra
{
    public static class Net35Polyfill
    {
        //https://stackoverflow.com/a/13022108
        public static void CopyToCompat(this Stream input, Stream output, long bytes = -1, int bufferSize = 80 * 1024)
        {
#if NETSTANDARD2_1_OR_GREATER
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                CopyToCompat(input, output, bytes, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
#else
            byte[] buffer = new byte[bufferSize];
            CopyToCompat(input, output, bytes, buffer);
#endif
        }

        private static void CopyToCompat(this Stream input, Stream output, long bytes, byte[] buffer)
        {
            int read;

            // set to largest value so we always go over buffer (hopefully)
            if (bytes == -1)
                bytes = long.MaxValue;

            // bufferSize will always be an int so if bytes is larger, it's also under the size of an int
            while (bytes > 0 && (read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        //https://stackoverflow.com/a/4108907
        public static bool HasFlag(Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
}
