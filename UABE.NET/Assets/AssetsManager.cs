using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//From treeview, a complicated yet working way to manage large amounts of files/assets better

namespace UABE.NET.Assets
{
    public class AssetsManagerLegacy
    {
        public Stream file;
        public Stream classFile;
        public AssetsFile initialFile;
        public AssetsFileTable initialTable;
        public ClassDatabaseFile initialClassFile;
        public List<Dependency> dependencies;
        public List<AssetTypeTemplateField> attfs;
        public AssetsManagerLegacy()
        {
            attfs = new List<AssetTypeTemplateField>();
            dependencies = new List<Dependency>();
        }
        public void LoadAssets(Stream stream, string directory)
        {
            file = stream;
            initialFile = new AssetsFile(new AssetsFileReader(file));
            initialTable = new AssetsFileTable(initialFile);

            for (int i = 0; i < initialFile.dependencies.dependencyCount; i++)
            {
                Dependency assetDependency;
                string depName = initialFile.dependencies.pDependencies[i].assetPath;
                string path = Path.Combine(directory, depName.Replace("library/", "Resources/"));
                if (File.Exists(path))
                {
                    FileStream depFile = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    AssetsFile depAssetsFile = new AssetsFile(new AssetsFileReader(depFile));
                    AssetsFileTable depAssetsFileTable = new AssetsFileTable(depAssetsFile);
                    assetDependency = new Dependency(depName, depFile, depAssetsFile, depAssetsFileTable);
                    dependencies.Add(assetDependency);
                }
            }
        }
        public void LoadAssets(string path)
        {
            LoadAssets(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), Path.GetDirectoryName(path));
        }
        public void LoadClassFile(Stream stream)
        {
            classFile = stream;
            initialClassFile = new ClassDatabaseFile();
            initialClassFile.Read(new AssetsFileReader(stream));
        }
        public void LoadClassFile(string path)
        {
            LoadClassFile(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        public AssetTypeInstance GetATI(Stream file, AssetFileInfoEx info)
        {
            int index = 0;
            bool foundInList = false;
            ClassDatabaseType type = AssetHelper.FindAssetClassByID(initialClassFile, info.curFileType);
            for (int i = 0; i < attfs.Count; i++)
            {
                if (attfs[i].type == type.name.GetString(initialClassFile))
                {
                    index = i;
                    foundInList = true;
                }
            }
            AssetTypeInstance ati;
            if (foundInList)
            {
                ati = new AssetTypeInstance(1, new[] { attfs[index] }, new AssetsFileReader(file), false, info.absoluteFilePos);
            }
            else
            {
                AssetTypeTemplateField pBaseField = new AssetTypeTemplateField();
                pBaseField.FromClassDatabase(initialClassFile, AssetHelper.FindAssetClassByID(initialClassFile, info.curFileType), 0);
                attfs.Add(pBaseField);
                ati = new AssetTypeInstance(1, new[] { attfs.Last() }, new AssetsFileReader(file), false, info.absoluteFilePos);
            }
            return ati;
        }
        public AssetExternal GetExtAsset(AssetTypeValueField atvf)
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
                Dependency dep = dependencies[(int)fileId - 1];
                ext.info = dep.aft.getAssetInfo(pathId);
                ext.instance = GetATI(dep.file, ext.info);
            } else
            {
                ext.info = initialTable.getAssetInfo(pathId);
                ext.instance = GetATI(file, ext.info);
            }
            return ext;
        }
        public Stream GetStream(uint fileId)
        {
            if (fileId == 0)
            {
                return file;
            } else
            {
                return dependencies[(int)fileId - 1].file;
            }
        }
        public AssetFileInfoEx GetInfo(uint fileId, ulong pathId)
        {
            if (fileId == 0)
            {
                return initialTable.getAssetInfo(pathId);
            } else
            {
                return dependencies[(int)fileId - 1].aft.getAssetInfo(pathId);
            }
        }
        public struct Dependency
        {
            public string name;
            public Stream file;
            public AssetsFile af;
            public AssetsFileTable aft;
            public Dependency(string name, Stream file, AssetsFile af, AssetsFileTable aft)
            {
                this.name = name;
                this.file = file;
                this.af = af;
                this.aft = aft;
            }
        }
        public struct AssetExternal
        {
            public AssetFileInfoEx info;
            public AssetTypeInstance instance;
        }
    }
}
