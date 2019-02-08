using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Structs
{
    public class AssetNodeDetails
    {
        public AssetNodeType type;
        public object data;
        public bool setup;
        public AssetNodeDetails(AssetNodeType type, object data)
        {
            this.type = type;
            this.data = data;

            setup = false;
        }
    }

    public enum AssetNodeType
    {
        RootAsset,
        GameObject,
        Component,
        Script,
        Folder
    }
}
