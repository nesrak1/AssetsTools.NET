using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
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
        /// Original path name listed in the assets file (if it was changed). You shouldn't modify this.
        /// </summary>
        public string OriginalPathName { get; set; }

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
    }
}
