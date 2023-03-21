using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, int fileId, long pathId, bool onlyGetInfo = false, AssetReadFlags readFlags = AssetReadFlags.None)
        {
            AssetExternal ext = new AssetExternal
            {
                info = null,
                baseField = null,
                file = null
            };

            if (fileId == 0 && pathId == 0)
            {
                return ext;
            }
            else if (fileId != 0)
            {
                AssetsFileInstance dep = relativeTo.GetDependency(this, fileId - 1);

                if (dep == null)
                    return ext;

                ext.file = dep;
                ext.info = dep.file.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.baseField = GetBaseField(dep, ext.info, readFlags);
                else
                    ext.baseField = null;

                return ext;
            }
            else
            {
                ext.file = relativeTo;
                ext.info = relativeTo.file.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.baseField = GetBaseField(relativeTo, ext.info, readFlags);
                else
                    ext.baseField = null;

                return ext;
            }
        }

        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, AssetTypeValueField pptrField, bool onlyGetInfo = false, AssetReadFlags readFlags = AssetReadFlags.None)
        {
            int fileId = pptrField["m_FileID"].AsInt;
            long pathId = pptrField["m_PathID"].AsLong;
            return GetExtAsset(relativeTo, fileId, pathId, onlyGetInfo, readFlags);
        }
    }
}
