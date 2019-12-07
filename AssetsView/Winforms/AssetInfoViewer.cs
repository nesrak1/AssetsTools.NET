using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class AssetInfoViewer : Form
    {
        //assets
        public AssetInfoViewer(uint type, ulong pos, uint size, ulong id, ushort mbid, string name, string typeName, string resourcesPath)
        {
            InitializeComponent();
            properties.Items.Clear();

            AddProperty("Name", name);
            AddProperty("Type", $"{typeName} (0x{type.ToString("X2")})");
            if (resourcesPath != string.Empty)
                AddProperty("Path", resourcesPath);
            AddProperty("Position", $"0x{pos.ToString("X2")}");
            AddProperty("Size", size.ToString());
            AddProperty("ID", ((long)id).ToString());
            if (mbid != 0xFFFF)
                AddProperty("Mono ID", mbid.ToString());
        }

        //folders
        public AssetInfoViewer(string name, string resourcesPath)
        {
            AddProperty("Name", name);
            AddProperty("Type", "Folder");
            if (resourcesPath != string.Empty)
                AddProperty("Path", resourcesPath);
        }

        private void AddProperty(string propertyName, string value)
        {
            properties.Items.Add(new ListViewItem(new[] { propertyName, value }));
        }
    }
}
