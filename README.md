# AssetsTools.NET v2

A .net library for reading and modifying unity assets and bundles based off of the AssetsTools library from [UABE](https://github.com/DerPopo/UABE/).

Jump to a tool:

[![AssetsTools](https://user-images.githubusercontent.com/12544505/73600757-7c97a280-451a-11ea-934b-afd392cc2bcc.png)](#assetstools)
[![AssetsView](https://user-images.githubusercontent.com/12544505/73600640-e57e1b00-4518-11ea-8aab-e8664947f435.png)](#assetsview)

# AssetsTools

[![Nuget](https://img.shields.io/nuget/v/AssetsTools.NET?style=flat-square)](https://www.nuget.org/packages/AssetsTools.NET) [![Prereleases](https://img.shields.io/github/v/release/nesrak1/AssetsTools.NET?include_prereleases&style=flat-square)](https://github.com/nesrak1/AssetsTools.NET/releases) [![discord](https://img.shields.io/discord/862035581491478558?label=discord&logo=discord&logoColor=FFFFFF&style=flat-square)](https://discord.gg/hd9VdswwZs)

## Table of contents

* What is this
* Getting started
* Read an assets file
* Write an assets file
* Value builder (add assets/fields)
* Read a bundle file
* Write a bundle file
* Compress a bundle file
* Bundle creator (create assets/bundle files)
* Reading a MonoBehaviour
* Reading asset paths from resources.assets and bundles
* Exporting a Texture2D
* Class database
* Noooo it's not working!!

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
var manager = new AssetsManager(); 
var assetsFile = manager.LoadAssetsFile("resources.assets", true);

manager.LoadClassPackage("classdata.tpk");
manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

foreach (var asset in assetsFile.table.GetAssetsOfType((int)AssetClassID.GameObject))
{
    var gameObj = manager.GetTypeInstance(assetsFile, asset).GetBaseField();
    
    var name = gameObj.Get("m_Name").GetValue().AsString();
    Console.WriteLine(name);
}

manager.UnloadAllAssetsFiles();
```

Below will go into more about what each part does.

#### Load assets file

To load an assets file, use the `LoadAssetsFile` method with an `AssetsManager`.

```cs
var manager = new AssetsManager(); 
var assetsFile = manager.LoadAssetsFile("resources.assets", true); //true = load dependencies
```

`LoadAssetsFile` returns an `AssetsFileInstance` which is a wrapper for two classes.

* `AssetsFile` contains file version info, the type tree for storing info about asset types, and a list of dependencies. Most of the time, you should just use the dependencies list in `AssetsFileInstance`.
* `AssetsFileTable` which is a list of `AssetFileInfoEx`s which contain pointers to the raw data in file and more.

You can think of `Standard` classes like `AssetsFile` and `AssetsFileTable` as structs directly representing the file format and `AssetsManager` classes like `AssetsFileInstance` as classes that help linking files together.

#### Load class database

Before going any further, type information needs to be loaded. This only applies to assets files and not bundles, which usually have type trees. The `classdata.tpk` file, which you can get from the releases page, contains type info that isn't included assets files. For more information about this file, see the classdata info section below.

```cs
manager.LoadClassPackage("classdata.tpk");
manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);
```

#### Get asset info

Before we starting reading an asset, we need to look through the asset info table. Asset infos contain information such as the size of the asset, the position in the file, and the type of asset it is. There are many ways to get asset infos depending on what you're doing.

If you want to load a single asset by asset id or name, you can use `inst.table.GetAssetInfo()`.

```cs
//if you know there is only one asset by the name RocketShip, you don't need to search by type
var asset1 = assetsFile.table.GetAssetInfo("RocketShip");
//if there are multiple assets by the same name, you can use type to narrow it down
var asset2 = assetsFile.table.GetAssetInfo("RocketShip", (int)AssetClassID.GameObject);
//if you want to get an asset by id (not recommended since they change over versions)
var asset3 = assetsFile.table.GetAssetInfo(428);
```

If you want to load all asset infos, loop over `inst.table.assetFileInfo`.

```cs
foreach (var asset in assetsFile.table.assetFileInfo)
{
    //...
}
```

Or if you want to load assets of a certain type, use `inst.table.GetAssetsOfType()`.

```cs
foreach (var asset in assetsFile.table.GetAssetsOfType((int)AssetClassID.GameObject))
{
    //...
}
```

You can get an asset's type id from the Type field in UABE or right click -> Properties menu in AssetsView.

#### Reading the asset's fields

Once you've done that, you will probably want to deserialize the asset. To do this, use `AssetManager`'s `GetTypeInstance` method, then call `GetBaseField` on that.

```cs
var assetObj = manager.GetTypeInstance(assetsFile, asset).GetBaseField();
```

Now you can browse the fields in this asset as if it were json. Use `Get` to get children fields and `GetValue` to get the actual value of field if it has one.

```cs
var m_Name = assetObj.Get("m_Name").GetValue().AsString();
```

If you prefer the syntax, `[]` is supported as well.

```cs
var m_Name = assetObj["m_Name"].value.AsString();
```

Arrays (or regular fields) can be iterated by `obj.GetChildrenList()`.

```cs
var names = assetObj.Get("names");
foreach (var name in names.GetChildrenList())
{
    var nameStr = name.GetValue().AsString();
}
```

#### Following asset pointers

You'll probably come across a PPtr field, a pointer to another asset that you might want to follow. Because assets can sometimes be in different files, it is best to use `manager.GetExtAsset()` rather than `assetsFile.table.GetAssetInfo()` and `manager.GetTypeInstance()` . `GetExtAsset` also returns the file it came from and the info of the asset in case you don't know the file or the type of the asset you're reading.

For example, if you wanted to get the Transform attached to a GameObject, you could use `GetExtAsset`.

```cs
var componentArray = gameObj.Get("m_Component").Get("Array");
//get first component in gameobject, which is always transform
var transformRef = componentArray[0].Get("component");
var transformAsset = manager.GetExtAsset(assetsFile, transformRef);
var transform = transformAsset.instance.GetBaseField();
//... do something with transform fields
```

### Write an assets file

Writing assets files is a bit tricky. Instead of just saving the modified assets file, you must pass in changes you make when you write.

The easiest way to do this is to create a `LambdaAssetReplacer`, passing it a lambda that modifies the deserialized asset. The replacer will handle the deserialization and reserialization for you, so you don't need to call `GetBaseField()` yourself.

First, make the changes to fields you want to fields with `field.GetValue().Set()`. Then, call `WriteToByteArray()` on the base field to get the new raw bytes of the asset. To save the new changes back to an assets file, create an `AssetsReplacer` and pass it to the assets file's `Write` method.

```cs
var manager = new AssetsManager();
manager.LoadClassPackage("classdata.tpk");

// true to load dependencies. If you know you're only
// reading this one asset, you can set to false to
// speed things up a bit
var assetsFile = manager.LoadAssetsFile("resources.assets", true);
manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

var asset = assetsFile.table.GetAssetInfo("MyBoringGameObject");

var replacer = new LambdaAssetReplacer(
    manager,
    assetsFile,
    asset,
    assetObj => assetObj["m_Name"].GetValue().Set("MyCoolGameObject")
);

using (var writer = new AssetsFileWriter("resources-modified.assets"))
{
    assetsFile.file.Write(writer, 0, new List<AssetsReplacer> { replacer });
}

manager.UnloadAllAssetsFiles();
```

For more complex changes, you can create your own replacer class that derives from `SerializingAssetReplacer` and overrides its abstract `Modify` method. Alternatively, you can first deserialize and modify the asset as shown previously, then serialize it using `assetObj.WriteToByteArray()` and wrap the result in an `AssetsReplacerFromMemory`, which you can then pass to `assetsFile.file.Write()` as above.

Some `AssetsReplacer`s take a monoScriptIndex parameter. If you won't be touching `MonoBehaviour` assets, leave this as `0xFFFF` (no script id) as a shortcut. If you are editing `MonoBehaviour`s, use `AssetHelper.GetScriptIndex()` to get the correct script index. More information about this in the MonoBehaviour reading/writing section.

### Value builder (add assets/fields)

With the above examples, you can change the value of existing fields, but you can't add new fields (like to add to an array or create an asset from scratch.) To do that, you can use the `ValueBuilder` which lets you create blank `AssetTypeValueField`s from `AssetTypeTemplateField`s.

#### Set array items

```cs
// Example for a GameObject
var componentArray = assetObj.Get("m_Component").Get("Array");

// Create two blank PPtr objects based on the item type of the array
var transform = ValueBuilder.DefaultValueFieldFromArrayTemplate(componentArray);
var rigidbody = ValueBuilder.DefaultValueFieldFromArrayTemplate(componentArray);

// Populate their fields
transform.Get("m_FileID").GetValue().Set(0);
transform.Get("m_PathID").GetValue().Set(123);
rigidbody.Get("m_FileID").GetValue().Set(0);
rigidbody.Get("m_PathID").GetValue().Set(456);

// Overwrite the children list with these new PPtrs
AssetTypeValueField[] newChildren = { transform, rigidbody };
componentArray.SetChildrenList(newChildren);
//... do replacer stuff
```

If you need to add items instead of set, you'll have to use array concat (I know, a little annoying)

```cs
componentArray.SetChildrenList(componentArray.children.Concat(newChildren));
```

#### Create new asset from scratch

```cs
// Example for TextAsset
var templateField = new AssetTypeTemplateField();

// If from an assets file, use the class database
var cldbType = AssetHelper.FindAssetClassByName(manager.classFile, "TextAsset");
templateField.FromClassDatabase(manager.classFile, cldbType, 0);

// If from a bundle, use the type tree (more on this in the bundle loading section below)
// var ttType = AssetHelper.FindTypeTreeTypeByName(assetsFile.file.typeTree, "TextAsset");
// templateField.From0D(ttType, 0);

var assetObj = ValueBuilder.DefaultValueFieldFromTemplate(templateField);
assetObj.Get("m_Name").GetValue().Set("MyCoolTextAsset");
assetObj.Get("m_Script").GetValue().Set("I have some sick text");

var nextAssetId = assetsFile.table.assetFileInfo.Max(i => i.index) + 1;
replacers.Add(new AssetsReplacerFromMemory(0, nextAssetId, cldbType.classId, 0xFFFF, assetObj.WriteToByteArray()));
//... do other replacer stuff
```

### Read a bundle file

Use `manager.LoadBundleFile()` to load bundles and `manager.LoadAssetsFileFromBundle()` to load assets files.

```cs
var manager = new AssetsManager();
var bundle = manager.LoadBundleFile("bundle.unity3d");

// Load first assets file from bundle
var assetsFile = manager.LoadAssetsFileFromBundle(bundle, 0, true);

// You can find out the number of files in the bundle using bundle.file.NumFiles,
// and check whether a certain file is an assets file using bundle.file.IsAssetsFile().
// It's also possible to call LoadAssetsFileFromBundle() with a name instead of an index.
```

If you want data files in the bundle like .resS or .resource, use `BundleHelper.LoadAssetDataFromBundle()` to get a byte array of that file.

If you want to read the files listing in a bundle but don't want to decompress the entire file yet, use `BundleHelper.UnpackInfoOnly()` to decompress only the file listing info block. Make sure to call `manager.LoadBundleFile()` with `unpackIfPacked` set to false.

#### Notes

1. Unity usually packs type information into bundles. That means you probably won't have to load class package files (classdata.tpk). There are exceptions, but most of the time you won't have to worry about it. Check `assetsFile.file.typeTree.hasTypeTree` if you're not sure.
2. If you are loading a huge bundle, consider writing it to file first.

```cs
// Set to false to prevent automatically decompressing to memory
var bundle = manager.LoadBundleFile("bundle.unity3d", false);

var bunDecompStream = File.OpenWrite("bun.decomp");
// Load new bundle file from newly written stream
bundle.file = BundleHelper.UnpackBundleToStream(bundle.file, bunDecompStream);
```

3. Do not iterate over `bundle.loadedAssetsFiles` to find assets. This list contains _all loaded assets files_ (from when you call `manager.LoadAssetsFileFromBundle()`) files from this bundle, not all files that are actually in the bundle. Instead, use either `manager.LoadAssetsFileFromBundle()` or `BundleHelper.LoadAssetFromBundle()`.

### Write a bundle file

Bundle writing works similar to assets files where you use replacers to replace files in the bundle.

Note that when you create a `BundleReplacer`, you have the option of renaming the asset in the bundle, or you can use the same name (or make `newName` null) to not rename the asset at all.

```cs
var manager = new AssetsManager();
manager.LoadClassPackage("classdata.tpk");

var bundle = manager.LoadBundleFile("boringbundle.unity3d");
var assetsFile = manager.LoadAssetsFileFromBundle(bundle, "BoringAssetsFile");

// Load class database in the rare case this bundle has no type info
if (!assetsFile.file.typeTree.hasTypeTree)
    manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

var asset = assetsFile.table.GetAssetInfo("BoringAsset");

var assetReplacer = new LambdaAssetReplacer(
    manager,
    assetsFile,
    asset,
    assetObj => assetObj["m_Name"].GetValue().Set("CoolAsset")
);

// Rename the assets file too as a demonstration
var bundleReplacer = new BundleReplacerFromAssets(assetsFile.name, "CoolAssetsFile", assetsFile.file, new List<AssetsReplacer> { assetReplacer });

using (var writer = new AssetsFileWriter("coolbundle.unity3d"))
{
    bundle.file.Write(writer, new List<BundleReplacer> { bundleReplacer });
}
```

### Compress a bundle file

You can also compress a bundle with LZMA or LZ4.

```cs
var manager = new AssetsManager();
var bundle = manager.LoadBundleFile("uncompressedbundle.unity3d");
using (var stream = File.OpenWrite("compressedbundle.unity3d"))
using (var writer = new AssetsFileWriter(stream))
{
    bundle.file.Pack(bundle.file.reader, writer, AssetBundleCompressionType.LZMA);
}
```

The packers are using managed implementations so they may be slow (especially LZMA, whose c# implementation hasn't been updated in years). You may want to find a way to use native libraries instead.

### Bundle creator (create assets/bundle files) (todo)

You can create a new assets file or bundle file with `BundleCreator`.

### Creating assets file

```cs
var manager = new AssetsManager();
var stream = new MemoryStream();

var engineVer = "2019.4.18f1";
var formatVer = 0x15u;
var typeTreeVer = 0x13u;

BundleCreator.CreateBlankAssets(stream, engineVer, formatVer, typeTreeVer);
stream.Position = 0;
var assetsFile = manager.LoadAssetsFile(stream, "fakefilepath.assets", false);
//...
```

To figure out the unity version string and format number for your game, open an assets file or bundle in AssetsView and open the Info->View Current Assets File Info menu item.

### Reading a MonoBehaviour

If you are reading a bundle file, most likely you can use the normal methods to read MonoBehaviours and skip this section. However, if you are reading an assets file or bundle with no type info, AssetsTools.NET needs to use the game's assemblies to deserialize MonoBehaviours. Only managed mono assemblies are read, so if your game uses il2cpp, you will need to dump the game's assemblies with il2cppdumper.

```cs
var managedFolder = Path.Combine(Path.GetDirectoryName(assetsFile.path), "Managed");
var monoObj = MonoDeserializer.GetMonoBaseField(manager, assetsFile, asset, managedFolder);
```

If you are using a bundle with type info, `GetTypeInstance()` or `GetExtAsset()` will work fine without this.

### Writing a MonoBehaviour

If you are adding a MonoBehaviour to a replacer, you'll need to give the replacer a mono id. Each unique script gets a unique mono id per file. For example, all MonoBehaviours using Script1.cs will be mono id 0 and all MonoBehaviours using Script2.cs will be mono id 1. To figure out which mono id your asset is, use `AssetHelper.GetScriptIndex()`.

```cs
var repl = new AssetsReplacerFromMemory(
    0, asset.index, (int)asset.curFileType,
    AssetHelper.GetScriptIndex(assetsFile.file, asset), newMonoBytes
);
```

### Full MonoBehaviour writing example

```cs
// Example for finding a specific script and modifying the script on a GameObject
var playerAsset = assetsFile.table.GetAssetInfo("PlayerObject");
var player = manager.GetTypeInstance(assetsFile, playerAsset).GetBaseField();
var playerComponents = player.Get("m_Component").Get("Array");

AssetFileInfoEx epicBehaviorAsset = null;

// First let's search for the MonoBehaviour we want in a GameObject
for (var i = 0; i < playerComponents.GetChildrenCount(); i++)
{
    // Get component info (but don't deserialize yet, loading assets we don't need is wasteful)
    var componentPtr = playerComponents[i].Get("second");
    var componentExtAsset = manager.GetExtAsset(assetsFile, componentPtr);

    // Skip if not MonoBehaviour
    if (componentExtAsset.info.curFileType != (uint)AssetClassID.MonoBehaviour)
        continue;

    // We found a MonoBehaviour, so let's check the MonoScript for its class name

    // Actually deserialize the MonoBehaviour asset now
    var behavior = componentExtAsset.instance.GetBaseField();
    var monoScriptPtr = behavior.Get("m_Script");

    // Get MonoScript from MonoBehaviour
    var monoScriptExtAsset = manager.GetExtAsset(componentExtAsset.file, monoScriptPtr);
    var monoScript = monoScriptExtAsset.instance.GetBaseField();

    var className = monoScript.Get("m_ClassName").GetValue().AsString();
    if (className == "SuperEpicScript")
    {
        // We found the super epic script! now we can edit it
        epicBehaviorAsset = componentExtAsset.info;
        break;
    }
}

if (epicBehaviorAsset == null)
    throw new Exception("Couldn't find SuperEpicScript on this GameObject");

// Load MonoBehaviour fields on top of regular fields
var managedFolder = Path.Combine(Path.GetDirectoryName(assetsFile.path), "Managed");
var epicBehavior = MonoDeserializer.GetMonoBaseField(manager, assetsFile, epicBehaviorAsset, managedFolder);

// Change runSpeed field
epicBehavior.Get("runSpeed").GetValue().Set(1);

var newMonoBytes = epicBehavior.WriteToByteArray();
var replacer = new AssetsReplacerFromMemory(
    0, epicBehaviorAsset.index, (int)epicBehaviorAsset.curFileType,
    AssetHelper.GetScriptIndex(assetsFile.file, epicBehaviorAsset),
    newMonoBytes
);

using (var writer = new AssetsFileWriter("resources-modified.assets"))
{
    assetsFile.file.Write(writer, 0, new List<AssetsReplacer> { replacer });
}

manager.UnloadAllAssetsFiles();
```

### Reading asset paths from resources.assets and bundles

This isn't a feature of the library, but it may be helpful to know. Some assets such as resources assets have full paths rather than just names. UABE shows these paths in the Container column.

#### resources.assets

Open `globalgamemanagers` and check the `ResourceManager` asset and its `m_Container` list.

```cs
var manager = new AssetsManager();
manager.LoadClassPackage("classdata.tpk");

var assetsFile = manager.LoadAssetsFile("globalgamemanagers", true);
manager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

var asset = assetsFile.table.GetAssetsOfType((int)AssetClassID.ResourceManager)[0];
var resourceMgr = manager.GetTypeInstance(assetsFile, asset).GetBaseField();

var m_Container = resourceMgr.Get("m_Container").Get("Array");

foreach (var pair in m_Container.children)
{
    var name = pair[0].GetValue().AsString();
    var pathId = pair[1].Get("m_PathID").GetValue().AsInt64();

    Console.WriteLine($"In resources.assets, pathid {pathId} = {name}");
}
```

This helps you give full paths to assets in resources.assets.

#### Bundles

Bundles are much the same, but in the `AssetBundle` asset (usually at path id 1).

```cs
var manager = new AssetsManager();

var bundle = manager.LoadBundleFile("bundle.unity3d");
var assetsFile = manager.LoadAssetsFileFromBundle(bundle, 0, true);

var bundleInfoAsset = assetsFile.table.GetAssetsOfType((int)AssetClassID.AssetBundle)[0];
var bundleInfo = manager.GetTypeInstance(assetsFile, bundleInfoAsset).GetBaseField();

var m_Container = bundleInfo.Get("m_Container").Get("Array");
foreach (var pair in m_Container.children)
{
    var name = pair[0].GetValue().AsString();
    var pathId = pair[1].Get("asset").Get("m_PathID").GetValue().AsInt64();

    Console.WriteLine($"In this bundle, pathid {pathId} = {name}");
}
```

### Exporting a Texture2D

AssetsTools.NET is fully managed and uses no native libraries to be portable. Most texture encoding/decoding libraries are written in c++, so using existing libraries was not an option. Some of the [detex](https://github.com/hglm/detex/) texture decoding code has been ported to c# (DXT1/DXT5/BC7/ETC1/ETC2) along with the RGBA formats. That means that decoding formats like ASTC and encoding any formats is not supported. If you are looking for a way to decode these formats, AssetStudio has a great (almost) no-dependencies native solution (Texture2DDecoderNative) and if you need encoding, see UABEA (TexToolWrap) but it uses dependencies.

The output of these are in BGRA which makes it easy to use Format32bppArgb with System.Drawing's bitmaps. Here's a quick and dirty way to implement that:

```cs
var textureObj = manager.GetTypeInstance(assetsFile.file, textureAsset).GetBaseField();
var texture = TextureFile.ReadTextureFile(textureObj);

// Some texture objects don't contain the image data, but instead refer to a .resS file
// in the assets file's directory or bundle. Depending on the situation, you can pass
// the assets file or the bundle to GetTextureData() to help it find this .resS file.
byte[] bgra = texture.GetTextureData(assetsFile);
if (bgra != null && bgra.Length > 0)
{
    fixed (byte* pBgra = bgra)
    {
        using var bitmap = new Bitmap(texture.m_Width, texture.m_Height, texture.m_Width * 4,
            PixelFormat.Format32bppArgb, (IntPtr)pBgra);
        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        bitmap.Save("out.png");
    }
}
```

Note that the original AssetsTools uses RGBA output instead of BGRA output. There is currently no way to output in RGBA at the moment, so you'll just need to swap the r and b bytes yourself for now.

If you're parsing the texture manually or have the bytes some other way, you can use `TextureFile.DecodeTextureToBgra` to decode a texture from bytes, a texture format, and size without having to create a TextureFile manually.

If you want to replace texture assets, the `Texture2DAssetReplacer` may come in handy. You can derive from this class and override its `GetBgra()` method to supply it with the new image data. It'll then take care of encoding this data in the texture's format and applying it to the texture object. Note that encoding support is more limited than decoding support - only the RGBA formats work right now.

### Class database

All assets are deserialized using a class database (.dat). Each class database includes all fields of most types from a minor release of unity (2018.1, 2018.2, etc.) All supported unity databases are stored in a class package (.tpk). There are updated class packages in the release zips.

#### Licensing

All versions up to 2019.3 are from UABE's original classdata.tpk file which is included under the BY-NC-SA license which is non-commercial. All versions starting at 2019.4 and up come from TypeTreeDumper.

I personally include the tpk in the release of the program and mention it being under BY-NC-SA. If you know the game you are using is 2019.4 and up, you can remove the older versions in UABE's type package editor and you won't have to worry about it. If you're using older unity versions and you don't want to chance it, it may be better to ask the user to [manually download the tpk themselves](https://github.com/DerPopo/UABE/files/4618273/classdata.with.2019.3.9.fix.zip) or create your own cldb with [TypeTreeDumper](https://github.com/DaZombieKiller/TypeTreeDumper). But IANAL, and I don't exactly know what the rules are with including it with differently licensed software.

If you're wondering "why not just use AssetStudio's code", it's for two big reasons. AssetStudio's code is hardcoded c# that would be hard to port over to AssetsTools' format. Also, AssetStudio supports a smaller number of types (~40 at time of writing) vs over 200 of AssetsTools' that can easily be generated for new versions.

### Noooo it's not working!!

If you're experiencing crashes, it could be many things. If it crashes on file open, the format version may be too new or it could be encrypted (check AssetStudio to see if it can load the file). If it crashes on asset deserialization, a minor unity version may have changed an asset and you'll have to figure out what changed on your own (again, check with AssetStudio to see if it can open the asset). If you have any questions, open an issue or ask on the discord.

# AssetsView

AssetsView is a viewer for assets files. Rather than being targeted toward extracting assets, AssetsView can view the raw data of assets. It improves on UABE by being easier to navigate with gameobject tree views and much more.

![AssetsView](https://user-images.githubusercontent.com/12544505/73774729-1f823380-474a-11ea-8e14-ce89691e63df.png)

### Follow Reference button in GameObject Viewer?

For PPtrs, select either the m_FileID or m_PathID fields and click `Follow Reference` to go to that asset.

### Red text in GameObject Viewer?

Disabled GameObject.