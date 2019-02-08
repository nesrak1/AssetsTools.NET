using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Structs
{
    public class AssetDetails
    {
        public AssetPPtr pointer;
        public AssetIcon icon;
        public string path;
        public string type;
        public int size;
        public AssetDetails(AssetPPtr pointer, AssetIcon icon, string path = "", string type = "", int size = 0)
        {
            this.pointer = pointer;
            this.icon = icon;
            this.path = path;
            this.type = type;
            this.size = size;
        }
    }
}
