using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        /// <summary>
        /// Load a <see cref="BundleFileInstance"/> from a stream with a path.
        /// Use the <see cref="FileStream"/> version of this method to skip the path argument.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path to set on the <see cref="AssetsFileInstance"/>.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="BundleFileInstance"/>.</returns>
        public BundleFileInstance LoadBundleFile(Stream stream, string path, bool unpackIfPacked = true)
        {
            BundleFileInstance bunInst;
            string lookupKey = GetFileLookupKey(path);
            if (BundleLookup.TryGetValue(lookupKey, out bunInst))
                return bunInst;

            bunInst = new BundleFileInstance(stream, path, unpackIfPacked);
            Bundles.Add(bunInst);
            BundleLookup[lookupKey] = bunInst;

            return bunInst;
        }

        /// <summary>
        /// Load a <see cref="BundleFileInstance"/> from a stream.
        /// Assigns the <see cref="BundleFileInstance"/>'s path from the stream's file path.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="BundleFileInstance"/>.</returns>
        public BundleFileInstance LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            return LoadBundleFile(stream, Path.GetFullPath(stream.Name), unpackIfPacked);
        }

        /// <summary>
        /// Load a <see cref="BundleFileInstance"/> from a path.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="path">The path of the file to read from.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="BundleFileInstance"/>.</returns>
        public BundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(File.OpenRead(path), unpackIfPacked);
        }

        /// <summary>
        /// Unload an <see cref="BundleFileInstance"/> by path.
        /// </summary>
        /// <param name="path">The path of the <see cref="BundleFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadBundleFile(string path)
        {
            string lookupKey = GetFileLookupKey(path);
            if (BundleLookup.TryGetValue(lookupKey, out BundleFileInstance bunInst))
            {
                bunInst.file.Close();

                foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                {
                    assetsInst.file.Close();
                }

                Bundles.Remove(bunInst);
                BundleLookup.Remove(lookupKey);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unload an <see cref="BundleFileInstance"/>.
        /// </summary>
        /// <param name="bunInst">The <see cref="BundleFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadBundleFile(BundleFileInstance bunInst)
        {
            bunInst.file.Close();

            foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
            {
                UnloadAssetsFile(assetsInst);
            }

            bunInst.loadedAssetsFiles.Clear();

            if (Bundles.Contains(bunInst))
            {
                string lookupKey = GetFileLookupKey(bunInst.path);
                BundleLookup.Remove(lookupKey);
                Bundles.Remove(bunInst);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unload all <see cref="AssetsFileInstance"/>s.
        /// </summary>
        /// <returns>True if there are files that can be cleared, and false if no files are loaded.</returns>
        public bool UnloadAllBundleFiles()
        {
            if (Bundles.Count != 0)
            {
                foreach (BundleFileInstance bunInst in Bundles)
                {
                    bunInst.file.Close();

                    foreach (AssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                    {
                        UnloadAssetsFile(assetsInst);
                    }

                    bunInst.loadedAssetsFiles.Clear();
                }
                Bundles.Clear();
                BundleLookup.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from a <see cref="BundleFileInstance"/> by index.
        /// </summary>
        /// <param name="bunInst">The bundle to load from.</param>
        /// <param name="index">The index of the file in the bundle to load from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, int index, bool loadDeps = false)
        {
            string assetMemPath = Path.Combine(bunInst.path, bunInst.file.GetFileName(index));
            string assetLookupKey = GetFileLookupKey(assetMemPath);

            if (!FileLookup.TryGetValue(assetLookupKey, out AssetsFileInstance fileInst))
            {
                if (bunInst.file.IsAssetsFile(index))
                {
                    bunInst.file.GetFileRange(index, out long offset, out long length);
                    SegmentStream stream = new SegmentStream(bunInst.DataStream, offset, length);
                    AssetsFileInstance assetsInst = LoadAssetsFile(stream, assetMemPath, loadDeps, bunInst: bunInst);
                    bunInst.loadedAssetsFiles.Add(assetsInst);
                    return assetsInst;
                }
                return null;
            }
            else
            {
                return fileInst;
            }
        }

        /// <summary>
        /// Load an <see cref="AssetsFileInstance"/> from a <see cref="BundleFileInstance"/> by name.
        /// </summary>
        /// <param name="bunInst">The bundle to load from.</param>
        /// <param name="name">The name of the file in the bundle to load from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AssetsFileInstance"/>.</returns>
        public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bunInst, string name, bool loadDeps = false)
        {
            int index = bunInst.file.GetFileIndex(name);
            if (index < 0)
                return null;

            return LoadAssetsFileFromBundle(bunInst, index, loadDeps);
        }
    }
}
