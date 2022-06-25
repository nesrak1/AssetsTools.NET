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
        public bool useTemplateFieldCache = false;
        public ClassDatabaseFile classDatabase;
        public ClassPackageFile classPackage;
        public string activeDirectory = string.Empty; // should be readonly property ?
        public readonly Dictionary<string, AssetsFile> files = new Dictionary<string, AssetsFile>();
        public readonly Dictionary<string, AssetBundleFile> bundles = new Dictionary<string, AssetBundleFile>();
        private IMonoBehaviourTemplateGenerator monoTempGenerator;
        private readonly Dictionary<int, AssetTypeTemplateField> templateFieldCache = new Dictionary<int, AssetTypeTemplateField>();
        private readonly Dictionary<string, AssetTypeTemplateField> monoTemplateFieldCache = new Dictionary<string, AssetTypeTemplateField>();
        private readonly Dictionary<AssetsFile, AssetBundleFile> bundleMap = new Dictionary<AssetsFile, AssetBundleFile>();
        private readonly Dictionary<AssetBundleFile, List<AssetsFile>> bundleLoadedFilesMap = new Dictionary<AssetBundleFile, List<AssetsFile>>();

        public void SetActiveDirectory(string directory)
        {
            activeDirectory = directory;
            if (monoTempGenerator != null)
            {
                monoTempGenerator.SetActiveDirectory(directory);
            }
        }
    }

    public struct AssetExternal
    {
        public AssetFileInfo info;
        public AssetTypeValueField baseField;
        public AssetsFile file;
    }
}
