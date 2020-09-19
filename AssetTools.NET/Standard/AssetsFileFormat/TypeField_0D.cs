using System.Text;

namespace AssetsTools.NET
{
    public class TypeField_0D
    {
        public ushort version;
        public byte depth;
        public byte isArray;
        public uint typeStringOffset;
        public uint nameStringOffset;
        public int size;
        public uint index;
        public uint flags;
        public byte[] unknown;
        public void Read(AssetsFileReader reader, uint format)
        {
            version = reader.ReadUInt16();
            depth = reader.ReadByte();
            isArray = reader.ReadByte();
            typeStringOffset = reader.ReadUInt32();
            nameStringOffset = reader.ReadUInt32();
            size = reader.ReadInt32();
            index = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            if (format >= 0x12)
                unknown = reader.ReadBytes(8);
            else
                unknown = new byte[0];
        }
        public void Write(AssetsFileWriter writer, uint format)
        {
            writer.Write(version);
            writer.Write(depth);
            writer.Write(isArray);
            writer.Write(typeStringOffset);
            writer.Write(nameStringOffset);
            writer.Write(size);
            writer.Write(index);
            writer.Write(flags);
            if (format >= 0x12)
            {
                if (unknown == null)
                {
                    writer.Write(new byte[8]);
                }
                else
                {
                    writer.Write(unknown);
                }
            }
        }
        public enum TypeFieldArrayType
        {
            Array       = 0b0001,
            Ref         = 0b0010,
            Registry    = 0b0100,
            ArrayOfRefs = 0b1000
        }
        public string GetTypeString(string stringTable)
        {
            StringBuilder str = new StringBuilder();
            uint newTypeStringOffset = typeStringOffset;
            if (newTypeStringOffset >= 0x80000000)
            {
                newTypeStringOffset -= 0x80000000;
                stringTable = Type_0D.strTable;
            }
            int pos = (int)newTypeStringOffset;
            char c;
            while ((c = stringTable[pos]) != 0x00)
            {
                str.Append(c);
                pos++;
            }
            return str.ToString();
        }
        public string GetNameString(string stringTable)
        {
            StringBuilder str = new StringBuilder();
            uint newNameStringOffset = nameStringOffset;
            if (newNameStringOffset >= 0x80000000)
            {
                newNameStringOffset -= 0x80000000;
                stringTable = Type_0D.strTable;
            }
            int pos = (int)newNameStringOffset;
            char c;
            while ((c = stringTable[pos]) != 0x00)
            {
                str.Append(c);
                pos++;
            }
            return str.ToString();
        }
    }
}
