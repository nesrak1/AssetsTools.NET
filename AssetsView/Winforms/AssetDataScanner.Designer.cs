namespace AssetsView.Winforms
{
    partial class AssetDataScanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetDataScanner));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.fileTree = new System.Windows.Forms.TreeView();
            this.matchesList = new System.Windows.Forms.ListBox();
            this.searchBar = new System.Windows.Forms.TextBox();
            this.searchStringBtn = new System.Windows.Forms.Button();
            this.searchHexBtn = new System.Windows.Forms.Button();
            this.searchPositionBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(0, 28);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.fileTree);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.matchesList);
            this.splitContainer.Size = new System.Drawing.Size(506, 289);
            this.splitContainer.SplitterDistance = 204;
            this.splitContainer.TabIndex = 0;
            // 
            // fileTree
            // 
            this.fileTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileTree.Location = new System.Drawing.Point(0, 0);
            this.fileTree.Name = "fileTree";
            this.fileTree.Size = new System.Drawing.Size(204, 289);
            this.fileTree.TabIndex = 1;
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.scriptTree_AfterSelect);
            // 
            // matchesList
            // 
            this.matchesList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.matchesList.FormattingEnabled = true;
            this.matchesList.IntegralHeight = false;
            this.matchesList.Location = new System.Drawing.Point(0, 0);
            this.matchesList.Name = "matchesList";
            this.matchesList.Size = new System.Drawing.Size(298, 289);
            this.matchesList.TabIndex = 0;
            this.matchesList.DoubleClick += new System.EventHandler(this.matchesList_DoubleClick);
            // 
            // searchBar
            // 
            this.searchBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBar.Location = new System.Drawing.Point(4, 4);
            this.searchBar.Name = "searchBar";
            this.searchBar.Size = new System.Drawing.Size(296, 20);
            this.searchBar.TabIndex = 1;
            // 
            // searchStringBtn
            // 
            this.searchStringBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchStringBtn.Location = new System.Drawing.Point(306, 4);
            this.searchStringBtn.Name = "searchStringBtn";
            this.searchStringBtn.Size = new System.Drawing.Size(57, 20);
            this.searchStringBtn.TabIndex = 2;
            this.searchStringBtn.Text = "By String";
            this.searchStringBtn.UseVisualStyleBackColor = true;
            this.searchStringBtn.Click += new System.EventHandler(this.searchStringBtn_Click);
            // 
            // searchHexBtn
            // 
            this.searchHexBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchHexBtn.Location = new System.Drawing.Point(369, 4);
            this.searchHexBtn.Name = "searchHexBtn";
            this.searchHexBtn.Size = new System.Drawing.Size(57, 20);
            this.searchHexBtn.TabIndex = 2;
            this.searchHexBtn.Text = "By Hex";
            this.searchHexBtn.UseVisualStyleBackColor = true;
            this.searchHexBtn.Click += new System.EventHandler(this.searchHexBtn_Click);
            // 
            // searchPositionBtn
            // 
            this.searchPositionBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchPositionBtn.Location = new System.Drawing.Point(432, 4);
            this.searchPositionBtn.Name = "searchPositionBtn";
            this.searchPositionBtn.Size = new System.Drawing.Size(71, 20);
            this.searchPositionBtn.TabIndex = 2;
            this.searchPositionBtn.Text = "By Position";
            this.searchPositionBtn.UseVisualStyleBackColor = true;
            this.searchPositionBtn.Click += new System.EventHandler(this.searchPositionBtn_Click);
            // 
            // AssetDataScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 317);
            this.Controls.Add(this.searchPositionBtn);
            this.Controls.Add(this.searchHexBtn);
            this.Controls.Add(this.searchStringBtn);
            this.Controls.Add(this.searchBar);
            this.Controls.Add(this.splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AssetDataScanner";
            this.Text = "Asset Data Scanner";
            this.Load += new System.EventHandler(this.AssetDataScanner_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.ListBox matchesList;
        private System.Windows.Forms.TextBox searchBar;
        private System.Windows.Forms.Button searchStringBtn;
        private System.Windows.Forms.Button searchHexBtn;
        private System.Windows.Forms.Button searchPositionBtn;
    }
}