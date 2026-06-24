using System.Collections.Generic;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        /// <summary>
        /// Cache template fields, including MonoBehaviours generated from class databases
        /// </summary>
        public bool UseTemplateFieldCache { get; set; } = false;
        /// <summary>
        /// Cache MonoBehaviour template fields from type trees and generated mono temp generators
        /// </summary>
        public bool UseMonoTemplateFieldCache { get; set; } = false;
        /// <summary>
        /// Cache managed reference type template fields
        /// </summary>
        public bool UseRefTypeManagerCache { get; set; } = false;
        /// <summary>
        /// Use a dictionary to look up asset infos rather than a simple sequential search
        /// </summary>
        public bool UseQuickLookup { get; set; } = false;

        public ClassDatabaseFile ClassDatabase { get; private set; }
        public ClassPackageFile ClassPackage { get; private set; }

        public List<AssetsFileInstance> Files { get; private set; } = new List<AssetsFileInstance>();
        public Dictionary<string, AssetsFileInstance> FileLookup { get; private set; } = new Dictionary<string, AssetsFileInstance>();

        public List<BundleFileInstance> Bundles { get; private set; } = new List<BundleFileInstance>();
        public Dictionary<string, BundleFileInstance> BundleLookup { get; private set; } = new Dictionary<string, BundleFileInstance>();

        private IMonoBehaviourTemplateGenerator monoTempGenerator = null;
        public IMonoBehaviourTemplateGenerator MonoTempGenerator
        {
            get
            {
                return monoTempGenerator;
            }
            set
            {
                monoTempGenerator = value;

                // refmans aren't connected to the manager so update all of their temp gens manually
                foreach (var kvp in refTypeManagerCache)
                {
                    AssetsFileInstance fileInst = kvp.Key;
                    RefTypeManager refMan = kvp.Value;
                    refMan.WithMonoTemplateGenerator(fileInst.file.Metadata, monoTempGenerator, UseMonoTemplateFieldCache ? monoTemplateFieldCache : null);
                }
            }
        }

        private readonly ConcurrentDictionary<int, AssetTypeTemplateField> templateFieldCache = new ConcurrentDictionary<int, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = new ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>> monoTypeTreeTemplateFieldCache = new ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>> monoCldbTemplateFieldCache = new ConcurrentDictionary<AssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AssetsFileInstance, RefTypeManager> refTypeManagerCache = new ConcurrentDictionary<AssetsFileInstance, RefTypeManager>();

        // will be problematic if we try to load two in the same place.
        // probably need to refactor the file key -> object code across the whole manager.
        private readonly ConcurrentDictionary<Hash128, TypeTreeBlob> typeTreeBlobs = new ConcurrentDictionary<Hash128, TypeTreeBlob>();
        private readonly ConcurrentDictionary<BundleFileInstance, HashSet<Hash128>> typeTreeBlobOwners = new ConcurrentDictionary<BundleFileInstance, HashSet<Hash128>>();

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
