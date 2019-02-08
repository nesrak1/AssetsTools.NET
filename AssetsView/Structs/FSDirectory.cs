using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;

namespace AssetsView.Structs
{
    public class FSDirectory : FSObject
    {
        public List<FSObject> children = new List<FSObject>();

        //public void Create(Dictionary<string, AssetDetails> paths)
        //{
        //    List<string> existingDirs = new List<string>();
        //    foreach (KeyValuePair<string, AssetDetails> path in paths)
        //    {
        //        string pathText = path.Key;
        //        AssetDetails pathDetails = path.Value;
        //        if (pathText.Contains("/"))
        //        {
        //            string dirName = pathText.Split('/')[0];
        //            if (!existingDirs.Contains(dirName))
        //            {
        //                existingDirs.Add(dirName);
        //                Dictionary<string, AssetDetails> newPaths = new Dictionary<string, AssetDetails>();
        //                foreach (KeyValuePair<string, AssetDetails> newPath in paths)
        //                {
        //                    string newPathText = newPath.Key;
        //                    AssetDetails newPathDetails = newPath.Value;
        //                    if (newPathText.Contains("/") && newPathText.Split('/')[0] == dirName)
        //                    {
        //                        newPaths.Add(newPathText.Substring(newPathText.IndexOf("/") + 1), newPathDetails);
        //                    }
        //                }
        //                FSDirectory newDir = new FSDirectory();
        //                children.Add(newDir);
        //                newDir.name = dirName.Replace("\t", "");
        //                if (this.path == null)
        //                    newDir.path = newDir.name;
        //                else
        //                    newDir.path = Path.Combine(this.path, dirName).Replace('\\','/');
        //                newDir.parent = this;
        //                newDir.Create(newPaths);
        //            }
        //        }
        //        else
        //        {
        //            FSAsset asset = new FSAsset();
        //            asset.name = pathText;
        //            if (this.path == null)
        //                asset.path = asset.name;
        //            else
        //                asset.path = Path.Combine(this.path, pathText).Replace('\\', '/');
        //            asset.details = pathDetails;
        //            //asset.details.pointer = new AssetPPtr(pathPPtr.fileID, pathPPtr.pathID);
        //            asset.parent = this;
        //            children.Add(asset);
        //        }
        //    }
        //}
        public void Create(List<AssetDetails> assets)
        {
            List<string> existingDirs = new List<string>();
            foreach (AssetDetails assetDet in assets)
            {
                string pathText = assetDet.path;
                if (pathText.Contains("/"))
                {
                    string dirName = pathText.Split('/')[0];
                    if (!existingDirs.Contains(dirName))
                    {
                        existingDirs.Add(dirName);
                        List<AssetDetails> newAssets = new List<AssetDetails>();
                        foreach (AssetDetails oldAsset in assets)
                        {
                            string oldPath = oldAsset.path;
                            if (oldPath.Contains("/") && oldPath.Split('/')[0] == dirName)
                            {
                                string newPath = oldPath.Substring(oldPath.IndexOf("/") + 1);
                                AssetDetails newAsset = new AssetDetails(oldAsset.pointer, oldAsset.icon, newPath, oldAsset.type, oldAsset.size);
                                newAssets.Add(newAsset);
                            }
                        }
                        FSDirectory newDir = new FSDirectory();
                        children.Add(newDir);
                        newDir.name = dirName.Replace("\t", "");
                        if (path == null)
                            newDir.path = newDir.name;
                        else
                            newDir.path = Path.Combine(path, dirName).Replace('\\','/');
                        newDir.parent = this;
                        newDir.Create(newAssets);
                    }
                }
                else
                {
                    FSAsset asset = new FSAsset();
                    asset.name = pathText;
                    if (path == null)
                        asset.path = asset.name;
                    else
                        asset.path = Path.Combine(path, pathText).Replace('\\', '/');
                    asset.details = assetDet;
                    //asset.details.pointer = new AssetPPtr(pathPPtr.fileID, pathPPtr.pathID);
                    asset.parent = this;
                    children.Add(asset);
                }
            }
        }
    }
}