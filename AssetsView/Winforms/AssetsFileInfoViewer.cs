using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class AssetsFileInfoViewer : Form
    {
        AssetsFile file;
        ClassDatabaseFile cldb;
        public AssetsFileInfoViewer(AssetsFile file, ClassDatabaseFile cldb)
        {
            InitializeComponent();
            this.file = file;
            this.cldb = cldb;
        }

        private void AssetInfo_Load(object sender, EventArgs e)
        {
            AssetsFileHeader header = file.header;
            TypeTree typeTree = file.typeTree;
            //header
            hdr_mds.Text = header.metadataSize.ToString();
            hdr_fs.Text = header.fileSize.ToString();
            hdr_fmt.Text = $"{header.format.ToString()} (0x{header.format.ToString("x")})";
            hdr_ffo.Text = $"{header.firstFileOffset.ToString()} (0x{header.firstFileOffset.ToString("x")})";
            hdr_en.Text = header.endianness == 1 ? "big endian" : "little endian";
            hdr_uvr.Text = typeTree.unityVersion;
            hdr_ver.Text = $"{typeTree.version.ToString()} (0x{typeTree.version.ToString("x")})";
            hdr_htt.Text = typeTree.hasTypeTree == true ? "true" : "false";
            //type tree
            if (!typeTree.hasTypeTree)
            {
                ttr_tree.Nodes.Add("There is no type tree data available.");
            }
            else
            {
                ttr_tree.Nodes.Add("Select a type to show the type tree data.");
            }
            foreach (Type_0D type in typeTree.unity5Types)
            {
                if (type.typeFieldsExCount == 0)
                {
                    ClassDatabaseType cldt = cldb.classes.First(c => c.classId == type.classId);
                    ttr_list.Items.Add($"[{cldt.name.GetString(cldb)}] (0x{type.classId.ToString("x")})");
                }
                else
                {
                    TypeField_0D baseField = type.typeFieldsEx[0];
                    ttr_list.Items.Add($"{baseField.GetTypeString(type.stringTable)} (0x{type.classId.ToString("x")})");
                }
            }
            //preload list
            foreach (AssetPPtr pptr in file.preloadTable.items)
            {
                string pptrFileName = "[self]";
                if (pptr.fileID != 0)
                    pptrFileName = file.dependencies.dependencies[pptr.fileID-1].assetPath;
                plt_list.Items.Add(new ListViewItem(new[] { pptrFileName, pptr.pathID.ToString() }));
            }
            plt_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //dependencies
            foreach (AssetsFileDependency dep in file.dependencies.dependencies)
            {
                string guid = string.Empty;
                if (dep.guid.mostSignificant != 0 || dep.guid.leastSignificant != 0)
                {
                    guid = $"{dep.guid.mostSignificant.ToString("x8")}{dep.guid.leastSignificant.ToString("x8")}";
                }
                dep_list.Items.Add(new ListViewItem(new[] { dep.assetPath, "0x" + dep.type.ToString(), guid }));
            }
            dep_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void Ttr_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            TypeTree typeTree = file.typeTree;
            Type_0D type = typeTree.unity5Types[ttr_list.SelectedIndex];
            if (type.typeFieldsExCount == 0)
            {
                ClassDatabaseType cldt = cldb.classes.First(c => c.classId == type.classId);
                ttr_type.Text = cldt.name.GetString(cldb);
            }
            else
            {
                TypeField_0D baseField = type.typeFieldsEx[0];
                ttr_type.Text = baseField.GetTypeString(type.stringTable);
            }
            ttr_typeid.Text = type.classId.ToString();
            ttr_scriptid.Text = type.scriptIndex.ToString();

            if (type.typeHash1 != 0 || type.typeHash2 != 0 || type.typeHash3 != 0 || type.typeHash4 != 0)
                ttr_hash.Text = $"{type.typeHash1.ToString("x8")}{type.typeHash2.ToString("x8")}{type.typeHash3.ToString("x8")}{type.typeHash4.ToString("x8")}";
            else
                ttr_hash.Text = "";

            if (type.scriptHash1 != 0 || type.scriptHash2 != 0 || type.scriptHash3 != 0 || type.scriptHash4 != 0)
                ttr_monohash.Text = $"{type.scriptHash1.ToString("x8")}{type.scriptHash2.ToString("x8")}{type.scriptHash3.ToString("x8")}{type.scriptHash4.ToString("x8")}";
            else
                ttr_monohash.Text = "";

            if (typeTree.hasTypeTree)
                GenerateTtrTree(type);
        }

        private void GenerateTtrTree(Type_0D type)
        {
            ttr_tree.Nodes.Clear();
            List<TreeNode> treeNodeStack = new List<TreeNode>();
            TypeField_0D baseField = type.typeFieldsEx[0];
            TreeNode rootNode = ttr_tree.Nodes.Add(TypeFieldToString(baseField, type));
            rootNode.Tag = baseField;
            treeNodeStack.Add(rootNode);
            for (int i = 1; i < type.typeFieldsEx.Length; i++)
            {
                TypeField_0D field = type.typeFieldsEx[i];
                TreeNode parentNode = treeNodeStack[field.depth - 1];
                TreeNode node = parentNode.Nodes.Add(TypeFieldToString(field, type));
                node.Tag = field;
                if (treeNodeStack.Count > field.depth)
                    treeNodeStack[field.depth] = node;
                else
                    treeNodeStack.Add(node);
            }
        }

        private string TypeFieldToString(TypeField_0D field, Type_0D type)
        {
            string stringTable = type.stringTable;
            return $"{field.GetTypeString(stringTable)} {field.GetNameString(stringTable)}";
        }

        private void ttr_tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag != null)
            {
                TypeField_0D field = (TypeField_0D)node.Tag;
                ttr_version.Text = field.version.ToString();
                ttr_depth.Text = field.depth.ToString();
                ttr_isarray.Text = (field.isArray == 1).ToString().ToLower();
                ttr_size.Text = field.size.ToString();
                ttr_index.Text = field.index.ToString();
                ttr_flags.Text = "0x" + field.flags.ToString("X4");
            }
            else
            {
                ttr_version.Text = string.Empty;
                ttr_depth.Text = string.Empty;
                ttr_isarray.Text = string.Empty;
                ttr_size.Text = string.Empty;
                ttr_index.Text = string.Empty;
                ttr_flags.Text = string.Empty;
            }
        }
    }
}
