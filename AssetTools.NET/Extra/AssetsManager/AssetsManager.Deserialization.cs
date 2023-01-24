using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public AssetTypeTemplateField GetTemplateBaseField(
            AssetsFileInstance inst, AssetFileInfo info,
            bool preferEditor = false, bool skipMonoBehaviourFields = false)
        {
            long absFilePos = info.AbsoluteByteStart;
            ushort scriptIndex = inst.file.GetScriptIndex(info);
            return GetTemplateBaseField(inst, inst.file.Reader, absFilePos, info.TypeId, scriptIndex, preferEditor, skipMonoBehaviourFields);
        }

        public AssetTypeTemplateField GetTemplateBaseField(
            AssetsFileInstance inst, AssetsFileReader reader, long absByteStart,
            int typeId, ushort scriptIndex,
            bool preferEditor = false, bool skipMonoBehaviourFields = false)
        {
            AssetsFile file = inst.file;
            bool hasTypeTree = file.Metadata.TypeTreeEnabled;
            AssetTypeTemplateField baseField;
            if (UseTemplateFieldCache && templateFieldCache.ContainsKey(typeId))
            {
                baseField = templateFieldCache[typeId];
                return baseField;
            }
            else
            {
                if (hasTypeTree)
                {
                    TypeTreeType ttType = file.Metadata.FindTypeTreeTypeByID(typeId, scriptIndex);
                    if (ttType != null && ttType.Nodes.Count > 0)
                    {
                        baseField = new AssetTypeTemplateField();
                        baseField.FromTypeTree(ttType);

                        // todo: handle monos
                        if (UseTemplateFieldCache && typeId != (int)AssetClassID.MonoBehaviour)
                        {
                            templateFieldCache[typeId] = baseField;
                        }

                        return baseField;
                    }
                }

                ClassDatabaseType cldbType = ClassDatabase.FindAssetClassByID(typeId);
                if (cldbType != null)
                {
                    baseField = new AssetTypeTemplateField();
                    baseField.FromClassDatabase(ClassDatabase, cldbType, preferEditor);

                    // todo: handle monos
                    if (UseTemplateFieldCache && typeId != (int)AssetClassID.MonoBehaviour)
                    {
                        templateFieldCache[typeId] = baseField;
                    }

                    if (typeId == (int)AssetClassID.MonoBehaviour && MonoTempGenerator != null && !skipMonoBehaviourFields)
                    {
                        AssetTypeValueField mbBaseField = baseField.MakeValue(reader, absByteStart);
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

                            // newer games don't have .dll
                            // let's just be consistent and remove .dll from all assemblyName strings
                            if (assemblyName.EndsWith(".dll"))
                            {
                                assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
                            }

                            if (success)
                            {
                                AssetTypeTemplateField newBaseField =
                                    MonoTempGenerator.GetTemplateField(baseField, assemblyName, nameSpace, className, new UnityVersion(file.Metadata.UnityVersion));

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

            // reader argument doesn't matter since it's only used for MonoBehaviours
            AssetTypeTemplateField templateField = GetTemplateBaseField(inst, null, absFilePos, typeId, scriptIndex, preferEditor);
            if (templateField == null)
                return false;

            inst.file.Reader.Position = absFilePos;
            AssetTypeValueField valueField = templateField.MakeValue(inst.file.Reader);
            assemblyName = valueField["m_AssemblyName"].AsString;
            nameSpace = valueField["m_Namespace"].AsString;
            className = valueField["m_ClassName"].AsString;

            return true;
        }

        public AssetTypeTemplateField CreateTemplateBaseField(AssetsFileInstance inst, int id, ushort scriptIndex = 0xffff)
        {
            return CreateTemplateBaseField(inst.file, id, scriptIndex);
        }

        public AssetTypeTemplateField CreateTemplateBaseField(AssetsFile file, int id, ushort scriptIndex = 0xffff)
        {
            AssetTypeTemplateField templateField = new AssetTypeTemplateField();
            if (file.Metadata.TypeTreeEnabled)
            {
                var typeTreeType = file.Metadata.FindTypeTreeTypeByID(id, scriptIndex);
                templateField.FromTypeTree(typeTreeType);
            }
            else
            {
                var cldbType = ClassDatabase.FindAssetClassByID(id);
                templateField.FromClassDatabase(ClassDatabase, cldbType);
            }
            return templateField;
        }

        public AssetTypeValueField CreateValueBaseField(AssetsFileInstance inst, int id, ushort scriptIndex = 0xffff)
        {
            return CreateValueBaseField(inst.file, id, scriptIndex);
        }

        public AssetTypeValueField CreateValueBaseField(AssetsFile file, int id, ushort scriptIndex = 0xffff)
        {
            AssetTypeTemplateField tempField = CreateTemplateBaseField(file, id, scriptIndex);
            return ValueBuilder.DefaultValueFieldFromTemplate(tempField);
        }
    }
}
