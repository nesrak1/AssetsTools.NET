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
    public partial class PPtrScanner : Form
    {
        private AssetsManager am;
        private string dirName;
        private List<string> fileNames;
        private Dictionary<AssetID, List<AssetID>> refLookup;

        private BackgroundWorker bw;
        private string curFileName;
        private bool usedTfc;

        public PPtrScanner(AssetsManager am, string dirName)
        {
            this.am = am;
            this.dirName = dirName;
            usedTfc = am.UseTemplateFieldCache;
            InitializeComponent();
        }

        private void PPtrScanner_Load(object sender, EventArgs e)
        {
            am.UseTemplateFieldCache = true;

            GetAllFilesInDirectory();
            refLookup = new Dictionary<AssetID, List<AssetID>>();
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += BackgroundWork;
            bw.ProgressChanged += ProgressChanged;
            bw.RunWorkerAsync();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0) //file update
            {
                fileProgress.Value = (int)((float)e.UserState * 1000);
            }
            else if (e.ProgressPercentage == 1) //file name update
            {
                curFileName = Path.GetFileName((string)e.UserState);
            }
            else if (e.ProgressPercentage == 2) //asset update
            {
                assetProgress.Value = (int)((float)e.UserState * 1000);
            }
            else if (e.ProgressPercentage == 3) //finish
            {
                fileProgress.Value = 1000;
                assetProgress.Value = 1000;
                progressLbl.Text = $"Done.";
                WritePPtrMap();

                am.UseTemplateFieldCache = usedTfc;
                return;
            }
            progressLbl.Text = $"Scanning file {curFileName} ({fileProgress.Value / 10}%)...\nScanning assets ({assetProgress.Value / 10}%)...";
        }

        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            foreach (string fileName in fileNames)
            {
                bw.ReportProgress(0, (float)i / fileNames.Count);
                bw.ReportProgress(1, fileName);
                AssetsFileInstance inst = am.LoadAssetsFile(fileName, true);
                am.LoadClassDatabaseFromPackage(inst.file.Metadata.UnityVersion);
                int j = 0;
                foreach (AssetFileInfo inf in inst.file.Metadata.AssetInfos)
                {
                    if (j % 500 == 0) bw.ReportProgress(2, (float)j / inst.file.Metadata.AssetInfos.Count);

                    AssetID id = new AssetID(Path.GetFileName(inst.name), inf.PathId);
                    AssetTypeValueField baseField;
                    //if (inf.TypeId == 0x72)
                    //{
                    //    baseField = am.GetMonoBaseFieldCached(inst, inf, Path.Combine(dirName, "Managed"));
                    //}
                    //else
                    //{
                        baseField = am.GetBaseField(inst, inf);
                    //}

                    RecurseReferences(inst, id, baseField);

                    j++;
                }
                i++;
            }
            bw.ReportProgress(3);
        }

        private void RecurseReferences(AssetsFileInstance inst, AssetID thisId, AssetTypeValueField field, int depth = 0)
        {
            foreach (AssetTypeValueField child in field.Children)
            {
                //not a value (ie not an int)
                if (!child.TemplateField.HasValue || child.TemplateField.IsArray)
                {
                    //not null
                    if (child == null)
                        return;
                    //not array of values either
                    if (child.TemplateField.IsArray && child.TemplateField.Children[1].ValueType != AssetValueType.None)
                        continue;
                    string typeName = child.TemplateField.Type;
                    //is a pptr
                    if (typeName.StartsWith("PPtr<") && typeName.EndsWith(">") && child.Children.Count == 2)
                    {
                        int fileId = child.Get("m_FileID").AsInt;
                        long pathId = child.Get("m_PathID").AsLong;

                        //not a null pptr
                        if (pathId == 0)
                            continue;

                        AssetID aid = ConvertToAssetID(inst, fileId, pathId);

                        if (!refLookup.ContainsKey(aid))
                            refLookup[aid] = new List<AssetID>();

                        if (!refLookup[aid].Contains(thisId))
                            refLookup[aid].Add(thisId);
                    }
                    else
                    {
                        RecurseReferences(inst, thisId, child, depth + 1);
                    }
                }
            }
        }

        private void WritePPtrMap()
        {
            using (BinaryWriter pmw = new BinaryWriter(File.OpenWrite(Path.Combine(dirName, "avpm.dat"))))
            {
                Dictionary<string, int> allFileNames = new Dictionary<string, int>();
                pmw.Write(1);
                pmw.Write(0);

                List<AssetID> keys = refLookup.Keys.ToList();
                pmw.Write(keys.Count);
                int pos = 12 + keys.Count * 16;
                foreach (AssetID key in keys)
                {
                    pmw.Write(GetFileName(allFileNames, key));
                    pmw.Write(key.pathID);
                    pmw.Write(pos);
                    pos += 4 + refLookup[key].Count * 12;
                }
                foreach (AssetID key in keys)
                {
                    List<AssetID> refIds = refLookup[key];
                    pmw.Write(refIds.Count);
                    foreach (AssetID refId in refIds)
                    {
                        if (!allFileNames.ContainsKey(refId.fileName))
                            allFileNames.Add(refId.fileName, allFileNames.Count);
                        pmw.Write(allFileNames[refId.fileName]);
                        pmw.Write(refId.pathID);
                    }
                }
                long strTablePos = pmw.BaseStream.Position;
                pmw.Write(allFileNames.Count);
                foreach (string fileName in allFileNames.Keys)
                {
                    pmw.Write(fileName);
                }
                pmw.BaseStream.Position = 4;
                pmw.Write((int)strTablePos);
            }
        }

        private int GetFileName(Dictionary<string, int> allFileNames, AssetID id)
        {
            if (!allFileNames.ContainsKey(id.fileName))
                allFileNames.Add(id.fileName, allFileNames.Count);
            return allFileNames[id.fileName];
        }

        private AssetID ConvertToAssetID(AssetsFileInstance inst, int fileId, long pathId)
        {
            string fileName;
            if (fileId == 0)
                fileName = inst.path;
            else
                fileName = inst.file.Metadata.Externals[fileId - 1].PathName;
            return new AssetID(Path.GetFileName(fileName), pathId);
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
    }
}
