﻿using AssetsTools.NET;
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
            helper.updateAfterLoad = false;
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
                            string name = bundleFile.bundleInf6.dirInf[i].name;
                            AssetsFileInstance inst = helper.LoadAssetsFile(stream, name, openFile.selection == 1);
                            inst.parentBundle = bundleInst;
                        }
                    }
                    MemoryStream mainStream = new MemoryStream(files[0]);
                    string mainName = bundleFile.bundleInf6.dirInf[0].name;
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
                inst.table.GenerateQuickLookupTree();
                helper.UpdateDependencies();
                helper.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);
                if (helper.classFile == null)
                {
                    // Modification to find closest Unity version match
                    int index = helper.classPackage.header.files.FindIndex(f => f.name.StartsWith("U" + inst.file.typeTree.unityVersion));
                    if (index >= 0) // -1 mean's it isn't in current tpk
                    {
                        helper.classFile = helper.classPackage.files[index]; // If am.classFile is Null you get errors.
                    }
                    else
                    {
                        List<ClassDatabaseFile> files = helper.classPackage.files;

                        // Filter all classDbFiles for matching versions
                        var unityMajMinVer = inst.file.typeTree.unityVersion.Substring(0, 6);
                        var filter1 = files.Where(f => f.header.unityVersions[0].StartsWith(unityMajMinVer)).OrderBy(v => v.header.unityVersions[v.header.unityVersionCount - 1]);
                        var filter2 = filter1.Where(f => f.header.unityVersions[f.header.unityVersionCount - 1].Trim().Length == inst.file.typeTree.unityVersion.Length).OrderBy(v => v.header.unityVersions[v.header.unityVersionCount - 1]);

                        // Get the SubMinor version number
                        string unitySubMinVer = inst.file.typeTree.unityVersion.Split('.')[2];
                        string tmpNum = "";
                        if (unitySubMinVer.Length >= 4) { tmpNum = unitySubMinVer.Substring(0, 2); } else { tmpNum = unitySubMinVer.Substring(0, 1); }

                        // Make Existing Num List 
                        List<int> nums = new List<int>();
                        foreach (var pos in filter2) { nums.Add(int.Parse(pos.header.unityVersions[pos.header.unityVersionCount - 1].Split('.')[2].Substring(0, tmpNum.Length))); }
                        int closest = nums.Aggregate((x, y) => Math.Abs(x - int.Parse(tmpNum)) < Math.Abs(y - int.Parse(tmpNum)) ? x : y);

                        ClassDatabaseFile result = filter2.Where(f => f.header.unityVersions[f.header.unityVersionCount - 1].StartsWith(unityMajMinVer + "." + closest)).Single();
                        helper.classFile = result;
                    }


                    /* ORIGINAL CODE - will be Unity 5.x, due to order of list
                    //may still not work but better than nothing I guess in the future we should probably do a selector like uabe does
                    List<ClassDatabaseFile> files = helper.classPackage.files;
                    helper.classFile = files[files.Count - 1];
                    */
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
                        helper.UpdateDependencies();
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

                string[] vers = helper.classFile.header.unityVersions;
                string corVer = vers.FirstOrDefault(v => !v.Contains("*"));
                Text = "AssetsView .NET - ver " + inst.file.typeTree.unityVersion + " / db " + corVer;
            }
        }

        private void clearFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            assetTree.Nodes.Clear();
            helper.files.ForEach(d => {
                if (d != null)
                {
                    d.file.readerPar.Close(); d.table.assetFileInfo = null;
                }
            });
            helper.files.Clear();
            rootDir = null;
            currentFile = null;
            assetList.Rows.Clear();
        }

        private void LoadResources(AssetsFileInstance ggm)
        {
            foreach (AssetFileInfoEx info in ggm.table.assetFileInfo)
            {
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType);
                if (type.name.GetString(helper.classFile) == "ResourceManager")
                {
                    AssetTypeInstance inst = helper.GetTypeInstance(ggm.file, info);
                    AssetTypeValueField baseField = inst.GetBaseField();
                    AssetTypeValueField m_Container = baseField.Get("m_Container").Get("Array");
                    List<AssetDetails> assets = new List<AssetDetails>();
                    for (int i = 0; i < m_Container.GetValue().AsArray().size; i++)
                    {
                        AssetTypeValueField item = m_Container[i];
                        string path = item.Get("first").GetValue().AsString();
                        AssetTypeValueField pointerField = item.Get("second");
                        //paths[path] = new AssetDetails(new AssetPPtr(fileID, pathID));

                        AssetExternal assetExt = helper.GetExtAsset(ggm, pointerField, true);
                        AssetFileInfoEx assetInfo = assetExt.info;
                        if (assetInfo == null)
                            continue;
                        ClassDatabaseType assetType = AssetHelper.FindAssetClassByID(helper.classFile, assetInfo.curFileType);
                        if (assetType == null)
                            continue;
                        string assetTypeName = assetType.name.GetString(helper.classFile);
                        string assetName = AssetHelper.GetAssetNameFast(assetExt.file.file, helper.classFile, assetInfo);
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

                        assets.Add(new AssetDetails(new AssetPPtr(0, assetInfo.index), GetIconForName(assetTypeName), path, assetTypeName, (int)assetInfo.curFileSize));
                    }
                    rootDir = new FSDirectory();
                    //rootDir.Create(paths);
                    rootDir.Create(assets);
                    ChangeDirectory("");
                    helper.UpdateDependencies();
                    CheckResourcesInfo();
                    return;
                }
            }
        }

        private void LoadGeneric(AssetsFileInstance mainFile, bool isLevel)
        {
            List<AssetDetails> assets = new List<AssetDetails>();
            foreach (AssetFileInfoEx info in mainFile.table.assetFileInfo)
            {
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType);
                if (type == null)
                    continue;
                string typeName = type.name.GetString(helper.classFile);
                if (typeName != "GameObject" && isLevel)
                    continue;
                string name = AssetHelper.GetAssetNameFast(mainFile.file, helper.classFile, info);
                if (name == "")
                {
                    name = "[Unnamed]";
                }
                assets.Add(new AssetDetails(new AssetPPtr(0, info.index), GetIconForName(typeName), name, typeName, (int)info.curFileSize));
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
            foreach (AssetsFileInstance dep in helper.files)
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
                        row.CreateCells(assetList, images[(int)dets.icon], assetObj.name, dets.type, dets.pointer.pathID, dets.size);
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
                }
                else
                {
                    AssetFileInfoEx info = currentFile.table.GetAssetInfo((long)selRow.Cells[3].Value);
                    viewTextureToolStripMenuItem.Visible = info.curFileType == 0x1C;
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
                    AssetFileInfoEx info = currentFile.table.GetAssetInfo((long)selRow.Cells[3].Value);
                    ushort monoId = AssetHelper.GetScriptIndex(currentFile.file, info);
                    AssetInfoViewer viewer = new AssetInfoViewer(
                        info.curFileType,
                        info.absoluteFilePos,
                        info.curFileSize,
                        info.index,
                        monoId,
                        assetName,
                        typeName,
                        string.Empty //todo
                    );
                    viewer.ShowDialog();
                }
            }
        }

        private void viewTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
                return;
            if (assetList.SelectedCells.Count > 0)
            {
                var selRow = assetList.SelectedRows[0];
                AssetFileInfoEx info = currentFile.table.GetAssetInfo((long)selRow.Cells[3].Value);
                AssetTypeValueField baseField = helper.GetTypeInstance(currentFile.file, info).GetBaseField();

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
                AssetFileInfoEx info = afi.table.GetAssetInfo(asset.details.pointer.pathID);
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType);
                string typeName = type.name.GetString(helper.classFile);

                asset.details.type = typeName;
                asset.details.size = (int)info.curFileSize;
                asset.details.icon = GetIconForName(typeName);
            }
            foreach (FSDirectory directory in dir.children.OfType<FSDirectory>())
            {
                RecurseForResourcesInfo(directory, afi);
            }
        }

        private void CheckResourcesInfo()
        {
            if (currentFile.name == "globalgamemanagers" && rsrcDataAdded == false && helper.files.Any(f => f.name == "resources.assets"))
            {
                AssetsFileInstance afi = helper.files.First(f => f.name == "resources.assets");
                RecurseForResourcesInfo(rootDir, afi);
                rsrcDataAdded = true;
                UpdateDirectoryList();
            }
        }

        public void UpdateDependencies()
        {
            helper.UpdateDependencies();
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
                helper.LoadAssetsFile(currentFile.stream, currentFile.path, true);
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
            new AssetsFileInfoViewer(currentFile.file, helper.classFile).Show();
        }

        private void AssetTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AssetsFileInstance inst = helper.files[e.Node.Index];
            inst.table.GenerateQuickLookupTree();
            helper.UpdateDependencies();
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
