namespace AssetsTools.NET
{
    public enum TypeTreeTypeFlags
    {
        None        = 0b0000,
        Array       = 0b0001,
        Ref         = 0b0010,
        Registry    = 0b0100,
        ArrayOfRefs = 0b1000
    }
}
