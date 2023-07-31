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

    assetsManager.MonoTempGenerator = new MonoCecilTempGenerator(Path.Combine(Path.GetDirectoryName(afileInst.path), "Managed"));

    var bf = assetsManager.GetBaseField(afileInst, afile.GetAssetInfo(7306));
    //PrintTemplate(bf);
    Console.Write("");
}

//void PrintTemplate(AssetTypeTemplateField field, int depth = 0)
//{
//    var depthStr = new string(' ', depth * 2);
//    Console.WriteLine($"{depthStr}{field.Type} {field.Name}");
//    foreach (var child in field.Children)
//    {
//        PrintTemplate(child, depth + 1);
//    }
//}

if (args.Length < 1)
{
    Console.WriteLine("need a file argument");
    return;
}

Load();
