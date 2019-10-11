using System.Diagnostics;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class AboutScreen : Form
    {
        public AboutScreen()
        {
            InitializeComponent();
        }

        private void AboutLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openLink("https://github.com/nesrak1/AssetsTools.NET/");
        }

        private void openLink(string url)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = url;
            p.Start();
        }
    }
}
