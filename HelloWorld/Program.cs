using AssetsTools.NET;
using AssetsTools.NET.Extra;

void Load()
{
    var assetsManager = new AssetsManager();
    assetsManager.LoadClassPackage("classdata.tpk");

    var afileInst = assetsManager.LoadAssetsFile(args[0], false);
    var afile = afileInst.file;

    assetsManager.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);

    foreach (var goInfo in afile.GetAssetsOfType(AssetClassID.GameObject))
    {
        var goBase = assetsManager.GetBaseField(afile, goInfo);
        var name = goBase["m_Name"].AsString;
        Console.WriteLine(name);
    }
}

if (args.Length < 1)
{
    Console.WriteLine("need a file argument");
    return;
}

Load();
