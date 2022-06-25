using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetExternal GetExternal(AssetsFile relativeTo, int fileId, long pathId, bool onlyGetInfo = false, bool preferEditor = false)
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
                AssetsFile dep = GetDependency(relativeTo, fileId - 1);

                if (dep == null)
                    return ext;

                ext.file = dep;
                ext.info = dep.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.baseField = GetBaseField(dep, ext.info, preferEditor);
                else
                    ext.baseField = null;

                return ext;
            }
            else
            {
                ext.file = relativeTo;
                ext.info = relativeTo.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.baseField = GetBaseField(relativeTo, ext.info, preferEditor);
                else
                    ext.baseField = null;

                return ext;
            }
        }

        public AssetsFile GetDependency(AssetsFile assetsFile, int index, bool loadIfNotLoaded = true)
        {
            if (index >= assetsFile.Metadata.Externals.Count)
            {
                return null;
            }

            string depName = Path.GetFileName(assetsFile.Metadata.Externals[index].PathName);
            string depNameLower = depName.ToLower();
            if (files.ContainsKey(depNameLower))
            {
                return files[depNameLower];
            }
            else if (loadIfNotLoaded)
            {
                string depPath = Path.Combine(activeDirectory, depName);
                if (File.Exists(depPath))
                {
                    return LoadAssetsFile(depPath, false);
                }
            }

            return null;
        }

        public AssetExternal GetExternal(AssetsFile assetsFile, AssetTypeValueField field, bool onlyGetInfo = false, bool preferEditor = false)
        {
            int fileId = field["m_FileID"].AsInt;
            long pathId = field["m_PathID"].AsLong;
            return GetExternal(assetsFile, fileId, pathId, onlyGetInfo, preferEditor);
        }

        public AssetTypeValueField GetBaseField(AssetsFile assetsFile, AssetFileInfo info, bool preferEditor = false)
        {
            AssetTypeTemplateField tempField = GetTemplateBaseField(assetsFile, info, preferEditor);
            AssetTypeValueField valueField = tempField.MakeValue(assetsFile.Reader, info.AbsoluteByteStart);
            return valueField;
        }
    }
}
