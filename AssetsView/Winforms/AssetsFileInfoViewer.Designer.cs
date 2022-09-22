namespace AssetsView.Winforms
{
    partial class AssetsFileInfoViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssetsFileInfoViewer));
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dep_list = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.plt_list = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2_ = new System.Windows.Forms.ColumnHeader();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ttr_tree = new System.Windows.Forms.TreeView();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.ttr_flags = new System.Windows.Forms.TextBox();
            this.ttr_index = new System.Windows.Forms.TextBox();
            this.ttr_size = new System.Windows.Forms.TextBox();
            this.ttr_isarray = new System.Windows.Forms.TextBox();
            this.ttr_depth = new System.Windows.Forms.TextBox();
            this.ttr_version = new System.Windows.Forms.TextBox();
            this.ttr_monohash = new System.Windows.Forms.TextBox();
            this.ttr_hash = new System.Windows.Forms.TextBox();
            this.ttr_scriptid = new System.Windows.Forms.TextBox();
            this.ttr_typeid = new System.Windows.Forms.TextBox();
            this.ttr_type = new System.Windows.Forms.TextBox();
            this.ttr_list = new System.Windows.Forms.ListBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.hdr_uvr = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.hdr_ver = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.hdr_htt = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.hdr_en = new System.Windows.Forms.TextBox();
            this.hdr_mds = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.hdr_ffo = new System.Windows.Forms.TextBox();
            this.hdr_fs = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.hdr_fmt = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.label20 = new System.Windows.Forms.Label();
            this.ttr_aligned = new System.Windows.Forms.TextBox();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.dep_list);
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(716, 362);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Dependencies";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // dep_list
            // 
            this.dep_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dep_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.dep_list.HideSelection = false;
            this.dep_list.Location = new System.Drawing.Point(4, 3);
            this.dep_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dep_list.Name = "dep_list";
            this.dep_list.Size = new System.Drawing.Size(708, 352);
            this.dep_list.TabIndex = 1;
            this.dep_list.UseCompatibleStateImageBehavior = false;
            this.dep_list.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "File";
            this.columnHeader3.Width = 10;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Type";
            this.columnHeader4.Width = 10;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "GUID";
            this.columnHeader5.Width = 10;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.plt_list);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(716, 362);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Preload Table";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // plt_list
            // 
            this.plt_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plt_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2_});
            this.plt_list.HideSelection = false;
            this.plt_list.Location = new System.Drawing.Point(4, 3);
            this.plt_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.plt_list.Name = "plt_list";
            this.plt_list.Size = new System.Drawing.Size(708, 352);
            this.plt_list.TabIndex = 0;
            this.plt_list.UseCompatibleStateImageBehavior = false;
            this.plt_list.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 10;
            // 
            // columnHeader2_
            // 
            this.columnHeader2_.Text = "Path";
            this.columnHeader2_.Width = 10;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer1);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Size = new System.Drawing.Size(716, 416);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Type Tree";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ttr_tree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label20);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_aligned);
            this.splitContainer1.Panel2.Controls.Add(this.label19);
            this.splitContainer1.Panel2.Controls.Add(this.label18);
            this.splitContainer1.Panel2.Controls.Add(this.label17);
            this.splitContainer1.Panel2.Controls.Add(this.label16);
            this.splitContainer1.Panel2.Controls.Add(this.label15);
            this.splitContainer1.Panel2.Controls.Add(this.label14);
            this.splitContainer1.Panel2.Controls.Add(this.label13);
            this.splitContainer1.Panel2.Controls.Add(this.label12);
            this.splitContainer1.Panel2.Controls.Add(this.label11);
            this.splitContainer1.Panel2.Controls.Add(this.label10);
            this.splitContainer1.Panel2.Controls.Add(this.label9);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_flags);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_index);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_size);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_isarray);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_depth);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_version);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_monohash);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_hash);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_scriptid);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_typeid);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_type);
            this.splitContainer1.Panel2.Controls.Add(this.ttr_list);
            this.splitContainer1.Size = new System.Drawing.Size(715, 414);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // ttr_tree
            // 
            this.ttr_tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_tree.Location = new System.Drawing.Point(4, 3);
            this.ttr_tree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_tree.Name = "ttr_tree";
            this.ttr_tree.Size = new System.Drawing.Size(233, 403);
            this.ttr_tree.TabIndex = 0;
            this.ttr_tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ttr_tree_AfterSelect);
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label19.Location = new System.Drawing.Point(243, 333);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(77, 23);
            this.label19.TabIndex = 2;
            this.label19.Text = "Flags";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label18.Location = new System.Drawing.Point(243, 303);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(77, 23);
            this.label18.TabIndex = 2;
            this.label18.Text = "Index";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label17.Location = new System.Drawing.Point(243, 273);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(77, 23);
            this.label17.TabIndex = 2;
            this.label17.Text = "Size";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label16.Location = new System.Drawing.Point(243, 243);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 23);
            this.label16.TabIndex = 2;
            this.label16.Text = "Is Array";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.Location = new System.Drawing.Point(243, 213);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 23);
            this.label15.TabIndex = 2;
            this.label15.Text = "Depth";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.Location = new System.Drawing.Point(243, 183);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 23);
            this.label14.TabIndex = 2;
            this.label14.Text = "Version";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.Location = new System.Drawing.Point(243, 123);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 23);
            this.label13.TabIndex = 2;
            this.label13.Text = "Mono Hash";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.Location = new System.Drawing.Point(243, 93);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(77, 23);
            this.label12.TabIndex = 2;
            this.label12.Text = "Hash";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.Location = new System.Drawing.Point(243, 63);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 23);
            this.label11.TabIndex = 2;
            this.label11.Text = "Script ID";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Location = new System.Drawing.Point(243, 33);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 23);
            this.label10.TabIndex = 2;
            this.label10.Text = "Type ID";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Location = new System.Drawing.Point(243, 3);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 23);
            this.label9.TabIndex = 2;
            this.label9.Text = "Type";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ttr_flags
            // 
            this.ttr_flags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_flags.Location = new System.Drawing.Point(327, 333);
            this.ttr_flags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_flags.Name = "ttr_flags";
            this.ttr_flags.ReadOnly = true;
            this.ttr_flags.Size = new System.Drawing.Size(131, 23);
            this.ttr_flags.TabIndex = 3;
            // 
            // ttr_index
            // 
            this.ttr_index.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_index.Location = new System.Drawing.Point(327, 303);
            this.ttr_index.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_index.Name = "ttr_index";
            this.ttr_index.ReadOnly = true;
            this.ttr_index.Size = new System.Drawing.Size(131, 23);
            this.ttr_index.TabIndex = 3;
            // 
            // ttr_size
            // 
            this.ttr_size.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_size.Location = new System.Drawing.Point(327, 273);
            this.ttr_size.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_size.Name = "ttr_size";
            this.ttr_size.ReadOnly = true;
            this.ttr_size.Size = new System.Drawing.Size(131, 23);
            this.ttr_size.TabIndex = 3;
            // 
            // ttr_isarray
            // 
            this.ttr_isarray.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_isarray.Location = new System.Drawing.Point(327, 243);
            this.ttr_isarray.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_isarray.Name = "ttr_isarray";
            this.ttr_isarray.ReadOnly = true;
            this.ttr_isarray.Size = new System.Drawing.Size(131, 23);
            this.ttr_isarray.TabIndex = 3;
            // 
            // ttr_depth
            // 
            this.ttr_depth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_depth.Location = new System.Drawing.Point(327, 213);
            this.ttr_depth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_depth.Name = "ttr_depth";
            this.ttr_depth.ReadOnly = true;
            this.ttr_depth.Size = new System.Drawing.Size(131, 23);
            this.ttr_depth.TabIndex = 3;
            // 
            // ttr_version
            // 
            this.ttr_version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_version.Location = new System.Drawing.Point(327, 183);
            this.ttr_version.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_version.Name = "ttr_version";
            this.ttr_version.ReadOnly = true;
            this.ttr_version.Size = new System.Drawing.Size(131, 23);
            this.ttr_version.TabIndex = 3;
            // 
            // ttr_monohash
            // 
            this.ttr_monohash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_monohash.Location = new System.Drawing.Point(327, 123);
            this.ttr_monohash.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_monohash.Name = "ttr_monohash";
            this.ttr_monohash.ReadOnly = true;
            this.ttr_monohash.Size = new System.Drawing.Size(131, 23);
            this.ttr_monohash.TabIndex = 3;
            // 
            // ttr_hash
            // 
            this.ttr_hash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_hash.Location = new System.Drawing.Point(327, 93);
            this.ttr_hash.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_hash.Name = "ttr_hash";
            this.ttr_hash.ReadOnly = true;
            this.ttr_hash.Size = new System.Drawing.Size(131, 23);
            this.ttr_hash.TabIndex = 3;
            // 
            // ttr_scriptid
            // 
            this.ttr_scriptid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_scriptid.Location = new System.Drawing.Point(327, 63);
            this.ttr_scriptid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_scriptid.Name = "ttr_scriptid";
            this.ttr_scriptid.ReadOnly = true;
            this.ttr_scriptid.Size = new System.Drawing.Size(131, 23);
            this.ttr_scriptid.TabIndex = 3;
            // 
            // ttr_typeid
            // 
            this.ttr_typeid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_typeid.Location = new System.Drawing.Point(327, 33);
            this.ttr_typeid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_typeid.Name = "ttr_typeid";
            this.ttr_typeid.ReadOnly = true;
            this.ttr_typeid.Size = new System.Drawing.Size(131, 23);
            this.ttr_typeid.TabIndex = 3;
            // 
            // ttr_type
            // 
            this.ttr_type.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_type.Location = new System.Drawing.Point(327, 3);
            this.ttr_type.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_type.Name = "ttr_type";
            this.ttr_type.ReadOnly = true;
            this.ttr_type.Size = new System.Drawing.Size(131, 23);
            this.ttr_type.TabIndex = 3;
            // 
            // ttr_list
            // 
            this.ttr_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_list.FormattingEnabled = true;
            this.ttr_list.IntegralHeight = false;
            this.ttr_list.ItemHeight = 15;
            this.ttr_list.Location = new System.Drawing.Point(4, 3);
            this.ttr_list.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_list.Name = "ttr_list";
            this.ttr_list.Size = new System.Drawing.Size(232, 403);
            this.ttr_list.TabIndex = 0;
            this.ttr_list.SelectedIndexChanged += new System.EventHandler(this.Ttr_list_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Size = new System.Drawing.Size(716, 362);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Asset/Type Tree Headers";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.hdr_uvr);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.hdr_ver);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.hdr_htt);
            this.groupBox2.Location = new System.Drawing.Point(7, 194);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(701, 117);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Type Tree Info";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(7, 18);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(127, 23);
            this.label6.TabIndex = 0;
            this.label6.Text = "Unity Version";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_uvr
            // 
            this.hdr_uvr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_uvr.Location = new System.Drawing.Point(141, 18);
            this.hdr_uvr.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_uvr.Name = "hdr_uvr";
            this.hdr_uvr.ReadOnly = true;
            this.hdr_uvr.Size = new System.Drawing.Size(552, 23);
            this.hdr_uvr.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(7, 48);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 23);
            this.label7.TabIndex = 0;
            this.label7.Text = "Version";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_ver
            // 
            this.hdr_ver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_ver.Location = new System.Drawing.Point(141, 48);
            this.hdr_ver.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_ver.Name = "hdr_ver";
            this.hdr_ver.ReadOnly = true;
            this.hdr_ver.Size = new System.Drawing.Size(552, 23);
            this.hdr_ver.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(7, 78);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(127, 23);
            this.label8.TabIndex = 0;
            this.label8.Text = "Has Type Tree";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_htt
            // 
            this.hdr_htt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_htt.Location = new System.Drawing.Point(141, 78);
            this.hdr_htt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_htt.Name = "hdr_htt";
            this.hdr_htt.ReadOnly = true;
            this.hdr_htt.Size = new System.Drawing.Size(552, 23);
            this.hdr_htt.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.hdr_en);
            this.groupBox1.Controls.Add(this.hdr_mds);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.hdr_ffo);
            this.groupBox1.Controls.Add(this.hdr_fs);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.hdr_fmt);
            this.groupBox1.Location = new System.Drawing.Point(7, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(701, 180);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Asset Info";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Metadata Size";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_en
            // 
            this.hdr_en.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_en.Location = new System.Drawing.Point(141, 138);
            this.hdr_en.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_en.Name = "hdr_en";
            this.hdr_en.ReadOnly = true;
            this.hdr_en.Size = new System.Drawing.Size(552, 23);
            this.hdr_en.TabIndex = 1;
            // 
            // hdr_mds
            // 
            this.hdr_mds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_mds.Location = new System.Drawing.Point(141, 18);
            this.hdr_mds.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_mds.Name = "hdr_mds";
            this.hdr_mds.ReadOnly = true;
            this.hdr_mds.Size = new System.Drawing.Size(552, 23);
            this.hdr_mds.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 138);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 23);
            this.label5.TabIndex = 0;
            this.label5.Text = "Endianness";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(7, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "File Size";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_ffo
            // 
            this.hdr_ffo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_ffo.Location = new System.Drawing.Point(141, 108);
            this.hdr_ffo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_ffo.Name = "hdr_ffo";
            this.hdr_ffo.ReadOnly = true;
            this.hdr_ffo.Size = new System.Drawing.Size(552, 23);
            this.hdr_ffo.TabIndex = 1;
            // 
            // hdr_fs
            // 
            this.hdr_fs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_fs.Location = new System.Drawing.Point(141, 48);
            this.hdr_fs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_fs.Name = "hdr_fs";
            this.hdr_fs.ReadOnly = true;
            this.hdr_fs.Size = new System.Drawing.Size(552, 23);
            this.hdr_fs.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 108);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "First File Offset";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 78);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 23);
            this.label3.TabIndex = 0;
            this.label3.Text = "Format";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hdr_fmt
            // 
            this.hdr_fmt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hdr_fmt.Location = new System.Drawing.Point(141, 78);
            this.hdr_fmt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hdr_fmt.Name = "hdr_fmt";
            this.hdr_fmt.ReadOnly = true;
            this.hdr_fmt.Size = new System.Drawing.Size(552, 23);
            this.hdr_fmt.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(14, 14);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(724, 444);
            this.tabControl1.TabIndex = 0;
            // 
            // label20
            // 
            this.label20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label20.Location = new System.Drawing.Point(243, 363);
            this.label20.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(77, 23);
            this.label20.TabIndex = 4;
            this.label20.Text = "Aligned";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ttr_aligned
            // 
            this.ttr_aligned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ttr_aligned.Location = new System.Drawing.Point(327, 363);
            this.ttr_aligned.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ttr_aligned.Name = "ttr_aligned";
            this.ttr_aligned.ReadOnly = true;
            this.ttr_aligned.Size = new System.Drawing.Size(131, 23);
            this.ttr_aligned.TabIndex = 5;
            // 
            // AssetsFileInfoViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 472);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "AssetsFileInfoViewer";
            this.Text = "Assets File Info Viewer";
            this.Load += new System.EventHandler(this.AssetInfo_Load);
            this.tabPage4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ListView dep_list;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListView plt_list;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView ttr_tree;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ttr_flags;
        private System.Windows.Forms.TextBox ttr_index;
        private System.Windows.Forms.TextBox ttr_size;
        private System.Windows.Forms.TextBox ttr_isarray;
        private System.Windows.Forms.TextBox ttr_depth;
        private System.Windows.Forms.TextBox ttr_version;
        private System.Windows.Forms.TextBox ttr_monohash;
        private System.Windows.Forms.TextBox ttr_hash;
        private System.Windows.Forms.TextBox ttr_scriptid;
        private System.Windows.Forms.TextBox ttr_typeid;
        private System.Windows.Forms.TextBox ttr_type;
        private System.Windows.Forms.ListBox ttr_list;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox hdr_uvr;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox hdr_ver;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox hdr_htt;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hdr_en;
        private System.Windows.Forms.TextBox hdr_mds;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox hdr_ffo;
        private System.Windows.Forms.TextBox hdr_fs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox hdr_fmt;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2_;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox ttr_aligned;
    }
}