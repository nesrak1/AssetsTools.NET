using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetTypeTemplateField GetTemplateBaseField(AssetsFileInstance inst, AssetFileInfo info, bool preferEditor = false)
        {
            long absFilePos = info.AbsoluteByteStart;
            ushort scriptIndex = inst.file.GetScriptIndex(info);
            int fixedId = AssetHelper.FixAudioID(info.TypeId);
            return GetTemplateBaseField(inst, absFilePos, fixedId, scriptIndex, preferEditor);
        }

        public AssetTypeTemplateField GetTemplateBaseField(AssetsFileInstance inst, long absFilePos, int typeId, ushort scriptIndex, bool preferEditor = false)
        {
            AssetsFile file = inst.file;
            bool hasTypeTree = file.Metadata.TypeTreeEnabled;
            AssetTypeTemplateField baseField;
            if (useTemplateFieldCache && templateFieldCache.ContainsKey(typeId))
            {
                baseField = templateFieldCache[typeId];
                return baseField;
            }
            else
            {
                if (hasTypeTree)
                {
                    TypeTreeType ttType = AssetHelper.FindTypeTreeTypeByID(file.Metadata, typeId, scriptIndex);
                    if (ttType != null && ttType.Nodes.Count > 0)
                    {
                        baseField = new AssetTypeTemplateField();
                        baseField.FromTypeTree(ttType);

                        if (useTemplateFieldCache)
                        {
                            templateFieldCache[typeId] = baseField;
                        }

                        return baseField;
                    }
                }

                ClassDatabaseType cldbType = AssetHelper.FindAssetClassByID(classDatabase, typeId);
                if (cldbType != null)
                {
                    baseField = new AssetTypeTemplateField();
                    baseField.FromClassDatabase(classDatabase, cldbType, preferEditor);

                    if (useTemplateFieldCache)
                    {
                        templateFieldCache[typeId] = baseField;
                    }

                    if (typeId == (int)AssetClassID.MonoBehaviour && monoTempGenerator != null)
                    {
                        AssetTypeValueField mbBaseField = baseField.MakeValue(file.Reader, absFilePos);
                        AssetPPtr msPtr = AssetPPtr.FromField(mbBaseField["m_Script"]);
                        if (!msPtr.IsNull())
                        {
                            AssetsFileInstance monoScriptFile;
                            if (msPtr.FileId == 0)
                                monoScriptFile = inst;
                            else
                                monoScriptFile = inst.GetDependency(this, msPtr.FileId - 1);

                            AssetFileInfo monoScriptInfo = monoScriptFile.file.GetAssetInfo(msPtr.PathId);
                            long monoScriptAbsFilePos = monoScriptInfo.AbsoluteByteStart;
                            int monoScriptTypeId = monoScriptInfo.TypeId;
                            ushort monoScriptScriptIndex = monoScriptFile.file.GetScriptIndex(monoScriptInfo);

                            bool success = GetMonoScriptInfo(
                                monoScriptFile, monoScriptAbsFilePos, monoScriptTypeId, monoScriptScriptIndex,
                                out string assemblyName, out string nameSpace, out string className, preferEditor);

                            if (success)
                            {
                                AssetTypeTemplateField newBaseField =
                                    monoTempGenerator.GetTemplateField(baseField, assemblyName, nameSpace, className, new UnityVersion(file.Metadata.UnityVersion));

                                if (newBaseField != null)
                                {
                                    baseField = newBaseField;
                                }
                                else
                                {
                                    // failure, maybe report why?
                                }
                            }
                        }
                    }

                    return baseField;
                }

                return null;
            }
        }

        private bool GetMonoScriptInfo(
            AssetsFileInstance inst, long absFilePos, int typeId, ushort scriptIndex,
            out string assemblyName, out string nameSpace, out string className, bool preferEditor = false)
        {
            assemblyName = null;
            nameSpace = null;
            className = null;

            AssetTypeTemplateField templateField = GetTemplateBaseField(inst, absFilePos, typeId, scriptIndex, preferEditor);
            if (templateField == null)
                return false;

            inst.file.Reader.Position = absFilePos;
            AssetTypeValueField valueField = templateField.MakeValue(inst.file.Reader);
            assemblyName = valueField["m_AssemblyName"].AsString;
            nameSpace = valueField["m_Namespace"].AsString;
            className = valueField["m_ClassName"].AsString;

            return true;
        }

        public void SetMonoTempGenerator(IMonoBehaviourTemplateGenerator generator)
        {
            monoTempGenerator = generator;
        }
    }
}
