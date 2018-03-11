namespace UABE.NET.Winforms
{
    partial class BundleLoader
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
            this.decompressButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.compressButton = new System.Windows.Forms.Button();
            this.compressionMethod = new System.Windows.Forms.Label();
            this.note = new System.Windows.Forms.Label();
            this.loadButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // decompressButton
            // 
            this.decompressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressButton.Enabled = false;
            this.decompressButton.Location = new System.Drawing.Point(3, 3);
            this.decompressButton.Name = "decompressButton";
            this.decompressButton.Size = new System.Drawing.Size(159, 30);
            this.decompressButton.TabIndex = 1;
            this.decompressButton.Text = "Decompress Bundle";
            this.decompressButton.UseVisualStyleBackColor = true;
            this.decompressButton.Click += new System.EventHandler(this.decompressButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.compressButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.decompressButton, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 81);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(331, 36);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // compressButton
            // 
            this.compressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.compressButton.Enabled = false;
            this.compressButton.Location = new System.Drawing.Point(168, 3);
            this.compressButton.Name = "compressButton";
            this.compressButton.Size = new System.Drawing.Size(160, 30);
            this.compressButton.TabIndex = 2;
            this.compressButton.Text = "Compress Bundle";
            this.compressButton.UseVisualStyleBackColor = true;
            this.compressButton.Click += new System.EventHandler(this.compressButton_Click);
            // 
            // compressionMethod
            // 
            this.compressionMethod.AutoSize = true;
            this.compressionMethod.Location = new System.Drawing.Point(12, 9);
            this.compressionMethod.Name = "compressionMethod";
            this.compressionMethod.Size = new System.Drawing.Size(112, 13);
            this.compressionMethod.TabIndex = 4;
            this.compressionMethod.Text = "Compression Method: ";
            // 
            // note
            // 
            this.note.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.note.AutoSize = true;
            this.note.Location = new System.Drawing.Point(12, 29);
            this.note.Name = "note";
            this.note.Size = new System.Drawing.Size(30, 13);
            this.note.TabIndex = 6;
            this.note.Text = "Note";
            // 
            // loadButton
            // 
            this.loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadButton.Enabled = false;
            this.loadButton.Location = new System.Drawing.Point(15, 45);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(325, 30);
            this.loadButton.TabIndex = 7;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // BundleLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 129);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.note);
            this.Controls.Add(this.compressionMethod);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BundleLoader";
            this.Text = "Bundle Loader";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button decompressButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button compressButton;
        private System.Windows.Forms.Label compressionMethod;
        private System.Windows.Forms.Label note;
        private System.Windows.Forms.Button loadButton;
    }
}