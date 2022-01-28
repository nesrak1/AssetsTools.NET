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

            ClassDatabaseFile classFile = helper.classFile;
            AssetFileInfoEx info = inst.table.GetAssetInfo(selectedId);
            AssetTypeTemplateField typeTemplate = helper.GetTemplateBaseField(inst.file, info);
            string typeName = typeTemplate.type;

            if (typeName == "GameObject")
            {
                selectedGameObjectId = selectedId;
                AssetExternal firstExt = helper.GetExtAsset(inst, 0, selectedId);
                AssetExternal rootExt = GetRootGameObject(firstExt);
                PopulateHierarchyTree(null, rootExt);
            }
            else
            {
                bool hasGameObjectField = typeTemplate.children.Any(f => f.name == "m_GameObject");
                if (hasGameObjectField)
                {
                    AssetTypeValueField firstBaseField = helper.GetTypeInstance(inst.file, info).GetBaseField();
                    AssetExternal firstExt = helper.GetExtAsset(inst, firstBaseField.Get("m_GameObject"));
                    if (firstExt.info != null)
                    {
                        selectedGameObjectId = firstExt.info.index;
                        AssetExternal rootExt = GetRootGameObject(firstExt);
                        PopulateHierarchyTree(null, rootExt);
                    }
                    else
                    {
                        TreeNode node = goTree.Nodes.Add($"[{typeName} (parentless)]");
                        node.Tag = helper.GetExtAsset(inst, 0, info.index);
                    }
                }
                else
                {
                    TreeNode node = goTree.Nodes.Add($"[{typeName}]");
                    node.Tag = helper.GetExtAsset(inst, 0, info.index);
                }
            }
            GetSelectedNodeData();
        }

        private AssetExternal GetRootGameObject(AssetExternal ext)
        {
            AssetExternal transformExt = helper.GetExtAsset(inst, ext.instance.GetBaseField().Get("m_Component").Get("Array")[0].GetLastChild());
            while (true)
            {
                AssetExternal parentExt = helper.GetExtAsset(inst, transformExt.instance.GetBaseField().Get("m_Father"));
                if (parentExt.instance == null)
                {
                    AssetExternal gameObjectExt = helper.GetExtAsset(inst, transformExt.instance.GetBaseField().Get("m_GameObject"));
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
                    AssetTypeValueField baseField = ext.instance.GetBaseField();
                    string typeName = baseField.templateField.type;
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
                        LoadComponentData(root, ext.instance.GetBaseField(), ext.info, 0, 0);
                        valueGrid.SelectedObject = root;
                    }
                }
            }
        }

        bool loadedWithErrors = false;
        private PGProperty LoadGameObjectData(AssetTypeValueField field)
        {
            PGProperty root = new PGProperty("root");
            AssetTypeValueField components = field.Get("m_Component").Get("Array");
            int componentSize = components.GetValue().AsArray().size;
            for (int i = 0; i < componentSize; i++)
            {
                AssetTypeValueField componentPtr = components[i].GetLastChild();
                if (ModifierKeys == Keys.Shift)
                {
                    AssetExternal ext = helper.GetExtAsset(inst, componentPtr);
                    if (ext.instance != null)
                        LoadComponentData(root, ext.instance.GetBaseField(), ext.info, i, componentSize);
                }
                else
                {
                    try
                    {
                        AssetExternal ext = helper.GetExtAsset(inst, componentPtr);
                        if (ext.instance != null)
                            LoadComponentData(root, ext.instance.GetBaseField(), ext.info, i, componentSize);
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

        private void LoadComponentData(PGProperty root, AssetTypeValueField baseField, AssetFileInfoEx info, int index, int size)
        {
            string className = AssetHelper.FindAssetClassByID(helper.classFile, info.curFileType)
                .name.GetString(helper.classFile);
            AssetTypeValueField targetBaseField = baseField;
            if (className == "MonoBehaviour")
            {
                className += $" ({GetClassName(helper, inst, targetBaseField)})";
                string managedPath = Path.Combine(Path.GetDirectoryName(inst.path), "Managed");
                if (Directory.Exists(managedPath))
                {
                    targetBaseField = helper.GetMonoBaseFieldCached(inst, info, managedPath);
                }
            }
            string category = new string('\t', size - index) + className;
            PopulateDataGrid(targetBaseField, root, info, category);
        }

        private void PopulateDataGrid(AssetTypeValueField atvf, PGProperty node, AssetFileInfoEx info, string category, bool arrayChildren = false)
        {
            if (atvf.childrenCount == 0)
                return;
        
            string arrayName = string.Empty;
            if (arrayChildren && atvf.childrenCount > 0)
                arrayName = atvf.children[0].templateField.name;
        
            for (int i = 0; i < atvf.childrenCount; i++)
            {
                AssetTypeValueField atvfc = atvf.children[i];
                if (atvfc == null)
                    return;
        
                string key;
                if (!arrayChildren)
                    key = atvfc.GetName();
                else
                    key = $"{arrayName}[{i}]";
        
                EnumValueTypes evt;
                if (atvfc.GetValue() != null)
                {
                    evt = atvfc.GetValue().GetValueType();
                    if (evt != EnumValueTypes.None)
                    {
                        if (1 <= (int)evt && (int)evt <= 12)
                        {
                            string value = atvfc.GetValue().AsString();
                            PGProperty prop = new PGProperty(key, value);
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            PopulateDataGrid(atvfc, prop, info, category);
                        }
                        else if (evt == EnumValueTypes.Array)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[size: {atvfc.childrenCount}]");
                            PGProperty prop = new PGProperty(key, childProps, $"[size: {atvfc.childrenCount}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            PopulateDataGrid(atvfc, childProps, info, category, true);
                        }
                        else if (evt == EnumValueTypes.ByteArray)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[bytes size: {atvfc.GetValue().AsByteArray().size}]");
                            PGProperty prop = new PGProperty(key, childProps, $"[bytes size: {atvfc.GetValue().AsByteArray().size}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                        }
                    }
                }
                else
                {
                    PGProperty childProps;
                    if (atvfc.childrenCount == 2)
                    {
                        AssetTypeValueField fileId = atvfc.children[0];
                        AssetTypeValueField pathId = atvfc.children[1];
                        string fileIdName = fileId.templateField.name;
                        string fileIdType = fileId.templateField.type;
                        string pathIdName = pathId.templateField.name;
                        string pathIdType = pathId.templateField.type;
                        if (fileIdName == "m_FileID" && fileIdType == "int" &&
                            pathIdName == "m_PathID" && pathIdType == "SInt64")
                        {
                            int fileIdValue = fileId.GetValue().AsInt();
                            long pathIdValue = pathId.GetValue().AsInt64();
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

        private void SetSelectedStateIfSelected(AssetFileInfoEx info, PGProperty prop)
        {
            if (info.index == selectedId)
            {
                prop.description = "SELECTEDCOMPONENT";
            }
        }

        private void PopulateHierarchyTree(TreeNode node, AssetExternal ext)
        {
            AssetTypeValueField baseField = ext.instance.GetBaseField();
            long thisId = ext.info.index;

            TreeNode newNode;
            if (node == null)
                newNode = goTree.Nodes.Add(baseField.Get("m_Name").GetValue().AsString());
            else
                newNode = node.Nodes.Add(baseField.Get("m_Name").GetValue().AsString());

            if (!baseField.Get("m_IsActive").GetValue().AsBool())
                newNode.ForeColor = Color.DarkRed;

            newNode.Tag = ext;
            if (thisId == selectedGameObjectId)
            {
                goTree.SelectedNode = newNode;
            }

            AssetTypeValueField children =
                helper.GetExtAsset(inst, baseField.Get("m_Component").Get("Array")[0].GetLastChild()).instance
                                                  .GetBaseField().Get("m_Children").Get("Array");

            for (int i = 0; i < children.childrenCount; i++)
            {
                AssetExternal childExternal = helper.GetExtAsset(inst, children[i]);
                AssetExternal gameObjExt = helper.GetExtAsset(inst, childExternal.instance.GetBaseField().Get("m_GameObject"));
                PopulateHierarchyTree(newNode, gameObjExt);
            }
        }
        private string GetClassName(AssetsManager manager, AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            AssetTypeInstance scriptAti = manager.GetExtAsset(inst, baseField.Get("m_Script")).instance;
            if (scriptAti == null)
                return "Unknown class name";
            return scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
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
                GameObjectViewer view = new GameObjectViewer(helper, ext.file, ext.info.index);
                view.Show();
            }
        }
    }
}
