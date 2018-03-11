using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using UABE.NET.Assets;
using static UABE.NET.Winforms.AssetViewer;

namespace UABE.NET.Winforms
{
    public partial class MonobehaviourSearch : Form
    {
        bool startedScanning = false;
        bool finishedScanning = false;
        int searchIndex = 0;
        List<AssetDetails> details;
        AssetsFile file;
        AssetsManager manager;
        public MonobehaviourSearch(List<AssetDetails> details, AssetsFile file, AssetsManager manager)
        {
            InitializeComponent();
            this.details = details;
            this.file = file;
            this.manager = manager;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (startedScanning || finishedScanning) return;
            startedScanning = true;
            loadingBar.Maximum = details.Count;

            Dictionary<ulong, string> monos = new Dictionary<ulong, string>();

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += delegate (object s, DoWorkEventArgs ev) {
                for (int i = 0; i < details.Count; i++)
                {
                    AssetDetails ad = details[i];
                    if (ad.fileID != 0 && !checkBox1.Checked) continue;
                    string text = null;
                    if (ad.typeName == "MonoBehaviour")
                    {
                        if (ad.name != "MonoBehaviour")
                        {
                            text = ad.name + " ("+ad.fileID+"/"+ad.pathID+")";
                        } else
                        {
                            AssetTypeInstance behaviourAti = manager.GetATI(manager.GetStream(ad.fileID), manager.GetInfo(ad.fileID, ad.pathID));
                            AssetTypeInstance scriptAti = manager.GetExtAsset(behaviourAti.GetBaseField().Get("m_Script")).instance;
                            string scriptName = scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
                            text = scriptName + " ("+ad.fileID+"/"+ad.pathID+")";
                        }
                        monos.Add(ad.pathID, text);
                    }
                    bw.ReportProgress(i);
                }
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
                            long id = components.Get(j).Get("component").Get("m_PathID").GetValue().AsInt64();
                            if (monos.ContainsKey((ulong)id))
                            {
                                monos[(ulong)id] += " -> " + gameObjectAti.GetBaseField().Get("m_Name").GetValue().AsString() + "("+ad.fileID+"/"+ad.pathID+")";
                            }
                        }
                        bw.ReportProgress(i);
                    }
                }
                foreach (string str in monos.Values)
                {
                    bw.ReportProgress(details.Count, str);
                }
                startedScanning = false;
                finishedScanning = true;
            };
            bw.ProgressChanged += delegate (object s, ProgressChangedEventArgs ev) {
                loadingBar.Value = ev.ProgressPercentage;
                if (ev.UserState != null) listBox1.Items.Add(ev.UserState);
            };
            bw.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!finishedScanning)
            {
                MessageBox.Show("No Monobehaviours scanned. Click Scan Monobehaviours to search.");
            } else if (startedScanning)
            {
                MessageBox.Show("Monobehaviours are still being scanned. Please wait.");
            } else
            {
                searchIndex = 0;
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i].ToString().Contains(textBox1.Text))
                    {
                        searchIndex = i;
                        listBox1.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!finishedScanning)
            {
                MessageBox.Show("No Monobehaviours scanned. Click Scan Monobehaviours to search.");
            } else if (startedScanning)
            {
                MessageBox.Show("Monobehaviours are still being scanned. Please wait.");
            } else
            {
                if (searchIndex != 0)
                {
                    for (int i = searchIndex - 1; i >= 0; i--)
                    {
                        if (listBox1.Items[i].ToString().Contains(textBox1.Text))
                        {
                            searchIndex = i;
                            listBox1.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (searchIndex != listBox1.Items.Count - 1)
            {
                for (int i = searchIndex + 1; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i].ToString().Contains(textBox1.Text))
                    {
                        searchIndex = i;
                        listBox1.SelectedIndex = i;
                        return;
                    }
                }
            }
        }
    }
}
