using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public static class Net35Polyfill
    {
        //https://stackoverflow.com/a/13022108
        public static void CopyToCompat(this Stream input, Stream output, long bytes = -1, int bufferSize = 80 * 1024)
        {
            byte[] buffer = new byte[bufferSize];
            int read;

            //set to largest value so we always go over buffer (hopefully)
            if (bytes == -1)
                bytes = long.MaxValue;

            //bufferSize will always be an int so if bytes is larger, it's also under the size of an int
            while (bytes > 0 && (read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
