﻿namespace AssetsTools.NET
{
    public class AssetsFileExternal
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public string VirtualAssetPathName { get; set; }
        /// <summary>
        /// GUID for dependencies used in editor. Otherwise this is 0.
        /// </summary>
        public GUID128 Guid { get; set; }
        /// <summary>
        /// Dependency type.
        /// </summary>
        public AssetsFileExternalType Type { get; set; }
        /// <summary>
        /// Real path name to the other file.
        /// </summary>
        public string PathName { get; set; }
        /// <summary>
        /// Original path name listed in the assets file (if it was changed).
        /// You shouldn't modify this.
        /// </summary>
        public string OriginalPathName { get; set; }

        /// <summary>
        /// Read the <see cref="AssetsFileExternal"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            VirtualAssetPathName = reader.ReadNullTerminated();
            Guid = new GUID128(reader);
            Type = (AssetsFileExternalType)reader.ReadInt32();
            PathName = reader.ReadNullTerminated();
            OriginalPathName = PathName;

            // Because lowercase "resources" is read by Unity fine on Linux, it either uses
            // hardcoded replaces like below or it has case insensitive pathing somehow.
            // This isn't consistent with the original AssetsTools but it only supported
            // Windows anyway, so this will only create issues if more than these three
            // pop up in the future. Also, the reason I don't just replace all "library"
            // with "Resources" is so that when saving, I can change it back to the original
            // (like how unity_builtin_extra goes back to "resources", not "library")
            if (PathName == "resources/unity_builtin_extra")
            {
                PathName = "Resources/unity_builtin_extra";
            }
            else if (PathName == "library/unity default resources" || PathName == "Library/unity default resources")
            {
                PathName = "Resources/unity default resources";
            }
            else if (PathName == "library/unity editor resources" || PathName == "Library/unity editor resources")
            {
                PathName = "Resources/unity editor resources";
            }
        }

        /// <summary>
        /// Write the <see cref="AssetsFileExternal"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(VirtualAssetPathName);
            Guid.Write(writer);
            writer.Write((int)Type);
            string assetPathTemp = PathName;
            if ((PathName == "Resources/unity_builtin_extra" ||
                PathName == "Resources/unity default resources" ||
                PathName == "Resources/unity editor resources")
                && OriginalPathName != string.Empty)
            {
                assetPathTemp = OriginalPathName;
            }
            writer.WriteNullTerminated(assetPathTemp);
        }

        /// <summary>
        /// Get the maximum size of this external.
        /// </summary>
        public long GetSize()
        {
            long size = 0;
            size += VirtualAssetPathName.Length + 1;
            size += 16;
            size += 4;

            if ((PathName == "Resources/unity_builtin_extra" ||
                PathName == "Resources/unity default resources" ||
                PathName == "Resources/unity editor resources")
                && OriginalPathName != string.Empty)
            {
                size += OriginalPathName.Length + 1;
            }
            else
            {
                size += PathName.Length + 1;
            }

            return size;
        }
    }
}
