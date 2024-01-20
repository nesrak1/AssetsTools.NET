using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public bool UseTemplateFieldCache { get; set; } = false;
        public bool UseMonoTemplateFieldCache { get; set; } = false;
        public bool UseRefTypeManagerCache { get; set; } = false;
        public bool UseQuickLookup { get; set; } = false;

        public ClassDatabaseFile ClassDatabase { get; private set; }
        public ClassPackageFile ClassPackage { get; private set; }

        public List<AssetsFileInstance> Files { get; private set; } = new List<AssetsFileInstance>();
        public Dictionary<string, AssetsFileInstance> FileLookup { get; private set; } = new Dictionary<string, AssetsFileInstance>();

        public List<BundleFileInstance> Bundles { get; private set; } = new List<BundleFileInstance>();
        public Dictionary<string, BundleFileInstance> BundleLookup { get; private set; } = new Dictionary<string, BundleFileInstance>();

        public IMonoBehaviourTemplateGenerator MonoTempGenerator { get; set; } = null;

        private readonly ConcurrentDictionary<int, AssetTypeTemplateField> templateFieldCache = new ConcurrentDictionary<int, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = new ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>> monoTypeTreeTemplateFieldCache = new ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>> monoCldbTemplateFieldCache = new ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AssetsFileInstance, RefTypeManager> refTypeManagerCache = new ConcurrentDictionary<AssetsFileInstance, RefTypeManager>();

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
