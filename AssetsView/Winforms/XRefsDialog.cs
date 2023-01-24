using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsView.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class XRefsDialog : Form
    {
        private StartScreen ss;
        private AssetsManager am;
        private string dirName;

        private PPtrMap map;
        private AssetID id;
        public XRefsDialog(StartScreen ss, AssetsManager am, string dirName, PPtrMap map, AssetID id)
        {
            this.ss = ss;
            this.am = am;
            this.dirName = dirName;
            this.map = map;
            this.id = id;
            InitializeComponent();
        }

        private void XRefsDialog_Load(object sender, EventArgs e)
        {
            List<AssetID> xrefs = map.GetXRefs(id);
            if (xrefs == null)
            {
                xrefList.Items.Add("[no xrefs]");
            }
            else
            {
                foreach (AssetID id in xrefs)
                {
                    int instIndex = am.Files.FindIndex(f => Path.GetFileName(f.path).ToLower() == Path.GetFileName(id.fileName).ToLower());
                    if (instIndex != -1)
                    {
                        AssetsFileInstance xrefInst = am.Files[instIndex];
                        AssetsFile xrefFile = xrefInst.file;
                        AssetFileInfo xrefInf = xrefInst.file.GetAssetInfo(id.pathID);
                        int typeId = xrefInf.TypeId;
                        bool hasTypeTree = xrefFile.Metadata.TypeTreeEnabled;

                        string assetName = AssetHelper.GetAssetNameFast(xrefFile, am.ClassDatabase, xrefInf);
                        string typeName;
                        if (hasTypeTree)
                        {
                            TypeTreeType xrefType = xrefFile.Metadata.FindTypeTreeTypeByID(typeId);
                            typeName = xrefType.Nodes[0].GetTypeString(xrefType.StringBuffer);
                        }
                        else
                        {
                            ClassDatabaseType xrefType = am.ClassDatabase.FindAssetClassByID(typeId);
                            typeName = am.ClassDatabase.GetString(xrefType.Name);
                        }
                        xrefList.Items.Add(new ListBoxInfo($"{id.fileName} {id.pathID} ({typeName} {assetName})", id));
                    }
                    else
                    {
                        xrefList.Items.Add(new ListBoxInfo($"{id.fileName} {id.pathID}", id));
                    }
                }
            }
        }

        private class ListBoxInfo
        {
            public string displayText;
            public AssetID assetId;
            public ListBoxInfo(string displayText, AssetID assetId)
            {
                this.displayText = displayText;
                this.assetId = assetId;
            }
            public override string ToString()
            {
                return displayText;
            }
        }

        private void xrefList_DoubleClick(object sender, EventArgs e)
        {
            object selectedItem = xrefList.SelectedItem;
            if (selectedItem != null && selectedItem is ListBoxInfo info)
            {
                AssetID assetId = info.assetId;
                ss.LoadMainAssetsFile(am.LoadAssetsFile(Path.Combine(dirName, assetId.fileName), true));
                ss.OpenAsset(assetId.pathID);
            }
        }
    }
}
