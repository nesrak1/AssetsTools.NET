namespace AssetsView.Winforms
{
    partial class AssetInfoViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetInfoViewer));
            this.properties = new System.Windows.Forms.ListView();
            this.property = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // properties
            // 
            this.properties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.properties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.property,
            this.value});
            this.properties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.properties.HideSelection = false;
            this.properties.Location = new System.Drawing.Point(0, 0);
            this.properties.Name = "properties";
            this.properties.Size = new System.Drawing.Size(285, 365);
            this.properties.TabIndex = 0;
            this.properties.UseCompatibleStateImageBehavior = false;
            this.properties.View = System.Windows.Forms.View.Details;
            // 
            // property
            // 
            this.property.Text = "Property";
            this.property.Width = 98;
            // 
            // value
            // 
            this.value.Text = "Value";
            this.value.Width = 179;
            // 
            // AssetInfoViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 365);
            this.Controls.Add(this.properties);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AssetInfoViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asset Properties";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView properties;
        private System.Windows.Forms.ColumnHeader property;
        private System.Windows.Forms.ColumnHeader value;
    }
}