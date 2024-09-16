namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
public class Palette
{
    public uint Ofs;
    public uint Size;
    public ushort Num;

    public Palette(AssetsFileReader reader)
    {
        Ofs = reader.ReadUInt24();
        Size = reader.ReadUInt24();
        Num = reader.ReadUInt16();
    }
}
