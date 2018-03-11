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

namespace UABE.NET.Winforms
{
    public partial class Dependencies : Form
    {
        AssetsFile af;
        string fileName;
        public Dependencies(string fileName, AssetsFile af)
        {
            InitializeComponent();
            this.af = af;
            this.fileName = fileName;
            listBox1.Items.Add("0 - " + fileName);
            for (int i = 0; i < af.dependencies.dependencyCount; i++)
            {
                listBox1.Items.Add((i + 1) + " - " + af.dependencies.pDependencies[i].assetPath.Replace("library/", "resources\\"));
            }
            listBox2.Items.Add("UABE.NET currently does");
            listBox2.Items.Add("not load dependencies");
            listBox2.Items.Add("at this time.");
        }
    }
}
