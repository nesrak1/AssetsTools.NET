# AssetsTools.NET
A c# rewrite of the c++ AssetsTools [https://github.com/DerPopo/UABE/](https://github.com/DerPopo/UABE/)

# What it does
The library allows reading assets files from Unity games for tools and mods.

It **does not** extract assets into usable forms (fbx, png, etc.) unless you write exporters to do so.

# How to use it
To read the type data, you'll need a class database file which you can export from UABE (see "extracting database"). However, if you do not need to read type data, you do not need the class database.

AssetsTools.NET comes with various helper classes, with the AssetsManager being the most useful. You can load multiple assets and follow their dependencies easily using this class, however it is not required.

You can load the file using the following:

```cs
var am = new AssetsManager(); 
var instance = am.LoadAssetsFile(assetsPath, true); //true will load dependencies
```

You can load the class database with:

```cs
var classFile = am.LoadClassPackage(classPath);
```

With the instance, you get access to the AssetsFile and AssetsFileTable instances. The AssetsFile class contains metadata about the file (typetree, dependencies) and the table contains asset listings.

To get an asset's info, you can find its info by id (`getAssetInfo(id)`) or by index (`pAssetFileInfo[index]`) then use AssetsManager to get the base field:

```cs
var firstInfo = assetsTable.getAssetInfo(5);
var baseField = am.GetATI(instance.file, firstInfo).GetBaseField();
```

From here, you can navigate the tree by using `.Get(name/index)` and getting values through `.GetValue().AsXXX()` (You can see this from UABE's View Data button)

```cs
var m_Name = baseField.Get("m_Name").GetValue().AsString();
```

If you find a PPtr (reference to another asset) you can get the ATI automatically:

```cs
var componentArray = baseField.Get("m_Component").Get("Array");
var transformRef = componentArray[0].Get("component");
var transform = am.GetExtAsset(instance, transformRef);
var transformBaseField = transform.instance.GetBaseField();
```

If you need to load a MonoBehaviour and have access to Assembly-CSharp.dll, you can load its type data too:

```cs
//The fourth parameter points toward the folder where Assembly-CSharp.dll is contained
var baseField = MonoClass.GetMonoBaseField(am, instance, secondInfo, Path.GetDirectoryName(instance.path));
```

Make sure to load dependencies in `am.LoadAssetsFile()` or manually load the assets file with the script asset or this won't work.

There's much more the library has to offer and it follows closely with the original library. If you have a question about it, feel free to create an issue and ask it. You can also check the original [UABE github repo](https://github.com/DerPopo/UABE/) for questions already asked by others.

# Features that still need to be added

## AssetsTools.NET
- [ ] API function for decompressing assets
- [ ] Loading tpk files
- [ ] Loading compressed databases
- [ ] Older asset writing support (I don't need it, but if you do, make an issue or pull request)

## UABE.NET
- [ ] Rework using new AssetManager (opening assets in assets)
- [ ] Actual working search support
- [ ] Faster loading speeds
- [ ] Bundle creation
- [ ] Asset saving

(I don't use UABE.NET all that much anymore other than the MonoBehaviour searcher so I probably won't be keeping up with it after these things are fixed.)

Feel free to help add any of the following, hopefully I can get to it all at some point.

# Extracting Database
In UABE, go to `Options > Edit Type Package`. Open classdata.tpk in the same folder as UABE. Find the Unity version you need and click "Export". Then close the window and go to `Options > Edit Type Database`. Open the file you exported, uncheck `Compress the file (LZMA)`, click OK, then OK again.

# Comments
Older comments that I wrote were written with `//-` while UABE's original comments were written in `//`. Commented out functions that still need to be implemented were written with `///Original AssetsTools function name`. Most comments I write now (mostly in methods) will now just be `//`.