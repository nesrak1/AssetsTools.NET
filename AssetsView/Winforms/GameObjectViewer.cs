using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsView.Util;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class GameObjectViewer : Form
    {
        private AssetsManager helper;
        private AssetsFileInstance inst;
        private long selectedId;
        private long selectedGameObjectId;
        public GameObjectViewer(AssetsManager helper, AssetsFileInstance inst, long selectedId)
        {
            InitializeComponent();
            this.helper = helper;
            this.inst = inst;
            this.selectedId = selectedId;
        }

        private void GameObjectViewer_Load(object sender, EventArgs e)
        {
            //mono won't be able to do this so ignore it
            if (Type.GetType("Mono.Runtime") == null)
            {
                PInvoke.SetWindowTheme(goTree.Handle, "explorer", null);
            }
            valueGrid.PropertySort = PropertySort.Categorized;

            ClassDatabaseFile classFile = helper.ClassDatabase;
            AssetFileInfo info = inst.file.GetAssetInfo(selectedId);
            AssetTypeTemplateField typeTemplate = helper.GetTemplateBaseField(inst, info);
            string typeName = typeTemplate.Type;

            if (typeName == "GameObject")
            {
                selectedGameObjectId = selectedId;
                AssetExternal firstExt = helper.GetExtAsset(inst, 0, selectedId);
                AssetExternal rootExt = GetRootGameObject(firstExt);
                PopulateHierarchyTree(null, rootExt);
            }
            else
            {
                bool hasGameObjectField = typeTemplate.Children.Any(f => f.Name == "m_GameObject");
                if (hasGameObjectField)
                {
                    AssetTypeValueField firstBaseField = helper.GetBaseField(inst, info);
                    AssetExternal firstExt = helper.GetExtAsset(inst, firstBaseField["m_GameObject"]);
                    if (firstExt.info != null)
                    {
                        selectedGameObjectId = firstExt.info.PathId;
                        AssetExternal rootExt = GetRootGameObject(firstExt);
                        PopulateHierarchyTree(null, rootExt);
                    }
                    else
                    {
                        TreeNode node = goTree.Nodes.Add($"[{typeName} (parentless)]");
                        node.Tag = helper.GetExtAsset(inst, 0, info.PathId);
                    }
                }
                else
                {
                    TreeNode node = goTree.Nodes.Add($"[{typeName}]");
                    node.Tag = helper.GetExtAsset(inst, 0, info.PathId);
                }
            }
            GetSelectedNodeData();
        }

        private AssetExternal GetRootGameObject(AssetExternal ext)
        {
            AssetExternal transformExt = helper.GetExtAsset(inst, ext.baseField["m_Component.Array"][0].GetLastChild());
            while (true)
            {
                AssetExternal parentExt = helper.GetExtAsset(inst, transformExt.baseField["m_Father"]);
                if (parentExt.baseField == null)
                {
                    AssetExternal gameObjectExt = helper.GetExtAsset(inst, transformExt.baseField["m_GameObject"]);
                    return gameObjectExt;
                }
                transformExt = parentExt;
            }
        }

        private void GetSelectedNodeData()
        {
            if (goTree.Nodes.Count > 0)
            {
                object nodeTag = null;
                if (goTree.SelectedNode != null)
                    nodeTag = goTree.SelectedNode.Tag;

                if (nodeTag is AssetExternal ext)
                {
                    AssetTypeValueField baseField = ext.baseField;
                    string typeName = baseField.TemplateField.Type;
                    if (typeName == "GameObject")
                    {
                        PGProperty dataRoot = LoadGameObjectData(baseField);
                        valueGrid.SelectedObject = dataRoot;

                        GridItem possibleComponent = valueGrid.EnumerateAllItems().FirstOrDefault(i =>
                            i.PropertyDescriptor != null &&
                            i.PropertyDescriptor.Description == "SELECTEDCOMPONENT");
                        if (possibleComponent != null)
                        {
                            valueGrid.Focus();
                            possibleComponent.Select();
                        }
                    }
                    else
                    {
                        PGProperty root = new PGProperty("root");
                        LoadComponentData(root, ext.baseField, ext.info, 0, 0);
                        valueGrid.SelectedObject = root;
                    }
                }
            }
        }

        bool loadedWithErrors = false;
        private PGProperty LoadGameObjectData(AssetTypeValueField field)
        {
            PGProperty root = new PGProperty("root");
            AssetTypeValueField components = field["m_Component.Array"];
            int componentSize = components.Children.Count;
            for (int i = 0; i < componentSize; i++)
            {
                AssetTypeValueField componentPtr = components[i].GetLastChild();
                if (ModifierKeys == Keys.Shift)
                {
                    AssetExternal ext = helper.GetExtAsset(inst, componentPtr);
                    if (ext.baseField != null)
                        LoadComponentData(root, ext.baseField, ext.info, i, componentSize);
                }
                else
                {
                    try
                    {
                        AssetExternal ext = helper.GetExtAsset(inst, componentPtr);
                        if (ext.baseField != null)
                            LoadComponentData(root, ext.baseField, ext.info, i, componentSize);
                    }
                    catch
                    {
                        if (loadedWithErrors == false)
                        {
                            loadedWithErrors = true;
                            MessageBox.Show("A component failed to load. (tell nes or hold shift to throw)");
                        }
                    }
                }
            }
            return root;
        }

        private void LoadComponentData(PGProperty root, AssetTypeValueField baseField, AssetFileInfo info, int index, int size)
        {
            string className = helper.ClassDatabase.GetString(
                helper.ClassDatabase.FindAssetClassByID(info.TypeId).Name);

            AssetTypeValueField targetBaseField = baseField;
            if (className == "MonoBehaviour")
            {
                className += $" ({GetClassName(helper, inst, targetBaseField)})";
                //string managedPath = Path.Combine(Path.GetDirectoryName(inst.path), "Managed");
                //if (Directory.Exists(managedPath))
                //{
                //    targetBaseField = helper.GetMonoBaseFieldCached(inst, info, managedPath);
                //}
            }
            string category = new string('\t', size - index) + className;
            PopulateDataGrid(targetBaseField, root, info, category);
        }

        private void PopulateDataGrid(AssetTypeValueField atvf, PGProperty node, AssetFileInfo info, string category, bool arrayChildren = false)
        {
            if (atvf.Children == null || atvf.Children.Count == 0)
                return;
        
            string arrayName = string.Empty;
            if (arrayChildren && atvf.Children.Count > 0)
                arrayName = atvf.Children[0].TemplateField.Name;
        
            for (int i = 0; i < atvf.Children.Count; i++)
            {
                AssetTypeValueField atvfc = atvf.Children[i];
                if (atvfc == null)
                    return;
        
                string key;
                if (!arrayChildren)
                    key = atvfc.TemplateField.Name;
                else
                    key = $"{arrayName}[{i}]";
        
                AssetValueType evt;
                if (atvfc.Value != null)
                {
                    evt = atvfc.Value.ValueType;
                    if (evt != AssetValueType.None)
                    {
                        if (1 <= (int)evt && (int)evt <= 12)
                        {
                            string value = atvfc.AsString;
                            PGProperty prop = new PGProperty(key, value);
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            PopulateDataGrid(atvfc, prop, info, category);
                        }
                        else if (evt == AssetValueType.Array)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[size: {atvfc.Children.Count}]");
                            PGProperty prop = new PGProperty(key, childProps, $"[size: {atvfc.Children.Count}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            PopulateDataGrid(atvfc, childProps, info, category, true);
                        }
                        else if (evt == AssetValueType.ByteArray)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[bytes size: {atvfc.AsByteArray.Length}]");
                            PGProperty prop = new PGProperty(key, childProps, $"[bytes size: {atvfc.AsByteArray.Length}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                        }
                    }
                }
                else
                {
                    PGProperty childProps;
                    if (atvfc.Children.Count == 2)
                    {
                        AssetTypeValueField fileId = atvfc.Children[0];
                        AssetTypeValueField pathId = atvfc.Children[1];
                        string fileIdName = fileId.TemplateField.Name;
                        string fileIdType = fileId.TemplateField.Type;
                        string pathIdName = pathId.TemplateField.Name;
                        string pathIdType = pathId.TemplateField.Type;
                        if (fileIdName == "m_FileID" && fileIdType == "int" &&
                            pathIdName == "m_PathID" && pathIdType == "SInt64")
                        {
                            int fileIdValue = fileId.AsInt;
                            long pathIdValue = pathId.AsLong;
                            childProps = new PGProperty("child", "", $"[fileid: {fileIdValue}, pathid: {pathIdValue}]");
                        }
                        else
                        {
                            childProps = new PGProperty("child", "");
                        }
                    }
                    else
                    {
                        childProps = new PGProperty("child", "");
                    }
                    PGProperty prop = new PGProperty(key, childProps);
                    prop.category = category;
                    SetSelectedStateIfSelected(info, prop);
                    node.Add(prop);
                    PopulateDataGrid(atvfc, childProps, info, category);
                }
            }
        }

        private void SetSelectedStateIfSelected(AssetFileInfo info, PGProperty prop)
        {
            if (info.PathId == selectedId)
            {
                prop.description = "SELECTEDCOMPONENT";
            }
        }

        private void PopulateHierarchyTree(TreeNode node, AssetExternal ext)
        {
            AssetTypeValueField baseField = ext.baseField;
            long thisId = ext.info.PathId;

            TreeNode newNode;
            if (node == null)
                newNode = goTree.Nodes.Add(baseField["m_Name"].AsString);
            else
                newNode = node.Nodes.Add(baseField["m_Name"].AsString);

            if (!baseField["m_IsActive"].AsBool)
                newNode.ForeColor = Color.DarkRed;

            newNode.Tag = ext;
            if (thisId == selectedGameObjectId)
            {
                goTree.SelectedNode = newNode;
            }

            AssetTypeValueField children =
                helper.GetExtAsset(inst, baseField["m_Component.Array"][0].GetLastChild()).baseField["m_Children.Array"];

            for (int i = 0; i < children.Children.Count; i++)
            {
                AssetExternal childExternal = helper.GetExtAsset(inst, children[i]);
                AssetExternal gameObjExt = helper.GetExtAsset(inst, childExternal.baseField["m_GameObject"]);
                PopulateHierarchyTree(newNode, gameObjExt);
            }
        }
        private string GetClassName(AssetsManager manager, AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            AssetTypeValueField scriptBaseField = manager.GetExtAsset(inst, baseField["m_Script"]).baseField;
            if (scriptBaseField == null)
                return "Unknown class name";

            return scriptBaseField["m_Name"].AsString;
        }

        private void goTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            GetSelectedNodeData();
        }
        private void FollowBtn_Click(object sender, EventArgs e)
        {
            valueGrid.Focus();
            GridItem item = valueGrid.SelectedGridItem;
            if (item.Label == "m_FileID" || item.Label == "m_PathID")
            {
                int fileId = -1;
                long pathId = -1;
                foreach (GridItem gi in item.Parent.EnumerateAllItems())
                {
                    if (gi.Label == "m_FileID")
                        fileId = int.Parse((string)gi.Value);
                    if (gi.Label == "m_PathID")
                        pathId = long.Parse((string)gi.Value);
                }
                if (fileId == 0 && pathId == 0)
                {
                    MessageBox.Show("Cannot open null reference", "Assets View");
                    return;
                }
                else if (fileId == -1 || pathId == -1)
                {
                    MessageBox.Show("Could not find other id, is this really a pptr?", "Assets View");
                    return;
                }
                AssetExternal ext = helper.GetExtAsset(inst, fileId, pathId, true);
                if (ext.file == null)
                {
                    MessageBox.Show("Dependency could not be loaded. Maybe the file couldn't be loaded or doesn't exist?", "Assets View");
                    return;
                }
                GameObjectViewer view = new GameObjectViewer(helper, ext.file, ext.info.PathId);
                view.Show();
            }
        }
    }
}
