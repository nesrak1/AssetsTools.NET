namespace AssetsTools.NET.Texture.TextureDecoders.CrnUnity;
public class Header
{
    public ushort Sig;
    public ushort HeaderSize;
    public ushort HeaderCrc16;

    public uint DataSize;
    public ushort DataCrc16;

    public ushort Width;
    public ushort Height;

    public byte Levels;
    public byte Faces;

    public byte Format;
    public ushort Flags;

    public uint Reserved;
    public uint Userdata0;
    public uint Userdata1;

    public Palette ColorEndpoints;
    public Palette ColorSelectors;

    public Palette AlphaEndpoints;
    public Palette AlphaSelectors;

    public ushort TablesSize;
    public uint TablesOfs;

    public uint[] LevelOfs;

    public Header(AssetsFileReader reader)
    {
        Sig = reader.ReadUInt16();
        HeaderSize = reader.ReadUInt16();
        HeaderCrc16 = reader.ReadUInt16();

        DataSize = reader.ReadUInt32();
        DataCrc16 = reader.ReadUInt16();

        Width = reader.ReadUInt16();
        Height = reader.ReadUInt16();

        Levels = reader.ReadByte();
        Faces = reader.ReadByte();

        Format = reader.ReadByte();
        Flags = reader.ReadUInt16();

        Reserved = reader.ReadUInt32();
        Userdata0 = reader.ReadUInt32();
        Userdata1 = reader.ReadUInt32();

        ColorEndpoints = new Palette(reader);
        ColorSelectors = new Palette(reader);

        AlphaEndpoints = new Palette(reader);
        AlphaSelectors = new Palette(reader);

        TablesSize = reader.ReadUInt16();
        TablesOfs = reader.ReadUInt24();

        LevelOfs = new uint[Levels];
        for (int i = 0; i < Levels; i++)
        {
            LevelOfs[i] = reader.ReadUInt32();
        }
    }
}
