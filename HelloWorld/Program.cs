using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Diagnostics;

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

    var sw = new Stopwatch();
    sw.Start();
    foreach (var goInfo in afile.GetAssetsOfType(AssetClassID.GameObject))
    {
        var goBase = assetsManager.GetBaseField(afileInst, goInfo);

        var name = goBase["m_Name"].AsString;
        var revName = Reverse(name + "abcdef");
        goBase["m_Name"].AsString = revName;
        goInfo.SetNewData(goBase);

        //Console.WriteLine($"{name} -> {revName}");
    }
    sw.Stop();
    Console.WriteLine($"first loop time was {sw.ElapsedMilliseconds} ms");

    sw = new Stopwatch();
    sw.Start();
    foreach (var goInfo in afile.GetAssetsOfType(AssetClassID.GameObject))
    {
        var goBase = assetsManager.GetBaseField(afileInst, goInfo);
    
        var name = goBase["m_Name"].AsString;
        var revName = Reverse(name) + "hijklm";
        goBase["m_Name"].AsString = revName;
        goInfo.SetNewData(goBase);
    }
    sw.Stop();
    Console.WriteLine($"second loop time was {sw.ElapsedMilliseconds} ms");

    var textAssetClassId = (int)AssetClassID.TextAsset;
    var textAssetBf = assetsManager.CreateValueBaseField(afileInst, textAssetClassId);
    textAssetBf["m_Name"].AsString = "Some name";
    textAssetBf["m_Script"].AsString = "Some text";
    var textAssetInfo = AssetFileInfo.Create(afile, 1234567, textAssetClassId, assetsManager.ClassDatabase);
    textAssetInfo.SetNewData(textAssetBf);
    afile.AssetInfos.Add(textAssetInfo);
    
    Console.WriteLine("the script I just set:");
    Console.WriteLine(assetsManager.GetBaseField(afileInst, 1234567)["m_Script"].AsString);

    var writer = new AssetsFileWriter(args[0] + ".mod");
    afile.Write(writer);
    writer.Close();
}

if (args.Length < 1)
{
    Console.WriteLine("need a file argument");
    return;
}

Load();
