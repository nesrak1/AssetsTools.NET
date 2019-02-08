using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Structs
{
    public class DataGridData
    {
        public Image Icon { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public object ID { get; set; }
        public string Size { get; set; }
        public DataGridData(Image icon, string name, string type, object id, string size)
        {
            Icon = icon;
            Name = name;
            Type = type;
            ID = id;
            Size = size;
        }
    }
}
