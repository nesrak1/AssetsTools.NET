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
    public partial class OpenAssetsDialog : Form
    {
        public int selection = -1;
        public OpenAssetsDialog(string filePath)
        {
            InitializeComponent();
            string fileName = Path.GetFileName(filePath);
            status.Text = $"Opening assets file {fileName}...";
        }

        private void justThisFile_Click(object sender, EventArgs e)
        {
            selection = 0;
            Close();
        }

        private void fileAndDependencies_Click(object sender, EventArgs e)
        {
            selection = 1;
            Close();
        }
    }
}
