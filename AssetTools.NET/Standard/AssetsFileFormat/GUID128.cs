using System;
using System.Text;

namespace AssetsTools.NET
{
    // todo: very similar to hash128?
    public struct GUID128
    {
        private const string HexToLiteral = "0123456789abcdef";

        public uint data0;
        public uint data1;
        public uint data2;
        public uint data3;

        public bool IsEmpty => data0 == 0 && data1 == 0 && data2 == 0 && data3 == 0;

        public GUID128(AssetsFileReader reader) : this()
        {
            Read(reader);
        }

        public void Read(AssetsFileReader reader)
        {
            data0 = reader.ReadUInt32();
            data1 = reader.ReadUInt32();
            data2 = reader.ReadUInt32();
            data3 = reader.ReadUInt32();
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(data0);
            writer.Write(data1);
            writer.Write(data2);
            writer.Write(data3);
        }

        public uint this[int i]
        {
            get
            {
                return i switch
                {
                    0 => data0,
                    1 => data1,
                    2 => data2,
                    3 => data3,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
            set
            {
                switch (i)
                {
                    case 0:
                        data0 = value;
                        break;
                    case 1:
                        data1 = value;
                        break;
                    case 2:
                        data2 = value;
                        break;
                    case 3:
                        data3 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(32);
            for (int i = 3; i >= 0; i--)
            {
                for (int j = 7; j >= 0; j--)
                {
                    var cur = this[i];
                    cur >>= (j * 4);
                    cur &= 0xF;
                    stringBuilder.Insert(0, HexToLiteral[(int)cur]);
                }
            }
            return stringBuilder.ToString();
        }

        public static bool TryParse(string str, out GUID128 guid)
        {
            guid = new GUID128();
            if (str.Length != 32)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                uint cur = 0;
                for (int j = 7; j >= 0; j--)
                {
                    uint curHex = LiteralToHex(str[i * 8 + j]);
                    if (curHex == uint.MaxValue)
                    {
                        return false;
                    }
                    cur |= (curHex << (j * 4));
                }
                guid[i] = cur;
            }

            return true;
        }

        private static uint LiteralToHex(char c)
        {
            return c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'a' => 10,
                'b' => 11,
                'c' => 12,
                'd' => 13,
                'e' => 14,
                'f' => 15,
                _ => uint.MaxValue
            };
        }
    }
}
