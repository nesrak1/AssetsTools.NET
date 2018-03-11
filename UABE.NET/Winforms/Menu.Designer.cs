namespace UABE.NET
{
    partial class Menu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPackageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modMakerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createStandaloneexeInstallerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createPackageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editTypeDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editTypePackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileName = new System.Windows.Forms.Label();
            this.bundleContents = new System.Windows.Forms.ComboBox();
            this.exportButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.infoButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(370, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.loadPackageFileToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.modMakerToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // loadPackageFileToolStripMenuItem
            // 
            this.loadPackageFileToolStripMenuItem.Enabled = false;
            this.loadPackageFileToolStripMenuItem.Name = "loadPackageFileToolStripMenuItem";
            this.loadPackageFileToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.loadPackageFileToolStripMenuItem.Text = "&Load Package File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            // 
            // modMakerToolStripMenuItem
            // 
            this.modMakerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createStandaloneexeInstallerToolStripMenuItem,
            this.createPackageFileToolStripMenuItem});
            this.modMakerToolStripMenuItem.Enabled = false;
            this.modMakerToolStripMenuItem.Name = "modMakerToolStripMenuItem";
            this.modMakerToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.modMakerToolStripMenuItem.Text = "Mod Maker";
            // 
            // createStandaloneexeInstallerToolStripMenuItem
            // 
            this.createStandaloneexeInstallerToolStripMenuItem.Name = "createStandaloneexeInstallerToolStripMenuItem";
            this.createStandaloneexeInstallerToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.createStandaloneexeInstallerToolStripMenuItem.Text = "Create standalone .exe installer";
            // 
            // createPackageFileToolStripMenuItem
            // 
            this.createPackageFileToolStripMenuItem.Name = "createPackageFileToolStripMenuItem";
            this.createPackageFileToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.createPackageFileToolStripMenuItem.Text = "Create package file";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editTypeDatabaseToolStripMenuItem,
            this.editTypePackageToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // editTypeDatabaseToolStripMenuItem
            // 
            this.editTypeDatabaseToolStripMenuItem.Enabled = false;
            this.editTypeDatabaseToolStripMenuItem.Name = "editTypeDatabaseToolStripMenuItem";
            this.editTypeDatabaseToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.editTypeDatabaseToolStripMenuItem.Text = "Edit Type &Database";
            // 
            // editTypePackageToolStripMenuItem
            // 
            this.editTypePackageToolStripMenuItem.Enabled = false;
            this.editTypePackageToolStripMenuItem.Name = "editTypePackageToolStripMenuItem";
            this.editTypePackageToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.editTypePackageToolStripMenuItem.Text = "Edit Type &Package";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // fileName
            // 
            this.fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileName.Location = new System.Drawing.Point(12, 24);
            this.fileName.Name = "fileName";
            this.fileName.Size = new System.Drawing.Size(346, 13);
            this.fileName.TabIndex = 1;
            this.fileName.Text = "No file opened.";
            this.fileName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // bundleContents
            // 
            this.bundleContents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bundleContents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bundleContents.Enabled = false;
            this.bundleContents.FormattingEnabled = true;
            this.bundleContents.Location = new System.Drawing.Point(12, 40);
            this.bundleContents.Name = "bundleContents";
            this.bundleContents.Size = new System.Drawing.Size(346, 21);
            this.bundleContents.TabIndex = 2;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Enabled = false;
            this.exportButton.Location = new System.Drawing.Point(3, 3);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(109, 25);
            this.exportButton.TabIndex = 3;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importButton.Enabled = false;
            this.importButton.Location = new System.Drawing.Point(118, 3);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(109, 25);
            this.importButton.TabIndex = 4;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // infoButton
            // 
            this.infoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoButton.Enabled = false;
            this.infoButton.Location = new System.Drawing.Point(233, 3);
            this.infoButton.Name = "infoButton";
            this.infoButton.Size = new System.Drawing.Size(110, 25);
            this.infoButton.TabIndex = 5;
            this.infoButton.Text = "Info";
            this.infoButton.UseVisualStyleBackColor = true;
            this.infoButton.Click += new System.EventHandler(this.infoButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.infoButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.importButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.exportButton, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 67);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(346, 31);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // Menu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 110);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.bundleContents);
            this.Controls.Add(this.fileName);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "Menu";
            this.Text = "Unity Assets Bundle Extractor .NET";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPackageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modMakerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createStandaloneexeInstallerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createPackageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTypeDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTypePackageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Label fileName;
        private System.Windows.Forms.ComboBox bundleContents;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button infoButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

