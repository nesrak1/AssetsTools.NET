using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class MonoBehaviourScanner : Form
    {
        private StartScreen ss;
        private AssetsManager am;
        private string dirName;
        private List<string> fileNames;

        private BackgroundWorker bw;

        private Dictionary<AssetID, ScriptInfo> monoScriptToInfo = new Dictionary<AssetID, ScriptInfo>();
        private Dictionary<AssetID, List<AssetID>> monoScriptRefs = new Dictionary<AssetID, List<AssetID>>();

        public MonoBehaviourScanner(StartScreen ss, AssetsManager am, string dirName)
        {
            this.ss = ss;
            this.am = am;
            this.dirName = dirName;
            InitializeComponent();
        }

        private void MonoBehaviourScanner_Load(object sender, EventArgs e)
        {
            Text = "MonoBehaviour Scanner (Loading...)";

            GetAllFilesInDirectory();
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += BackgroundWork;
            bw.ProgressChanged += ProgressChanged;
            bw.RunWorkerAsync();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is AssetID assetId)
            {
                ScriptInfo inf = monoScriptToInfo[assetId];
                TreeNode asmNode;
                if (scriptTree.Nodes.ContainsKey(inf.assemblyName))
                {
                    asmNode = scriptTree.Nodes.Find(inf.assemblyName, false)[0];
                }
                else
                {
                    asmNode = scriptTree.Nodes.Add(inf.assemblyName);
                    asmNode.Name = inf.assemblyName;
                }

                if (inf.nameSpace == "")
                    inf.nameSpace = "-";

                TreeNode nsNode;
                if (asmNode.Nodes.ContainsKey(inf.nameSpace))
                {
                    nsNode = asmNode.Nodes.Find(inf.nameSpace, false)[0];
                }
                else
                {
                    nsNode = asmNode.Nodes.Add(inf.nameSpace);
                    nsNode.Name = inf.nameSpace;
                }

                if (!nsNode.Nodes.ContainsKey(inf.className))
                {
                    TreeNode classNode = nsNode.Nodes.Add(inf.className);
                    classNode.Name = inf.className;
                    classNode.Tag = assetId;
                }
            }
            else //finished
            {
                scriptTree.Sort();
                Text = "MonoBehaviour Scanner";
            }
        }

        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            List<AssetID> assetIds = new List<AssetID>();
            foreach (string fileName in fileNames)
            {
                AssetsFileInstance inst = am.LoadAssetsFile(fileName, true);
                inst.file.GenerateQuickLookupTree();
                am.LoadClassDatabaseFromPackage(inst.file.Metadata.UnityVersion);
                foreach (AssetFileInfo inf in inst.file.GetAssetsOfType(0x73))
                {
                    AssetsFileReader fr = new AssetsFileReader(inst.file.Reader.BaseStream);
                    fr.BigEndian = false;
                    fr.Position = inf.AbsoluteByteStart;
                    fr.ReadCountStringInt32(); fr.Align();
                    fr.Position += 20;
                    string m_ClassName = fr.ReadCountStringInt32(); fr.Align();
                    string m_Namespace = fr.ReadCountStringInt32(); fr.Align();
                    string m_AssemblyName = fr.ReadCountStringInt32(); fr.Align();
                    AssetID assetId = new AssetID(Path.GetFileName(fileName), inf.PathId);
                    ScriptInfo scriptInfo = new ScriptInfo(m_AssemblyName, m_Namespace, m_ClassName);
                    assetIds.Add(assetId);
                    monoScriptToInfo.Add(assetId, scriptInfo);
                    monoScriptRefs.Add(assetId, new List<AssetID>());
                    bw.ReportProgress(0, assetId);
                }
            }
            foreach (string fileName in fileNames)
            {
                //don't worry, this doesn't load them twice
                AssetsFileInstance inst = am.LoadAssetsFile(fileName, true);
                foreach (AssetFileInfo inf in inst.file.GetAssetsOfType(0x72))
                {
                    try
                    {
                        AssetsFileReader fr = new AssetsFileReader(inst.file.Reader.BaseStream);
                        fr.BigEndian = false;
                        fr.Position = inf.AbsoluteByteStart;
                        fr.Position += 16;
                        int m_FileID = fr.ReadInt32();
                        long m_PathID = fr.ReadInt64();
                        string refFileName = fileName;
                        if (m_FileID != 0)
                            refFileName = inst.file.Metadata.Externals[m_FileID - 1].PathName;

                        AssetID scriptId = new AssetID(Path.GetFileName(refFileName), m_PathID);
                        AssetID selfId = new AssetID(Path.GetFileName(fileName), inf.PathId);
                        if (monoScriptRefs.ContainsKey(scriptId))
                        {
                            monoScriptRefs[scriptId].Add(selfId);
                        }
                    }
                    catch { }
                }
            }
            bw.ReportProgress(0);
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
            if (e.Node.Tag != null && e.Node.Tag is AssetID assetId)
            {
                List<AssetID> mbList = monoScriptRefs[assetId];
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
    }
}