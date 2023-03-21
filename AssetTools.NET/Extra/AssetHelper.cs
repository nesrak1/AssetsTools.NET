using System.Collections.Generic;
using System.Reflection;

namespace AssetsTools.NET.Extra
{
    public static class AssetHelper
    {
        public static Dictionary<int, AssetTypeReference> GetAssetsFileScriptInfos(AssetsManager am, AssetsFileInstance inst)
        {
            Dictionary<int, AssetTypeReference> infos = new Dictionary<int, AssetTypeReference>();
            List<AssetPPtr> scriptTypes = inst.file.Metadata.ScriptTypes;
            for (int i = 0; i < scriptTypes.Count; i++)
            {
                AssetTypeReference typeRef = GetAssetsFileScriptInfo(am, inst, i);
                if (typeRef == null)
                {
                    continue;
                }

                infos[i] = typeRef;
            }

            return infos;
        }

        public static AssetTypeReference GetAssetsFileScriptInfo(AssetsManager am, AssetsFileInstance inst, int index)
        {
            List<AssetPPtr> scriptTypes = inst.file.Metadata.ScriptTypes;
            AssetPPtr scriptPPtr = scriptTypes[index];

            AssetTypeValueField msBaseField;
            try
            {
                msBaseField = am.GetExtAsset(inst, scriptPPtr.FileId, scriptPPtr.PathId).baseField;
                if (msBaseField == null)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            AssetTypeValueField assemblyNameField = msBaseField["m_AssemblyName"];
            AssetTypeValueField nameSpaceField = msBaseField["m_Namespace"];
            AssetTypeValueField classNameField = msBaseField["m_ClassName"];
            if (assemblyNameField.IsDummy || nameSpaceField.IsDummy || classNameField.IsDummy)
            {
                return null;
            }

            string assemblyName = assemblyNameField.AsString;
            string nameSpace = nameSpaceField.AsString;
            string className = classNameField.AsString;

            AssetTypeReference info = new AssetTypeReference(className, nameSpace, assemblyName);
            return info;
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
}
