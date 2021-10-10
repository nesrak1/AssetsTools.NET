namespace AssetsView.Winforms
{
    partial class TextureViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureViewer));
            this.SuspendLayout();
            // 
            // TextureViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(60)))), ((int)(((byte)(61)))));
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "TextureViewer";
            this.Text = "Texture Viewer";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.TextureViewer_Paint);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TextureViewer_FormClosed);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextureViewer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TextureViewer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TextureViewer_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.TextureViewer_MouseWheel);
            this.ResumeLayout(false);

        }

        #endregion
    }
}