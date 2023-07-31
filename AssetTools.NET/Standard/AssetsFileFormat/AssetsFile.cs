using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetsTools.NET
{
    public class AssetsFile
    {
        /// <summary>
        /// Assets file header.
        /// </summary>
        public AssetsFileHeader Header { get; set; }
        /// <summary>
        /// Contains metadata about the file (TypeTree, engine version, dependencies, etc.)
        /// </summary>
        public AssetsFileMetadata Metadata { get; set; }
        /// <summary>
        /// The <see cref="AssetsFileReader"/> that reads the file.
        /// </summary>
        public AssetsFileReader Reader { get; set; }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public void Close()
        {
            Reader.Close();
        }

        /// <summary>
        /// Read the <see cref="AssetsFile"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            Reader = reader;
            
            Header = new AssetsFileHeader();
            Header.Read(reader);

            Metadata = new AssetsFileMetadata();
            Metadata.Read(reader, Header);
        }

        /// <summary>
        /// Read the <see cref="AssetsFile"/> with the provided stream.
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        public void Read(Stream stream)
        {
            Read(new AssetsFileReader(stream));
        }

        /// <summary>
        /// Write the <see cref="AssetsFile"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="filePos">Where in the stream to start writing. Use -1 to start writing at the current stream position.</param>
        public void Write(AssetsFileWriter writer, long filePos = 0)
        {
            long writeStart = filePos;
            if (filePos == -1)
                writeStart = writer.Position;
            else
                writer.Position = filePos;

            // we'll write the header now even if we replace it
            // later so we start the metadata in the right spot
            Header.Write(writer);

            // the list we're building for our new file
            List<AssetFileInfo> newAssetInfos = new List<AssetFileInfo>();

            int infoCount = Metadata.AssetInfos.Count;
            for (int i = 0; i < infoCount; i++)
            {
                AssetFileInfo assetInfo = Metadata.AssetInfos[i];
                ContentReplacerType replacerType = assetInfo.ReplacerType;

                if (replacerType == ContentReplacerType.Remove)
                    continue;

                if (replacerType == ContentReplacerType.AddOrModify)
                {
                    if (assetInfo.Replacer == null)
                    {
                        throw new Exception($"{nameof(assetInfo.Replacer)} must be non-null when status is Modified!");
                    }
                }

                newAssetInfos.Add(new AssetFileInfo()
                {
                    PathId = assetInfo.PathId,
                    ByteOffset = assetInfo.ByteOffset,
                    ByteSize = assetInfo.ByteSize,
                    TypeIdOrIndex = assetInfo.TypeIdOrIndex,
                    OldTypeId = assetInfo.OldTypeId,
                    ScriptTypeIndex = assetInfo.ScriptTypeIndex,
                    Stripped = assetInfo.Stripped,
                    // skip TypeId, not read
                    Replacer = assetInfo.Replacer,
                });
            }

            // required by unity
            newAssetInfos.Sort((i1, i2) => i1.PathId.CompareTo(i2.PathId));

            AssetsFileMetadata newMetadata = new AssetsFileMetadata
            {
                UnityVersion = Metadata.UnityVersion,
                TargetPlatform = Metadata.TargetPlatform,
                TypeTreeEnabled = Metadata.TypeTreeEnabled,
                TypeTreeTypes = Metadata.TypeTreeTypes,
                AssetInfos = newAssetInfos,
                ScriptTypes = Metadata.ScriptTypes,
                Externals = Metadata.Externals,
                RefTypes = Metadata.RefTypes,
                UserInformation = Metadata.UserInformation
            };

            long newMetadataStart = writer.Position;
            newMetadata.Write(writer, Header.Version);
            int newMetadataSize = (int)(writer.Position - newMetadataStart);

            if (writer.Position < 0x1000)
            {
                // for padding only: if we're already past address 0x1000, this is skipped
                while (writer.Position < 0x1000)
                {
                    writer.Write((byte)0x00);
                }
            }
            else
            {
                // otherwise align to 16 bytes, even if already aligned
                if (writer.Position % 16 == 0)
                    writer.Position += 16;
                else
                    writer.Align16();
            }

            long newFirstFileOffset = writer.Position;

            // write all asset data
            for (int i = 0; i < newAssetInfos.Count; i++)
            {
                AssetFileInfo assetInfo = newAssetInfos[i];
                long startPosition = writer.Position;
                long newByteStart = startPosition - newFirstFileOffset;

                ContentReplacerType replacerType = assetInfo.ReplacerType;
                if (replacerType == ContentReplacerType.AddOrModify)
                {
                    assetInfo.Replacer.Write(writer);
                }
                else
                {
                    Reader.Position = assetInfo.GetAbsoluteByteOffset(this);
                    Reader.BaseStream.CopyToCompat(writer.BaseStream, assetInfo.ByteSize);
                }

                assetInfo.ByteOffset = newByteStart;
                assetInfo.ByteSize = (uint)(writer.Position - startPosition);

                if (i != newAssetInfos.Count - 1)
                    writer.Align8();
            }

            long newFileSize = writer.Position - writeStart;

            // write new header
            AssetsFileHeader newHeader = new AssetsFileHeader
            {
                MetadataSize = newMetadataSize,
                FileSize = newFileSize,
                Version = Header.Version,
                DataOffset = newFirstFileOffset,
                Endianness = Header.Endianness
            };

            writer.Position = writeStart;
            newHeader.Write(writer);

            // write new asset infos again (this time with offsets and sizes filled in)
            writer.Position = newMetadataStart;
            newMetadata.Write(writer, Header.Version);

            // Set writer position back to end of file
            writer.Position = writeStart + newFileSize;
        }

        /// <summary>
        /// Get the script index for an <see cref="AssetFileInfo"/>.
        /// Always use this method instead of ScriptTypeIndex, as it handles all versions.
        /// </summary>
        /// <param name="info">The file info to check.</param>
        /// <returns>The script index of the asset.</returns>
        public ushort GetScriptIndex(AssetFileInfo info)
        {
            if (Header.Version < 0x10)
                return info.ScriptTypeIndex;
            else
                return Metadata.TypeTreeTypes[info.TypeIdOrIndex].ScriptTypeIndex;
        }

        /// <summary>
        /// Check if a file at a path is an <see cref="AssetsFile"/> or not.
        /// </summary>
        /// <param name="filePath">The file path to read from and check.</param>
        /// <returns>True if the file is an assets file, otherwise false.</returns>
        public static bool IsAssetsFile(string filePath)
        {
            using AssetsFileReader reader = new AssetsFileReader(filePath);
            return IsAssetsFile(reader, 0, reader.BaseStream.Length);
        }

        /// <summary>
        /// Check if a file at a position in a stream is an <see cref="AssetsFile"/> or not.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="offset">The offset to start at (this value cannot be -1).</param>
        /// <param name="length">The length of the file. You can use <c>reader.BaseStream.Length</c> for this.</param>
        /// <returns></returns>
        public static bool IsAssetsFile(AssetsFileReader reader, long offset, long length)
        {
            reader.BigEndian = true;

            if (length < 0x30)
                return false;

            reader.Position = offset;
            string possibleBundleHeader = reader.ReadStringLength(5);
            if (possibleBundleHeader == "Unity")
                return false;

            reader.Position = offset + 0x08;
            int possibleFormat = reader.ReadInt32();
            if (possibleFormat > 99)
                return false;

            reader.Position = offset + 0x14;

            if (possibleFormat >= 0x16)
            {
                reader.Position += 0x1c;
            }

            string possibleVersion = "";
            char curChar;
            while (reader.Position < reader.BaseStream.Length && (curChar = (char)reader.ReadByte()) != 0x00)
            {
                possibleVersion += curChar;
                if (possibleVersion.Length > 0xFF)
                {
                    return false;
                }
            }

            string emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.\\n]", "");
            string fullVersion = Regex.Replace(possibleVersion, "[^a-zA-Z0-9\\.\\n]", "");
            return emptyVersion == "" && fullVersion.Length > 0;
        }

        // for convenience

        /// <summary>
        /// Get an <see cref="AssetFileInfo"/> from a path ID.
        /// </summary>
        /// <param name="pathId">The path ID to search for.</param>
        /// <returns>An info for that path ID.</returns>
        public AssetFileInfo GetAssetInfo(long pathId) => Metadata.GetAssetInfo(pathId);
        /// <summary>
        /// Generate a dictionary lookup for assets instead of a brute force search.
        /// Takes a little bit more memory but results in quicker lookups.
        /// </summary>
        public void GenerateQuickLookup() => Metadata.GenerateQuickLookup();
        /// <summary>
        /// Get all assets of a specific type ID.
        /// </summary>
        /// <param name="typeId">The type ID to search for.</param>
        /// <returns>A list of infos for that type ID.</returns>
        public List<AssetFileInfo> GetAssetsOfType(int typeId) => Metadata.GetAssetsOfType(typeId);
        /// <summary>
        /// Get all assets of a specific type ID.
        /// </summary>
        /// <param name="typeId">The type ID to search for.</param>
        /// <returns>A list of infos for that type ID.</returns>
        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId) => Metadata.GetAssetsOfType(typeId);
        /// <summary>
        /// Get all assets of a specific type ID and script index. The script index of an asset can be
        /// found from <see cref="GetScriptIndex(AssetFileInfo)"/> or <see cref="AssetsFileMetadata.ScriptTypes"/>.
        /// </summary>
        /// <param name="typeId">The type ID to search for.</param>
        /// <param name="scriptIndex">The script index to search for.</param>
        /// <returns>A list of infos for that type ID and script index.</returns>
        public List<AssetFileInfo> GetAssetsOfType(int typeId, ushort scriptIndex) => Metadata.GetAssetsOfType(typeId, scriptIndex);
        /// <summary>
        /// Get all assets of a specific type ID and script index. The script index of an asset can be
        /// found from <see cref="GetScriptIndex(AssetFileInfo)"/> or <see cref="AssetsFileMetadata.ScriptTypes"/>.
        /// </summary>
        /// <param name="typeId">The type ID to search for.</param>
        /// <param name="scriptIndex">The script index to search for.</param>
        /// <returns>A list of infos for that type ID and script index.</returns>
        public List<AssetFileInfo> GetAssetsOfType(AssetClassID typeId, ushort scriptIndex) => Metadata.GetAssetsOfType(typeId, scriptIndex);

        /// <summary>
        /// A list of all asset infos in this file.
        /// </summary>
        public IList<AssetFileInfo> AssetInfos => Metadata.AssetInfos;
    }
}
