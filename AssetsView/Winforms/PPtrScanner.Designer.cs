namespace AssetsView.Winforms
{
    partial class PPtrScanner
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
            this.assetProgress = new System.Windows.Forms.ProgressBar();
            this.infoLabel = new System.Windows.Forms.Label();
            this.progressLbl = new System.Windows.Forms.Label();
            this.fileProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // assetProgress
            // 
            this.assetProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetProgress.Location = new System.Drawing.Point(12, 116);
            this.assetProgress.Maximum = 1000;
            this.assetProgress.Name = "assetProgress";
            this.assetProgress.Size = new System.Drawing.Size(351, 23);
            this.assetProgress.TabIndex = 0;
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(351, 36);
            this.infoLabel.TabIndex = 1;
            this.infoLabel.Text = "This operation will take a while!\r\nOnce it\'s done, right click an asset and click" +
    " refs.";
            // 
            // progressLbl
            // 
            this.progressLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressLbl.Location = new System.Drawing.Point(12, 45);
            this.progressLbl.Name = "progressLbl";
            this.progressLbl.Size = new System.Drawing.Size(351, 68);
            this.progressLbl.TabIndex = 1;
            // 
            // fileProgress
            // 
            this.fileProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileProgress.Location = new System.Drawing.Point(12, 145);
            this.fileProgress.Maximum = 1000;
            this.fileProgress.Name = "fileProgress";
            this.fileProgress.Size = new System.Drawing.Size(351, 23);
            this.fileProgress.TabIndex = 0;
            // 
            // PPtrScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(375, 180);
            this.Controls.Add(this.progressLbl);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.fileProgress);
            this.Controls.Add(this.assetProgress);
            this.Name = "PPtrScanner";
            this.Text = "PPtr Scanner";
            this.Load += new System.EventHandler(this.PPtrScanner_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar assetProgress;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label progressLbl;
        private System.Windows.Forms.ProgressBar fileProgress;
    }
}