# AssetsTools.NET v2

A .net library for reading and modifying unity assets and bundles based off of the AssetsTools library from [UABE](https://github.com/DerPopo/UABE/).

Jump to a tool:

[![AssetsTools](https://user-images.githubusercontent.com/12544505/73600757-7c97a280-451a-11ea-934b-afd392cc2bcc.png)](#assetstools)
[![AssetsView](https://user-images.githubusercontent.com/12544505/73600640-e57e1b00-4518-11ea-8aab-e8664947f435.png)](#assetsview)

# AssetsTools

[![Nuget](https://img.shields.io/nuget/v/AssetsTools.NET?style=flat-square)](https://www.nuget.org/packages/AssetsTools.NET)
[![Prereleases](https://img.shields.io/github/v/release/nesrak1/AssetsTools.NET?include_prereleases&style=flat-square)](https://github.com/nesrak1/AssetsTools.NET/releases)

## Terminology

Programs/Libraries (since they all have similar names)

* UABE - The original UABE program by DerPopo
* UABE.NET - UABE._NET_ is the program in this repo, otherwise, it's the original
* AssetsTools - The original AssetsTools library by DerPopo
* AssetsTools.NET - This asset viewing/modifying library in this repo
* AssetsView/AssetsView.NET - The asset viewer program in this repo

Files (both assetstools and unity)

* cldb/Class Database - Stores info about how to deserialize assets
* cltpk/tpk/Class Type Package - Stores multiple class databases
* asset - Any data of an asset, not to be confused with "assets file"
* .assets/assets file - Stores multiple assets and optionally a type tree
* .unity3d/bundle file - Stores multiple .assets files
* ggm/globalgamemanagers - Metadata for unity games
* resources.assets - The file where assets built in the Resources folder go
* level file - Gameobjects and components go here
* sharedassets file - Gameobjects shared across scenes and other non-scene specific assets like materials go here

## Basic usage of AssetsTools.NET

AssetsTools is separated into two parts, `Standard` and `Extra`. Standard classes come with UABE's AssetsTools. `Extra` classes are unique to AssetsTools.NET. The two most important classes in `Extra` are `AssetsManager` and `MonoClass`. Other than that, the difference between AssetsTools and AssetsTools.NET is not much different.

It is recommended to use `AssetsManager` for most cases unless you only need to read/write one assets file and it's a simple task.

### Assets file reading

To load an assets file, you can use `LoadAssetsFile(string assetPath, bool loadDependencies)` to get an `AssetsFileInstance`:

```cs
var am = new AssetsManager(); 
var inst = am.LoadAssetsFile("resources.assets", true);
```

See the bottom for how to load asset bundles (usually `.unity3d`)

An `AssetsFileInstance` holds the `AssetsFile` and `AssetsFileTable` instances. The `AssetsFile` contains information about the version of the file, the `TypeTree` which may contain serialization data and other info on decoding assets, and the dependencies that the file needs (it may be easier to look in `AssetsFileInstance.dependencies` though). The `AssetsFileTable` is a table of information about assets like their path id, type id, and pointer in data.

To get the info of an asset, if you know the path id, you can use `GetAssetInfo(long pathId)` to get an `AssetFileInfoEx`:

```cs
var table = inst.table;
var inf = table.GetAssetInfo(1);
```

If you know the name you can use `GetAssetInfo(string name[, uint typeId])`:

```cs
//if you know there is only one asset by the name RocketShip, you don't need to search by type
var inf1 = table.GetAssetInfo("RocketShip");
//if there are multiple assets by the same name, you can use type to narrow it down
var inf2 = table.GetAssetInfo("RocketShip", 0x01); //0x01 - GameObject type id
```

Otherwise, if you just want to loop through all assets or all assets of a specific type, you can use `assetFileInfo` or the extension method `GetAssetsOfType(int typeId)`:

```cs
foreach (var inf in table.assetFileInfo)
{
    Console.WriteLine($"{inf.index} {inf.absoluteFilePos}");
}
foreach (var inf in table.GetAssetsOfType(0x01))
{
    Console.WriteLine($"{inf.index} {inf.absoluteFilePos}");
}
```

### Serialized data loading

Once you have the info for an asset, you can start to get the serialized data of it. For the library to understand what how to deserialize the fields, it needs a class database or a type tree. For assets files in built games, there isn't a type tree, so a class database is needed. UABE has class databases (dat files) stored in the class package file (classdata.tpk). If you are targeting multiple unity versions, you can use `AssetsManager`'s `LoadClassDatabaseFromPackage`:

```cs
am.LoadClassPackage("classdata.tpk");
am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);
```

If you only want to load one class, you can use `Options -> Edit Type Package` in UABE's main window, click a version, and click export.

###### NOTE: You may want to open the exported file in a hex editor to make sure the version is what you want it to be. UABE's classdata.tpk file can have some versions listed as the wrong version (for example, exporting a `U2019.1.0f2` database and the file actually being a `2018.4.5f1` database.)

```cs
am.LoadClassDatabase("2018.3.0f2.dat");
```

With a loaded class database, you can finally use `GetATI(AssetsFile file, AssetFileInfoEx info[, fromTypeTree])`:

```cs
var inf = table.GetAssetInfo("Pineapple");
//AssetTypeInstance isn't too useful, so we go directly into the base field
var baseField = am.GetATI(inst.file, inf).GetBaseField();
```

The base field is the first field of a serialized asset. From there, you can use `Get(string name)`, `Get(int index)`, or `[int index]` to get child fields. You can use `GetValue()` to get the value of the field and `AsXXX()` to convert it to a .net type.

```cs
//example for a GameObject
var m_Name = baseField.Get("m_Name")
                      .GetValue()
                      .AsString();
Console.WriteLine("gameobject's name is " + m_Name);
```

The AssetTypeInstance only has one basefield, the field we opened. To view the data of another asset, you can use `GetExtAsset(AssetsFileInstance relativeTo, AssetTypeValueField atvf[, bool onlyGetInfo])`

```cs
//example for a GameObject
var componentArray = baseField.Get("m_Component").Get("Array");
//get first component in gameobject, which is always transform
var transformRef = componentArray[0].Get("component");
var transform = am.GetExtAsset(instance, transformRef);
var transformBf = transform.instance.GetBaseField();
```

Set `onlyGetInfo` if you only want the asset info without reading the serialized data. You may want to do this if you want to only read a specific type which is much faster than reading all of the types you don't need to read.

### MonoBehaviour loading

Reading MonoBehaviours are a little different because the information for deserialization is stored in assemblies in the Managed folder, rather than the type tree or the class database file.

```cs
//example for a GameObject
var componentArray = baseField.Get("m_Component").Get("Array");
var startMenuRef = componentArray[1].Get("component");
var startMenu = am.GetExtAsset(instance, transformRef);
var managedFolderPath = Path.Combine(Path.GetDirectoryName(inst.path), "Managed");
var startMenuBf = MonoDeserializer.GetMonoBaseField(am, inst, startMenu.info, managedFolderPath);
```

You can also use `AssetsManager.GetMonoBaseFieldCached` to cache types that take long to load, however, with the speed improvements `MonoDeserializer` has now, it may not be needed.

### Assets file writing

To modify a assets file, edit the values with `Set(object value)`, get the bytes with `WriteToByteArray()`, create an `AssetsReplacer`, and call `Write(AssetsFileWriter writer, AssetsReplacer[] replacers)` on the `AssetsFile`:

```cs
//example for a GameObject
var am = new AssetsManager();
var inst = am.LoadAssetsFile("resources.assets", true);
var inf = inst.table.GetAssetInfo("MyBoringAsset");
var baseField = am.GetATI(inst.file, inf).GetBaseField();
baseField.Get("m_Name")
         .GetValue()
         .Set("MyCoolAsset");
var newGoBytes = baseField.WriteToByteArray();
//AssetsReplacerFromMemory's monoScriptIndex should always be 0xFFFF unless it's a MonoBehaviour
var repl = new AssetsReplacerFromMemory(0, inf.index, inf.curFileType, 0xFFFF, newGoBytes);
var writer = new AssetsFileWriter(File.OpenWrite("resources-modified.assets"));
inst.file.Write(writer, new AssetsReplacer[] { repl });
```

Once you write changes to a file, you will need to reopen the file to see the changes.

### Info on type trees

The type tree is essentially like a class database but inside of the assets file itself. They are usually found in editor assets files and bundle assets files. You can check if there is a real type tree with `file.typeTree.hasTypeTree`. AssetsManager will try to read from the type tree if one exists, but if you still want to override this for whatever reason, the `forceFromCldb` in methods such as `GetExtAsset` can be used to do so.

### Loading bundle files

Bundles are files that can hold multiple assets files. Sometimes they only hold one, but usually the assets file inside has a real type tree rather than just the list of types most assets files have. Bundles can be read with the bundle loader in `AssetsManager`.

```cs
var am = new AssetsManager();
var bun = am.LoadBundleFile("bundle.unity3d");
var firstAssetsFile = BundleHelper.LoadAssetFromBundle(bun, 0); //or use name instead
//...
```

If you need to load binary entries such as .resS files in bundles, you can use `BundleHelper.LoadAssetDataFromBundle` to get a byte array.

### Loading textures

Texture2Ds can contain data in many different kinds of compression types. AssetsTools.NET is meant to be portable and doesn't rely on any native libraries or use any unsafe code. As a result, the Texture2D decoder won't be 100% as fast as the native versions, however, they are fast enough for most tasks (85%-95% of native speed, depending on compression method.) If you know how to, you can always hook up a native library for extra speed using the data from resS or the data byte array.

Supported formats:

* R8
* R16
* RG16
* RGB24
* RGBA32
* ARGB32
* RGBA4444
* ARGB4444
* Alpha8
* DXT1
* DXT5
* BC7

In the future I'll be adding more formats but these should be good for most games.

The output of these are in BGRA which makes it easy to use Format32bppArgb with System.Drawing's bitmaps. Here's a quick and dirty way to implement that:

```cs
var atvf = am.GetATI(inst.file, texInf).GetBaseField();
var tf = TextureFile.ReadTextureFile(atvf);
var texDat = tf.GetTextureData();
if (texDat != null && texDat.Length > 0)
{
    var canvas = new Bitmap(tf.m_Width, tf.m_Height, tf.m_Width * 4, PixelFormat.Format32bppArgb,
        Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
    canvas.RotateFlip(RotateFlipType.RotateNoneFlipY);
    canvas.Save("out.png");
}
```

Note that the original AssetsTools uses RGBA output instead of BGRA output. In the future I'll probably add a flag to support choosing which order the output is in.

## Hmms 🤔

### Why does AssetsView.NET or my program with AssetsTools.NET crash when reading a specific type

Most likely a minor version update changed a field or two and broke the reader. You can check if that's the issue by attempting to open the asset in UABE. If it shows up blank or shows an error message, that's probably the issue. There's not much you can do unless you can generate a new cldb somehow or reading the asset manually with the `AssetsFileReader`.

### Does AssetsTools.NET work for versions below Unity 5.5

Unity versions 5.0-5.4 are not supported right now, unless you can help out and add it.

### Does the library have a way to extract assets

Extracting assets into non-serialized formats (like `obj`s, `wav`s, etc.) is not supported by the library. I have no plan to write any extractors for them as they are not part of the original library, and as mentioned, there are already other tools that can do that.

### Do I need Mono.Cecil

Only if you're using MonoDeserializer.

### Some other issue or need help

Create a github issue and I will try to get back to you when I can.

# AssetsView

AssetsView is a viewer for assets files. Rather than being target toward extracting assets, AssetsView can view the raw data of assets. It improves on UABE by being easier to navigate with gameobject tree views and much more.

![AssetsView](https://user-images.githubusercontent.com/12544505/73774729-1f823380-474a-11ea-8e14-ce89691e63df.png)

## Hmms 🤔

### "Can't display monobehaviour data until dependencies are loaded"?

Run File->Update Dependencies. This check is just there to make sure that the script that the MonoBehaviour needs can be loaded. Eventually, you would only need to load the file that has the script file. For bundles that have type trees (most likely), you can safely ignore this message for now.

### Follow Reference button in GameObject Viewer?

For PPtrs, select either the m_FileID or m_PathID fields and click `Follow Reference` to go to that asset.

### Red text in GameObject Viewer?

Disabled GameObject.