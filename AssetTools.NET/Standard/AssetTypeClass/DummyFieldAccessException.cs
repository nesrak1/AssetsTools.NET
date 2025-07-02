using System;

namespace AssetsTools.NET
{
    public class DummyFieldAccessException : Exception
    {
        public DummyFieldAccessException(string message) : base(message)
        {
        }
    }

    public class NonexistentTypeException : Exception
    {
        public NonexistentTypeException(string assembly, string nameSpace, string typeName) : base(
            $"Type `{(nameSpace != "" ? $"{nameSpace}." : "")}{typeName}` does not exist in assembly `{assembly}`"
        ) {}
    }
}
