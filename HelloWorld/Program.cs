using AssetsTools.NET;
using AssetsTools.NET.Extra;

string Reverse(string s)
{
    char[] charArray = s.ToCharArray();
    Array.Reverse(charArray);
    return new string(charArray);
}

void Load()
{
    var assetsManager = new AssetsManager();
    assetsManager.LoadClassPackage("classdata.tpk");

    var afileInst = assetsManager.LoadAssetsFile(args[0], false);
    var afile = afileInst.file;

    assetsManager.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);

    var replacers = new List<AssetsReplacer>();
    foreach (var goInfo in afile.GetAssetsOfType(AssetClassID.GameObject))
    {
        var goBase = assetsManager.GetBaseField(afileInst, goInfo);
        var name = goBase["m_Name"].AsString;
        var revName = Reverse(name);
        Console.WriteLine($"{name} -> {revName}");
        name = revName;
        goBase["m_Name"].AsString = name;
        replacers.Add(new AssetsReplacerFromMemory(afile, goInfo, goBase));
    }

    var writer = new AssetsFileWriter(args[0] + ".mod");
    afile.Write(writer, 0, replacers);
    writer.Close();
}

if (args.Length < 1)
{
    Console.WriteLine("need a file argument");
    return;
}

Load();
