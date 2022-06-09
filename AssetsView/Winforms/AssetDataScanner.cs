using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class AssetDataScanner : Form
    {
        private StartScreen ss;
        private AssetsManager am;
        private string dirName;
        private List<string> fileNames;

        private BackgroundWorker bw;
        private byte[] searchQuery;
        private long searchPosition;
        private bool running;

        private Dictionary<string, List<AssetID>> fileMatches = new Dictionary<string, List<AssetID>>();

        public AssetDataScanner(StartScreen ss, AssetsManager am, string dirName)
        {
            this.ss = ss;
            this.am = am;
            this.dirName = dirName;
            InitializeComponent();
        }

        private void AssetDataScanner_Load(object sender, EventArgs e)
        {
            GetAllFilesInDirectory();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string file)
            {
                if (!fileTree.Nodes.ContainsKey(file))
                {
                    TreeNode node = fileTree.Nodes.Add(Path.GetFileName(file));
                    node.Name = file;
                    node.Tag = file;
                }
            }
            else //finished
            {
                fileTree.Sort();
            }
        }

        private void BackgroundWorkData(object sender, DoWorkEventArgs e)
        {
            running = true;
            foreach (string fileName in fileNames)
            {
                AssetsFileInstance inst = am.LoadAssetsFile(fileName, true);
                inst.file.GenerateQuickLookupTree();
                am.LoadClassDatabaseFromPackage(inst.file.Metadata.UnityVersion);
                foreach (AssetFileInfo inf in inst.file.Metadata.AssetInfos)
                {
                    AssetsFileReader fr = new AssetsFileReader(inst.file.Reader.BaseStream);
                    fr.BigEndian = false;
                    fr.Position = inf.AbsoluteByteStart;
                    byte[] data = fr.ReadBytes((int)inf.ByteSize);
                    int location = SearchBytes(data, searchQuery);
                    if (location != -1)
                    {
                        AssetID assetId = new AssetID(Path.GetFileName(fileName), inf.PathId);
                        if (fileMatches.ContainsKey(fileName))
                        {
                            fileMatches[fileName].Add(assetId);
                        }
                        else
                        {
                            fileMatches[fileName] = new List<AssetID>();
                            fileMatches[fileName].Add(assetId);
                            bw.ReportProgress(0, fileName);
                        }
                    }
                }
            }
            bw.ReportProgress(0);
            running = false;
        }

        private void WorkerFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            searchStringBtn.Enabled = true;
            searchHexBtn.Enabled = true;
            searchPositionBtn.Enabled = true;
        }

        private void BackgroundWorkPosition(object sender, DoWorkEventArgs e)
        {
            running = true;
            foreach (string fileName in fileNames)
            {
                AssetsFileInstance inst = am.LoadAssetsFile(fileName, true);
                inst.file.GenerateQuickLookupTree();
                am.LoadClassDatabaseFromPackage(inst.file.Metadata.UnityVersion);
                foreach (AssetFileInfo inf in inst.file.Metadata.AssetInfos)
                {
                    if (inf.AbsoluteByteStart >= searchPosition && searchPosition < (inf.AbsoluteByteStart + inf.ByteSize))
                    {
                        if (fileMatches.ContainsKey(fileName))
                        {
                            AssetID assetId = new AssetID(Path.GetFileName(fileName), inf.PathId);
                            fileMatches[fileName].Add(assetId);
                        }
                        else
                        {
                            bw.ReportProgress(0, fileName);
                        }
                    }
                }
            }
            bw.ReportProgress(0);
            running = false;
        }

        public int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (int i = 0; i <= limit; i++)
            {
                int k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        private void GetAllFilesInDirectory()
        {
            fileNames = new List<string>();
            //not checking for mainData since there's no support for pre 5.5 rn
            AddIfExists("globalgamemanagers");
            AddIfExists("globalgamemanagers.assets");
            AddIfExists("resources.assets");
            int idx = 0;
            while (AddIfExists($"level{idx}"))
            {
                idx++;
            }
            idx = 0;
            while (AddIfExists($"sharedassets{idx}.assets"))
            {
                idx++;
            }
        }

        private bool AddIfExists(string file)
        {
            string filePath = Path.Combine(dirName, file);
            if (File.Exists(filePath))
            {
                fileNames.Add(filePath);
                return true;
            }
            return false;
        }

        private class ScriptInfo
        {
            public string assemblyName;
            public string nameSpace;
            public string className;
            public ScriptInfo(string assemblyName, string nameSpace, string className)
            {
                this.assemblyName = assemblyName;
                this.nameSpace = nameSpace;
                this.className = className;
            }
        }

        private class ListBoxInfo
        {
            public string displayText;
            public AssetID assetId;
            public ListBoxInfo(string displayText, AssetID assetId)
            {
                this.displayText = displayText;
                this.assetId = assetId;
            }
            public override string ToString()
            {
                return displayText;
            }
        }

        private void scriptTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag is string fileName)
            {
                List<AssetID> mbList = fileMatches[fileName];
                matchesList.Items.Clear();
                foreach (AssetID mb in mbList)
                {
                    matchesList.Items.Add(new ListBoxInfo($"{mb.fileName}: {mb.pathID}", mb));
                }
            }
        }

        private void matchesList_DoubleClick(object sender, EventArgs e)
        {
            object selectedItem = matchesList.SelectedItem;
            if (selectedItem != null && selectedItem is ListBoxInfo info)
            {
                AssetID assetId = info.assetId;
                ss.LoadMainAssetsFile(am.LoadAssetsFile(Path.Combine(dirName, assetId.fileName), true));
                ss.OpenAsset(assetId.pathID);
                WindowState = FormWindowState.Minimized;
            }
        }

        private byte[] FromHex(string text)
        {
            if (text.Length % 2 != 0)
                return null;
            if (Regex.IsMatch(text, "[^a-fA-F0-9]"))
                return null;
            byte[] buf = new byte[text.Length / 2];
            for (int i = 0; i < text.Length / 2; i++)
            {
                buf[i] = byte.Parse(text.Substring(i*2, 2), NumberStyles.HexNumber);
            }
            return buf;
        }

        private void RunBackgroundWorker()
        {
            searchStringBtn.Enabled = false;
            searchHexBtn.Enabled = false;
            searchPositionBtn.Enabled = false;

            fileTree.Nodes.Clear();
            matchesList.Items.Clear();
            fileMatches.Clear();

            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += ProgressChanged;
            bw.RunWorkerCompleted += WorkerFinished;
            bw.RunWorkerAsync();
        }

        private void searchStringBtn_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                searchQuery = Encoding.UTF8.GetBytes(searchBar.Text);
                bw = new BackgroundWorker();
                bw.DoWork += BackgroundWorkData;
                RunBackgroundWorker();
            }
        }

        private void searchHexBtn_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                searchQuery = FromHex(searchBar.Text);
                if (searchQuery != null)
                {
                    bw = new BackgroundWorker();
                    bw.DoWork += BackgroundWorkData;
                    RunBackgroundWorker();
                }
            }
        }

        private void searchPositionBtn_Click(object sender, EventArgs e)
        {
            if (searchBar.Text.StartsWith("0x"))
            {
                string text = searchBar.Text;
                if (text.Length % 2 != 0)
                {
                    text = "0" + text;
                }
                searchPosition = BitConverter.ToInt64(FromHex(text), 0);
                if (searchQuery != null)
                {
                    bw = new BackgroundWorker();
                    bw.DoWork += BackgroundWorkPosition;
                    RunBackgroundWorker();
                }
            }
            else if (long.TryParse(searchBar.Text, out searchPosition) && searchPosition >= 0)
            {
                bw = new BackgroundWorker();
                bw.DoWork += BackgroundWorkPosition;
                RunBackgroundWorker();
            }
        }
    }
}