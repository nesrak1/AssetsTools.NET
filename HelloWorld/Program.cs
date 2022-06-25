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
    assetsManager.SetMonoTempGenerator(new MonoCecilTempGenerator("Managed"));

    var afile = assetsManager.LoadAssetsFile(args[0], false);

    assetsManager.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);

    var replacers = new List<AssetsReplacer>();
    foreach (var monoInfo in afile.GetAssetsOfType(AssetClassID.MonoBehaviour))
    {
        var monoBase = assetsManager.GetBaseField(afile, monoInfo);
        var monoScriptBase = assetsManager.GetExternal(afile, monoBase["m_Script"]).baseField;
        var scriptName = monoScriptBase["m_ClassName"].AsString;
        Console.WriteLine($"root fields of {scriptName}:");
        foreach (var child in monoBase.Children)
        {
            var quote = child.Value.ValueType == AssetValueType.String ? "\"" : "";

            if (child.Value.ValueType != AssetValueType.None && !child.TemplateField.IsArray)
                Console.WriteLine($"  {child.TypeName} {child.FieldName} = {quote}{child.AsString}{quote}");
            else
                Console.WriteLine($"  {child.TypeName} {child.FieldName}");
        }
    }
}

if (args.Length < 1)
{
    Console.WriteLine("need a file argument");
    return;
}

Load();
