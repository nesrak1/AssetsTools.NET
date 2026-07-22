using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassPackageCommonString
    {
        public List<KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]>> VersionInformation { get; set; }

        /// <summary>
        /// Read the <see cref="ClassPackageCommonString"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="fileVersion">The version of the file.</param>
        /// <param name="stringTable">The tpk string table. Unused in version > 2.</param>
        public void Read(AssetsFileReader reader, byte fileVersion, ClassDatabaseStringTable stringTable)
        {
            if (fileVersion >= 2)
            {
                int versionCount = reader.ReadInt32();
                VersionInformation = new List<KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]>>(versionCount);
                for (int i = 0; i < versionCount; i++)
                {
                    UnityVersion version = UnityVersion.FromUInt64(reader.ReadUInt64());
                    int entryCount = reader.ReadInt32();
                    var entries = new ClassPackageCommonStringEntry[entryCount];
                    for (int j = 0; j < entryCount; j++)
                    {
                        entries[j] = new ClassPackageCommonStringEntry();
                        entries[j].Read(reader);
                    }
                    var verInfo = new KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]>(version, entries);
                    VersionInformation.Add(verInfo);
                }
            }
            else if (fileVersion == 1)
            {
                int versionCount = reader.ReadInt32();
                VersionInformation = new List<KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]>>(versionCount);
                // skip version info for now. to be consistent with v2 format,
                // we need the string buffer indices to be read first.
                long versionAddr = reader.Position;
                reader.Position += (sizeof(ulong) + sizeof(byte)) * versionCount;

                int indicesCount = reader.ReadInt32();
                List<ushort> stringBufferIndices = new List<ushort>(indicesCount);
                for (int i = 0; i < indicesCount; i++)
                {
                    stringBufferIndices.Add(reader.ReadUInt16());
                }

                // return and read the version info
                long endAddr = reader.Position;
                reader.Position = versionAddr;
                for (int i = 0; i < versionCount; i++)
                {
                    UnityVersion version = UnityVersion.FromUInt64(reader.ReadUInt64());
                    byte stringCount = reader.ReadByte();
                    var entries = new ClassPackageCommonStringEntry[stringCount];
                    int stringOffset = 0;
                    for (int j = 0; j < stringCount; j++)
                    {
                        int stringIndex = stringBufferIndices[j];
                        int stringLength = stringTable.Strings[stringIndex].Length;
                        entries[j] = new ClassPackageCommonStringEntry()
                        {
                            Offset = (ushort)stringOffset,
                            StringIndex = (ushort)stringIndex,
                        };

                        // + 1 for null byte
                        stringOffset += stringLength + 1;
                    }
                    var verInfo = new KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]>(version, entries);
                    VersionInformation.Add(verInfo);
                }

                reader.Position = endAddr;
            }
        }

        /// <summary>
        /// Write the <see cref="ClassPackageCommonString"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="fileVersion">The version of the file.</param>
        /// <param name="stringTable">The tpk string table. Unused in version > 2.</param>
        public void Write(AssetsFileWriter writer, byte fileVersion, ClassDatabaseStringTable stringTable)
        {
            if (fileVersion >= 2)
            {
                writer.Write(VersionInformation.Count);
                foreach (KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]> versionCountPair in VersionInformation)
                {
                    writer.Write(versionCountPair.Key.ToUInt64());
                    var entries = versionCountPair.Value;
                    writer.Write(entries.Length);
                    for (int i = 0; i < entries.Length; i++)
                    {
                        entries[i].Write(writer);
                    }
                }
            }
            else if (fileVersion == 1)
            {
                // first, build/verify the string buffer indices list
                var stringBufferIndices = new List<ushort>();
                var stringBufferOffsets = new List<int>();
                int stringBufferCurOffset = 0;
                foreach (KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]> versionCountPair in VersionInformation)
                {
                    var entries = versionCountPair.Value;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        ClassPackageCommonStringEntry entry = entries[i];
                        if (i >= stringBufferIndices.Count)
                        {
                            // this is our first time, only check offset
                            if (entry.Offset != stringBufferCurOffset)
                            {
                                throw new ArgumentException(
                                    $"Excepted string offset {stringBufferCurOffset} " +
                                    $"but found conflicting offset {entry.Offset}.");
                            }

                            stringBufferIndices.Add(entry.StringIndex);
                            stringBufferOffsets.Add(entry.Offset);

                            // + 1 for null byte
                            int stringLength = stringTable.Strings[entry.StringIndex].Length;
                            stringBufferCurOffset += stringLength + 1;
                        }
                        else
                        {
                            // this is not our first time, verify offset and string index
                            if (entry.Offset != stringBufferOffsets[i])
                            {
                                throw new ArgumentException(
                                    $"Excepted string offset {stringBufferCurOffset} " +
                                    $"but found conflicting offset {entry.Offset}.");
                            }

                            if (entry.StringIndex != stringBufferIndices[i])
                            {
                                throw new ArgumentException(
                                    $"Excepted string index {stringBufferIndices[i]} " +
                                    $"but found conflicting index {entry.StringIndex}.");
                            }
                        }
                    }
                }

                // write the version info first...
                writer.Write(VersionInformation.Count);
                foreach (KeyValuePair<UnityVersion, ClassPackageCommonStringEntry[]> versionCountPair in VersionInformation)
                {
                    writer.Write(versionCountPair.Key.ToUInt64());

                    // since the arrays will all be the same, just differing in length,
                    // the length is the same as the old stringCount byte field.
                    writer.Write((byte)versionCountPair.Value.Length);
                }

                // ...then the string buffer indices list last
                writer.Write(stringBufferIndices.Count);
                for (int i = 0; i < stringBufferIndices.Count; i++)
                {
                    writer.Write(stringBufferIndices[i]);
                }
            }
        }

        /// <summary>
        /// Skip past this class without fully parsing it. This is needed because
        /// <see cref="ClassDatabaseStringTable"/> is needed to convert the v1 format
        /// to the v2 format, but the string table only exists at the end of the file.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="fileVersion">The version of the file.</param>
        public static void Skip(AssetsFileReader reader, byte fileVersion)
        {
            if (fileVersion >= 2)
            {
                int versionCount = reader.ReadInt32();
                for (int i = 0; i < versionCount; i++)
                {
                    reader.Position += sizeof(ulong);
                    int entryCount = reader.ReadInt32();
                    reader.Position += ClassPackageCommonStringEntry.GetSize() * entryCount;
                }
            }
            else if (fileVersion == 1)
            {
                int versionCount = reader.ReadInt32();
                reader.Position += (sizeof(ulong) + sizeof(byte)) * versionCount;

                int indicesCount = reader.ReadInt32();
                reader.Position += sizeof(ushort) * indicesCount;
            }
        }

        /// <summary>
        /// Get the length of the common string for a Unity version. <br/>
        /// Since the common string is only appended in new versions, never edited, only the
        /// length of the string for each version needs to be stored rather than the string
        /// in its entirety.
        /// </summary>
        /// <param name="version">The Unity version to make a string for.</param>
        /// <returns>The common string entries for the version.</returns>
        public ClassPackageCommonStringEntry[] GetStringEntriesForVersion(UnityVersion version)
        {
            if (VersionInformation.Count == 0)
            {
                return new ClassPackageCommonStringEntry[0];
            }

            ClassPackageCommonStringEntry[] lastEntry = VersionInformation[0].Value;
            for (int i = 0; i < VersionInformation.Count; i++)
            {
                if (VersionInformation[i].Key.ToUInt64() >= version.ToUInt64())
                {
                    return lastEntry;
                }
                lastEntry = VersionInformation[i].Value;
            }

            return lastEntry;
        }
    }
}
