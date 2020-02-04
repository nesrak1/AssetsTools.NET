// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace UABE.NET.Winforms
{
    public partial class BundleLoader : Form
    {
        public AssetsFileReader reader;
        public AssetBundleFile bundle;
        public bool loaded = false;
        public string bundleFilename = "";
        FileStream bundleStream;
        public BundleLoader(FileStream stream)
        {
            InitializeComponent();
            bundleStream = stream;
            ReadBundle(bundleStream);
        }

        private void ReadBundle(FileStream stream)
        {
            reader = new AssetsFileReader(stream);
            bundle = new AssetBundleFile();
            bundleFilename = stream.Name;
            bundle.Read(reader, true);
            reader.Position = 0;
            switch (bundle.bundleHeader6.flags & 0x3F)
            {
                case 0:
                    compressionMethod.Text = "Compression Method: None";
                    break;
                case 1:
                    compressionMethod.Text = "Compression Method: LZMA";
                    break;
                case 2:
                case 3:
                    compressionMethod.Text = "Compression Method: LZ4";
                    break;
                default:
                    compressionMethod.Text = "Compression Method: Unknown";
                    break;
            }
            if ((bundle.bundleHeader6.flags & 0x3F) == 0)
            {
                note.Text = "Bundle is not compressed. You can load it or compress it.";
                compressButton.Enabled = true;
                loadButton.Enabled = true;
            }
            else
            {
                note.Text = "Bundle is compressed. You must decompress the bundle to load.";
                decompressButton.Enabled = true;
            }
        }

        private void decompressButton_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileName(bundleStream.Name) + ".unpacked";
            sfd.Filter = "All types (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                note.Text = "Decompressing...";
                decompressButton.Enabled = false;
                bundleFilename = sfd.FileName;
                FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create);
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += delegate
                {
                    bundle.Unpack(reader, new AssetsFileWriter(fileStream));
                    fileStream.Position = 0;
                    bundle = new AssetBundleFile();
                    bundle.Read(new AssetsFileReader(fileStream), false);
                    fileStream.Close();
                };
                bw.RunWorkerCompleted += delegate
                {
                    note.Text = "Done. Click Load to open the file.";
                    decompressButton.Enabled = false;
                    loadButton.Enabled = true;
                };
                bw.RunWorkerAsync();
            }
        }

        private void compressButton_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("Compression is not supported at this time");
        }

        private void loadButton_Click(object sender, System.EventArgs e)
        {
            reader.BaseStream.Close();
            loaded = true;
            Close();
        }
    }
}
