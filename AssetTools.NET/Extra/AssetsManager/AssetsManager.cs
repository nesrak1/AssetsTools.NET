using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public bool updateAfterLoad = true;
        public bool useTemplateFieldCache = false;
        public ClassDatabaseFile classDatabase;
        public ClassPackageFile classPackage;   
        public List<AssetsFileInstance> files = new List<AssetsFileInstance>();
        public List<BundleFileInstance> bundles = new List<BundleFileInstance>();
        private IMonoBehaviourTemplateGenerator monoTempGenerator = null;
        private readonly Dictionary<int, AssetTypeTemplateField> templateFieldCache = new Dictionary<int, AssetTypeTemplateField>();
        private readonly Dictionary<string, AssetTypeTemplateField> monoTemplateFieldCache = new Dictionary<string, AssetTypeTemplateField>();

        public void UnloadAll(bool unloadClassData = false)
        {
            UnloadAllAssetsFiles(true);
            UnloadAllBundleFiles();
            if (unloadClassData)
            {
                classPackage = null;
                classDatabase = null;
            }
        }
    }

    public struct AssetExternal
    {
        public AssetFileInfo info;
        public AssetTypeValueField baseField;
        public AssetsFileInstance file;
    }
}
