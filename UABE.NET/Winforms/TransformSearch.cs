using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UABE.NET.Assets;
using static UABE.NET.Winforms.AssetViewer;

namespace UABE.NET.Winforms
{
    public partial class TransformSearch : Form
    {
        bool startedScanning = false;
        List<AssetDetails> details;
        AssetsFile file;
        AssetsManagerLegacy manager;
        public TransformSearch(List<AssetDetails> details, AssetsFile file, AssetsManagerLegacy manager)
        {
            InitializeComponent();
            this.details = details;
            this.file = file;
            this.manager = manager;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (startedScanning) return;
            listBox1.Items.Clear();
            startedScanning = true;
            loadingBar.Maximum = details.Count;

            if (textBox1.Text == "") return;
            long searchNum = long.Parse(textBox1.Text);

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += delegate (object s, DoWorkEventArgs ev) {
                for (int i = 0; i < details.Count; i++)
                {
                    AssetDetails ad = details[i];
                    if (ad.fileID != 0 && !checkBox1.Checked) continue;
                    if (ad.typeName == "GameObject")
                    {
                        AssetTypeInstance gameObjectAti = manager.GetATI(manager.GetStream(ad.fileID), manager.GetInfo(ad.fileID, ad.pathID));
                        AssetTypeValueField components = gameObjectAti.GetBaseField().Get("m_Component").Get("Array");
                        for (uint j = 0; j < components.GetValue().AsArray().size; j++)
                        {
                            int fileId = components.Get(j).Get("component").Get("m_FileID").GetValue().AsInt();
                            long pathId = components.Get(j).Get("component").Get("m_PathID").GetValue().AsInt64();
                            if (pathId == searchNum)
                            {
                                bw.ReportProgress(i, gameObjectAti.GetBaseField().Get("m_Name").GetValue().AsString() + "(" + ad.fileID + "/" + ad.pathID + ")");
                            }
                        }
                        bw.ReportProgress(i);
                    }
                }
                startedScanning = false;
            };
            bw.ProgressChanged += delegate (object s, ProgressChangedEventArgs ev) {
                loadingBar.Value = ev.ProgressPercentage;
                if (ev.UserState != null) listBox1.Items.Add(ev.UserState);
            };
            bw.RunWorkerAsync();
        }
    }
}
