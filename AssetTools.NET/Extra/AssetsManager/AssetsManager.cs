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
        public bool UseTemplateFieldCache { get; set; } = false;
        public bool UseMonoTemplateFieldCache { get; set; } = false;
        public bool UseRefTypeManagerCache { get; set; } = false;

        public ClassDatabaseFile ClassDatabase { get; private set; }
        public ClassPackageFile ClassPackage { get; private set; }

        public List<AssetsFileInstance> Files { get; private set; } = new List<AssetsFileInstance>();
        public Dictionary<string, AssetsFileInstance> FileLookup { get; private set; } = new Dictionary<string, AssetsFileInstance>();

        public List<BundleFileInstance> Bundles { get; private set; } = new List<BundleFileInstance>();
        public Dictionary<string, BundleFileInstance> BundleLookup { get; private set; } = new Dictionary<string, BundleFileInstance>();

        public IMonoBehaviourTemplateGenerator MonoTempGenerator { get; set; } = null;

        private readonly Dictionary<int, AssetTypeTemplateField> templateFieldCache = new Dictionary<int, AssetTypeTemplateField>();
        private readonly Dictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
        private readonly Dictionary<AssetsFileInstance, Dictionary<ushort, AssetTypeTemplateField>> monoTypeTreeTemplateFieldCache = new Dictionary<AssetsFileInstance, Dictionary<ushort, AssetTypeTemplateField>>();
        private readonly Dictionary<AssetsFileInstance, Dictionary<long, AssetTypeTemplateField>> monoCldbTemplateFieldCache = new Dictionary<AssetsFileInstance, Dictionary<long, AssetTypeTemplateField>>();
        private readonly Dictionary<AssetsFileInstance, RefTypeManager> refTypeManagerCache = new Dictionary<AssetsFileInstance, RefTypeManager>();

        public void UnloadAll(bool unloadClassData = false)
        {
            UnloadAllAssetsFiles(true);
            UnloadAllBundleFiles();
            MonoTempGenerator?.Dispose();
            if (unloadClassData)
            {
                ClassPackage = null;
                ClassDatabase = null;
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
