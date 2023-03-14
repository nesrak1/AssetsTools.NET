using AssetsTools.NET;
using AssetsTools.NET.Extra;
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
            AssetsFileHeader header = file.Header;
            AssetsFileMetadata metadata = file.Metadata;
            //header
            hdr_mds.Text = header.MetadataSize.ToString();
            hdr_fs.Text = header.FileSize.ToString();
            hdr_fmt.Text = $"{header.Version} (0x{header.Version:x})";
            hdr_ffo.Text = $"{header.DataOffset} (0x{header.DataOffset:x})";
            hdr_en.Text = header.Endianness ? "big endian" : "little endian";
            hdr_uvr.Text = metadata.UnityVersion;
            hdr_ver.Text = $"{metadata.TargetPlatform} (0x{metadata.TargetPlatform:x})";
            hdr_htt.Text = metadata.TypeTreeEnabled ? "true" : "false";
            //type tree
            if (!metadata.TypeTreeEnabled)
            {
                ttr_tree.Nodes.Add("There is no type tree data available.");
            }
            else
            {
                ttr_tree.Nodes.Add("Select a type to show the type tree data.");
            }
            foreach (TypeTreeType type in metadata.TypeTreeTypes)
            {
                if (type.Nodes == null || type.Nodes.Count == 0)
                {
                    ClassDatabaseType cldt = cldb.FindAssetClassByID(type.TypeId);
                    ttr_list.Items.Add($"[{cldb.GetString(cldt.Name)}] (0x{type.TypeId.ToString("x")})");
                }
                else
                {
                    TypeTreeNode baseField = type.Nodes[0];
                    ttr_list.Items.Add($"{baseField.GetTypeString(type.StringBuffer)} (0x{type.TypeId.ToString("x")})");
                }
            }
            foreach (TypeTreeType type in metadata.RefTypes)
            {
                if (type.Nodes == null || type.Nodes.Count == 0)
                {
                    ClassDatabaseType cldt = cldb.FindAssetClassByID(type.TypeId);
                    ttr_list.Items.Add($"[{cldb.GetString(cldt.Name)}] (0x{type.TypeId.ToString("x")}) REF");
                }
                else
                {
                    TypeTreeNode baseField = type.Nodes[0];
                    ttr_list.Items.Add($"{baseField.GetTypeString(type.StringBuffer)} (0x{type.TypeId.ToString("x")}) REF");
                }
            }
            //preload list
            foreach (AssetPPtr pptr in metadata.ScriptTypes)
            {
                string pptrFileName = "[self]";
                if (pptr.FileId != 0)
                    pptrFileName = metadata.Externals[pptr.FileId - 1].PathName;
                plt_list.Items.Add(new ListViewItem(new[] { pptrFileName, pptr.PathId.ToString() }));
            }
            plt_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //dependencies
            foreach (AssetsFileExternal dep in metadata.Externals)
            {
                string guid = string.Empty;
                if (!dep.Guid.IsEmpty)
                {
                    guid = dep.Guid.ToString();
                }
                dep_list.Items.Add(new ListViewItem(new[] { dep.PathName, dep.Type.ToString(), guid }));
            }
            dep_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void Ttr_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssetsFileMetadata metadata = file.Metadata;
            int index = ttr_list.SelectedIndex;

            TypeTreeType type;
            if (index >= metadata.TypeTreeTypes.Count)
            {
                type = metadata.RefTypes[index - metadata.TypeTreeTypes.Count];
            }
            else
            {
                type = metadata.TypeTreeTypes[ttr_list.SelectedIndex];
            }

            if (type.Nodes == null || type.Nodes.Count == 0)
            {
                ClassDatabaseType cldt = cldb.FindAssetClassByID(type.TypeId);
                ttr_type.Text = cldb.GetString(cldt.Name);
            }
            else
            {
                TypeTreeNode baseField = type.Nodes[0];
                ttr_type.Text = baseField.GetTypeString(type.StringBuffer);
            }
            ttr_typeid.Text = type.TypeId.ToString();
            ttr_scriptid.Text = type.ScriptTypeIndex.ToString();

            if (!type.TypeHash.IsZero())
                ttr_hash.Text = $"{type.TypeHash}";
            else
                ttr_hash.Text = "";

            if (!type.ScriptIdHash.IsZero())
                ttr_monohash.Text = $"{type.ScriptIdHash}";
            else
                ttr_monohash.Text = "";

            if (metadata.TypeTreeEnabled)
                GenerateTtrTree(type);
        }

        private void GenerateTtrTree(TypeTreeType type)
        {
            ttr_tree.Nodes.Clear();
            List<TreeNode> treeNodeStack = new List<TreeNode>();
            TypeTreeNode baseField = type.Nodes[0];
            TreeNode rootNode = ttr_tree.Nodes.Add(TypeFieldToString(baseField, type));
            rootNode.Tag = baseField;
            treeNodeStack.Add(rootNode);
            for (int i = 1; i < type.Nodes.Count; i++)
            {
                TypeTreeNode field = type.Nodes[i];
                TreeNode parentNode = treeNodeStack[field.Level - 1];
                TreeNode node = parentNode.Nodes.Add(TypeFieldToString(field, type));
                node.Tag = field;
                if (treeNodeStack.Count > field.Level)
                    treeNodeStack[field.Level] = node;
                else
                    treeNodeStack.Add(node);
            }
        }

        private string TypeFieldToString(TypeTreeNode node, TypeTreeType type)
        {
            string stringTable = type.StringBuffer;
            return $"{node.GetTypeString(stringTable)} {node.GetNameString(stringTable)}";
        }

        private void ttr_tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag != null)
            {
                TypeTreeNode field = (TypeTreeNode)node.Tag;
                ttr_version.Text = field.Version.ToString();
                ttr_depth.Text = field.Level.ToString();
                ttr_isarray.Text = Net35Polyfill.HasFlag(field.TypeFlags, TypeTreeNodeFlags.Array).ToString().ToLower();
                ttr_size.Text = field.ByteSize.ToString();
                ttr_index.Text = field.Index.ToString();
                ttr_flags.Text = field.TypeFlags.ToString();
                ttr_aligned.Text = "0x" + field.MetaFlags.ToString("X4");
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
