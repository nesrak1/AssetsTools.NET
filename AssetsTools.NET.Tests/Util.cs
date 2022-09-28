using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools.NET.Tests
{
    public static class Util
    {
        // https://stackoverflow.com/a/800469
        public static string GetHashSHA1(this byte[] data)
        {
            using var sha1 = SHA1.Create();
            return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
        }
    }
}
