namespace UABE.NET.Winforms
{
    partial class AssetSearch
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
            this.searchResults = new System.Windows.Forms.ListBox();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.searchPosButton = new System.Windows.Forms.Button();
            this.SearchHexButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // searchResults
            // 
            this.searchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchResults.FormattingEnabled = true;
            this.searchResults.Location = new System.Drawing.Point(12, 38);
            this.searchResults.Name = "searchResults";
            this.searchResults.Size = new System.Drawing.Size(480, 290);
            this.searchResults.TabIndex = 0;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Location = new System.Drawing.Point(12, 12);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(261, 20);
            this.searchTextBox.TabIndex = 1;
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Location = new System.Drawing.Point(279, 12);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(73, 20);
            this.searchButton.TabIndex = 2;
            this.searchButton.Text = "Search Text";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // searchPosButton
            // 
            this.searchPosButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchPosButton.Location = new System.Drawing.Point(358, 12);
            this.searchPosButton.Name = "searchPosButton";
            this.searchPosButton.Size = new System.Drawing.Size(73, 20);
            this.searchPosButton.TabIndex = 3;
            this.searchPosButton.Text = "Search Pos";
            this.searchPosButton.UseVisualStyleBackColor = true;
            this.searchPosButton.Click += new System.EventHandler(this.searchPosButton_Click);
            // 
            // SearchHexButton
            // 
            this.SearchHexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchHexButton.Location = new System.Drawing.Point(437, 12);
            this.SearchHexButton.Name = "SearchHexButton";
            this.SearchHexButton.Size = new System.Drawing.Size(55, 20);
            this.SearchHexButton.TabIndex = 4;
            this.SearchHexButton.Text = "Hex";
            this.SearchHexButton.UseVisualStyleBackColor = true;
            this.SearchHexButton.Click += new System.EventHandler(this.SearchHexButton_Click);
            // 
            // AssetSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 347);
            this.Controls.Add(this.SearchHexButton);
            this.Controls.Add(this.searchPosButton);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.searchResults);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AssetSearch";
            this.Text = "Asset Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox searchResults;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Button searchPosButton;
        private System.Windows.Forms.Button SearchHexButton;
    }
}