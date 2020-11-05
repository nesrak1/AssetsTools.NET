namespace AssetsView.Winforms
{
    partial class XRefsDialog
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
            this.xrefList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // xrefList
            // 
            this.xrefList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xrefList.FormattingEnabled = true;
            this.xrefList.IntegralHeight = false;
            this.xrefList.Location = new System.Drawing.Point(0, 0);
            this.xrefList.Name = "xrefList";
            this.xrefList.Size = new System.Drawing.Size(284, 409);
            this.xrefList.TabIndex = 0;
            this.xrefList.DoubleClick += new System.EventHandler(this.xrefList_DoubleClick);
            // 
            // XRefsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 409);
            this.Controls.Add(this.xrefList);
            this.Name = "XRefsDialog";
            this.Text = "XRefs";
            this.Load += new System.EventHandler(this.XRefsDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox xrefList;
    }
}