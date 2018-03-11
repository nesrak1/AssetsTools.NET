// ASSETSTOOLS.NET v1

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace UABE.NET
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openLink("https://7daystodie.com/forums/member.php?908-DerPopo");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openLink("https://github.com/nesrak1/");
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
