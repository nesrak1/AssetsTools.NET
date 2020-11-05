using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetsTools.NET.Extra
{
    public class AssetsManager
    {
        public bool updateAfterLoad = true;
        public bool useTemplateFieldCache = false;
        public ClassDatabasePackage classPackage;
        public ClassDatabaseFile classFile;
        public List<AssetsFileInstance> files = new List<AssetsFileInstance>();
        public List<BundleFileInstance> bundles = new List<BundleFileInstance>();
        private Dictionary<uint, AssetTypeTemplateField> templateFieldCache = new Dictionary<uint, AssetTypeTemplateField>();
        private Dictionary<string, AssetTypeTemplateField> monoTemplateFieldCache = new Dictionary<string, AssetTypeTemplateField>();

        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, string root = "")
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index == -1)
            {
                instance = new AssetsFileInstance(stream, path, root);
                files.Add(instance);
            }
            else
            {
                instance = files[index];
            }

            if (updateAfterLoad)
                UpdateDependency(instance);
            if (loadDeps)
                LoadDeps(instance, Path.GetDirectoryName(path));
            return instance;
        }
        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps, string root = "")
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(stream.Name).ToLower());
            if (index == -1)
            {
                instance = new AssetsFileInstance(stream, root);
                files.Add(instance);
            }
            else
            {
                instance = files[index];
            }

            if (updateAfterLoad)
                UpdateDependency(instance);
            if (loadDeps)
                LoadDeps(instance, Path.GetDirectoryName(stream.Name));
            return instance;
        }
        public BundleFileInstance LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            BundleFileInstance instance;
            int index = bundles.FindIndex(f => f.path.ToLower() == Path.GetFullPath(stream.Name).ToLower());
            if (index == -1)
            {
                instance = new BundleFileInstance(stream, "", unpackIfPacked);
                bundles.Add(instance);
            }
            else
            {
                instance = bundles[index];
            }
            return instance;
        }
        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), loadDeps, root);
        }
        public BundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), unpackIfPacked);
        }
        
        private void UpdateDependency(AssetsFileInstance ofFile)
        {
            for (int i = 0; i < files.Count; i++)
            {
                AssetsFileInstance file = files[i];
                for (int j = 0; j < file.file.dependencies.dependencyCount; j++)
                {
                    AssetsFileDependency dep = file.file.dependencies.dependencies[j];
                    if (Path.GetFileName(dep.assetPath.ToLower()) == Path.GetFileName(ofFile.path.ToLower()))
                    {
                        file.dependencies[j] = ofFile;
                    }
                }
            }
        }
        public void UpdateDependencies()
        {
            for (int x = 0; x < files.Count; x++)
            {
                AssetsFileInstance ofFile = files[x];

                for (int i = 0; i < files.Count; i++)
                {
                    AssetsFileInstance file = files[i];
                    for (int j = 0; j < file.file.dependencies.dependencyCount; j++)
                    {
                        AssetsFileDependency dep = file.file.dependencies.dependencies[j];
                        if (Path.GetFileName(dep.assetPath.ToLower()) == Path.GetFileName(ofFile.path.ToLower()))
                        {
                            file.dependencies[j] = ofFile;
                        }
                    }
                }
            }
        }

        //todo, set stream options
        private void LoadDeps(AssetsFileInstance ofFile, string path)
        {
            for (int i = 0; i < ofFile.dependencies.Count; i++)
            {
                string depPath = ofFile.file.dependencies.dependencies[i].assetPath;
                if (files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower()) == -1)
                {
                    string absPath = Path.Combine(path, depPath);
                    string localAbsPath = Path.Combine(path, Path.GetFileName(depPath));
                    if (File.Exists(absPath))
                    {
                        LoadAssetsFile(
                            new FileStream(absPath, FileMode.Open, FileAccess.Read, FileShare.Read), true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        LoadAssetsFile(
                            new FileStream(localAbsPath, FileMode.Open, FileAccess.Read, FileShare.Read), true);
                    }
                }
            }
        }

        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, int fileId, long pathId, bool onlyGetInfo = false, bool forceFromCldb = false)
        {
            AssetExternal ext = new AssetExternal();
            if (fileId == 0 && pathId == 0)
            {
                ext.info = null;
                ext.instance = null;
                ext.file = null;
            }
            else if (fileId != 0)
            {
                AssetsFileInstance dep = relativeTo.GetDependency(this, fileId - 1);
                ext.info = dep.table.GetAssetInfo(pathId);
                if (!onlyGetInfo)
                    ext.instance = GetTypeInstance(dep.file, ext.info, forceFromCldb);
                else
                    ext.instance = null;
                ext.file = dep;
            }
            else
            {
                ext.info = relativeTo.table.GetAssetInfo(pathId);
                if (!onlyGetInfo)
                    ext.instance = GetTypeInstance(relativeTo.file, ext.info, forceFromCldb);
                else
                    ext.instance = null;
                ext.file = relativeTo;
            }
            return ext;
        }

        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, AssetTypeValueField atvf, bool onlyGetInfo = false, bool forceFromCldb = false)
        {
            int fileId = atvf.Get("m_FileID").GetValue().AsInt();
            long pathId = atvf.Get("m_PathID").GetValue().AsInt64();
            return GetExtAsset(relativeTo, fileId, pathId, onlyGetInfo, forceFromCldb);
        }

        public AssetTypeInstance GetTypeInstance(AssetsFileInstance inst, AssetFileInfoEx info, bool forceFromCldb = false)
        {
            return GetTypeInstance(inst.file, info, forceFromCldb);
        }

        public AssetTypeInstance GetTypeInstance(AssetsFile file, AssetFileInfoEx info, bool forceFromCldb = false)
        {
            return new AssetTypeInstance(GetTemplateBaseField(file, info, forceFromCldb), file.reader, info.absoluteFilePos);
        }

        //this method was renamed for consistency/clarity
        //because it's used so much, I don't want to deprecate it right away
        //so I'll keep the old method here for a while
        public AssetTypeInstance GetATI(AssetsFile file, AssetFileInfoEx info, bool forceFromCldb = false)
        {
            return GetTypeInstance(file, info, forceFromCldb);
        }

        public AssetTypeTemplateField GetTemplateBaseField(AssetsFile file, AssetFileInfoEx info, bool forceFromCldb = false)
        {
            ushort scriptIndex = AssetHelper.GetScriptIndex(file, info);
            uint fixedId = AssetHelper.FixAudioID(info.curFileType);

            bool hasTypeTree = file.typeTree.hasTypeTree;
            AssetTypeTemplateField baseField;
            if (useTemplateFieldCache && templateFieldCache.ContainsKey(fixedId))
            {
                baseField = templateFieldCache[fixedId];
            }
            else
            {
                baseField = new AssetTypeTemplateField();
                if (hasTypeTree && !forceFromCldb)
                {
                    if (file.header.format < 0x10)
                    {
                        if (scriptIndex == 0xFFFF)
                            baseField.From0D(AssetHelper.FindTypeTreeTypeByID(file.typeTree, fixedId), 0);
                        else
                            baseField.From0D(AssetHelper.FindTypeTreeTypeByScriptIndex(file.typeTree, scriptIndex), 0);
                    }
                    else
                    {
                        baseField.From0D(AssetHelper.FindTypeTreeTypeByID(file.typeTree, fixedId), 0);
                    }
                }
                else
                {
                    baseField.FromClassDatabase(classFile, AssetHelper.FindAssetClassByID(classFile, fixedId), 0);
                }

                if (useTemplateFieldCache)
                {
                    templateFieldCache[fixedId] = baseField;
                }
            }

            return baseField;
        }

        public AssetTypeValueField GetMonoBaseFieldCached(AssetsFileInstance inst, AssetFileInfoEx info, string managedPath)
        {
            //AssetsFile file = inst.file;
            //ushort scriptIndex = AssetHelper.GetScriptIndex(file, info);
            //if (scriptIndex != 0xFFFF && inst.templateFieldCache.ContainsKey(scriptIndex))
            //{
            //    AssetTypeTemplateField baseTemplateField = inst.templateFieldCache[scriptIndex];
            //    AssetTypeInstance baseAti = new AssetTypeInstance(baseTemplateField, file.reader, info.absoluteFilePos);
            //    return baseAti.GetBaseField();
            //}
            //else
            //{
            //    AssetTypeValueField baseValueField = MonoDeserializer.GetMonoBaseField(this, inst, info, managedPath);
            //    inst.templateFieldCache[scriptIndex] = baseValueField.templateField;
            //    return baseValueField;
            //}
            AssetsFile file = inst.file;
            ushort scriptIndex = AssetHelper.GetScriptIndex(file, info);
            if (scriptIndex == 0xFFFF)
                return null;

            string scriptName;
            if (!inst.monoIdToName.ContainsKey(scriptIndex))
            {
                AssetTypeInstance scriptAti = GetExtAsset(inst, GetATI(inst.file, info).GetBaseField().Get("m_Script")).instance;
                scriptName = scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
                string scriptNamespace = scriptAti.GetBaseField().Get("m_Namespace").GetValue().AsString();
                string assemblyName = scriptAti.GetBaseField().Get("m_AssemblyName").GetValue().AsString();

                if (scriptNamespace != string.Empty)
                {
                    scriptNamespace = "-";
                }

                scriptName = $"{assemblyName}.{scriptNamespace}.{scriptName}";
                inst.monoIdToName[scriptIndex] = scriptName;
            }
            else
            {
                scriptName = inst.monoIdToName[scriptIndex];
            }

            if (monoTemplateFieldCache.ContainsKey(scriptName))
            {
                AssetTypeTemplateField baseTemplateField = monoTemplateFieldCache[scriptName];
                AssetTypeInstance baseAti = new AssetTypeInstance(baseTemplateField, file.reader, info.absoluteFilePos);
                return baseAti.GetBaseField();
            }
            else
            {
                AssetTypeValueField baseValueField = MonoDeserializer.GetMonoBaseField(this, inst, info, managedPath);
                monoTemplateFieldCache[scriptName] = baseValueField.templateField;
                return baseValueField;
            }
        }

        public ClassDatabaseFile LoadClassDatabase(Stream stream)
        {
            classFile = new ClassDatabaseFile();
            classFile.Read(new AssetsFileReader(stream));
            return classFile;
        }

        public ClassDatabaseFile LoadClassDatabase(string path)
        {
            return LoadClassDatabase(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(string version, bool specific = false)
        {
            if (classPackage == null)
                throw new Exception("No class package loaded!");

            if (specific)
            {
                if (!version.StartsWith("U"))
                    version = "U" + version;
                int index = classPackage.header.files.FindIndex(f => f.name == version);
                if (index == -1)
                    return null;

                classFile = classPackage.files[index];
                return classFile;
            }
            else
            {
                if (version.StartsWith("U"))
                    version = version.Substring(1);
                for (int i = 0; i < classPackage.files.Length; i++)
                {
                    ClassDatabaseFile file = classPackage.files[i];
                    for (int j = 0; j < file.header.unityVersions.Length; j++)
                    {
                        string unityVersion = file.header.unityVersions[j];
                        if (WildcardMatches(version, unityVersion))
                        {
                            classFile = file;
                            return classFile;
                        }
                    }
                }
                return null;
            }

        }
        private bool WildcardMatches(string test, string pattern)
        {
            return Regex.IsMatch(test, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }

        public ClassDatabasePackage LoadClassPackage(Stream stream)
        {
            classPackage = new ClassDatabasePackage();
            classPackage.Read(new AssetsFileReader(stream));
            return classPackage;
        }
        public ClassDatabasePackage LoadClassPackage(string path)
        {
            return LoadClassPackage(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
    }

    public struct AssetExternal
    {
        public AssetFileInfoEx info;
        public AssetTypeInstance instance;
        public AssetsFileInstance file;
    }
}
