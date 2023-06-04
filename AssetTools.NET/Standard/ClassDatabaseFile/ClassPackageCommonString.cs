using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageCommonString
    {
        public List<KeyValuePair<UnityVersion, byte>> VersionInformation { get; set; }
        public List<ushort> StringBufferIndices { get; set; }

        /// <summary>
        /// Read the <see cref="ClassPackageCommonString"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            int versionCount = reader.ReadInt32();
            VersionInformation = new List<KeyValuePair<UnityVersion, byte>>(versionCount);
            for (int i = 0; i < versionCount; i++)
            {
                UnityVersion version = UnityVersion.FromUInt64(reader.ReadUInt64());
                byte stringCount = reader.ReadByte();
                VersionInformation.Add(new KeyValuePair<UnityVersion, byte>(version, stringCount));
            }

            int indicesCount = reader.ReadInt32();
            StringBufferIndices = new List<ushort>(indicesCount);
            for (int i = 0; i < indicesCount; i++)
            {
                StringBufferIndices.Add(reader.ReadUInt16());
            }
        }

        /// <summary>
        /// Write the <see cref="ClassPackageCommonString"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(VersionInformation.Count);
            foreach (KeyValuePair<UnityVersion, byte> versionCountPair in VersionInformation)
            {
                writer.Write(versionCountPair.Key.ToUInt64());
                writer.Write(versionCountPair.Value);
            }

            writer.Write(StringBufferIndices.Count);
            for (int i = 0; i < StringBufferIndices.Count; i++)
            {
                writer.Write(StringBufferIndices[i]);
            }
        }

        /// <summary>
        /// Get the length of the common string for a version. <br/>
        /// Since the common string is only appended in new versions, never edited, only the
        /// length of the string for each version needs to be stored rather than the string
        /// in its entirety.
        /// </summary>
        /// <param name="version"></param>
        /// <returns>The length of the common string.</returns>
        public byte GetCommonStringLengthForVersion(UnityVersion version)
        {
            if (VersionInformation.Count == 0)
            {
                return 0;
            }

            byte lastLength = VersionInformation[0].Value;
            for (int i = 0; i < VersionInformation.Count; i++)
            {
                if (VersionInformation[i].Key.ToUInt64() >= version.ToUInt64())
                {
                    return lastLength;
                }
                lastLength = VersionInformation[i].Value;
            }

            return lastLength;
        }
    }
}
