namespace AssetsView.Winforms
{
    partial class GameObjectViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameObjectViewer));
            this.goTree = new System.Windows.Forms.TreeView();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.followBtn = new System.Windows.Forms.Button();
            this.valueGrid = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // goTree
            // 
            this.goTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.goTree.Location = new System.Drawing.Point(0, 0);
            this.goTree.Name = "goTree";
            this.goTree.Size = new System.Drawing.Size(259, 438);
            this.goTree.TabIndex = 0;
            this.goTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.goTree_AfterSelect);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.goTree);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.followBtn);
            this.splitContainer.Panel2.Controls.Add(this.valueGrid);
            this.splitContainer.Size = new System.Drawing.Size(719, 438);
            this.splitContainer.SplitterDistance = 259;
            this.splitContainer.TabIndex = 1;
            // 
            // followBtn
            // 
            this.followBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.followBtn.Location = new System.Drawing.Point(3, 412);
            this.followBtn.Name = "followBtn";
            this.followBtn.Size = new System.Drawing.Size(448, 23);
            this.followBtn.TabIndex = 3;
            this.followBtn.Text = "Follow Reference";
            this.followBtn.UseVisualStyleBackColor = true;
            this.followBtn.Click += new System.EventHandler(this.FollowBtn_Click);
            // 
            // valueGrid
            // 
            this.valueGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueGrid.HelpVisible = false;
            this.valueGrid.Location = new System.Drawing.Point(3, 3);
            this.valueGrid.Name = "valueGrid";
            this.valueGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.valueGrid.Size = new System.Drawing.Size(448, 406);
            this.valueGrid.TabIndex = 2;
            this.valueGrid.ToolbarVisible = false;
            // 
            // GameObjectViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 438);
            this.Controls.Add(this.splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameObjectViewer";
            this.Text = "GameObject Viewer";
            this.Load += new System.EventHandler(this.GameObjectViewer_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView goTree;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.PropertyGrid valueGrid;
        private System.Windows.Forms.Button followBtn;
    }
}