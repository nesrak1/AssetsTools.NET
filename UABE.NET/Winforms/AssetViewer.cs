// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UABE.NET.Assets;

namespace UABE.NET.Winforms
{
    public partial class AssetViewer : Form
    {
        Stream assetStream;
        //Stream classStream;
        string assetRootDir;
        string assetName;
        //AssetsFile af;
        //ClassDatabaseFile cldb;
        AssetsManagerLegacy assetsManager;
        List<AssetDetails> assetDetails = new List<AssetDetails>();
        //extra
        AssetNameSearch assetNameSearch = null;
        AssetSearch assetSearch = null;
        MonobehaviourSearch monoSearch = null;
        TransformSearch transformSearch = null;
        Dependencies dependencies = null;

        public AssetViewer(Stream stream, string rootDir, string name)
        {
            InitializeComponent();
            assetStream = stream;
            assetRootDir = rootDir;
            assetName = name;
            GetAssetList();
        }

        private void assetList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (assetDetails.Count != assetList.Items.Count) return;
            AssetDetails details = assetDetails[e.ItemIndex];
            nameField.Text = details.name;
            pathField.Text = details.pathID.ToString();
            fileField.Text = details.fileID.ToString();
            if (details.type != 0x72)
            {
                typeField.Text = $"0x{details.type.ToString("X8")} ({details.typeName})";
            } else
            {
                //$ just for consistency, if you disagree please tell me
                typeField.Text = $"0x{(0xFFFF-details.monoType).ToString("X8")}";
            }
        }

        private void viewDataButton_Click(object sender, System.EventArgs e)
        {
            if (assetList.SelectedIndices.Count == 0) return;
            AssetDetails details = assetDetails[assetList.SelectedIndices[assetList.SelectedIndices.Count - 1]];
            if (details.fileID == 0)
            {
                new AssetData(assetsManager.initialFile, assetsManager, details, assetRootDir).ShowDialog();
            } else
            {
                new AssetData(assetsManager.dependencies[(int)details.fileID-1].af, assetsManager, details, assetRootDir).ShowDialog();
            }
        }

        private void GetAssetList()
        {
            /*classStream = new FileStream(Path.Combine(Application.StartupPath, "cldb.dat"), FileMode.Open);

            af = new AssetsFile(new AssetsFileReader(assetStream));
            AssetsFileTable aft = new AssetsFileTable(af);

            cldb = new ClassDatabaseFile();
            cldb.Read(new AssetsFileReader(classStream));

            AssetsFileReader worker = new AssetsFileReader(assetStream);
            worker.bigEndian = false;

            foreach (AssetFileInfoEx afi in aft.pAssetFileInfo)
            {
                ClassDatabaseType type = AssetHelper.FindAssetClassByID(cldb, afi.curFileType);
                string assetName = GetAssetNameFast(afi, cldb, type, worker);
                string assetType = type.name.GetString(cldb);
                string fileID = file.ToString();
                string pathID = unchecked((long)afi.index).ToString();
                string size = afi.curFileSize.ToString();
                string modified = "";
                if (assetName.Trim() == "") assetName = assetType; //todo, seems redundant now, check on this later
                string[] items = new string[] { assetName, assetType, fileID, pathID, size, modified };
                assetDetails.Add(
                    new AssetDetails(
                        assetName,
                        assetType,
                        afi.index,
                        (uint)file,
                        afi.curFileType,
                        afi.absoluteFilePos,
                        af.typeTree.pTypes_Unity5[afi.curFileTypeOrIndex].scriptIndex
                    )
                );
                assetList.Items.Add(new ListViewItem(items));
            }
            assetList.Items.RemoveAt(0); //remove blank item that's here for some reason*/
            assetsManager = new AssetsManagerLegacy();
            assetsManager.LoadAssets(assetStream, assetRootDir);
            assetsManager.LoadClassFile(Path.Combine(Application.StartupPath, "cldb.dat"));
            AssetsFileReader worker = new AssetsFileReader(assetStream);
            worker.bigEndian = false;
            foreach (AssetFileInfoEx afi in assetsManager.initialTable.pAssetFileInfo)
            {
                AddAssetItem(assetsManager.initialFile, afi, worker, 0);
            }
            assetList.Items.RemoveAt(0);
            uint id = 1;
            foreach (AssetsManagerLegacy.Dependency dep in assetsManager.dependencies)
            {
                worker = new AssetsFileReader(dep.file);
                worker.bigEndian = false;
                foreach (AssetFileInfoEx afi in dep.aft.pAssetFileInfo)
                {
                    AddAssetItem(dep.af, afi, worker, id);
                }
                id++;
            }
        }

        private void AddAssetItem(AssetsFile af, AssetFileInfoEx afi, AssetsFileReader worker, uint fileId)
        {
            uint classId;
            ushort monoId;
            if (af.header.format <= 0x10)
            {
                classId = afi.inheritedUnityClass;
                if (classId == 0x72)
                {
                    monoId = (ushort)(0xFFFFFFFF -afi.curFileTypeOrIndex);
                }
                else
                {
                    monoId = 0xFFFF;
                }

            }
            else
            {
                classId = (uint)af.typeTree.pTypes_Unity5[afi.curFileTypeOrIndex].classId;
                monoId = af.typeTree.pTypes_Unity5[afi.curFileTypeOrIndex].scriptIndex;
            }
            ClassDatabaseType type = AssetHelper.FindAssetClassByID(assetsManager.initialClassFile, classId);
            if (type == null)
            {
                string tfileID = fileId.ToString();
                string tpathID = unchecked((long)afi.index).ToString();
                string tsize = afi.curFileSize.ToString();
                string tmodified = "";
                string[] titems = new string[] { "Unknown", "??? Custom ???", tfileID, tpathID, tsize, tmodified };
                assetDetails.Add(
                    new AssetDetails(
                        "Unknown",
                        "??? Custom ???",
                        afi.index,
                        fileId,
                        classId,
                        afi.absoluteFilePos,
                        monoId
                    )
                );
                System.Diagnostics.Debug.WriteLine("unknown id " + tpathID + " classid " + afi.curFileType);
                assetList.Items.Add(new ListViewItem(titems));
                return;
            }
            string assetName;
            if (classId == 0x72)
            {
                assetName = "";
            } else
            {
                assetName = GetAssetNameFast(afi, assetsManager.initialClassFile, type, worker);
            }
             
            string assetType = type.name.GetString(assetsManager.initialClassFile);
            string fileID = fileId.ToString();
            string pathID = unchecked((long)afi.index).ToString();
            string size = afi.curFileSize.ToString();
            string modified = "";
            if (assetName.Trim() == "") assetName = assetType; //todo, seems redundant now, check on this later
            string[] items = new string[] { assetName, assetType, fileID, pathID, size, modified };
            
            assetDetails.Add(
                new AssetDetails(
                    assetName,
                    assetType,
                    afi.index,
                    fileId,
                    classId,
                    afi.absoluteFilePos,
                    monoId
                )
            );
            assetList.Items.Add(new ListViewItem(items));
        }
        
        private string GetAssetNameFast(AssetFileInfoEx afi, ClassDatabaseFile cldb, ClassDatabaseType type, AssetsFileReader reader)
        {
            if (type.fields.Count <= 1) return type.name.GetString(cldb);
            if (type.fields[1].fieldName.GetString(cldb) == "m_Name")
            {
                reader.Position = afi.absoluteFilePos;
                return reader.ReadCountStringInt32();
            } else if (type.name.GetString(cldb) == "GameObject")
            {
                reader.Position = afi.absoluteFilePos;
                int size = reader.ReadInt32();
                reader.Position += (ulong)(size * 12);
                reader.Position += 4;
                return reader.ReadCountStringInt32();
            } else if (type.name.GetString(cldb) == "MonoBehaviour")
            {
                reader.Position = afi.absoluteFilePos;
                reader.Position += 28;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return type.name.GetString(cldb);
        }
        
        //not used now, was originally for the size thing
        private string ConvertSizes(uint size)
        {
            if (size >= 1048576)
                return (size/1048576f).ToString("N2") + "m";
            else if (size >= 1024)
                return (size/1024f).ToString("N2") + "k";
            return size + "b";
        }

        public struct AssetDetails
        {
            public string name;
            public string typeName;
            public ulong pathID;
            public uint fileID;
            public uint type;
            public ulong position;
            public ushort monoType;

            public AssetDetails(string name, string typeName, ulong pathID, uint fileID, uint type, ulong position, ushort monoType)
            {
                this.name = name;
                this.typeName = typeName;
                this.pathID = pathID;
                this.fileID = fileID;
                this.type = type;
                this.position = position;
                this.monoType = monoType;
            }
        }
        
        private void AssetViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            assetsManager.file.Close();
            assetsManager.classFile.Close();
            foreach (AssetsManagerLegacy.Dependency dep in assetsManager.dependencies)
                dep.file.Close();
            assetsManager = null;
            //assetStream.Close();
            //classStream.Close();
        }

        private void exportRawButton_Click(object sender, System.EventArgs e)
        {

        }

        private void exportDumpButton_Click(object sender, System.EventArgs e)
        {

        }

        private void importRawButton_Click(object sender, System.EventArgs e)
        {

        }

        private void importDumpButton_Click(object sender, System.EventArgs e)
        {

        }

        private void byNameToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (assetNameSearch == null)
            {
                assetNameSearch = new AssetNameSearch(assetDetails, assetsManager.initialFile);
            }
            assetNameSearch.ShowDialog();
        }

        private void binaryContentSearchToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (assetSearch == null)
            {
                assetSearch = new AssetSearch(assetDetails, assetsManager.initialFile);
            }
            assetSearch.ShowDialog();
        }

        private void monobehaviourSearchToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (monoSearch == null)
            {
                monoSearch = new MonobehaviourSearch(assetDetails, assetsManager.initialFile, assetsManager);
            }
            monoSearch.ShowDialog();
        }

        private void transformSearchToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (transformSearch == null)
            {
                transformSearch = new TransformSearch(assetDetails, assetsManager.initialFile, assetsManager);
            }
            transformSearch.ShowDialog();
        }

        private void dependenciesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (dependencies == null)
            {
                dependencies = new Dependencies(assetName, assetsManager.initialFile);
            }
            dependencies.ShowDialog();
        }
    }
}
