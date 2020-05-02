using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class TextureViewer : Form
    {
        Bitmap image;

        bool loaded;
        int x, y, width, height;
        int lx, ly;
        float sc;
        bool mouseDown;

        public TextureViewer(AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            InitializeComponent();

            loaded = false;
            TextureFile tf = TextureFile.ReadTextureFile(baseField);
            byte[] texDat = tf.GetTextureData(inst);
            if (texDat != null && texDat.Length > 0)
            {
                string fmtName = ((TextureFormat)tf.m_TextureFormat).ToString().Replace("_", " ");
                Text = $"Texture Viewer [{fmtName}]";

                image = new Bitmap(tf.m_Width, tf.m_Height, tf.m_Width * 4, PixelFormat.Format32bppArgb,
                    Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                x = -image.Width / 2;
                y = -image.Height / 2;
                width = image.Width;
                height = image.Height;
                sc = 1f;
                mouseDown = false;

                DoubleBuffered = true;
                ClientSize = new Size(width, height);

                loaded = true;
            }
        }

        private void TextureViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            sc *= 1 + (float)e.Delta / 1200;
            Refresh();
        }

        private void TextureViewer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (loaded)
            {
                int drawWidth = (int)(width * sc);
                int drawHeight = (int)(height * sc);
                g.DrawImage(image, x + (drawWidth / 2), y + (drawHeight / 2), drawWidth, drawHeight);
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
            if (mouseDown)
            {
                x += e.X - lx;
                y += e.Y - ly;
                lx = e.X;
                ly = e.Y;
                Refresh();
            }
        }
    }
}
