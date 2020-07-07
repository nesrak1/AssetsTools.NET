using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsView.Util;
using Plurally;
using System;
//using System.Data.Entity.Design.PluralizationServices;
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
        private AssetTypeValueField baseField;
        private AssetFileInfoEx componentInfo;
        private long selectedIndex;
        private long selectedComponentIndex;
        private bool firstTimeMBMessage;
        private Pluralizer plurServ; //for arrays
        public GameObjectViewer(AssetsManager helper, AssetsFileInstance inst, AssetTypeValueField baseField, long selectedIndex, long selectedComponentIndex = long.MaxValue)
        {
            InitializeComponent();
            this.helper = helper;
            this.inst = inst;
            this.baseField = baseField;
            this.selectedIndex = selectedIndex;
            this.selectedComponentIndex = selectedComponentIndex;
        }
        //if component doesn't know its father
        public GameObjectViewer(AssetsManager helper, AssetsFileInstance inst, AssetTypeValueField baseField, AssetFileInfoEx componentInfo)
        {
            InitializeComponent();
            this.helper = helper;
            this.inst = inst;
            this.baseField = baseField;
            this.componentInfo = componentInfo;
        }

        private void GameObjectViewer_Load(object sender, EventArgs e)
        {
            //mono won't be able to do this so ignore it
            if (Type.GetType("Mono.Runtime") == null)
            {
                PInvoke.SetWindowTheme(goTree.Handle, "explorer", null);
            }
            //plurServ = PluralizationService.CreateService(new CultureInfo("en-US")); //assuming most of the time the fields are english
            plurServ = new Pluralizer(new CultureInfo("en-US"));

            valueGrid.PropertySort = PropertySort.Categorized;
            if (componentInfo == null)
            {
                string baseName = baseField["m_Name"].GetValue().AsString();
                TreeNode node = goTree.Nodes.Add(baseName);
                if (!baseField["m_IsActive"].GetValue().AsBool())
                    node.ForeColor = Color.DarkRed;
                node.Tag = baseField;
                RecursiveChildSearch(node, baseField);
                GetSelectedNodeData();
            }
            else
            {
                TreeNode node = goTree.Nodes.Add("[Unknown GameObject]");
                node.Tag = null;
                Width = 700;
                GetSelectedNodeData();
            }
        }

        private void GetSelectedNodeData()
        {
            if (goTree.Nodes.Count > 0)
            {
                object nodeTag = null;
                if (goTree.SelectedNode != null)
                    nodeTag = goTree.SelectedNode.Tag;
                if (nodeTag is AssetTypeValueField)
                {
                    PGProperty dataRoot = LoadGameObjectData((AssetTypeValueField)nodeTag);
                    valueGrid.SelectedObject = dataRoot;
                    if (selectedComponentIndex != long.MaxValue)
                    {
                        GridItem gi = valueGrid.EnumerateAllItems().FirstOrDefault(i =>
                                          i.PropertyDescriptor != null &&
                                          i.PropertyDescriptor.Description == "SELECTEDCOMPONENT");
                        if (gi != null)
                        {
                            valueGrid.Focus();
                            gi.Select();
                        }
                    }
                }
                else
                {
                    if (componentInfo == null)
                    {
                        valueGrid.SelectedObject = null;
                    }
                    else
                    {
                        PGProperty root = new PGProperty("root");
                        LoadComponentData(root, baseField, componentInfo, 0, 1);
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
                            MessageBox.Show("a component failed to load. (tell nes or hold shift to throw)");
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
                if (AssetUtils.AllDependenciesLoaded(helper, inst))
                {
                    className += $" ({GetClassName(helper, inst, targetBaseField)})";
                    string managedPath = Path.Combine(Path.GetDirectoryName(inst.path), "Managed");
                    if (Directory.Exists(managedPath))
                    {
                        targetBaseField = helper.GetMonoBaseFieldCached(inst, info, managedPath);
                    }
                }
                else if (!firstTimeMBMessage)
                {
                    firstTimeMBMessage = true;
                    MessageBox.Show("Can't display MonoBehaviour data until dependencies are loaded", "Assets View");
                }
            }
            string category = new string('\t', size - index) + className;
            RecursiveTreeLoad(targetBaseField, root, info, category);
        }

        private void RecursiveTreeLoad(AssetTypeValueField atvf, PGProperty node, AssetFileInfoEx info, string category, bool arrayChildren = false)
        {
            if (atvf.childrenCount == 0)
                return;

            string arraySingular = string.Empty;
            if (arrayChildren && atvf.childrenCount > 0)
                arraySingular = plurServ.Singularize(atvf.children[0].templateField.name);

            for (int i = 0; i < atvf.childrenCount; i++)
            {
                AssetTypeValueField atvfc = atvf.children[i];
                if (atvfc == null)
                    return;

                string key;
                if (!arrayChildren)
                    key = atvfc.GetName();
                else
                    key = $"{arraySingular}[{i}]";

                EnumValueTypes evt;
                if (atvfc.GetValue() != null)
                {
                    evt = atvfc.GetValue().GetValueType();
                    if (evt != EnumValueTypes.ValueType_None)
                    {
                        if (1 <= (int)evt && (int)evt <= 12)
                        {
                            string value = atvfc.GetValue().AsString();
                            PGProperty prop = new PGProperty(key, value);
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            RecursiveTreeLoad(atvfc, prop, info, category);
                        }
                        else if (evt == EnumValueTypes.ValueType_Array ||
                                 evt == EnumValueTypes.ValueType_ByteArray)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[size: {atvfc.childrenCount}]");
                            PGProperty prop = new PGProperty(key, childProps, $"[size: {atvfc.childrenCount}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            RecursiveTreeLoad(atvfc, childProps, info, category, true);
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
                    RecursiveTreeLoad(atvfc, childProps, info, category);
                }
            }
        }

        private void SetSelectedStateIfSelected(AssetFileInfoEx info, PGProperty prop)
        {
            if (info.index == selectedComponentIndex)
            {
                prop.description = "SELECTEDCOMPONENT";
            }
        }

        private void RecursiveChildSearch(TreeNode node, AssetTypeValueField field)
        {
            AssetTypeValueField children =
                helper.GetExtAsset(inst, field.Get("m_Component")
                                              .Get("Array")
                                              .Get(0)
                                              .GetLastChild()).instance
                                              .GetBaseField()
                                              .Get("m_Children")
                                              .Get("Array");
            for (int i = 0; i < children.GetValue().AsArray().size; i++)
            {
                AssetTypeInstance newInstance = helper.GetExtAsset(inst, children.Get(i)).instance;
                AssetExternal gameObjExt = helper.GetExtAsset(inst, newInstance.GetBaseField().Get("m_GameObject"));
                AssetTypeInstance newAti = gameObjExt.instance;
                AssetTypeValueField newBaseField = newAti.GetBaseField();
                TreeNode newNode = node.Nodes.Add(newBaseField.Get("m_Name").GetValue().AsString());
                if (!newBaseField.Get("m_IsActive").GetValue().AsBool())
                    newNode.ForeColor = Color.DarkRed;
                newNode.Tag = newBaseField;
                if (gameObjExt.info.index == selectedIndex)
                {
                    goTree.SelectedNode = newNode;
                }
                RecursiveChildSearch(newNode, newBaseField);
            }
        }

        private string GetClassName(AssetsManager manager, AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            AssetTypeInstance scriptAti = manager.GetExtAsset(inst, baseField.Get("m_Script")).instance;
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
                AssetExternal ext = helper.GetExtAsset(inst, fileId, pathId);

                GameObjectViewer view = new GameObjectViewer(helper, ext.file, ext.instance.GetBaseField(), ext.info);
                view.Show();
            }
        }
    }
}
