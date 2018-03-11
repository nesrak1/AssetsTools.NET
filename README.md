# AssetsTools.NET
AssetsTools is a remake and port at the same time (much wowness) of UABE's AssetsTools. Much of it is still a work in progress, however it is very much functional. I have none of the original c++ code from AssetsTools except for header files so I am rewriting as much as I can while preserving the original structure (which may sound strange considering I'm porting from c++ to c# but it's somewhat working out). I don't know much about the format myself, so if you want to help, fork and pr to help out!

## Why rewrite it?

The main purpose of rewriting it was to have a library to integrate with Unity (and to make it easier for myself for regular programs :D) without the use of wrappers. Another thing that was annoying was using vc++2010. You had to download an old version of the installer off of the wayback archive, change some system settings around to install, and get it to integrate with vs2015/17 (this was mostly fixed but ClassDatabaseType still uses vectors which are incompatible with vc12 and with later versions.) I had worked on Unity TreeView for a small time and the amount of marshaling from managed to unmanaged was crazy. I had hoped not to do that with Unity either. It also relies less on native code which would be incompatible on other platforms that weren't Windows. And not to mention, this one is open source.

## But there's [UnityStudio!](https://github.com/Perfare/UnityStudio)
UnityStudio has a different method of doing things. Instead of reading from an asset using a class database, it is hardcoded to read select assets (which means you are limited only to what it can read). It's also just a program and not a library, so it's not like you can use this in any project. UABE.NET supports both, so it has features from both UABE and UnityStudio while AssetsTools.NET provides an api.

## Where's the documentation?
There's none yet, however you can check out my silly issues on github like [here](https://github.com/DerPopo/UABE/issues/63) or [here](https://github.com/DerPopo/UABE/issues/58#issuecomment-234706976). The syntax is still mostly the same however some redundant parameters will be removed (such as the filestream parameter which is usually used by the AssetsReaderByFile/AssetsWriterByFile function which is now replaced with a binary reader.)

## What's up with the types of comments?
```cs
// uabe header comment - found in original header files in assets tools api
/// not implemented comment - this method isn't implemented yet
//- my comment - these are written by me, not found in headers
```