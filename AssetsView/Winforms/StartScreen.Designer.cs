namespace AssetsView.Winforms
{
    partial class StartScreen
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartScreen));
            this.assetTree = new System.Windows.Forms.TreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dependenciesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateListInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.assetList = new System.Windows.Forms.DataGridView();
            this.IconCol = new System.Windows.Forms.DataGridViewImageColumn();
            this.NameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IDCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.goDirectory = new System.Windows.Forms.Button();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.upDirectory = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).BeginInit();
            this.SuspendLayout();
            // 
            // assetTree
            // 
            this.assetTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetTree.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.assetTree.Location = new System.Drawing.Point(0, 0);
            this.assetTree.Margin = new System.Windows.Forms.Padding(2);
            this.assetTree.Name = "assetTree";
            this.assetTree.Size = new System.Drawing.Size(288, 387);
            this.assetTree.TabIndex = 0;
            this.assetTree.DoubleClick += new System.EventHandler(this.assetTree_DoubleClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dependenciesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(700, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFileToolStripMenuItem,
            this.addDirectoryToolStripMenuItem,
            this.clearFilesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // addFileToolStripMenuItem
            // 
            this.addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            this.addFileToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.addFileToolStripMenuItem.Text = "Add File";
            this.addFileToolStripMenuItem.Click += new System.EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // addDirectoryToolStripMenuItem
            // 
            this.addDirectoryToolStripMenuItem.Name = "addDirectoryToolStripMenuItem";
            this.addDirectoryToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.addDirectoryToolStripMenuItem.Text = "Add Directory";
            // 
            // clearFilesToolStripMenuItem
            // 
            this.clearFilesToolStripMenuItem.Name = "clearFilesToolStripMenuItem";
            this.clearFilesToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.clearFilesToolStripMenuItem.Text = "Clear files";
            // 
            // dependenciesToolStripMenuItem
            // 
            this.dependenciesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateListInformationToolStripMenuItem});
            this.dependenciesToolStripMenuItem.Name = "dependenciesToolStripMenuItem";
            this.dependenciesToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.dependenciesToolStripMenuItem.Text = "Dependencies";
            // 
            // updateListInformationToolStripMenuItem
            // 
            this.updateListInformationToolStripMenuItem.Name = "updateListInformationToolStripMenuItem";
            this.updateListInformationToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.updateListInformationToolStripMenuItem.Text = "Update list information";
            this.updateListInformationToolStripMenuItem.Click += new System.EventHandler(this.updateListInformationToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 23);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.assetTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.assetList);
            this.splitContainer1.Panel2.Controls.Add(this.goDirectory);
            this.splitContainer1.Panel2.Controls.Add(this.pathBox);
            this.splitContainer1.Panel2.Controls.Add(this.upDirectory);
            this.splitContainer1.Size = new System.Drawing.Size(700, 386);
            this.splitContainer1.SplitterDistance = 287;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
            // 
            // assetList
            // 
            this.assetList.AllowUserToAddRows = false;
            this.assetList.AllowUserToDeleteRows = false;
            this.assetList.AllowUserToOrderColumns = true;
            this.assetList.AllowUserToResizeRows = false;
            this.assetList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetList.BackgroundColor = System.Drawing.SystemColors.Control;
            this.assetList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.assetList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.assetList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IconCol,
            this.NameCol,
            this.TypeCol,
            this.IDCol,
            this.SizeCol});
            this.assetList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.assetList.Location = new System.Drawing.Point(1, 28);
            this.assetList.Margin = new System.Windows.Forms.Padding(2);
            this.assetList.Name = "assetList";
            this.assetList.RowHeadersVisible = false;
            this.assetList.RowTemplate.Height = 28;
            this.assetList.Size = new System.Drawing.Size(410, 358);
            this.assetList.TabIndex = 4;
            this.assetList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_CellDoubleClick);
            // 
            // IconCol
            // 
            this.IconCol.Frozen = true;
            this.IconCol.HeaderText = "";
            this.IconCol.Image = global::AssetsView.Properties.Resources.blankicon;
            this.IconCol.MinimumWidth = 30;
            this.IconCol.Name = "IconCol";
            this.IconCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.IconCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.IconCol.Width = 30;
            // 
            // NameCol
            // 
            this.NameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.NameCol.FillWeight = 75F;
            this.NameCol.HeaderText = "Name";
            this.NameCol.Name = "NameCol";
            this.NameCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // TypeCol
            // 
            this.TypeCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TypeCol.FillWeight = 70F;
            this.TypeCol.HeaderText = "Type";
            this.TypeCol.Name = "TypeCol";
            // 
            // IDCol
            // 
            this.IDCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.IDCol.FillWeight = 70F;
            this.IDCol.HeaderText = "ID";
            this.IDCol.Name = "IDCol";
            // 
            // SizeCol
            // 
            this.SizeCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SizeCol.FillWeight = 40F;
            this.SizeCol.HeaderText = "Size";
            this.SizeCol.Name = "SizeCol";
            // 
            // goDirectory
            // 
            this.goDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goDirectory.Location = new System.Drawing.Point(373, 2);
            this.goDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.goDirectory.Name = "goDirectory";
            this.goDirectory.Size = new System.Drawing.Size(36, 22);
            this.goDirectory.TabIndex = 3;
            this.goDirectory.Text = "Go";
            this.goDirectory.UseVisualStyleBackColor = true;
            // 
            // pathBox
            // 
            this.pathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pathBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pathBox.Location = new System.Drawing.Point(42, 2);
            this.pathBox.Margin = new System.Windows.Forms.Padding(2);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(328, 25);
            this.pathBox.TabIndex = 2;
            // 
            // upDirectory
            // 
            this.upDirectory.Location = new System.Drawing.Point(2, 2);
            this.upDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.upDirectory.Name = "upDirectory";
            this.upDirectory.Size = new System.Drawing.Size(36, 22);
            this.upDirectory.TabIndex = 1;
            this.upDirectory.Text = "Up";
            this.upDirectory.UseVisualStyleBackColor = true;
            this.upDirectory.Click += new System.EventHandler(this.upDirectory_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "unknown_32.png");
            this.imageList.Images.SetKeyName(1, "folder_32.png");
            this.imageList.Images.SetKeyName(2, "gameobject_32.png");
            this.imageList.Images.SetKeyName(3, "transform_32.png");
            this.imageList.Images.SetKeyName(4, "script_32.png");
            this.imageList.Images.SetKeyName(5, "scriptdata_32.png");
            this.imageList.Images.SetKeyName(6, "image_32.png");
            this.imageList.Images.SetKeyName(7, "cube_32.png");
            this.imageList.Images.SetKeyName(8, "material_32.png");
            // 
            // StartScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 409);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartScreen";
            this.Text = "AssetsView .NET";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView assetTree;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem addDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button upDirectory;
        private System.Windows.Forms.Button goDirectory;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.ToolStripMenuItem dependenciesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateListInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFilesToolStripMenuItem;
        private System.Windows.Forms.DataGridView assetList;
        private System.Windows.Forms.DataGridViewImageColumn IconCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeCol;
    }
}

