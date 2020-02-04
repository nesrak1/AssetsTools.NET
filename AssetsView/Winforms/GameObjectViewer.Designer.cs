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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.followBtn = new System.Windows.Forms.Button();
            this.valueGrid = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.goTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.followBtn);
            this.splitContainer1.Panel2.Controls.Add(this.valueGrid);
            this.splitContainer1.Size = new System.Drawing.Size(719, 438);
            this.splitContainer1.SplitterDistance = 259;
            this.splitContainer1.TabIndex = 1;
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
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameObjectViewer";
            this.Text = "GameObject Viewer";
            this.Load += new System.EventHandler(this.GameObjectViewer_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView goTree;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PropertyGrid valueGrid;
        private System.Windows.Forms.Button followBtn;
    }
}