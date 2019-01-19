using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public class AssetsManager
    {
        public bool updateAfterLoad = true;
        public ClassDatabaseFile classFile;
        public List<AssetsFileInstance> files = new List<AssetsFileInstance>();

        public AssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps, string root = "")
        {
            AssetsFileInstance instance;
            int index = files.FindIndex(f => f.path == Path.GetFullPath(path));
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
            int index = files.FindIndex(f => f.path == Path.GetFullPath(stream.Name));
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
        public AssetsFileInstance LoadAssetsFile(string path, bool loadDeps, string root = "")
        {
            return LoadAssetsFile(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), loadDeps, root);
        }

        //linq this somehow
        private void UpdateDependency(AssetsFileInstance ofFile)
        {
            for (int i = 0; i < files.Count; i++)
            {
                AssetsFileInstance file = files[i];
                for (int j = 0; j < file.file.dependencies.dependencyCount; j++)
                {
                    AssetsFileDependency dep = file.file.dependencies.pDependencies[j];
                    if (dep.assetPath == ofFile.name)
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
                        AssetsFileDependency dep = file.file.dependencies.pDependencies[j];
                        if (dep.assetPath == ofFile.name)
                        {
                            file.dependencies[j] = ofFile;
                        }
                    }
                }
            }
        }

        private void LoadDeps(AssetsFileInstance ofFile, string path)
        {
            for (int i = 0; i < ofFile.dependencies.Count; i++)
            {
                string depPath = ofFile.file.dependencies.pDependencies[i].assetPath;
                if (files.FindIndex(f => Path.GetFileName(f.path) == Path.GetFileName(depPath)) == -1)
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

        public AssetExternal GetExtAsset(AssetsFileInstance relativeTo, AssetTypeValueField atvf, bool onlyGetInfo = false)
        {
            AssetExternal ext = new AssetExternal();
            uint fileId = (uint)atvf.Get("m_FileID").GetValue().AsInt();
            ulong pathId = (ulong)atvf.Get("m_PathID").GetValue().AsInt64();
            if (fileId == 0 && pathId == 0)
            {
                ext.info = null;
                ext.instance = null;
            }
            else if (atvf.Get("m_FileID").GetValue().AsInt() != 0)
            {
                AssetsFileInstance dep = relativeTo.dependencies[(int)fileId - 1];
                ext.info = dep.table.getAssetInfo(pathId);
                if (!onlyGetInfo)
                    ext.instance = GetATI(dep.file, ext.info);
                else
                    ext.instance = null;
            }
            else
            {
                ext.info = relativeTo.table.getAssetInfo(pathId);
                if (!onlyGetInfo)
                    ext.instance = GetATI(relativeTo.file, ext.info);
                else
                    ext.instance = null;
            }
            return ext;
        }

        public AssetTypeInstance GetATI(AssetsFile file, AssetFileInfoEx info)
        {
            //do we still need pooling?
            //(it accumulates memory over time and we don't
            // really need to read the same ati twice)
            AssetTypeTemplateField pBaseField = new AssetTypeTemplateField();
            pBaseField.FromClassDatabase(classFile, AssetHelper.FindAssetClassByID(classFile, info.curFileType), 0);
            return new AssetTypeInstance(pBaseField, file.reader, false, info.absoluteFilePos);
        }

        public ClassDatabaseFile LoadClassPackage(Stream stream)
        {
            classFile = new ClassDatabaseFile();
            classFile.Read(new AssetsFileReader(stream));
            return classFile;
        }
        public ClassDatabaseFile LoadClassPackage(string path)
        {
            return LoadClassPackage(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public void LoadClassDatabase()
        {
            throw new NotImplementedException("Please extract pacakges from databases in UABE before loading");
        }

        public struct AssetExternal
        {
            public AssetFileInfoEx info;
            public AssetTypeInstance instance;
        }
    }
}
