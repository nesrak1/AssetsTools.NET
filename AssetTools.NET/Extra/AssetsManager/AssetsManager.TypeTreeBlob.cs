using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        private void LoadTypeTreeBlobsFromBundle(BundleFileInstance bunInst)
        {
            AssetBundleFile bunFile = bunInst.file;
            lock (bunInst.file.DataReader)
            {
                foreach (AssetBundleDirectoryInfo dirInf in bunFile.BlockAndDirInfo.DirectoryInfos)
                {
                    // skip serialized files
                    if ((dirInf.Flags & 4) != 0)
                        continue;

                    string fileName = dirInf.Name;
                    if (fileName.Length != 32)
                        continue;

                    // not a hash file
                    if (!Hash128.TryParse(fileName, out Hash128 typeBlobHash))
                        continue;

                    // already loaded
                    if (typeTreeBlobs.ContainsKey(typeBlobHash))
                        continue;

                    long offset = dirInf.Offset;
                    long length = dirInf.DecompressedSize;

                    // not big enough for the header
                    if (length < 8)
                        continue;

                    SegmentStream stream = new SegmentStream(bunInst.DataStream, offset, length);
                    LoadTypeTreeBlob(stream, typeBlobHash, bunInst);
                }
            }
        }

        public bool LoadTypeTreeBlob(Stream stream, Hash128 typeBlobHash, BundleFileInstance owner = null)
        {
            AssetsFileReader reader = new AssetsFileReader(stream);

            try
            {
                TypeTreeBlob blob = new TypeTreeBlob();
                blob.Read(reader, uint.MaxValue);

                typeTreeBlobs[typeBlobHash] = blob;

                if (owner != null)
                {
                    HashSet<Hash128> ownedHashesList;
                    if (!typeTreeBlobOwners.TryGetValue(owner, out ownedHashesList))
                    {
                        ownedHashesList = typeTreeBlobOwners[owner] = new HashSet<Hash128>();
                    }

                    ownedHashesList.Add(typeBlobHash);
                }

                return true;
            }
            catch
            {
                // skip if we can't read it
                return false;
            }
        }

        public TypeTreeBlob GetTypeBlob(Hash128 typeBlobHash)
        {
            return typeTreeBlobs[typeBlobHash];
        }
    }
}
