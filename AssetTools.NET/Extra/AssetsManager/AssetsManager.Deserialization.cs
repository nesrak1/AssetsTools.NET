using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public RefTypeManager GetRefTypeManager(AssetsFileInstance inst)
        {
            if (UseRefTypeManagerCache && refTypeManagerCache.TryGetValue(inst, out RefTypeManager refMan))
            {
                return refMan;
            }

            refMan = new RefTypeManager();
            refMan.FromTypeTree(inst.file.Metadata);
            
            if (MonoTempGenerator != null)
            {
                refMan.WithMonoTemplateGenerator(inst.file.Metadata, MonoTempGenerator, UseMonoTemplateFieldCache ? monoTemplateFieldCache : null);
            }

            if (UseRefTypeManagerCache)
            {
                refTypeManagerCache[inst] = refMan;
            }

            return refMan;
        }

        public AssetTypeTemplateField GetTemplateBaseField(
            AssetsFileInstance inst, AssetFileInfo info,
            AssetReadFlags readFlags = AssetReadFlags.None)
        {
            long absFilePos = info.AbsoluteByteStart;
            ushort scriptIndex = inst.file.GetScriptIndex(info);
            return GetTemplateBaseField(inst, inst.file.Reader, absFilePos, info.TypeId, scriptIndex, readFlags);
        }

        public AssetTypeTemplateField GetTemplateBaseField(
            AssetsFileInstance inst, AssetsFileReader reader, long absByteStart,
            int typeId, ushort scriptIndex, AssetReadFlags readFlags)
        {
            AssetsFile file = inst.file;
            AssetTypeTemplateField baseField = null;
            bool hasTypeTree = inst.file.Metadata.TypeTreeEnabled;

            bool preferEditor = Net35Polyfill.HasFlag(readFlags, AssetReadFlags.PreferEditor);
            bool forceFromCldb = Net35Polyfill.HasFlag(readFlags, AssetReadFlags.ForceFromCldb);
            bool skipMonoBehaviourFields = Net35Polyfill.HasFlag(readFlags, AssetReadFlags.SkipMonoBehaviourFields);

            if (UseTemplateFieldCache && typeId != (int)AssetClassID.MonoBehaviour && templateFieldCache.TryGetValue(typeId, out baseField))
            {
                return baseField;
            }

            if (hasTypeTree && !forceFromCldb)
            {
                if (UseMonoTemplateFieldCache && typeId == (int)AssetClassID.MonoBehaviour)
                {
                    if (monoTypeTreeTemplateFieldCache.TryGetValue(inst, out Dictionary<ushort, AssetTypeTemplateField> templates) &&
                        templates.TryGetValue(scriptIndex, out AssetTypeTemplateField template))
                    {
                        return template;
                    }
                }

                TypeTreeType ttType = file.Metadata.FindTypeTreeTypeByID(typeId, scriptIndex);
                if (ttType != null && ttType.Nodes.Count > 0)
                {
                    baseField = new AssetTypeTemplateField();
                    baseField.FromTypeTree(ttType);

                    if (UseTemplateFieldCache && typeId != (int)AssetClassID.MonoBehaviour)
                    {
                        templateFieldCache[typeId] = baseField;
                    }
                    else if (UseMonoTemplateFieldCache && typeId == (uint)AssetClassID.MonoBehaviour)
                    {
                        if (!monoTypeTreeTemplateFieldCache.TryGetValue(inst, out Dictionary<ushort, AssetTypeTemplateField> templates))
                        {
                            monoTypeTreeTemplateFieldCache[inst] = templates = new Dictionary<ushort, AssetTypeTemplateField>();
                        }
                        templates[scriptIndex] = baseField;
                    }

                    return baseField;
                }
            }

            if (UseTemplateFieldCache && UseMonoTemplateFieldCache && typeId == (int)AssetClassID.MonoBehaviour)
            {
                if (templateFieldCache.TryGetValue(typeId, out baseField))
                {
                    baseField = baseField.Clone();
                }
            }

            if (baseField == null)
            {
                ClassDatabaseType cldbType = ClassDatabase.FindAssetClassByID(typeId);
                if (cldbType == null)
                {
                    return null;
                }

                baseField = new AssetTypeTemplateField();
                baseField.FromClassDatabase(ClassDatabase, cldbType, preferEditor);

                if (UseTemplateFieldCache)
                {
                    if (typeId == (int)AssetClassID.MonoBehaviour)
                    {
                        templateFieldCache[typeId] = baseField.Clone();
                    }
                    else
                    {
                        templateFieldCache[typeId] = baseField;
                    }
                }
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

                    Dictionary<long, AssetTypeTemplateField> templates = null;
                    if (UseMonoTemplateFieldCache)
                    {
                        if (monoCldbTemplateFieldCache.TryGetValue(monoScriptFile, out templates))
                        {
                            if (templates.TryGetValue(msPtr.PathId, out AssetTypeTemplateField template))
                            {
                                return template;
                            }
                        }
                        else
                        {
                            monoCldbTemplateFieldCache[monoScriptFile] = templates = new Dictionary<long, AssetTypeTemplateField>();
                        }
                    }

                    AssetFileInfo monoScriptInfo = monoScriptFile.file.GetAssetInfo(msPtr.PathId);
                    long monoScriptAbsFilePos = monoScriptInfo.AbsoluteByteStart;
                    int monoScriptTypeId = monoScriptInfo.TypeId;
                    ushort monoScriptScriptIndex = monoScriptFile.file.GetScriptIndex(monoScriptInfo);

                    bool success = GetMonoScriptInfo(
                        monoScriptFile, monoScriptAbsFilePos, monoScriptTypeId, monoScriptScriptIndex,
                        out string assemblyName, out string nameSpace, out string className, readFlags);

                    // newer games don't have .dll
                    // let's just be consistent and remove .dll from all assemblyName strings
                    if (assemblyName.EndsWith(".dll"))
                    {
                        assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
                    }

                    if (success)
                    {
                        AssetTypeReference reference = new AssetTypeReference(className, nameSpace, assemblyName);
                        if (UseMonoTemplateFieldCache)
                        {
                            if (monoTemplateFieldCache.TryGetValue(reference, out AssetTypeTemplateField template))
                            {
                                templates[msPtr.PathId] = template;
                                return template;
                            }
                        }

                        AssetTypeTemplateField newBaseField =
                            MonoTempGenerator.GetTemplateField(baseField, assemblyName, nameSpace, className, new UnityVersion(file.Metadata.UnityVersion));

                        if (newBaseField != null)
                        {
                            baseField = newBaseField;
                            if (UseMonoTemplateFieldCache)
                            {
                                templates[msPtr.PathId] = monoTemplateFieldCache[reference] = baseField;
                                return baseField;
                            }
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

        private bool GetMonoScriptInfo(
            AssetsFileInstance inst, long absFilePos, int typeId, ushort scriptIndex,
            out string assemblyName, out string nameSpace, out string className,
            AssetReadFlags readFlags)
        {
            assemblyName = null;
            nameSpace = null;
            className = null;

            // reader argument doesn't matter since it's only used for MonoBehaviours
            AssetTypeTemplateField templateField = GetTemplateBaseField(inst, null, absFilePos, typeId, scriptIndex, readFlags);
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
            AssetsFile file = inst.file;
            AssetTypeTemplateField templateField = new AssetTypeTemplateField();
            if (file.Metadata.TypeTreeEnabled)
            {
                var typeTreeType = file.Metadata.FindTypeTreeTypeByID(id, scriptIndex);
                templateField.FromTypeTree(typeTreeType);
            }
            else
            {
                if (id != 0x72 || scriptIndex == 0xffff)
                {
                    var cldbType = ClassDatabase.FindAssetClassByID(id);
                    templateField.FromClassDatabase(ClassDatabase, cldbType);
                }
                else
                {
                    if (MonoTempGenerator == null)
                    {
                        throw new Exception($"{nameof(MonoTempGenerator)} must be non-null to create a MonoBehaviour!");
                    }

                    // MonoBehaviour doesn't exist yet, so we can't just read
                    // the m_Script field. instead, we look in ScriptTypes.
                    var scriptTypeInfo = AssetHelper.GetAssetsFileScriptInfo(this, inst, scriptIndex);
                    var mbTempField = GetTemplateBaseField(inst, file.Reader, -1, (int)AssetClassID.MonoBehaviour, scriptIndex, AssetReadFlags.SkipMonoBehaviourFields);
                    var unityVersion = new UnityVersion(file.Metadata.UnityVersion);
                    templateField = MonoTempGenerator.GetTemplateField(
                        mbTempField,
                        scriptTypeInfo.AsmName,
                        scriptTypeInfo.Namespace,
                        scriptTypeInfo.ClassName,
                        unityVersion
                    );
                }
            }
            return templateField;
        }

        public AssetTypeValueField CreateValueBaseField(AssetsFileInstance inst, int id, ushort scriptIndex = 0xffff)
        {
            AssetTypeTemplateField tempField = CreateTemplateBaseField(inst, id, scriptIndex);
            return ValueBuilder.DefaultValueFieldFromTemplate(tempField);
        }

        public AssetTypeValueField GetBaseField(AssetsFileInstance inst, AssetFileInfo info, AssetReadFlags readFlags = AssetReadFlags.None)
        {
            AssetTypeTemplateField tempField = GetTemplateBaseField(inst, info, readFlags);
            RefTypeManager refMan = GetRefTypeManager(inst);
            AssetTypeValueField valueField = tempField.MakeValue(inst.file.Reader, info.AbsoluteByteStart, refMan);
            return valueField;
        }

        public AssetTypeValueField GetBaseField(AssetsFileInstance inst, long pathId, AssetReadFlags readFlags = AssetReadFlags.None)
        {
            AssetFileInfo info = inst.file.GetAssetInfo(pathId);
            return GetBaseField(inst, info, readFlags);
        }
    }
}
