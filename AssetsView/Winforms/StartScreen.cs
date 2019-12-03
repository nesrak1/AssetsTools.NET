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
            helper.LoadClassPackage("classdata.tpk");
            //helper.LoadClassDatabase(new FileStream("cldb.dat", FileMode.Open, FileAccess.Read, FileShare.Read));
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
                OpenAssetsDialog openFile = new OpenAssetsDialog(ofd.FileName);
                openFile.ShowDialog();
                if (openFile.selection > -1)
                {
                    AssetsFileInstance inst = helper.LoadAssetsFile(ofd.FileName, (openFile.selection == 0) ? false : true);
                    inst.table.GenerateQuickLookupTree();
                    helper.UpdateDependencies();
                    helper.LoadClassDatabaseFromPackage(inst.file.typeTree.unityVersion);
                    UpdateFileList();
                    currentFile = inst;
                    LoadGeneric(inst, false);

                    string[] vers = helper.classFile.header.pUnityVersions;
                    string corVer = vers.FirstOrDefault(v => !v.Contains("*"));
                    Text = "AssetsView .NET - ver " + inst.file.typeTree.unityVersion + " / db " + corVer;
                }
            }
        }

        private void clearFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            assetTree.Nodes.Clear();
            helper.files.ForEach(d => {
                if (d != null)
                {
                    d.file.readerPar.Close(); d.table.pAssetFileInfo = null;
                }
            });
            helper.files.Clear();
            rootDir = null;
            currentFile = null;
            assetList.Rows.Clear();
        }

        private void LoadGGM(AssetsFileInstance mainFile)
        {
            //swap this with resources so we can actually see ggm assets
            foreach (AssetFileInfoEx info in mainFile.table.pAssetFileInfo)
            {
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType);
                if (type.name.GetString(helper.classFile) == "ResourceManager")
                {
                    AssetTypeInstance inst = helper.GetATI(mainFile.file, info);
                    AssetTypeValueField baseField = inst.GetBaseField();
                    AssetTypeValueField m_Container = baseField.Get("m_Container").Get("Array");
                    //Dictionary<string, AssetDetails> paths = new Dictionary<string, AssetDetails>();
                    List<AssetDetails> assets = new List<AssetDetails>();
                    for (uint i = 0; i < m_Container.GetValue().AsArray().size; i++)
                    {
                        AssetTypeValueField item = m_Container[i];
                        string path = item.Get("first").GetValue().AsString();
                        AssetTypeValueField pointerField = item.Get("second");
                        uint fileID = (uint)pointerField.Get("m_FileID").GetValue().AsInt();
                        ulong pathID = (ulong)pointerField.Get("m_PathID").GetValue().AsInt64();
                        //paths[path] = new AssetDetails(new AssetPPtr(fileID, pathID));
                        assets.Add(new AssetDetails(new AssetPPtr(fileID, pathID), AssetIcon.Unknown, path));
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
            foreach (AssetFileInfoEx info in mainFile.table.pAssetFileInfo)
            {
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType);
                if (type == null)
                    continue;
                string typeName = type.name.GetString(helper.classFile);
                if (typeName != "GameObject" && isLevel)
                    continue;
                string name = AssetInfo.GetAssetNameFast(info, helper.classFile, type, mainFile);
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

        private void UpdateFileList()
        {
            assetTree.Nodes.Clear();
            foreach (AssetsFileInstance dep in helper.files)
            {
                assetTree.Nodes.Add(dep.name);
            }
        }

        private void UpdateDirectoryList()
        {
            assetList.Rows.Clear();
            pathBox.Text = currentDir.path;
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (FSObject obj in currentDir.children)
            {
                if (obj is FSDirectory)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.Height = 32;
                    row.CreateCells(assetList, imageList.Images[(int)AssetIcon.Folder], obj.name, "Folder", "", 0);
                    rows.Add(row);
                }
            }
            foreach (FSObject obj in currentDir.children)
            {
                if (obj is FSAsset assetObj)
                {
                    AssetDetails dets = assetObj.details;
                    object[] details = new object[] { imageList.Images[(int)dets.icon], assetObj.name, dets.type, dets.pointer.pathID, dets.size };

                    DataGridViewRow row = new DataGridViewRow();
                    row.Height = 32;
                    row.CreateCells(assetList, details);
                    rows.Add(row);
                }
            }
            assetList.Rows.AddRange(rows.ToArray());
        }
        
        private void upDirectory_Click(object sender, EventArgs e)
        {
            if (currentDir != null && currentDir.parent != null)
            {
                currentDir = currentDir.parent;
                UpdateDirectoryList();
            }
        }

        private void assetList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
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
                    ClassDatabaseFile classFile = helper.classFile;
                    AssetsFileInstance correctAti = currentFile;
                    ClassDatabaseType classType = AssetHelper.FindAssetClassByName(classFile, typeName);
                    if (currentFile.name == "globalgamemanagers")
                    {
                        int rsrcIndex = helper.files.FindIndex(f => f.name == "resources.assets");
                        if (rsrcIndex != -1)
                        {
                            correctAti = helper.files[rsrcIndex];
                        }
                        else
                        {
                            string rsrcPath = Path.Combine(Path.GetDirectoryName(currentFile.path), "resources.assets");
                            correctAti = helper.LoadAssetsFile(rsrcPath, false);
                            UpdateDependencies();
                        }
                        if (typeName == "")
                            return;
                    }
                    AssetFileInfoEx info = correctAti.table.getAssetInfo((ulong)selRow.Cells[3].Value);
                    bool hasGameobjectField = classType.fields.Any(f => f.fieldName.GetString(classFile) == "m_GameObject");
                    bool parentPointerNull = false;
                    if (typeName != "GameObject" && hasGameobjectField)
                    {
                        //get gameobject parent
                        AssetTypeValueField componentBaseField = helper.GetATI(correctAti.file, info).GetBaseField();
                        AssetFileInfoEx newInfo = helper.GetExtAsset(correctAti, componentBaseField["m_GameObject"], true).info;
                        if (newInfo != null && newInfo.index != 0)
                        {
                            info = newInfo;
                        }
                        else
                        {
                            parentPointerNull = true;
                        }
                    }
                    if ((typeName == "GameObject" || hasGameobjectField) && !parentPointerNull)
                    {
                        AssetTypeValueField baseField = helper.GetATI(correctAti.file, info).GetBaseField();

                        AssetTypeValueField transformPtr = baseField["m_Component"]["Array"][0]["component"];
                        AssetTypeValueField transform = helper.GetExtAsset(correctAti, transformPtr).instance.GetBaseField();
                        baseField = GetRootTransform(helper, currentFile, transform);
                        AssetTypeValueField gameObjectPtr = baseField["m_GameObject"];
                        AssetTypeValueField gameObject = helper.GetExtAsset(correctAti, gameObjectPtr).instance.GetBaseField();
                        GameObjectViewer view = new GameObjectViewer(helper, correctAti, gameObject, info.index, (ulong)selRow.Cells[3].Value);
                        view.Show();
                    }
                    else
                    {
                        AssetTypeValueField baseField = helper.GetATI(correctAti.file, info).GetBaseField();
                        GameObjectViewer view = new GameObjectViewer(helper, correctAti, baseField, info);
                        view.Show();
                    }
                }
            }
        }

        public static AssetTypeValueField GetRootTransform(AssetsManager helper, AssetsFileInstance currentFile, AssetTypeValueField transform)
        {
            AssetTypeValueField fatherPtr = transform["m_Father"];
            if (fatherPtr["m_PathID"].GetValue().AsInt64() != 0)
            {
                AssetTypeValueField father = helper.GetExtAsset(currentFile, fatherPtr).instance.GetBaseField();
                return GetRootTransform(helper, currentFile, father);
            }
            else
            {
                return transform;
            }
        }

        private void RecurseForResourcesInfo(FSDirectory dir, AssetsFileInstance afi)
        {
            foreach (FSAsset asset in dir.children.OfType<FSAsset>())
            {
                AssetFileInfoEx info = afi.table.getAssetInfo(asset.details.pointer.pathID);
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

        private void UpdateDependencies()
        {
            helper.UpdateDependencies();
            UpdateFileList();
            CheckResourcesInfo();
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
                    lastSearchedAsset = text;
                    lastSearchedIndex = row.Index + 1;
                    break;
                }
            }
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
                    "Load all referenced dependencies? This may take a while.",
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
            new AssetInfoViewer(currentFile.file, helper.classFile).Show();
        }
    }
}
