# AssetsTools.NET v2

A .net library for reading and modifying unity assets and bundles based off of the AssetsTools library from [UABE](https://github.com/DerPopo/UABE/).

Jump to a tool:

[![AssetsTools](https://user-images.githubusercontent.com/12544505/73600757-7c97a280-451a-11ea-934b-afd392cc2bcc.png)](#assetstools)
[![AssetsView](https://user-images.githubusercontent.com/12544505/73600640-e57e1b00-4518-11ea-8aab-e8664947f435.png)](#assetsview)

# AssetsTools

[![Nuget](https://img.shields.io/nuget/v/AssetsTools.NET?style=flat-square)](https://www.nuget.org/packages/AssetsTools.NET) [![Prereleases](https://img.shields.io/github/v/release/nesrak1/AssetsTools.NET?include_prereleases&style=flat-square)](https://github.com/nesrak1/AssetsTools.NET/releases) [![discord](https://img.shields.io/discord/862035581491478558?label=discord&logo=discord&logoColor=FFFFFF&style=flat-square)](https://discord.gg/hd9VdswwZs)

## Table of contents

* [What is this](https://github.com/nesrak1/AssetsTools.NET#what-is-this)
* [Getting started](https://github.com/nesrak1/AssetsTools.NET#getting-started)
* [Read an assets file](https://github.com/nesrak1/AssetsTools.NET#read-an-assets-file)
* [Write an assets file](https://github.com/nesrak1/AssetsTools.NET#write-an-assets-file)
* [Value builder (add assets/fields)](https://github.com/nesrak1/AssetsTools.NET#value-builder-add-assetsfields)
* [Read a bundle file](https://github.com/nesrak1/AssetsTools.NET#read-a-bundle-file)
* [Write a bundle file](https://github.com/nesrak1/AssetsTools.NET#write-a-bundle-file)
* [Compress a bundle file](https://github.com/nesrak1/AssetsTools.NET#compress-a-bundle-file)
* [Bundle creator (create assets/bundle files)](https://github.com/nesrak1/AssetsTools.NET#bundle-creator-create-assetsbundle-files-todo)
* [Reading a MonoBehaviour](https://github.com/nesrak1/AssetsTools.NET#reading-a-monobehaviour)
* [Reading asset paths from resources.assets and bundles](https://github.com/nesrak1/AssetsTools.NET#reading-asset-paths-from-resourcesassets-and-bundles)
* [Exporting a Texture2D](https://github.com/nesrak1/AssetsTools.NET#exporting-a-texture2d)
* [Class database](https://github.com/nesrak1/AssetsTools.NET#class-database)
* [Noooo it's not working!!](https://github.com/nesrak1/AssetsTools.NET#noooo-its-not-working)

### What is this

This is a library for reading and writing unity assets files and bundle files. The original api design and file formats (class package) comes from UABE's C++ AssetsTools library, but it has been rewritten in C# with many added helper functions. Because of that, there are usually two ways to do something.

* The `Extra` namespace contains classes and extension methods that aren't in the original library. For example, `AssetsManager` helps manage loading multiple files and their dependencies and `AssetHelper`/`BundleHelper` contains many useful miscellaneous methods. If there is a function here, you should prefer it over initializing things yourself.
* The `Standard` namespace contains classes and methods that are similar to the original library.

The library is very low-level, and even with helper functions, some features still take a bit of boilerplate. However, you can pretty much edit anything and everything you want. Any asset type is supported, both in assets files and bundles, but making sense of it is up to you. For example, there is no code to "extract sprites", but you can see the fields that store that information.

### Getting started

To help write code, you'll want to open the file you want to read/write in a tool like AssetsView.NET (in this repo) so you can see what assets have what fields.

### Read an assets file

Here's a full example to read all name strings from all `GameObject`s from resources.assets.

```cs
var am = new AssetsManager(); 
var inst = am.LoadAssetsFile("resources.assets", true);

am.LoadClassPackage("classdata.tpk");
am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);

foreach (var inf in inst.table.GetAssetsOfType((int)AssetClassID.GameObject))
{
    var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
    
    var name = baseField.Get("m_Name").GetValue().AsString();
    Console.WriteLine(name);
}

am.UnloadAllAssetsFiles();
```

Below will go into more about what each part does.

#### Load assets file

To load an assets file, use the `LoadAssetsFile` method with an `AssetsManager`.

```cs
var am = new AssetsManager(); 
var inst = am.LoadAssetsFile("resources.assets", true); //true = load dependencies
```

`LoadAssetsFile` returns an `AssetsFileInstance` which is a wrapper for two classes.

* `AssetsFile` contains file version info, the type tree for storing info about asset types, and a list of dependencies. Most of the time, you should just use the dependencies list in `AssetsFileInstance`.
* `AssetsFileTable` which is a list of `AssetFileInfoEx`s which contain pointers to the raw data in file and more.

You can think of `Standard` classes like `AssetsFile` and `AssetsFileTable` as structs directly representing the file format and `AssetsManager` classes like `AssetsFileInstance` as classes that help linking files together.

#### Load class database

Before going any further, type information needs to be loaded. This only applies to assets files and not bundles, which usually have type trees. The `classdata.tpk` file, which you can get from the releases page, contains type info that isn't included assets files. For more information about this file, see the classdata info section below.

```cs
am.LoadClassPackage("classdata.tpk");
am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);
```

#### Get asset info

Before we starting reading an asset, we need to look through the asset info table. Asset infos contain information such as the size of the asset, the position in the file, and the type of asset it is. There are many ways to get asset infos depending on what you're doing.

If you want to load a single asset by asset id or name, you can use `inst.table.GetAssetInfo()`.

```cs
var table = inst.table;
//if you know there is only one asset by the name RocketShip, you don't need to search by type
var inf1 = table.GetAssetInfo("RocketShip");
//if there are multiple assets by the same name, you can use type to narrow it down
var inf2 = table.GetAssetInfo("RocketShip", (int)AssetClassID.GameObject);
//if you want to get an asset by id (not recommended since they change over versions)
var inf3 = table.GetAssetInfo(428);
```

If you want to load all asset infos, loop over `inst.table.assetFileInfo`.

```cs
var table = inst.table;
foreach (var inf in table.assetFileInfo)
{
    //...
}
```

Or if you want to load assets of a certain type, use `inst.table.GetAssetsOfType()`.

```cs
var table = inst.table;
foreach (var inf in table.GetAssetsOfType((int)AssetClassID.GameObject))
{
    //...
}
```

You can get an asset's type id from the Type field in UABE or right click -> Properties menu in AssetsView.

#### Reading the asset's fields

Once you've done that, you will probably want to deserialize the asset. To do this, use `AssetManager`'s `GetTypeInstance` method, then call `GetBaseField` on that.

```cs
var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
```

Now you can browse the fields in this asset as if it were json. Use `Get` to get children fields and `GetValue` to get the actual value of field if it has one.

```cs
var m_Name = baseField.Get("m_Name").GetValue().AsString();
```

If you prefer the syntax, `[]` is supported as well.

```cs
var m_Name = baseField["m_Name"].value.AsString();
```

Arrays (or regular fields) can be iterated by `field.GetChildrenList()`.

```cs
var names = baseField.Get("names");
foreach (var name in names.GetChildrenList())
{
    var nameStr = name.GetValue().AsString();
}
```

#### Following asset pointers

You'll probably come across a PPtr field, a pointer to another asset that you might want to follow. Because assets can sometimes be in different files, it is best to use `am.GetExtAsset()` rather than `table.GetAssetInfo()` and `am.GetTypeInstance()` . `GetExtAsset` also returns the file it came from and the info of the asset in case you don't know the file or the type of the asset you're reading.

For example, if you wanted to get the Transform attached to a GameObject, you could use `GetExtAsset`.

```cs
var componentArray = gameObjectBf.Get("m_Component").Get("Array");
//get first component in gameobject, which is always transform
var transformRef = componentArray[0].Get("component");
var transform = am.GetExtAsset(instance, transformRef);
var transformBf = transform.instance.GetBaseField();
//... do something with transform fields
```

### Write an assets file

Writing assets files is a bit tricky. Instead of just saving the modified assets file, you must pass in changes you make when you write.

First, make the changes to fields you want to fields with `field.GetValue().Set()`. Then, call `WriteToByteArray()` on the base field to get the new raw bytes of the asset. To save the new changes back to an assets file, create an `AssetsReplacer` and pass it to the assets file's `Write` method.

```cs
var am = new AssetsManager();
am.LoadClassPackage("classdata.tpk");

//true to load dependencies. if you know you're only
//reading this one asset, you can set to false to
//speed things up a bit
var inst = am.LoadAssetsFile("resources.assets", true);
//load correct class database for this unity version
am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);

var inf = inst.table.GetAssetInfo("MyBoringGameObject");

var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
baseField.Get("m_Name").GetValue().Set("MyCoolGameObject");
var newGoBytes = baseField.WriteToByteArray();

var repl = new AssetsReplacerFromMemory(0, inf.index, (int)inf.curFileType, 0xffff, newGoBytes);

using (var stream = File.OpenWrite("resources-modified.assets"))
using (var writer = new AssetsFileWriter(stream))
{
    inst.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
}

am.UnloadAllAssetsFiles();
```

The constructor for `AssetsReplacer`s take a monoScriptIndex parameter. If you won't be touching `MonoBehaviour` assets, leave this as `0xffff` like in this example (no script id) as a shortcut. If you are editing `MonoBehaviour`s, use `AssetHelper.GetScriptIndex()` to get the correct script index. More information about this in the MonoBehaviour reading/writing section.

### Value builder (add assets/fields)

With the above examples, you can change the value of existing fields, but you can't add new fields (like to add to an array or create an asset from scratch.) To do that, you can use the `ValueBuilder` which lets you create blank `AssetTypeValueField`s from `AssetTypeTemplateField`s.

#### Set array items

```cs
//example for a GameObject
var componentArray = baseField.Get("m_Component").Get("Array");
//create two blank pptr fields
var transform = ValueBuilder.DefaultValueFieldFromArrayTemplate(componentArray);
var rigidbody = ValueBuilder.DefaultValueFieldFromArrayTemplate(componentArray);
transform.Get("m_FileID").GetValue().Set(0);
transform.Get("m_PathID").GetValue().Set(123);
rigidbody.Get("m_FileID").GetValue().Set(0);
rigidbody.Get("m_PathID").GetValue().Set(456);
AssetTypeValueField[] newChildren = new AssetTypeValueField[] 
{
    transform, rigidbody
};
componentArray.SetChildrenList(newChildren);
//... do replacer stuff
```

If you need to add items instead of set, you'll have to use array concat (I know, a little annoying)

```cs
componentArray.SetChildrenList(componentArray.children.Concat(newChildren));
```

#### Create new asset from scratch

```cs
//example for TextAsset
var templateField = new AssetTypeTemplateField();

//if from an assets file, use the class database
var cldbType = AssetHelper.FindAssetClassByName(am.classFile, "TextAsset");
templateField.FromClassDatabase(am.classFile, cldbType, 0);

//if from a bundle, use the type tree (more on this in the bundle loading section below)
//var ttType = AssetHelper.FindTypeTreeTypeByName(inst.file.typeTree, "TextAsset");
//templateField.From0D(ttType, 0);

var baseField = ValueBuilder.DefaultValueFieldFromTemplate(templateField);
baseField.Get("m_Name").GetValue().Set("MyCoolTextAsset");
baseField.Get("m_Script").GetValue().Set("I have some sick text");

var nextAssetId = table.assetFileInfo.Max(i => i.index) + 1;
replacers.Add(new AssetsReplacerFromMemory(0, nextAssetId, cldbType.classId, 0xffff, baseField.WriteToByteArray()));
//... do other replacer stuff
```

### Read a bundle file

Use `am.LoadBundleFile()` to load bundles and `am.LoadAssetsFileFromBundle()` to load assets files.

```cs
var am = new AssetsManager();
var bun = am.LoadBundleFile("bundle.unity3d");

//load first asset from bundle
var assetInst = am.LoadAssetsFileFromBundle(bun, 0, true);
//if you're not sure the asset you want is first,
//iterate over bun.file.bundleInf6.dirInf[x].name
//(i.e. skip files that end with .resS/.resource)
```

If you want data files in the bundle like .resS or .resource, use `BundleHelper.LoadAssetDataFromBundle()` to get a byte array of that file.

If you want to read the files listing in a bundle but don't want to decompress the entire file yet, use `BundleHelper.UnpackInfoOnly()` to decompress only the file listing info block. Make sure to call `am.LoadBundleFile()` with `unpackIfPacked` set to false.

#### Notes

1. Unity usually packs type information into bundles. That means you probably won't have to load class package files (classdata.tpk). There are exceptions, but most of the time you won't have to worry about it. Check `assetInst.file.typeTree.hasTypeTree` if you're not sure.
2. If you are loading a huge bundle, consider writing it to file first.

```cs
//set to false to prevent automatically decompressing to memory
var bun = am.LoadBundleFile("bundle.unity3d", false);

var bunDecompStream = File.OpenWrite("bun.decomp");
//load new bundle file from newly written stream
bun.file = BundleHelper.UnpackBundleToStream(bun.file, bunDecompStream);
```

3. Do not iterate over `bun.loadedAssetsFiles` to find assets. This list contains _all loaded assets files_ (from when you call `am.LoadAssetsFileFromBundle()`) files from this bundle, not all files that are actually in the bundle. Instead, use either `am.LoadAssetsFileFromBundle()` or `BundleHelper.LoadAssetFromBundle()`.

### Write a bundle file

Bundle writing works similar to assets files where you use replacers to replace files in the bundle.

Note that when you create a `BundleReplacer`, you have the option of renaming the asset in the bundle, or you can use the same name (or make `newName` null) to not rename the asset at all.

```cs
var am = new AssetsManager();
am.LoadClassPackage("classdata.tpk");

var bunInst = am.LoadBundleFile("boringbundle.unity3d");
//read the boring file from the bundle
var inst = am.LoadAssetsFileFromBundle(bunInst, "boring");

//load class database in the rare case this bundle has no type info
if (!inst.file.typeTree.hasTypeTree)
    am.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);

var inf = inst.table.GetAssetInfo("MyBoringAsset");
var baseField = am.GetTypeInstance(inst, inf).GetBaseField();
baseField.Get("m_Name").GetValue().Set("MyCoolAsset");

var newGoBytes = baseField.WriteToByteArray();
var repl = new AssetsReplacerFromMemory(0, inf.index, (int)inf.curFileType, 0xffff, newGoBytes);

//write changes to memory
byte[] newAssetData;
using (var stream = new MemoryStream())
using (var writer = new AssetsFileWriter(stream))
{
    inst.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
    newAssetData = stream.ToArray();
}

//rename this asset name from boring to cool when saving
var bunRepl = new BundleReplacerFromMemory("boring", "cool", true, newAssetData, -1);

var bunWriter = new AssetsFileWriter(File.OpenWrite("coolbundle.unity3d"));
bunInst.file.Write(bunWriter, new List<BundleReplacer>() { bunRepl });
```

### Compress a bundle file

You can also compress a bundle with LZMA or LZ4.

```cs
var am = new AssetsManager();
var bun = am.LoadBundleFile("uncompressedbundle.unity3d");
using (var stream = File.OpenWrite("compressedbundle.unity3d"))
using (var writer = new AssetsFileWriter(stream))
{
    bun.file.Pack(bun.file.reader, writer, AssetBundleCompressionType.LZMA);
}
```

The packers are using managed implementations so they may be slow (especially LZMA, whose c# implementation hasn't been updated in years). You may want to find a way to use native libraries instead.

### Bundle creator (create assets/bundle files) (todo)

You can create a new assets file or bundle file with `BundleCreator`.

### Creating assets file

```cs
var am = new AssetsManager();
var ms = new MemoryStream();

var engineVer = "2019.4.18f1";
var formatVer = 0x15;
var typeTreeVer = 0x13;

BundleCreator.CreateBlankAssets(ms, engineVer, formatVer, typeTreeVer);
ms.Position = 0;
var inst = am.LoadAssetsFile(ms, "fakefilepath.assets", false);
//...
```

To figure out the unity version string and format number for your game, open an assets file or bundle in AssetsView and open the Info->View Current Assets File Info menu item. You can find this programmatically with `typeTreeVer = AssetsFile.typeTree.version` and `formatVer = AssetsFile.header.format`.

### Creating bundle file

The process for bundles is pretty much the same, just use `BundleCreator.CreateBundleFile`. Note that since the type tree is empty, you will have to add types from the class database to the bundle.

```cs
... todo
```

### Reading a MonoBehaviour

If you are reading a bundle file, most likely you can use the normal methods to read MonoBehaviours and skip this section. However, if you are reading an assets file or bundle with no type info, AssetsTools.NET needs to use the game's assemblies to deserialize MonoBehaviours. Only managed mono assemblies are read, so if your game uses il2cpp, you will need to dump the game's assemblies with il2cppdumper.

```cs
var managedFolder = Path.Combine(Path.GetDirectoryName(fileInst.path), "Managed");
var monoBf = MonoDeserializer.GetMonoBaseField(am, fileInst, monoInf, managedFolder);
```

If you are using a bundle with type info, `GetTypeInstance()` or `GetExtAsset()` will work fine without this.

### Writing a MonoBehaviour

If you are adding a MonoBehaviour to a replacer, you'll need to give the replacer a mono id. Each unique script gets a unique mono id per file. For example, all MonoBehaviours using Script1.cs will be mono id 0 and all MonoBehaviours using Script2.cs will be mono id 1. To figure out which mono id your asset is, use `AssetHelper.GetScriptIndex()`.

```cs
var repl = new AssetsReplacerFromMemory(
    0, monoBehaviourInf.index, (int)monoInf.curFileType,
    AssetHelper.GetScriptIndex(inst.file, monoInf), newMonoBytes
);
```

### Full MonoBehaviour writing example

```cs
//example for finding a specific script and modifying the script on a GameObject
var playerInf = inst.table.GetAssetInfo("PlayerObject");
var playerBf = am.GetTypeInstance(inst, playerInf).GetBaseField();
var playerComponentArr = playerBf.Get("m_Component").Get("Array");

AssetFileInfoEx monoBehaviourInf = null;
AssetTypeValueField monoBehaviourBf = null;
//first let's search for the MonoBehaviour we want in a GameObject
for (var i = 0; i < playerComponentArr.GetChildrenCount(); i++)
{
    //get component info (but don't deserialize yet, loading assets we don't need is wasteful)
    var childPtr = playerComponentArr[i].Get("component");
    var childExt = am.GetExtAsset(inst, childPtr, true);
    var childInf = childExt.info;
    
    //skip if not MonoBehaviour
    if (childInf.curFileType != (uint)AssetClassID.MonoBehaviour)
        continue;
    
    //we found a MonoBehaviour, so let's check the MonoScript for its class name
    
    //actually deserialize the MonoBehaviour asset now
    childExt = am.GetExtAsset(inst, playerComponentArr[i], false);
    var childBf = childExt.instance.GetBaseField();
    var monoScriptPtr = childBf.Get("m_Script");
    
    //get MonoScript from MonoBehaviour
    var monoScriptExt = am.GetExtAsset(childExt.file, monoScriptPtr);
    var monoScriptBf = monoScriptExt.instance.GetBaseField();
    
    var className = monoScriptBf.Get("m_ClassName").GetValue().AsString();
    if (className == "SuperEpicScript")
    {
        //we found the super epic script! now we can edit it
        monoBehaviourInf = childInf;
        monoBehaviourBf = childBf;
        break;
    }
}

if (monoBehaviourInf == null)
    throw new Exception("couldn't find SuperEpicScript on this GameObject");

//load MonoBehaviour fields on top of regular fields
var managedFolder = Path.Combine(Path.GetDirectoryName(inst.path), "Managed");
monoBehaviourBf = MonoDeserializer.GetMonoBaseField(am, inst, monoBehaviourInf, managedFolder);

//change runSpeed field
monoBehaviourBf.Get("runSpeed").GetValue().Set(20f);

var newMonoBytes = monoBehaviourBf.WriteToByteArray();
var repl = new AssetsReplacerFromMemory(
    0, monoBehaviourInf.index, (int)monoBehaviourInf.curFileType,
    AssetHelper.GetScriptIndex(inst.file, monoBehaviourInf),
    newMonoBytes
);

using (var stream = File.OpenWrite("resources-modified.assets"))
using (var writer = new AssetsFileWriter(stream))
{
    inst.file.Write(writer, 0, new List<AssetsReplacer>() { repl }, 0);
}

am.UnloadAllAssetsFiles();
```

### Reading asset paths from resources.assets and bundles

This isn't a feature of the library, but it may be helpful to know. Some assets such as resources assets have full paths rather than just names. UABE shows these paths in the Container column.

#### resources.assets

Open `globalgamemanagers` and check the `ResourceManager` asset and its `m_Container` list.

```cs
var am = new AssetsManager();
am.LoadClassPackage("classdata.tpk");

var ggm = am.LoadAssetsFile("globalgamemanagers", true);
am.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

var rsrcInfo = ggm.table.GetAssetsOfType((int)AssetClassID.ResourceManager)[0];
var rsrcBf = am.GetTypeInstance(ggm, rsrcInfo).GetBaseField();

var m_Container = rsrcBf.Get("m_Container").Get("Array");

foreach (var data in m_Container.children)
{
    var name = data[0].GetValue().AsString();
    var pathId = data[1].Get("m_PathID").GetValue().AsInt64();

    Console.WriteLine($"in resources.assets, pathid {pathId} = {name}");
}
```

This helps you give full paths to assets in resources.assets.

#### Bundles

Bundles are much the same, but in the `AssetBundle` asset (usually at path id 1).

```cs
var am = new AssetsManager();

var bun = am.LoadBundleFile("bundle.unity3d");
var assetInst = am.LoadAssetsFileFromBundle(bun, 0, true);

var abInfo = assetInst.table.GetAssetsOfType((int)AssetClassID.AssetBundle)[0];
var abBf = am.GetTypeInstance(assetInst, rsrcInfo).GetBaseField();

var m_Container = abBf.Get("m_Container").Get("Array");
foreach (var data in m_Container.children)
{
    var name = data[0].GetValue().AsString();
    var pathId = data[1].Get("asset").Get("m_PathID").GetValue().AsInt64();

    Console.WriteLine($"in this bundle, pathid {pathId} = {name}");
}
```

### Exporting a Texture2D

AssetsTools.NET is fully managed and uses no native libraries to be portable. Most texture encoding/decoding libraries are written in c++, so using existing libraries was not an option. Some of the [detex](https://github.com/hglm/detex/) texture decoding code has been ported to c# (DXT1/DXT5/BC7/ETC1/ETC2) along with the RGBA formats. That means that decoding formats like ASTC and encoding any formats is not supported. If you are looking for a way to decode these formats, AssetStudio has a great (almost) no-dependencies native solution (Texture2DDecoderNative) and if you need encoding, see UABEA (TexToolWrap) but it uses dependencies.

The output of these are in BGRA which makes it easy to use Format32bppArgb with System.Drawing's bitmaps. Here's a quick and dirty way to implement that:

```cs
var atvf = am.GetTypeInstance(inst.file, texInf).GetBaseField();
var tf = TextureFile.ReadTextureFile(atvf);

//giving the instance will find .resS files in the same directory
//you can change this to a path if the .resS is somewhere else
//if you have the resS in memory instead, set the pictureData bytes
var texDat = tf.GetTextureData(inst);
if (texDat != null && texDat.Length > 0)
{
    var canvas = new Bitmap(tf.m_Width, tf.m_Height, tf.m_Width * 4, PixelFormat.Format32bppArgb,
        Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
    canvas.RotateFlip(RotateFlipType.RotateNoneFlipY);
    canvas.Save("out.png");
}
```

Note that the original AssetsTools uses RGBA output instead of BGRA output. There is currently no way to output in RGBA at the moment, so you'll just need to swap the r and b bytes yourself for now.

If you're parsing the texture manually or have the bytes some other way, you can use `TextureFile.GetTextureDataFromBytes` to decode a texture from bytes, a texture format, and size without having to create a TextureFile manually.

### Class database

All assets are deserialized using a class database (.dat). Each class database includes all fields of most types from a minor release of unity (2018.1, 2018.2, etc.) All supported unity databases are stored in a class package (.tpk). There are updated class packages in the release zips.

### Noooo it's not working!!

If you're experiencing crashes, it could be many things. If it crashes on file open, the format version may be too new or it could be encrypted (check AssetStudio to see if it can load the file). If it crashes on asset deserialization, a minor unity version may have changed an asset and you'll have to figure out what changed on your own (again, check with AssetStudio to see if it can open the asset). If you have any questions, open an issue or ask on the discord.

# AssetsView

AssetsView is a viewer for assets files. Rather than being targeted toward extracting assets, AssetsView can view the raw data of assets. It improves on UABE by being easier to navigate with gameobject tree views and much more.

![AssetsView](https://user-images.githubusercontent.com/12544505/73774729-1f823380-474a-11ea-8e14-ce89691e63df.png)

### Follow Reference button in GameObject Viewer?

For PPtrs, select either the m_FileID or m_PathID fields and click `Follow Reference` to go to that asset.

### Red text in GameObject Viewer?

Disabled GameObject.