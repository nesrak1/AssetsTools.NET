using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public class UnityVersion
    {
        public int major;
        public int minor;
        public int patch;
        public string type;
        public int typeNum;

        public UnityVersion() { }

        public UnityVersion(string version)
        {
            string[] versionSplit = version.Split('.');
            major = int.Parse(versionSplit[0]);
            minor = int.Parse(versionSplit[1]);

            int verTypeIndex = versionSplit[2].IndexOfAny(new[] { 'f', 'p', 'a', 'b', 'c', 'x' });
            if (verTypeIndex != -1)
            {
                type = versionSplit[2][verTypeIndex].ToString();
                patch = int.Parse(versionSplit[2].Substring(0, verTypeIndex));

                string patchNumString = versionSplit[2].Substring(verTypeIndex + 1);
                if (!int.TryParse(patchNumString, out typeNum))
                {
                    // sometimes pesky custom engine versions add text to the end
                    string newPatchNumString = "";
                    for (int i = 0; i < patchNumString.Length; i++)
                    {
                        if (patchNumString[i] >= '0' && patchNumString[i] <= '9')
                        {
                            newPatchNumString += patchNumString[i];
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (newPatchNumString.Length > 0)
                    {
                        typeNum = int.Parse(newPatchNumString);
                    }
                }
            }
            else
            {
                patch = int.Parse(versionSplit[2]);
                type = "";
                typeNum = 0;
            }
        }

        public override string ToString()
        {
            if (type == string.Empty)
                return $"{major}.{minor}.{patch}";
            else
                return $"{major}.{minor}.{patch}{type}{typeNum}";
        }

        public ulong ToUInt64()
        {
            byte typeByte = type switch
            {
                "a" => 0,
                "b" => 1,
                "c" => 2,
                "f" => 3,
                "p" => 4,
                "x" => 5,
                _ => 0xff
            };

            return ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)patch << 16) | ((ulong)typeByte << 8) | (uint)typeNum;
        }

        public static UnityVersion FromUInt64(ulong data)
        {
            UnityVersion version = new UnityVersion()
            {
                major = (int)((data >> 48) & 0xffff),
                minor = (int)((data >> 32) & 0xffff),
                patch = (int)((data >> 16) & 0xffff),
                type = ((data >> 8) & 0xff) switch
                {
                    0 => "a",
                    1 => "b",
                    2 => "c",
                    3 => "f",
                    4 => "p",
                    5 => "x",
                    _ => "?"
                },
                typeNum = (int)(data & 0xff)
            };

            return version;
        }
    }
}
