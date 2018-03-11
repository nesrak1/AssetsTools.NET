using AssetsTools;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
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

namespace AssetsTools.NETTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string mainFile = ofd.FileName;
            string classFile = "U5.6.0f3_AT.NET.dat";

            FileStream mffs = new FileStream(mainFile, FileMode.Open);
            AssetsFile assetsFile = new AssetsFile(new AssetsFileReader(mffs));
            AssetsFileTable assetsFileTable = new AssetsFileTable(assetsFile);
            AssetFileInfoEx assetsFileInfo = assetsFileTable.getAssetInfo(289578);

            FileStream cffs = new FileStream(classFile, FileMode.Open);
            AssetTypeTemplateField pBaseField = new AssetTypeTemplateField();
            ClassDatabaseFile classDatabaseFile = new ClassDatabaseFile();
            classDatabaseFile.Read(new AssetsFileReader(cffs));
            
            ClassDatabaseType classDatabaseType = AssetHelper.FindAssetClassByName(classDatabaseFile, "MonoBehaviour");

            pBaseField.FromClassDatabase(classDatabaseFile, classDatabaseType, 0);

            AssetTypeInstance ati = new AssetTypeInstance(1, new[] { pBaseField }, new AssetsFileReader(mffs), false, assetsFileInfo.absoluteFilePos);
            
            MonoClass mc = new MonoClass();
            mc.Read("Peeper", "E:\\Steam\\steamapps\\common\\Subnautica\\Subnautica_Data\\Managed\\Assembly-CSharp.dll");
            AssetTypeTemplateField[] templateField = pBaseField.children.Concat(mc.children).ToArray();
            pBaseField.children = templateField;
            pBaseField.childrenCount = (uint)pBaseField.children.Length;

            AssetTypeInstance ati2 = new AssetTypeInstance(1, new[] { pBaseField }, new AssetsFileReader(mffs), false, assetsFileInfo.absoluteFilePos);

            //MessageBox.Show("" + ati.GetBaseField().childrenCount);
            //MessageBox.Show("" + ati.GetBaseField().childrenCount);
            //MessageBox.Show("" + ati.GetBaseField().Get("m_Component").Get("Array").Get(0).Get("component").Get("m_PathID").GetValue().AsInt64());
            MessageBox.Show("" + ati2.GetBaseField().Get("rechargeInterval").GetValue().AsFloat());
            //Console.WriteLine(ati.GetBaseField().Get(2).GetValue().asString);
            //Console.ReadLine();
        }
    }
}
