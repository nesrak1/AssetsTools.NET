using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsView.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class GameObjectViewer : Form
    {
        private AssetsManager helper;
        private AssetsFileInstance inst;
        private AssetTypeValueField baseField;
        private AssetFileInfoEx componentInfo;
        private ulong selectedIndex;
        private ulong selectedComponentIndex;
        private bool firstTimeMBMessage;
        public GameObjectViewer(AssetsManager helper, AssetsFileInstance inst, AssetTypeValueField baseField, ulong selectedIndex, ulong selectedComponentIndex = ulong.MaxValue)
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
            PInvoke.SetWindowTheme(goTree.Handle, "explorer", null);
            valueGrid.PropertySort = PropertySort.Categorized;
            if (componentInfo == null)
            {
                string baseName = baseField["m_Name"].GetValue().AsString();
                TreeNode node = goTree.Nodes.Add(baseName);
                node.Tag = baseField;
                RecursiveChildSearch(node, baseField);
                GetSelectedNodeData();
            }
            else
            {
                TreeNode node = goTree.Nodes.Add("[Unknown GameObject]");
                node.Tag = null;
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
                    if (selectedComponentIndex != ulong.MaxValue)
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

        private PGProperty LoadGameObjectData(AssetTypeValueField field)
        {
            PGProperty root = new PGProperty("root");
            AssetTypeValueField components = field.Get("m_Component").Get("Array");
            int componentSize = (int)components.GetValue().AsArray().size;
            for (int i = 0; i < componentSize; i++)
            {
                AssetTypeValueField componentPtr = components[(uint)i].Get("component");
                AssetsManager.AssetExternal ext = helper.GetExtAsset(inst, componentPtr);
                LoadComponentData(root, ext.instance.GetBaseField(), ext.info, i, componentSize);
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
                    MessageBox.Show("can't display monobehaviour data until dependencies are loaded");
                }
            }
            string category = new string('\t', size - index) + className;
            RecursiveTreeLoad(targetBaseField, root, info, category, 0);
        }

        private void RecursiveTreeLoad(AssetTypeValueField atvf, PGProperty node, AssetFileInfoEx info, string category, int depth)
        {
            if (atvf.childrenCount == 0)
                return;
            foreach (AssetTypeValueField atvfc in atvf.pChildren)
            {
                if (atvfc == null)
                    return;
                object value = "";
                EnumValueTypes evt;
                if (atvfc.GetValue() != null)
                {
                    evt = atvfc.GetValue().GetValueType();
                    if (evt != EnumValueTypes.ValueType_None)
                    {
                        if (1 <= (int)evt && (int)evt <= 12)
                        {
                            value = atvfc.GetValue().AsString();

                            PGProperty prop = new PGProperty(atvfc.GetName(), value);
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            RecursiveTreeLoad(atvfc, prop, info, category, depth + 1);
                        }
                        else if (evt == EnumValueTypes.ValueType_Array ||
                                 evt == EnumValueTypes.ValueType_ByteArray)
                        {
                            PGProperty childProps = new PGProperty("child", null, $"[size: {atvfc.childrenCount}]");
                            PGProperty prop = new PGProperty(atvfc.GetName(), childProps, $"[size: {atvfc.childrenCount}]");
                            prop.category = category;
                            SetSelectedStateIfSelected(info, prop);
                            node.Add(prop);
                            RecursiveTreeLoad(atvfc, childProps, info, category, depth + 1);
                        }
                    }
                }
                else
                {
                    PGProperty childProps = new PGProperty("child");
                    PGProperty prop = new PGProperty(atvfc.GetName(), childProps);
                    prop.category = category;
                    SetSelectedStateIfSelected(info, prop);
                    node.Add(prop);
                    RecursiveTreeLoad(atvfc, childProps, info, category, depth + 1);
                }
            }
        }

        private void SetSelectedStateIfSelected(AssetFileInfoEx info, PGProperty prop)
        {
            Console.WriteLine($"{info.index} == {selectedComponentIndex}");
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
                                              .Get("component")).instance
                                              .GetBaseField()
                                              .Get("m_Children")
                                              .Get("Array");
            for (int i = 0; i < children.GetValue().AsArray().size; i++)
            {
                AssetTypeInstance newInstance = helper.GetExtAsset(inst, children.Get((uint)i)).instance;
                AssetsManager.AssetExternal gameObjExt = helper.GetExtAsset(inst, newInstance.GetBaseField().Get("m_GameObject"));
                AssetTypeInstance newAti = gameObjExt.instance;
                AssetTypeValueField newBaseField = newAti.GetBaseField();
                TreeNode newNode = node.Nodes.Add(newBaseField.Get("m_Name").GetValue().AsString());
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
    }
}
