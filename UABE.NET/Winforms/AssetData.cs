// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UABE.NET.Assets;
using static UABE.NET.Winforms.AssetViewer;

namespace UABE.NET.Winforms
{
    public partial class AssetData : Form
    {
        AssetsFile af;
        AssetsManagerLegacy am;
        AssetDetails assetDetails;
        AssetTypeInstance mainAti;
        string rootDir;
        public AssetData(AssetsFile af, AssetsManagerLegacy am, AssetDetails assetDetails, string rootDir)
        {
            InitializeComponent();
            this.af = af;
            this.am = am;
            this.assetDetails = assetDetails;
            this.rootDir = rootDir;
            PopulateTree();
        }
        
        private void PopulateTree()
        {
            ClassDatabaseType cldt = AssetHelper.FindAssetClassByID(am.initialClassFile, assetDetails.type);
            AssetTypeTemplateField pBaseField = new AssetTypeTemplateField();
            pBaseField.FromClassDatabase(am.initialClassFile, cldt, 0);
            mainAti = new AssetTypeInstance(1, new[] { pBaseField }, af.reader, false, assetDetails.position);
            if (assetDetails.type == 0x72)
            {
                AssetTypeTemplateField[] desMonos = TryDeserializeMono(mainAti);
                if (desMonos != null)
                {
                    AssetTypeTemplateField[] templateField = pBaseField.children.Concat(desMonos).ToArray();
                    pBaseField.children = templateField;
                    pBaseField.childrenCount = (uint)pBaseField.children.Length;

                    mainAti = new AssetTypeInstance(1, new[] { pBaseField }, af.reader, false, assetDetails.position);
                }
            }
            AssetTypeValueField baseField = mainAti.GetBaseField();
            rawViewTree.Nodes.Add(baseField.GetFieldType() + " " + baseField.GetName());
            RecursiveTreeLoad(mainAti.GetBaseField(), rawViewTree.Nodes[0], 0);
        }

        private void RecursiveTreeLoad(AssetTypeValueField atvf, TreeNode node, int depth)
        {
            if (atvf.childrenCount == 0) return;
            foreach (AssetTypeValueField atvfc in atvf.pChildren) //todo, if over certain amount of children, load later
            {
                if (atvfc == null) return; //derp check, usually doesn't break anything
                string value = "";
                if (atvfc.GetValue() != null)
                {
                    EnumValueTypes evt = atvfc.GetValue().GetValueType();
                    string str = "";
                    if (evt == EnumValueTypes.ValueType_String) str = "\"";
                    if (1 <= (int)evt && (int)evt <= 12)
                    {
                        value = " = " + str + atvfc.GetValue().AsString() + str;
                    }
                    if (evt == EnumValueTypes.ValueType_Array ||
                        evt == EnumValueTypes.ValueType_ByteArray)
                    {
                        value = $" (size {atvfc.childrenCount})";
                    }
                }
                node.Nodes.Add(atvfc.GetFieldType() + " " + atvfc.GetName() + value);
                RecursiveTreeLoad(atvfc, node.LastNode, depth+1);
            }
        }

        private AssetTypeTemplateField[] TryDeserializeMono(AssetTypeInstance ati)
        {
            AssetTypeInstance scriptAti = am.GetExtAsset(ati.GetBaseField().Get("m_Script")).instance;
            string scriptName = scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
            string assemblyName = scriptAti.GetBaseField().Get("m_AssemblyName").GetValue().AsString();
            string assemblyPath = Path.Combine(rootDir, "Managed", assemblyName);
            if (File.Exists(assemblyPath))
            {
                MonoClass mc = new MonoClass();
                mc.Read(scriptName, assemblyPath, af.header.format);
                return mc.children;
            }
            else
            {
                return null;
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            rawViewTree.ExpandAll();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            rawViewTree.CollapseAll();
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            rawViewTree.SelectedNode.ExpandAll();
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            rawViewTree.SelectedNode.Collapse(false);
        }
    }
}
