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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.updateDependenciesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCurrentAssetInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.globalSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.globalSearchInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.assetDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monoBehaviourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pptrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xRefsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // assetTree
            // 
            this.assetTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assetTree.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.assetTree.Location = new System.Drawing.Point(0, 0);
            this.assetTree.Margin = new System.Windows.Forms.Padding(2);
            this.assetTree.Name = "assetTree";
            this.assetTree.Size = new System.Drawing.Size(286, 448);
            this.assetTree.TabIndex = 0;
            this.assetTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.AssetTree_NodeMouseDoubleClick);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.globalSearchToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(817, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFileToolStripMenuItem,
            this.clearFilesToolStripMenuItem,
            this.toolStripSeparator1,
            this.updateDependenciesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // addFileToolStripMenuItem
            // 
            this.addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            this.addFileToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.addFileToolStripMenuItem.Text = "Add File";
            this.addFileToolStripMenuItem.Click += new System.EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // clearFilesToolStripMenuItem
            // 
            this.clearFilesToolStripMenuItem.Name = "clearFilesToolStripMenuItem";
            this.clearFilesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.clearFilesToolStripMenuItem.Text = "Clear files";
            this.clearFilesToolStripMenuItem.Click += new System.EventHandler(this.clearFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(185, 6);
            // 
            // updateDependenciesToolStripMenuItem
            // 
            this.updateDependenciesToolStripMenuItem.Name = "updateDependenciesToolStripMenuItem";
            this.updateDependenciesToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.updateDependenciesToolStripMenuItem.Text = "Update dependencies";
            this.updateDependenciesToolStripMenuItem.Click += new System.EventHandler(this.updateDependenciesToolStripMenuItem_Click);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewCurrentAssetInfoToolStripMenuItem});
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
            this.infoToolStripMenuItem.Text = "Info";
            // 
            // viewCurrentAssetInfoToolStripMenuItem
            // 
            this.viewCurrentAssetInfoToolStripMenuItem.Name = "viewCurrentAssetInfoToolStripMenuItem";
            this.viewCurrentAssetInfoToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.viewCurrentAssetInfoToolStripMenuItem.Text = "View Current Assets File Info";
            this.viewCurrentAssetInfoToolStripMenuItem.Click += new System.EventHandler(this.ViewCurrentAssetInfoToolStripMenuItem_Click);
            // 
            // globalSearchToolStripMenuItem
            // 
            this.globalSearchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.globalSearchInfoToolStripMenuItem,
            this.toolStripSeparator2,
            this.assetDataToolStripMenuItem,
            this.monoBehaviourToolStripMenuItem,
            this.pptrToolStripMenuItem});
            this.globalSearchToolStripMenuItem.Name = "globalSearchToolStripMenuItem";
            this.globalSearchToolStripMenuItem.Size = new System.Drawing.Size(91, 22);
            this.globalSearchToolStripMenuItem.Text = "Global Search";
            // 
            // globalSearchInfoToolStripMenuItem
            // 
            this.globalSearchInfoToolStripMenuItem.Enabled = false;
            this.globalSearchInfoToolStripMenuItem.Name = "globalSearchInfoToolStripMenuItem";
            this.globalSearchInfoToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.globalSearchInfoToolStripMenuItem.Text = "(These options search all files in a folder)";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(284, 6);
            // 
            // assetDataToolStripMenuItem
            // 
            this.assetDataToolStripMenuItem.Name = "assetDataToolStripMenuItem";
            this.assetDataToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.assetDataToolStripMenuItem.Text = "Asset Data";
            this.assetDataToolStripMenuItem.Click += new System.EventHandler(this.assetDataToolStripMenuItem_Click);
            // 
            // monoBehaviourToolStripMenuItem
            // 
            this.monoBehaviourToolStripMenuItem.Name = "monoBehaviourToolStripMenuItem";
            this.monoBehaviourToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.monoBehaviourToolStripMenuItem.Text = "MonoBehaviour";
            this.monoBehaviourToolStripMenuItem.Click += new System.EventHandler(this.monoBehaviourToolStripMenuItem_Click);
            // 
            // pptrToolStripMenuItem
            // 
            this.pptrToolStripMenuItem.Name = "pptrToolStripMenuItem";
            this.pptrToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.pptrToolStripMenuItem.Text = "PPtr";
            this.pptrToolStripMenuItem.Click += new System.EventHandler(this.pptrToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
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
            this.splitContainer1.Size = new System.Drawing.Size(817, 448);
            this.splitContainer1.SplitterDistance = 287;
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
            this.assetList.Location = new System.Drawing.Point(1, 32);
            this.assetList.Margin = new System.Windows.Forms.Padding(2);
            this.assetList.Name = "assetList";
            this.assetList.RowHeadersVisible = false;
            this.assetList.RowTemplate.Height = 28;
            this.assetList.Size = new System.Drawing.Size(526, 416);
            this.assetList.TabIndex = 4;
            this.assetList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_CellDoubleClick);
            this.assetList.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.AssetList_CellMouseDown);
            // 
            // IconCol
            // 
            this.IconCol.Frozen = true;
            this.IconCol.HeaderText = "";
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
            this.goDirectory.Location = new System.Drawing.Point(482, 5);
            this.goDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.goDirectory.Name = "goDirectory";
            this.goDirectory.Size = new System.Drawing.Size(42, 25);
            this.goDirectory.TabIndex = 3;
            this.goDirectory.Text = "Go";
            this.goDirectory.UseVisualStyleBackColor = true;
            this.goDirectory.Click += new System.EventHandler(this.GoDirectory_Click);
            // 
            // pathBox
            // 
            this.pathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pathBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.pathBox.Location = new System.Drawing.Point(48, 5);
            this.pathBox.Margin = new System.Windows.Forms.Padding(2);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(430, 25);
            this.pathBox.TabIndex = 2;
            this.pathBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PathBox_KeyPress);
            // 
            // upDirectory
            // 
            this.upDirectory.Location = new System.Drawing.Point(2, 5);
            this.upDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.upDirectory.Name = "upDirectory";
            this.upDirectory.Size = new System.Drawing.Size(42, 25);
            this.upDirectory.TabIndex = 1;
            this.upDirectory.Text = "Up";
            this.upDirectory.UseVisualStyleBackColor = true;
            this.upDirectory.Click += new System.EventHandler(this.upDirectory_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
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
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportTextureToolStripMenuItem,
            this.viewTextureToolStripMenuItem,
            this.xRefsToolStripMenuItem,
            this.propertiesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(141, 70);
            // 
            // viewTextureToolStripMenuItem
            // 
            this.viewTextureToolStripMenuItem.Name = "viewTextureToolStripMenuItem";
            this.viewTextureToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.viewTextureToolStripMenuItem.Text = "View Texture";
            this.viewTextureToolStripMenuItem.Visible = false;
            this.viewTextureToolStripMenuItem.Click += new System.EventHandler(this.viewTextureToolStripMenuItem_Click);
            //
            // exportTextureToolStripMenuItem
            //
            this.exportTextureToolStripMenuItem.Name = "exportTextureToolStripMenuItem";
            this.exportTextureToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.exportTextureToolStripMenuItem.Text = "Export Texture";
            this.exportTextureToolStripMenuItem.Visible = false;
            this.exportTextureToolStripMenuItem.Click += new System.EventHandler(this.exportTextureToolStripMenuItem_Click);
            // 
            // xRefsToolStripMenuItem
            // 
            this.xRefsToolStripMenuItem.Name = "xRefsToolStripMenuItem";
            this.xRefsToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.xRefsToolStripMenuItem.Text = "X-Refs";
            this.xRefsToolStripMenuItem.Click += new System.EventHandler(this.xRefsToolStripMenuItem_Click);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.PropertiesToolStripMenuItem_Click);
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.Frozen = true;
            this.dataGridViewImageColumn1.HeaderText = "";
            this.dataGridViewImageColumn1.MinimumWidth = 30;
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewImageColumn1.Width = 30;
            // 
            // StartScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 472);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartScreen";
            this.Text = "AssetsView .NET";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView assetTree;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button upDirectory;
        private System.Windows.Forms.Button goDirectory;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.DataGridView assetList;
        private System.Windows.Forms.DataGridViewImageColumn IconCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn IDCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeCol;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewCurrentAssetInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem updateDependenciesToolStripMenuItem;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem globalSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem globalSearchInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem monoBehaviourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assetDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xRefsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pptrToolStripMenuItem;
    }
}

