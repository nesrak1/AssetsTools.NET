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

        #region assets files
        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, string root = "", BundleFileInstance bunInst = null)
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index == -1)
            {
                instance = new AssetsFileInstance(stream, path, root);
                instance.parentBundle = bunInst;
                files.Add(instance);
            }
            else
            {
                instance = files[index];
            }

            if (loadDeps)
            {
                if (bunInst == null)
                    LoadDependencies(instance, Path.GetDirectoryName(path));
                else
                    LoadBundleDependencies(instance, bunInst, Path.GetDirectoryName(path));
            }
            if (updateAfterLoad)
                UpdateDependencies(instance);
            return instance;
        }
        public AssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(stream, stream.Name, loadDeps, root);
        }

        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(File.OpenRead(path), loadDeps, root);
        }

        public bool UnloadAssetsFile(string path)
        {
            int index = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index != -1)
            {
                AssetsFileInstance assetsInst = files[index];
                assetsInst.file.Close();
                files.Remove(assetsInst);
                return true;
            }
            return false;
        }

        public bool UnloadAllAssetsFiles(bool clearCache = false)
        {
            if (clearCache)
            {
                templateFieldCache.Clear();
                monoTemplateFieldCache.Clear();
            }

            if (files.Count != 0)
            {
                foreach (AssetsFileInstance assetsInst in files)
                {
                    assetsInst.file.Close();
                }
                files.Clear();
                return true;
            }
            return false;
        }

        public void UnloadAll(bool unloadClassData = false)
        {
            UnloadAllAssetsFiles(true);
            UnloadAllBundleFiles();
            if (unloadClassData)
            {
                classPackage = null;
                classFile = null;
            }
        }
        #endregion

        #region bundle files
        public BundleFileInstance LoadBundleFile(Stream stream, string path, bool unpackIfPacked = true)
        {
            BundleFileInstance bunInst;
            int index = bundles.FindIndex(f => f.path.ToLower() == path.ToLower());
            if (index == -1)
            {
                bunInst = new BundleFileInstance(stream, path, "", unpackIfPacked);
                bundles.Add(bunInst);
            }
            else
            {
                bunInst = bundles[index];
            }
            return bunInst;
        }
        public BundleFileInstance LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            return LoadBundleFile(stream, Path.GetFullPath(stream.Name), unpackIfPacked);
        }

        public BundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(File.OpenRead(path), unpackIfPacked);
        }

        public bool UnloadBundleFile(string path)
        {
            int index = bundles.FindIndex(f => f.path.ToLower() == Path.GetFullPath(path).ToLower());
            if (index != -1)
            {
                BundleFileInstance bunInst = bundles[index];
                bunInst.file.Close();

                foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                {
                    assetsInst.file.Close();
                }

                bundles.Remove(bunInst);
                return true;
            }
            return false;
        }

        public bool UnloadAllBundleFiles()
        {
            if (bundles.Count != 0)
            {
                foreach (BundleFileInstance bunInst in bundles)
                {
                    bunInst.file.Close();

                    foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                    {
                        assetsInst.file.Close();
                    }
                }
                bundles.Clear();
                return true;
            }
            return false;
        }

        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, int index, bool loadDeps = false)
        {
            string assetMemPath = Path.Combine(bunInst.path, bunInst.file.GetFileName(index));

            int listIndex = files.FindIndex(f => f.path.ToLower() == Path.GetFullPath(assetMemPath).ToLower());
            if (listIndex == -1)
            {
                if (bunInst.file.IsAssetsFile(index))
                {
                    bunInst.file.GetFileRange(index, out long offset, out long length);
                    SegmentStream stream = new SegmentStream(bunInst.BundleStream, offset, length);
                    AssetsFileInstance assetsInst = LoadAssetsFile(stream, assetMemPath, loadDeps, bunInst: bunInst);
                    bunInst.loadedAssetsFiles.Add(assetsInst);
                    return assetsInst;
                }
            }
            else
            {
                return files[listIndex];
            }
            return null;
        }

        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, string name, bool loadDeps = false)
        {
            int index = bunInst.file.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetsFileFromBundle(bunInst, index, loadDeps);
        }
        #endregion

        #region dependencies
        public void UpdateDependencies(AssetsFileInstance ofFile)
        {
            var depList = ofFile.file.dependencies;
            for (int i = 0; i < depList.dependencyCount; i++)
            {
                AssetsFileDependency dep = depList.dependencies[i];
                int index = files.FindIndex(f => Path.GetFileName(dep.assetPath.ToLower()) == Path.GetFileName(f.path.ToLower()));
                if (index != -1)
                {
                    ofFile.dependencies[i] = files[index];
                }
            }
        }
        public void UpdateDependencies()
        {
            foreach (AssetsFileInstance file in files)
            {
                UpdateDependencies(file);
            }
        }

        public void LoadDependencies(AssetsFileInstance ofFile, string path)
        {
            for (int i = 0; i < ofFile.dependencies.Count; i++)
            {
                string depPath = ofFile.file.dependencies.dependencies[i].assetPath;

                if (depPath == string.Empty)
                {
                    continue;
                }

                if (files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower()) == -1)
                {
                    string absPath = Path.Combine(path, depPath);
                    string localAbsPath = Path.Combine(path, Path.GetFileName(depPath));
                    if (File.Exists(absPath))
                    {
                        LoadAssetsFile(File.OpenRead(absPath), true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        LoadAssetsFile(File.OpenRead(localAbsPath), true);
                    }
                }
            }
        }

        public void LoadBundleDependencies(AssetsFileInstance ofFile, BundleFileInstance ofBundle, string path)
        {
            for (int i = 0; i < ofFile.dependencies.Count; i++)
            {
                string depPath = ofFile.file.dependencies.dependencies[i].assetPath;
                if (files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(depPath).ToLower()) == -1)
                {
                    string bunPath = Path.GetFileName(depPath);
                    int bunIndex = Array.FindIndex(ofBundle.file.bundleInf6.dirInf, d => Path.GetFileName(d.name) == bunPath);

                    //by default, the directory of an assets file is the bundle's file path (somepath\bundle.unity3d\file.assets)
                    //we back out again to get the directory the bundle is in
                    string noBunPath = Path.Combine(path, "..");
                    string nbAbsPath = Path.Combine(noBunPath, depPath);
                    string nbLocalAbsPath = Path.Combine(noBunPath, Path.GetFileName(depPath));

                    //if the user chose to set the path to the directory the bundle is in,
                    //we need to check for that as well
                    string absPath = Path.Combine(path, depPath);
                    string localAbsPath = Path.Combine(path, Path.GetFileName(depPath));

                    if (bunIndex != -1)
                    {
                        LoadAssetsFileFromBundle(ofBundle, bunIndex, true);
                    }
                    else if (File.Exists(absPath))
                    {
                        LoadAssetsFile(File.OpenRead(absPath), true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        LoadAssetsFile(File.OpenRead(localAbsPath), true);
                    }
                    else if (File.Exists(nbAbsPath))
                    {
                        LoadAssetsFile(File.OpenRead(nbAbsPath), true);
                    }
                    else if (File.Exists(nbLocalAbsPath))
                    {
                        LoadAssetsFile(File.OpenRead(nbLocalAbsPath), true);
                    }
                }
            }
        }
        #endregion

        #region asset resolving
        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, int fileId, long pathId, bool onlyGetInfo = false, bool forceFromCldb = false)
        {
            AssetExternal ext = new AssetExternal
            {
                info = null,
                instance = null,
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
                ext.info = dep.table.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.instance = GetTypeInstance(dep.file, ext.info, forceFromCldb);
                else
                    ext.instance = null;

                return ext;
            }
            else
            {
                ext.file = relativeTo;
                ext.info = relativeTo.table.GetAssetInfo(pathId);

                if (ext.info == null)
                    return ext;

                if (!onlyGetInfo)
                    ext.instance = GetTypeInstance(relativeTo.file, ext.info, forceFromCldb);
                else
                    ext.instance = null;

                return ext;
            }
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

        [Obsolete("Renamed to GetTypeInstance")]
        public AssetTypeInstance GetATI(AssetsFile file, AssetFileInfoEx info, bool forceFromCldb = false)
        {
            return GetTypeInstance(file, info, forceFromCldb);
        }
        #endregion

        #region deserialization
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
                    baseField.From0D(AssetHelper.FindTypeTreeTypeByID(file.typeTree, fixedId, scriptIndex), 0);
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
            AssetsFile file = inst.file;
            ushort scriptIndex = AssetHelper.GetScriptIndex(file, info);
            if (scriptIndex == 0xFFFF)
                return null;

            string scriptName;
            if (!inst.monoIdToName.ContainsKey(scriptIndex))
            {
                AssetTypeInstance scriptAti = GetExtAsset(inst, GetTypeInstance(inst.file, info).GetBaseField().Get("m_Script")).instance;

                //couldn't find asset
                if (scriptAti == null)
                    return null;

                scriptName = scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
                string scriptNamespace = scriptAti.GetBaseField().Get("m_Namespace").GetValue().AsString();
                string assemblyName = scriptAti.GetBaseField().Get("m_AssemblyName").GetValue().AsString();

                if (scriptNamespace == string.Empty)
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
        #endregion

        #region class database
        public ClassDatabaseFile LoadClassDatabase(Stream stream)
        {
            classFile = new ClassDatabaseFile();
            classFile.Read(new AssetsFileReader(stream));
            return classFile;
        }

        public ClassDatabaseFile LoadClassDatabase(string path)
        {
            return LoadClassDatabase(File.OpenRead(path));
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
                for (int i = 0; i < classPackage.files.Count; i++)
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
            return LoadClassPackage(File.OpenRead(path));
        }
        #endregion
    }

    public struct AssetExternal
    {
        public AssetFileInfoEx info;
        public AssetTypeInstance instance;
        public AssetsFileInstance file;
    }
}
