// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using System;
using System.IO;
using System.Windows.Forms;
using UABE.NET.Winforms;

namespace UABE.NET
{
    public partial class Menu : Form
    {
        //todo, refactor
        string bundlePath = "";
        AssetBundleFile bundle;
        public Menu()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Unity content (*.unity3d;*.assets)|*.unity3d;*.assets|Bundle file (*.unity3d)|*.unity3d|Assets file (*.assets)|*.assets|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //try
                //{
                    //todo, use using
                    FileStream stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.ReadWrite);
                    AssetsFileReader tempReader = new AssetsFileReader(stream); //todo .assets sanity check
                    if (tempReader.ReadStringLength(7) != "UnityFS")
                    {
                        tempReader.Position = 0;
                        AssetViewer av = new AssetViewer(stream, Path.GetDirectoryName(stream.Name), Path.GetFileName(stream.Name));
                        av.ShowDialog();
                    }
                    else
                    {
                        tempReader.Position = 0;
                        BundleLoader bl = new BundleLoader(stream);
                        bl.ShowDialog();
                        if (bl.loaded)
                        {
                            bundleContents.Enabled = true;
                            exportButton.Enabled = true;
                            importButton.Enabled = true;
                            infoButton.Enabled = true;
                            foreach (AssetBundleDirectoryInfo06 di in bl.bundle.bundleInf6.dirInf)
                            {
                                bundleContents.Items.Add(di.name);
                            }
                            bundleContents.SelectedIndex = 0;
                            bundlePath = bl.bundleFilename;
                            fileName.Text = Path.GetFileName(bundlePath);
                            bundle = bl.bundle;
                        }
                    }
                    tempReader = null;
                //} catch (Exception ex)
                //{
                //    MessageBox.Show("Unable to open the file!\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void exportButton_Click(object sender, EventArgs e)
        {

        }

        private void importButton_Click(object sender, EventArgs e)
        {

        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(bundlePath, FileMode.Open, FileAccess.ReadWrite);
            int start = (int)(bundle.bundleHeader6.GetFileDataOffset() + bundle.bundleInf6.dirInf[bundleContents.SelectedIndex].offset);
            int length = (int)bundle.bundleInf6.dirInf[bundleContents.SelectedIndex].decompressedSize;
            byte[] data;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position = start;
                data = reader.ReadBytes(length);
            }
            MemoryStream partition = new MemoryStream(data);
            AssetViewer av = new AssetViewer(partition, Path.GetDirectoryName(stream.Name), Path.GetFileName(stream.Name));
            av.ShowDialog();
        }
    }
}
