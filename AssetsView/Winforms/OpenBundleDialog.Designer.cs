namespace AssetsView.Winforms
{
    partial class OpenBundleDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenBundleDialog));
            this.status = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.fileAndDependencies = new System.Windows.Forms.Button();
            this.justThisFile = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.decompressBundleInMemory = new System.Windows.Forms.Button();
            this.decompressBundle = new System.Windows.Forms.Button();
            this.compressBundle = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // status
            // 
            this.status.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.status.Location = new System.Drawing.Point(8, 6);
            this.status.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(508, 26);
            this.status.TabIndex = 0;
            this.status.Text = "Opening bundle file...";
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.fileAndDependencies, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.justThisFile, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 34);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(508, 45);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // fileAndDependencies
            // 
            this.fileAndDependencies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileAndDependencies.Enabled = false;
            this.fileAndDependencies.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileAndDependencies.Location = new System.Drawing.Point(256, 2);
            this.fileAndDependencies.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fileAndDependencies.Name = "fileAndDependencies";
            this.fileAndDependencies.Size = new System.Drawing.Size(250, 41);
            this.fileAndDependencies.TabIndex = 1;
            this.fileAndDependencies.Text = "Open this file and dependencies";
            this.fileAndDependencies.UseVisualStyleBackColor = true;
            this.fileAndDependencies.Click += new System.EventHandler(this.fileAndDependencies_Click);
            // 
            // justThisFile
            // 
            this.justThisFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.justThisFile.Enabled = false;
            this.justThisFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.justThisFile.Location = new System.Drawing.Point(2, 2);
            this.justThisFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.justThisFile.Name = "justThisFile";
            this.justThisFile.Size = new System.Drawing.Size(250, 41);
            this.justThisFile.TabIndex = 0;
            this.justThisFile.Text = "Open just this file";
            this.justThisFile.UseVisualStyleBackColor = true;
            this.justThisFile.Click += new System.EventHandler(this.justThisFile_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.decompressBundleInMemory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.decompressBundle, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.compressBundle, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 83);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(508, 45);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // decompressBundleInMemory
            // 
            this.decompressBundleInMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressBundleInMemory.Enabled = false;
            this.decompressBundleInMemory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.decompressBundleInMemory.Location = new System.Drawing.Point(171, 2);
            this.decompressBundleInMemory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.decompressBundleInMemory.Name = "decompressBundleInMemory";
            this.decompressBundleInMemory.Size = new System.Drawing.Size(165, 41);
            this.decompressBundleInMemory.TabIndex = 1;
            this.decompressBundleInMemory.Text = "Decompress to memory";
            this.decompressBundleInMemory.UseVisualStyleBackColor = true;
            this.decompressBundleInMemory.Click += new System.EventHandler(this.decompressBundleInMemory_Click);
            // 
            // decompressBundle
            // 
            this.decompressBundle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressBundle.Enabled = false;
            this.decompressBundle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.decompressBundle.Location = new System.Drawing.Point(2, 2);
            this.decompressBundle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.decompressBundle.Name = "decompressBundle";
            this.decompressBundle.Size = new System.Drawing.Size(165, 41);
            this.decompressBundle.TabIndex = 0;
            this.decompressBundle.Text = "Decompress to file";
            this.decompressBundle.UseVisualStyleBackColor = true;
            this.decompressBundle.Click += new System.EventHandler(this.decompressBundle_Click);
            // 
            // compressBundle
            // 
            this.compressBundle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.compressBundle.Enabled = false;
            this.compressBundle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compressBundle.Location = new System.Drawing.Point(340, 2);
            this.compressBundle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.compressBundle.Name = "compressBundle";
            this.compressBundle.Size = new System.Drawing.Size(166, 41);
            this.compressBundle.TabIndex = 1;
            this.compressBundle.Text = "Compress to file";
            this.compressBundle.UseVisualStyleBackColor = true;
            this.compressBundle.Click += new System.EventHandler(this.compressBundle_Click);
            // 
            // OpenBundleDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 135);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.status);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "OpenBundleDialog";
            this.Text = "Opening bundle file...";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label status;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button fileAndDependencies;
        private System.Windows.Forms.Button justThisFile;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button decompressBundleInMemory;
        private System.Windows.Forms.Button decompressBundle;
        private System.Windows.Forms.Button compressBundle;
    }
}