using System.Collections.Generic;
using System.Reflection;

namespace AssetsTools.NET.Extra
{
    public static class AssetHelper
    {
        public static List<AssetHelperMonoScriptInfo> GetAssetsFileScriptInfos(AssetsManager am, AssetsFileInstance inst)
        {
            List<AssetHelperMonoScriptInfo> infos = new List<AssetHelperMonoScriptInfo>();
            AssetsFileMetadata metadata = inst.file.Metadata;
            foreach (AssetPPtr scriptPPtr in metadata.ScriptTypes)
            {
                AssetTypeValueField msBaseField = am.GetExtAsset(inst, scriptPPtr.FileId, scriptPPtr.PathId).baseField;
                string assemblyName = msBaseField["m_AssemblyName"].AsString;
                string nameSpace = msBaseField["m_Namespace"].AsString;
                string className = msBaseField["m_ClassName"].AsString;
                
                AssetHelperMonoScriptInfo info = new AssetHelperMonoScriptInfo(assemblyName, nameSpace, className);
                infos.Add(info);
            }

            return infos;
        }

        public static string GetAssetNameFast(AssetsFile file, ClassDatabaseFile cldb, AssetFileInfo info)
        {
            ClassDatabaseType type = cldb.FindAssetClassByID(info.TypeId);
            AssetsFileReader reader = file.Reader;

            if (file.Metadata.TypeTreeEnabled)
            {
                ushort scriptId = file.GetScriptIndex(info);

                TypeTreeType ttType = file.Metadata.FindTypeTreeTypeByID(info.TypeId, scriptId);

                string ttTypeName = ttType.Nodes[0].GetTypeString(ttType.StringBuffer);
                if (ttType.Nodes.Count == 0) return cldb.GetString(type.Name); // fallback to cldb
                if (ttType.Nodes.Count > 1 && ttType.Nodes[1].GetNameString(ttType.StringBuffer) == "m_Name")
                {
                    reader.Position = info.AbsoluteByteStart;
                    return reader.ReadCountStringInt32();
                }
                // todo, use the typetree since we have it already, there could be extra fields
                else if (ttTypeName == "GameObject")
                {
                    reader.Position = info.AbsoluteByteStart;
                    int size = reader.ReadInt32();
                    int componentSize = file.Header.Version > 0x10 ? 0x0c : 0x10;
                    reader.Position += size * componentSize;
                    reader.Position += 0x04;
                    return reader.ReadCountStringInt32();
                }
                else if (ttTypeName == "MonoBehaviour")
                {
                    reader.Position = info.AbsoluteByteStart;
                    reader.Position += 0x1c;
                    string name = reader.ReadCountStringInt32();
                    if (name != "")
                    {
                        return name;
                    }
                }
                return ttTypeName;
            }

            string typeName = cldb.GetString(type.Name);
            if (type.ReleaseRootNode.Children.Count == 0) return typeName;
            if (type.ReleaseRootNode.Children.Count > 1 && cldb.GetString(type.ReleaseRootNode.Children[0].FieldName) == "m_Name")
            {
                reader.Position = info.AbsoluteByteStart;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "GameObject")
            {
                reader.Position = info.AbsoluteByteStart;
                int size = reader.ReadInt32();
                int componentSize = file.Header.Version > 0x10 ? 0x0c : 0x10;
                reader.Position += size * componentSize;
                reader.Position += 0x04;
                return reader.ReadCountStringInt32();
            }
            else if (typeName == "MonoBehaviour")
            {
                reader.Position = info.AbsoluteByteStart;
                reader.Position += 0x1c;
                string name = reader.ReadCountStringInt32();
                if (name != "")
                {
                    return name;
                }
            }
            return typeName;
        }
    }

    public struct AssetHelperMonoScriptInfo
    {
        public string assemblyName;
        public string nameSpace;
        public string className;

        public AssetHelperMonoScriptInfo(string assemblyName, string nameSpace, string className)
        {
            this.assemblyName = assemblyName;
            this.nameSpace = nameSpace;
            this.className = className;
        }
    }
}
