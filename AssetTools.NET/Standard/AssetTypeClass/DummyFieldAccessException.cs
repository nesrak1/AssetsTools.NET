using System;

namespace AssetsTools.NET
{
    public class DummyFieldAccessException : Exception
    {
        public DummyFieldAccessException(string message) : base(message)
        {
        }
    }
}
