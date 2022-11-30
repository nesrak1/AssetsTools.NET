using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class TextureViewer : Form
    {
        Bitmap image;

        string loadedFileName;
        bool loaded;
        float x, y;
        int width, height;
        int lx, ly;
        int mx, my;
        float sc;
        bool mouseDown;

        public TextureViewer(AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            InitializeComponent();

            loaded = false;
            TextureFile tf = TextureFile.ReadTextureFile(baseField);

            //bundle resS
            TextureFile.StreamingInfo streamInfo = tf.m_StreamData;
            if (streamInfo.path != null && inst.parentBundle != null)
            {
                string searchPath = streamInfo.path;

                if (streamInfo.path.StartsWith("archive:/"))
                    searchPath = searchPath.Substring(9);

                searchPath = Path.GetFileName(searchPath);

                AssetBundleFile bundle = inst.parentBundle.file;

                AssetsFileReader reader = bundle.DataReader;
                AssetBundleDirectoryInfo[] dirInf = bundle.BlockAndDirInfo.DirectoryInfos;
                bool foundFile = false;
                for (int i = 0; i < dirInf.Length; i++)
                {
                    AssetBundleDirectoryInfo info = dirInf[i];
                    if (info.Name == searchPath)
                    {
                        reader.Position = info.Offset + (long)streamInfo.offset;
                        tf.pictureData = reader.ReadBytes((int)streamInfo.size);
                        tf.m_StreamData.offset = 0;
                        tf.m_StreamData.size = 0;
                        tf.m_StreamData.path = "";
                        foundFile = true;
                        break;
                    }
                }
                if (!foundFile)
                {
                    MessageBox.Show("resS was detected but no file was found in bundle");
                }
            }

            byte[] texDat = tf.GetTextureData(inst);
            if (texDat != null && texDat.Length > 0)
            {
                string fmtName = ((TextureFormat)tf.m_TextureFormat).ToString().Replace("_", " ");
                Text = $"Texture Viewer [{fmtName}]";
                loadedFileName = tf.m_Name;

                image = new Bitmap(tf.m_Width, tf.m_Height, PixelFormat.Format32bppArgb);

                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData picData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
                picData.Stride = tf.m_Width * 4;
                IntPtr startAddr = picData.Scan0;
                Marshal.Copy(texDat, 0, startAddr, texDat.Length);

                image.UnlockBits(picData);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                x = 0;
                y = 0;
                width = image.Width;
                height = image.Height;
                sc = 1f;
                mouseDown = false;

                DoubleBuffered = true;
                
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                int waWidth = workingArea.Width;
                int waHeight = workingArea.Height;
                int cliDiffWidth = Size.Width - ClientSize.Width;
                int cliDiffHeight = Size.Height - ClientSize.Height;
                ClientSize = new Size(Math.Min(width, waWidth - cliDiffWidth), Math.Min(height, waHeight - cliDiffHeight));

                loaded = true;
            }
        }

        private void TextureViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldSc = sc;
            sc *= 1 + (float)e.Delta / 1200;

            float oldImageX = mx / oldSc;
            float oldImageY = my / oldSc;

            float newImageX = mx / sc;
            float newImageY = my / sc;

            x = newImageX - oldImageX + x;
            y = newImageY - oldImageY + y;

            Refresh();
        }

        private void TextureViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (loaded && image != null)
            {
                image.Dispose();
                loaded = false;
            }
        }

        public void SaveTexture()
        {
            if(image == null)
            {
                return;
            }
            SaveFileDialog texSaveDialog = new SaveFileDialog();
            texSaveDialog.Filter = ".PNG File|*.png";
            texSaveDialog.Title = "Save texture as .PNG file";
            texSaveDialog.FileName = loadedFileName;
            DialogResult res = texSaveDialog.ShowDialog();
            string selectedName = texSaveDialog.FileName;
            if (res == DialogResult.Cancel || res == DialogResult.No || string.IsNullOrWhiteSpace(selectedName))
            {
                return;
            }
            if (File.Exists(selectedName))
            {
                File.Delete(selectedName);
            }
            image.Save(selectedName, ImageFormat.Png);
            MessageBox.Show("Done!");
        }

        private void TextureViewer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (loaded)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                Matrix oldTfm = g.Transform;
                g.ScaleTransform(sc, sc);
                g.TranslateTransform(x, y);
                g.DrawImage(image, 0, 0);
                //for the resizey thing on the bottom right (for some reason is affected by this)
                g.Transform = oldTfm;
            }
            else
            {
                using (Font font = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Regular))
                {
                    g.DrawString("Unsupported texture format", font, Brushes.Red, 20, 20);
                    g.DrawString("or texture could not be parsed.", font, Brushes.Red, 20, 50);
                }
            }
        }

        private void TextureViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = false;
            }
        }

        private void TextureViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                lx = e.X;
                ly = e.Y;
            }
        }

        private void TextureViewer_MouseMove(object sender, MouseEventArgs e)
        {
            mx = e.X;
            my = e.Y;
            if (mouseDown)
            {
                x += (e.X - lx) / sc;
                y += (e.Y - ly) / sc;
                lx = e.X;
                ly = e.Y;
                Refresh();
            }
        }
    }
}
