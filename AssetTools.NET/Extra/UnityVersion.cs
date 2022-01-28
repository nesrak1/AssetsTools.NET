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

        public UnityVersion(string version)
        {
            string[] versionSplit = version.Split('.');
            major = int.Parse(versionSplit[0]);
            minor = int.Parse(versionSplit[1]);

            int verTypeIndex = versionSplit[2].IndexOfAny(new[] { 'f', 'p', 'a', 'b' });
            if (verTypeIndex != -1)
            {
                type = versionSplit[2][verTypeIndex].ToString();
                patch = int.Parse(versionSplit[2].Substring(0, verTypeIndex));

                string patchNumString = versionSplit[2].Substring(verTypeIndex + 1);
                if (!int.TryParse(patchNumString, out typeNum))
                {
                    //sometimes pesky custom engine versions add text to the end
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
    }
}
