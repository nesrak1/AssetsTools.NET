using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsView.AssetHelpers;
using AssetsView.Structs;
using AssetsView.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class StartScreen : Form
    {
        private AssetsManager helper;
        private AssetsFileInstance currentFile;
        private FSDirectory rootDir;
        private FSDirectory currentDir;
        private bool rsrcDataAdded;
        private PPtrMap pptrMap;

        public StartScreen()
        {
            InitializeComponent();

            assetList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            assetList.RowPrePaint += new DataGridViewRowPrePaintEventHandler(prePaint);
            assetList.Rows.Add(imageList.Images[1], "Open an asset with", "", "");
            assetList.Rows.Add(imageList.Images[1], "File > Add File", "", "");

            rsrcDataAdded = false;

            helper = new AssetsManager();
            helper.UpdateAfterLoad = false;
            if (!File.Exists("classdata.tpk"))
            {
                MessageBox.Show("classdata.tpk could not be found. Make sure it exists and restart.", "Assets View");
                Application.Exit();
            }
            helper.LoadClassPackage("classdata.tpk");
        }

        private void prePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string possibleBundleHeader;
                int possibleFormat;
                string emptyVersion;
                using (FileStream fs = File.OpenRead(ofd.FileName))
                using (AssetsFileReader reader = new AssetsFileReader(fs))
                {
                    reader.BigEndian = true;

                    if (fs.Length < 0x20)
                    {
                        MessageBox.Show("File too small. Are you sure this is a unity file?", "Assets View");
                        return;
                    }
                    possibleBundleHeader = reader.ReadStringLength(7);
                    reader.Position = 0x08;
                    possibleFormat = reader.ReadInt32();
                    reader.Position = 0x14;

                    string possibleVersion = "";
                    char curChar;
                    while (reader.Position < reader.BaseStream.Length && (curChar = (char)reader.ReadByte()) != 0x00)
                    {
                        possibleVersion += curChar;
                        if (possibleVersion.Length < 0xFF)
                        {
                            break;
                        }
                    }
                    emptyVersion = Regex.Replace(possibleVersion, "[a-zA-Z0-9\\.]", "");
                }
                if (possibleBundleHeader == "UnityFS")
                {
                    LoadBundleFile(ofd.FileName);
                }
                else if (possibleFormat < 0xFF && emptyVersion == "")
                {
                    LoadAssetsFile(ofd.FileName);
                }
                else
                {
                    MessageBox.Show("Couldn't detect file type. Are you sure this is a unity file?", "Assets View");
                }
            }
        }

        private void LoadBundleFile(string path)
        {
            OpenBundleDialog openFile = new OpenBundleDialog(helper, path);
            openFile.ShowDialog();
            if (openFile.selection > -1)
            {
                AssetBundleFile bundleFile = openFile.file;
                BundleFileInstance bundleInst = openFile.inst;
                List<byte[]> files = BundleHelper.LoadAllAssetsDataFromBundle(bundleFile);
                if (files.Count > 0)
                {
                    if (files.Count > 1)
                    {
                        for (int i = 1; i < files.Count; i++)
                        {
                            MemoryStream stream = new MemoryStream(files[i]);
                            string name = bundleFile.BlockAndDirInfo.DirectoryInfos[i].Name;
                            AssetsFileInstance inst = helper.LoadAssetsFile(stream, name, openFile.selection == 1);
                            inst.parentBundle = bundleInst;
                        }
                    }
                    MemoryStream mainStream = new MemoryStream(files[0]);
                    string mainName = bundleFile.BlockAndDirInfo.DirectoryInfos[0].Name;
                    AssetsFileInstance mainInst = helper.LoadAssetsFile(mainStream, mainName, openFile.selection == 1);
                    mainInst.parentBundle = bundleInst;
                    LoadMainAssetsFile(mainInst);
                }
                else
                {
                    MessageBox.Show("No valid assets files found in the bundle.", "Assets View");
                }
            }
        }

        private void LoadAssetsFile(string path)
        {
            OpenAssetsDialog openFile = new OpenAssetsDialog(path);
            openFile.ShowDialog();
            if (openFile.selection > -1)
            {
                LoadMainAssetsFile(helper.LoadAssetsFile(path, openFile.selection == 1));
            }
        }

        public void LoadMainAssetsFile(AssetsFileInstance inst)
        {
            if (currentFile == null || Path.GetFullPath(currentFile.path) != Path.GetFullPath(inst.path))
            {
                inst.file.GenerateQuickLookupTree();
                helper.LoadClassDatabaseFromPackage(inst.file.Metadata.UnityVersion);
                if (helper.ClassDatabase == null) // new v3: shouldn't happen anymore since this is automatic
                {
                    // may still not work but better than nothing I guess
                    // in the future we should probably do a selector
                    // like uabe does
                    helper.LoadClassDatabaseFromPackage("65535.65535.65535f255");
                }
                UpdateFileList();
                currentFile = inst;

                string ggmPath = Path.Combine(Path.GetDirectoryName(inst.path), "globalgamemanagers");
                if (inst.name == "resources.assets" && File.Exists(ggmPath))
                {
                    if (MessageBox.Show("Load resources.assets in directory mode? (Significantly faster)", "Assets View",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        AssetsFileInstance ggmInst = helper.LoadAssetsFile(ggmPath, true);
                        LoadResources(ggmInst);
                    }
                    else
                    {
                        LoadGeneric(inst, false);
                    }
                }
                else
                {
                    LoadGeneric(inst, false);
                }

                string corVer = helper.ClassDatabase.Header.Version.ToString();
                Text = "AssetsView .NET - ver " + inst.file.Metadata.UnityVersion + " / db " + corVer;
            }
        }

        private void clearFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            assetTree.Nodes.Clear();
            helper.UnloadAll();
            rootDir = null;
            currentFile = null;
            assetList.Rows.Clear();
        }

        private void LoadResources(AssetsFileInstance ggm)
        {
            foreach (AssetFileInfo info in ggm.file.Metadata.AssetInfos)
            {
                ClassDatabaseType type = helper.ClassDatabase.FindAssetClassByID(info.TypeId);
                if (helper.ClassDatabase.GetString(type.Name) == "ResourceManager")
                {
                    AssetTypeValueField baseField = helper.GetBaseField(ggm, info);
                    AssetTypeValueField m_Container = baseField.Get("m_Container").Get("Array");
                    List<AssetDetails> assets = new List<AssetDetails>();
                    for (int i = 0; i < m_Container.Children.Count; i++)
                    {
                        AssetTypeValueField item = m_Container[i];
                        string path = item.Get("first").AsString;
                        AssetTypeValueField pointerField = item.Get("second");
                        //paths[path] = new AssetDetails(new AssetPPtr(fileID, pathID));

                        AssetExternal assetExt = helper.GetExtAsset(ggm, pointerField, true);
                        AssetFileInfo assetInfo = assetExt.info;
                        if (assetInfo == null)
                            continue;
                        ClassDatabaseType assetType = helper.ClassDatabase.FindAssetClassByID(assetInfo.TypeId);
                        if (assetType == null)
                            continue;
                        string assetTypeName = helper.ClassDatabase.GetString(assetType.Name);
                        string assetName = AssetHelper.GetAssetNameFast(assetExt.file.file, helper.ClassDatabase, assetInfo);
                        if (path.Contains("/"))
                        {
                            if (path.Substring(path.LastIndexOf('/') + 1) == assetName.ToLower())
                            {
                                path = path.Substring(0, path.LastIndexOf('/') + 1) + assetName;
                            }
                        }
                        else
                        {
                            if (path == assetName.ToLower())
                            {
                                path = path.Substring(0, path.LastIndexOf('/') + 1) + assetName;
                            }
                        }

                        assets.Add(new AssetDetails(new AssetPPtr(0, assetInfo.PathId), GetIconForName(assetTypeName), path, assetTypeName, (int)assetInfo.ByteSize));
                    }
                    rootDir = new FSDirectory();
                    //rootDir.Create(paths);
                    rootDir.Create(assets);
                    ChangeDirectory("");
                    CheckResourcesInfo();
                    return;
                }
            }
        }

        private void LoadGeneric(AssetsFileInstance mainFile, bool isLevel)
        {
            List<AssetDetails> assets = new List<AssetDetails>();
            foreach (AssetFileInfo info in mainFile.file.Metadata.AssetInfos)
            {
                ClassDatabaseType type = helper.ClassDatabase.FindAssetClassByID(info.TypeId);
                if (type == null)
                    continue;
                string typeName = helper.ClassDatabase.GetString(type.Name);
                if (typeName != "GameObject" && isLevel)
                    continue;
                string name = AssetHelper.GetAssetNameFast(mainFile.file, helper.ClassDatabase, info);
                if (name == "")
                {
                    name = "[Unnamed]";
                }
                assets.Add(new AssetDetails(new AssetPPtr(0, info.PathId), GetIconForName(typeName), name, typeName, (int)info.ByteSize));
            }
            rootDir = new FSDirectory();
            rootDir.Create(assets);
            ChangeDirectory("");
        }

        private void ChangeDirectory(string path)
        {
            path = path.TrimEnd('/');
            if (path.StartsWith("/") || path == "")
            {
                currentDir = rootDir;
            }
            if (path != "" && path != "/")
            {
                string[] paths = path.Replace('\\', '/').Split('/');
                foreach (string dir in paths)
                {
                    FSDirectory searchDir = currentDir.children.Where(
                        d => d is FSDirectory && d.name == dir
                    ).FirstOrDefault() as FSDirectory;
                    
                    if (searchDir != null)
                        currentDir = searchDir;
                    else
                        return;
                }
            }
            UpdateDirectoryList();
        }

        public void UpdateFileList()
        {
            assetTree.Nodes.Clear();
            foreach (AssetsFileInstance dep in helper.Files)
            {
                assetTree.Nodes.Add(dep.name);
            }
        }

        public void UpdateDirectoryList()
        {
            assetList.Rows.Clear();
            pathBox.Text = currentDir.path;
            assetList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            Image[] images = new Image[imageList.Images.Count];
            for (int i = 0; i < imageList.Images.Count; i++)
            {
                images[i] = imageList.Images[i];
            }
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            LoadingBar lb = new LoadingBar();
            if (currentDir.children.Count > 1000)
            {
                lb.Show(this);
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            lb.pb.Maximum = currentDir.children.Count * 2;
            bw.DoWork += delegate
            {
                int prog = 0;
                foreach (FSObject obj in currentDir.children)
                {
                    if (obj is FSDirectory)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.Height = 32;
                        row.CreateCells(assetList, images[(int)AssetIcon.Folder], obj.name, "Folder", "", 0);
                        rows.Add(row);
                    }
                    prog++;
                    if (prog % 50 == 0)
                        bw.ReportProgress(prog);
                }
                foreach (FSObject obj in currentDir.children)
                {
                    if (obj is FSAsset assetObj)
                    {
                        AssetDetails dets = assetObj.details;
                        DataGridViewRow row = new DataGridViewRow();
                        row.Height = 32;
                        row.CreateCells(assetList, images[(int)dets.icon], assetObj.name, dets.type, dets.pointer.PathId, dets.size);
                        rows.Add(row);
                    }
                    prog++;
                    if (prog % 50 == 0)
                        bw.ReportProgress(prog);
                }
            };
            bw.ProgressChanged += delegate (object s, ProgressChangedEventArgs ev)
            {
                lb.pb.Value = ev.ProgressPercentage;
            };
            bw.RunWorkerCompleted += delegate
            {
                assetList.Rows.AddRange(rows.ToArray());
                lb.Close();
                assetList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            };
            bw.RunWorkerAsync();
        }
        
        private void upDirectory_Click(object sender, EventArgs e)
        {
            if (currentDir != null && currentDir.parent != null)
            {
                currentDir = currentDir.parent;
                UpdateDirectoryList();
            }
        }

        private void AssetList_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == MouseButtons.Right)
            {
                DataGridViewCell c = dgv[e.ColumnIndex, e.RowIndex];
                dgv.ClearSelection();
                dgv.CurrentCell = c;
                c.Selected = true;

                var selRow = dgv.Rows[e.RowIndex];
                string typeName = (string)selRow.Cells[2].Value;
                if (typeName == "Folder")
                {
                    viewTextureToolStripMenuItem.Visible = false;
                    exportTextureToolStripMenuItem.Visible = false;
                }
                else
                {
                    AssetFileInfo info = currentFile.file.GetAssetInfo((long)selRow.Cells[3].Value);
                    viewTextureToolStripMenuItem.Visible = info.TypeId == (int)AssetClassID.Texture2D;
                    exportTextureToolStripMenuItem.Visible = info.TypeId == (int)AssetClassID.Texture2D;
                }

                Point p = dgv.PointToClient(Cursor.Position);
                contextMenuStrip.Show(dgv, p);
            }
        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                string assetName = (string)selRow.Cells[1].Value;
                string typeName = (string)selRow.Cells[2].Value;
                if (typeName == "Folder")
                {
                    AssetInfoViewer viewer = new AssetInfoViewer(
                        assetName,
                        string.Empty //todo
                    );
                    viewer.ShowDialog();
                }
                else
                {
                    AssetFileInfo info = currentFile.file.GetAssetInfo((long)selRow.Cells[3].Value);
                    ushort monoId = currentFile.file.GetScriptIndex(info);
                    AssetInfoViewer viewer = new AssetInfoViewer(
                        info.TypeId,
                        info.AbsoluteByteStart,
                        info.ByteSize,
                        info.PathId,
                        monoId,
                        assetName,
                        typeName,
                        string.Empty //todo
                    );
                    viewer.ShowDialog();
                }
            }
        }

        private void exportTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                AssetFileInfo info = currentFile.file.GetAssetInfo((long)selRow.Cells[3].Value);
                AssetTypeValueField baseField = helper.GetBaseField(currentFile, info);

                TextureViewer texView = new TextureViewer(currentFile, baseField);
                texView.SaveTexture();
            }
        }

        private void viewTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                AssetFileInfo info = currentFile.file.GetAssetInfo((long)selRow.Cells[3].Value);
                AssetTypeValueField baseField = helper.GetBaseField(currentFile, info);

                TextureViewer texView = new TextureViewer(currentFile, baseField);
                texView.Show();
            }
        }

        private void xRefsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                long pathId = (long)selRow.Cells[3].Value;
                string assetDir = Path.GetDirectoryName(currentFile.path);

                if (pptrMap == null)
                {
                    string avpmFilePath = Path.Combine(assetDir, "avpm.dat");
                    if (File.Exists(avpmFilePath))
                    {
                        pptrMap = new PPtrMap(new BinaryReader(File.OpenRead(avpmFilePath)));
                    }
                    else
                    {
                        MessageBox.Show("avpm.dat file does not exist.\nTry running Global Search -> PPtr.", "Assets View");
                        return;
                    }
                }
                XRefsDialog xrefs = new XRefsDialog(this, helper, assetDir, pptrMap, new AssetID(currentFile.name, pathId));
                xrefs.Show();
            }
        }

        private void assetList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                string typeName = (string)selRow.Cells[2].Value;
                if (typeName == "Folder")
                {
                    string dirName = (string)selRow.Cells[1].Value;
                    ChangeDirectory(dirName);
                }
                else
                {
                    OpenAsset((long)selRow.Cells[3].Value);
                }
            }
        }

        public void OpenAsset(long id)
        {
            GameObjectViewer view = new GameObjectViewer(helper, currentFile, id);
            view.Show();
            view.FormClosed += GameObjectViewer_FormClosed;
        }

        private void GameObjectViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateFileList();
        }

        private void RecurseForResourcesInfo(FSDirectory dir, AssetsFileInstance afi)
        {
            foreach (FSAsset asset in dir.children.OfType<FSAsset>())
            {
                AssetFileInfo info = afi.file.GetAssetInfo(asset.details.pointer.PathId);
                ClassDatabaseType type = helper.ClassDatabase.FindAssetClassByID(info.TypeId);
                string typeName = helper.ClassDatabase.GetString(type.Name);

                asset.details.type = typeName;
                asset.details.size = (int)info.ByteSize;
                asset.details.icon = GetIconForName(typeName);
            }
            foreach (FSDirectory directory in dir.children.OfType<FSDirectory>())
            {
                RecurseForResourcesInfo(directory, afi);
            }
        }

        private void CheckResourcesInfo()
        {
            if (currentFile.name == "globalgamemanagers" && rsrcDataAdded == false && helper.Files.Any(f => f.name == "resources.assets"))
            {
                AssetsFileInstance afi = helper.Files.First(f => f.name == "resources.assets");
                RecurseForResourcesInfo(rootDir, afi);
                rsrcDataAdded = true;
                UpdateDirectoryList();
            }
        }

        public void UpdateDependencies()
        {
            UpdateFileList();
            //CheckResourcesInfo();
        }

        private AssetIcon GetIconForName(string type)
        {
            if (Enum.TryParse(type, out AssetIcon res))
            {
                return res;
            }
            return AssetIcon.Unknown;
        }

        string lastSearchedAsset = "";
        int lastSearchedIndex = -1;
        private void SearchAsset()
        {
            string text = pathBox.Text;
            int startIndex = 0;
            if (text == lastSearchedAsset)
            {
                startIndex = lastSearchedIndex;
            }
            else
            {
                lastSearchedAsset = "";
                lastSearchedIndex = -1;
            }
            int cellIdx = 1;
            if (text.StartsWith("$id="))
            {
                text = text.Substring(4);
                cellIdx = 3;
            }
            text = text.ToLower();
            assetList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            foreach (DataGridViewRow row in assetList.Rows)
            {
                if (row.Index < startIndex)
                    continue;
                assetList.ClearSelection();
                string matchText = row.Cells[cellIdx].Value.ToString().ToLower();
                if (Regex.IsMatch(matchText, WildCardToRegular(text)))
                {
                    row.Selected = true;
                    assetList.FirstDisplayedScrollingRowIndex = row.Index;
                    lastSearchedAsset = pathBox.Text;
                    lastSearchedIndex = row.Index + 1;
                    return;
                }
            }
            lastSearchedIndex = -1;
        }

        private string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        private void updateDependenciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("No current file selected!", "Assets View");
                return;
            }
            if (!AssetUtils.AllDependenciesLoaded(helper, currentFile))
            {
                DialogResult res = MessageBox.Show(
                    "Load all referenced dependencies?",
                    "Assets View",
                    MessageBoxButtons.YesNo);
                if (res == DialogResult.No)
                {
                    return;
                }
                helper.LoadAssetsFile(currentFile.AssetsStream, currentFile.path, true);
                UpdateDependencies();
            }
            else
            {
                MessageBox.Show(
                    "All dependencies already loaded.",
                    "Assets View");
            }
        }

        private void GoDirectory_Click(object sender, EventArgs e)
        {
            SearchAsset();
        }

        private void PathBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                SearchAsset();
                e.Handled = true;
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutScreen().ShowDialog();
        }

        private void ViewCurrentAssetInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("No current file selected!", "Assets View");
                return;
            }
            new AssetsFileInfoViewer(currentFile.file, helper.ClassDatabase).Show();
        }

        private void AssetTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AssetsFileInstance inst = helper.Files[e.Node.Index];
            inst.file.GenerateQuickLookupTree();
            UpdateFileList();
            currentFile = inst;
            LoadGeneric(inst, false);
        }

        private string SelectFolderAndLoad()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "[select folder]",
                Title = "Select folder to scan"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string dirName = Path.GetDirectoryName(ofd.FileName);
                if (Directory.Exists(dirName))
                {
                    return dirName;
                }
                else
                {
                    MessageBox.Show("Directory does not exist.", "Assets View");
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        private void monoBehaviourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string dirName = SelectFolderAndLoad();
            new MonoBehaviourScanner(this, helper, dirName).Show();
        }

        private void assetDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string dirName = SelectFolderAndLoad();
            new AssetDataScanner(this, helper, dirName).Show();
        }

        private void pptrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string dirName = SelectFolderAndLoad();
            new PPtrScanner(helper, dirName).Show();
        }
    }
}
