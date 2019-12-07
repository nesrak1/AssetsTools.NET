using AssetsTools.NET;
using System;
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
            hdr_ffo.Text = $"{header.offs_firstFile.ToString()} (0x{header.offs_firstFile.ToString("x")})";
            hdr_en.Text = header.endianness == 1 ? "big endian" : "little endian";
            hdr_uvr.Text = typeTree.unityVersion;
            hdr_ver.Text = $"{typeTree.version.ToString()} (0x{typeTree.version.ToString("x")})";
            hdr_htt.Text = typeTree.hasTypeTree == true ? "true" : "false";
            //type tree
            if (!typeTree.hasTypeTree)
            {
                ttr_tree.Nodes.Add("There is no type tree data available.");
            }
            foreach (Type_0D type in typeTree.pTypes_Unity5)
            {
                if (type.typeFieldsExCount == 0)
                {
                    ClassDatabaseType cldt = cldb.classes.First(c => c.classId == type.classId);
                    ttr_list.Items.Add($"[{cldt.name.GetString(cldb)}] (0x{type.classId.ToString("x")})");
                }
                else
                {
                    TypeField_0D baseField = type.pTypeFieldsEx[0];
                    ttr_list.Items.Add($"{baseField.GetTypeString(type.pStringTable)} (0x{type.classId.ToString("x")})");
                }
            }
            //preload list
            foreach (AssetPPtr pptr in file.preloadTable.items)
            {
                string pptrFileName = "[self]";
                if (pptr.fileID != 0)
                    pptrFileName = file.dependencies.pDependencies[pptr.fileID-1].assetPath;
                plt_list.Items.Add(new ListViewItem(new[] { pptrFileName, pptr.pathID.ToString() }));
            }
            plt_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //dependencies
            foreach (AssetsFileDependency dep in file.dependencies.pDependencies)
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
            Type_0D type = file.typeTree.pTypes_Unity5[ttr_list.SelectedIndex];
            if (type.typeFieldsExCount == 0)
            {
                ClassDatabaseType cldt = cldb.classes.First(c => c.classId == type.classId);
                ttr_type.Text = cldt.name.GetString(cldb);
            }
            else
            {
                TypeField_0D baseField = type.pTypeFieldsEx[0];
                ttr_type.Text = baseField.GetTypeString(type.pStringTable);
            }
            ttr_typeid.Text = type.classId.ToString();
            ttr_scriptid.Text = type.scriptIndex.ToString();

            if (type.unknown5 != 0 || type.unknown6 != 0 || type.unknown7 != 0 || type.unknown8 != 0)
                ttr_hash.Text = $"{type.unknown5.ToString("x8")}{type.unknown6.ToString("x8")}{type.unknown7.ToString("x8")}{type.unknown8.ToString("x8")}";
            else
                ttr_hash.Text = "";

            if (type.unknown1 != 0 || type.unknown2 != 0 || type.unknown3 != 0 || type.unknown4 != 0)
                ttr_monohash.Text = $"{type.unknown1.ToString("x8")}{type.unknown2.ToString("x8")}{type.unknown3.ToString("x8")}{type.unknown4.ToString("x8")}";
            else
                ttr_monohash.Text = "";
        }
    }
}
