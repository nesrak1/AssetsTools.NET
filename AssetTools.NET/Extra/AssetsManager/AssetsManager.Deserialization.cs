using System;
using System.IO;

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
            ushort scriptIndex = info.GetScriptIndex(inst.file);
            if (info.ReplacerType != ContentReplacerType.AddOrModify)
            {
                long absFilePos = info.GetAbsoluteByteOffset(inst.file);
                return GetTemplateBaseField(inst, inst.file.Reader, absFilePos, info.TypeId, scriptIndex, readFlags);
            }
            else
            {
                if (info.Replacer.HasPreview())
                {
                    Stream stream = info.Replacer.GetPreviewStream();
                    AssetsFileReader reader = new AssetsFileReader(stream);
                    return GetTemplateBaseField(inst, reader, 0, info.TypeId, scriptIndex, readFlags);
                }
                else
                {
                    return GetTemplateBaseField(inst, null, 0, info.TypeId, scriptIndex, readFlags);
                }
            }
        }

        public AssetTypeTemplateField GetTemplateBaseField(
            AssetsFileInstance inst, AssetsFileReader reader, long absByteStart,
            int typeId, ushort scriptIndex, AssetReadFlags readFlags)
        {
            AssetsFile file = inst.file;
            AssetTypeTemplateField baseField = null;
            bool hasTypeTree = inst.file.Metadata.TypeTreeEnabled;

            bool preferEditor = (readFlags & AssetReadFlags.PreferEditor) != 0;
            bool forceFromCldb = (readFlags & AssetReadFlags.ForceFromCldb) != 0;
            bool skipMonoBehaviourFields = (readFlags & AssetReadFlags.SkipMonoBehaviourFields) != 0;

            // if non-monobehaviour type is in cache, return the cached item
            if (UseTemplateFieldCache && typeId != (int)AssetClassID.MonoBehaviour && templateFieldCache.TryGetValue(typeId, out baseField))
            {
                return baseField;
            }

            // if there's a type tree AND we aren't forcing from a class database
            // (with the condition that we actually have a class database) then
            // load from that instead
            if (hasTypeTree && (!forceFromCldb || ClassDatabase == null))
            {
                if (UseMonoTemplateFieldCache && typeId == (int)AssetClassID.MonoBehaviour)
                {
                    if (monoTypeTreeTemplateFieldCache.TryGetValue(inst, out ConcurrentDictionary<ushort, AssetTypeTemplateField> templates) &&
                        templates.TryGetValue(scriptIndex, out baseField))
                    {
                        return baseField;
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
                        if (!monoTypeTreeTemplateFieldCache.TryGetValue(inst, out ConcurrentDictionary<ushort, AssetTypeTemplateField> templates))
                        {
                            monoTypeTreeTemplateFieldCache[inst] = templates = new ConcurrentDictionary<ushort, AssetTypeTemplateField>();
                        }
                        templates[scriptIndex] = baseField;
                    }

                    return baseField;
                }
            }

            // if we cached a monobehaviour from a class database, clone a copy
            if (UseTemplateFieldCache && UseMonoTemplateFieldCache && typeId == (int)AssetClassID.MonoBehaviour)
            {
                if (templateFieldCache.TryGetValue(typeId, out baseField))
                {
                    baseField = baseField.Clone();
                }
            }

            // if we haven't got the basefield yet, the only option left is
            // the class database. if it's not there or the database isn't
            // loaded, we're out of luck.
            if (baseField == null)
            {
                if (ClassDatabase == null)
                {
                    return null;
                }

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

            // we need to generate the monobehaviour fields from a mono temp
            // generator. this requires parsing the base monobehaviour so we
            // can get the monoscript (we could also use the script index
            // but this is safer) and then passing the script from there to
            // the temp generator. we then append those fields to the base.
            if (typeId == (int)AssetClassID.MonoBehaviour && MonoTempGenerator != null && !skipMonoBehaviourFields && reader != null)
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

                    if (monoScriptFile == null)
                    {
                        return baseField;
                    }

                    ConcurrentDictionary<long, AssetTypeTemplateField> templates = null;
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
                            monoCldbTemplateFieldCache[monoScriptFile] = templates = new ConcurrentDictionary<long, AssetTypeTemplateField>();
                        }
                    }

                    AssetFileInfo monoScriptInfo = monoScriptFile.file.GetAssetInfo(msPtr.PathId);
                    long monoScriptAbsFilePos = monoScriptInfo.GetAbsoluteByteOffset(monoScriptFile.file);
                    int monoScriptTypeId = monoScriptInfo.TypeId;
                    ushort monoScriptScriptIndex = monoScriptInfo.GetScriptIndex(monoScriptFile.file);

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

            // this should be pretty fast, but you never know I guess.
            // might want to move the save to byte array pattern into
            // a new function at some point...
            AssetTypeValueField valueField;
            lock (inst.LockReader)
            {
                inst.file.Reader.Position = absFilePos;
                valueField = templateField.MakeValue(inst.file.Reader);
            }
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
                if (id != (int)AssetClassID.MonoBehaviour || scriptIndex == 0xffff)
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

            AssetTypeValueField valueField;
            if (info.IsReplacerPreviewable)
            {
                // probably not the best idea to lock this stream,
                // but how many times will we be reading the same
                // asset at the same time?
                Stream previewStream = info.Replacer.GetPreviewStream();
                lock (previewStream)
                {
                    valueField = tempField.MakeValue(new AssetsFileReader(previewStream), 0, refMan);
                }
            }
            else
            {
                using MemoryStream assetDataStream = new MemoryStream((int)info.ByteSize);
                lock (inst.LockReader)
                {
                    AssetsFileReader reader = inst.file.Reader;
                    reader.Position = info.GetAbsoluteByteOffset(inst.file);
                    reader.BaseStream.CopyToCompat(assetDataStream, info.ByteSize);
                }
                assetDataStream.Position = 0;
                valueField = tempField.MakeValue(new AssetsFileReader(assetDataStream), 0, refMan);
            }
            return valueField;
        }

        public AssetTypeValueField GetBaseField(AssetsFileInstance inst, long pathId, AssetReadFlags readFlags = AssetReadFlags.None)
        {
            AssetFileInfo info = inst.file.GetAssetInfo(pathId);
            return GetBaseField(inst, info, readFlags);
        }
    }
}
