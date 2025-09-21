using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace TobyCopy
{
    public static class TypeTreeCopier
    {
        // only set genManager and srcInst if you want to generate nodes (need to copy from src assets file -> dst bundle file)
        public static TypeTreeType? Copy(
            AssetsFile dstFile, TypeTreeType type, AssetsManager? genManager = null, AssetsFileInstance? srcInst = null)
        {
            var newType = Copy(dstFile.Metadata.TypeTreeTypes, type, genManager, srcInst);
            if (newType != null && newType.ScriptTypeIndex != ushort.MaxValue)
            {
                // need to replace the script index because it will be different
                // in the new file we're adding to
                var scriptIndex = (ushort)dstFile.Metadata.ScriptTypes.Count;
                newType.ScriptTypeIndex = scriptIndex;
                // needs to be filled in later when the MonoScript is added
                dstFile.Metadata.ScriptTypes.Add(new AssetPPtr(0, 0));
            }
            return newType;
        }

        public static TypeTreeType? CopyRefType(
            AssetsFile dstFile, TypeTreeType type, AssetsManager? genManager = null, AssetsFileInstance? srcInst = null)
        {
            var newType = Copy(dstFile.Metadata.RefTypes, type, genManager, srcInst);
            if (newType != null && newType.ScriptTypeIndex != ushort.MaxValue)
            {
                // going to assume ref ids are in order
                var scriptIndex = (ushort)dstFile.Metadata.RefTypes.Count;
                newType.ScriptTypeIndex = scriptIndex;
            }
            return newType;
        }

        private static TypeTreeType? Copy(
            List<TypeTreeType> dstTypes, TypeTreeType type, AssetsManager? genManager, AssetsFileInstance? srcInst)
        {
            if (type.TypeId != (int)AssetClassID.MonoBehaviour)
            {
                if (dstTypes.Any(t => t.TypeId == type.TypeId))
                {
                    return null;
                }
            }

            var newType = new TypeTreeType
            {
                TypeId = type.TypeId,
                IsStrippedType = type.IsStrippedType,
                ScriptTypeIndex = type.ScriptTypeIndex,
                ScriptIdHash = type.ScriptIdHash,
                TypeHash = type.TypeHash,
                Nodes = type.Nodes != null ? CopyNodes(type, genManager, srcInst) : null,
                StringBufferBytes = type.StringBufferBytes?.ToArray(),
                IsRefType = type.IsRefType,
                TypeDependencies = type.TypeDependencies?.ToArray(),
                TypeReference = type.TypeReference != null ? CopyTypeReference(type.TypeReference) : null
            };

            dstTypes.Add(newType);
            return newType;
        }

        private static List<TypeTreeNode>? CopyNodes(TypeTreeType type, AssetsManager? genManager, AssetsFileInstance? srcInst)
        {
            var nodes = type.Nodes;
            if ((nodes == null || nodes.Count == 0) && genManager == null)
            {
                return nodes;
            }

            var newNodes = new List<TypeTreeNode>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    newNodes.Add(new TypeTreeNode
                    {
                        Version = node.Version,
                        Level = node.Level,
                        TypeFlags = node.TypeFlags,
                        TypeStrOffset = node.TypeStrOffset,
                        NameStrOffset = node.NameStrOffset,
                        ByteSize = node.ByteSize,
                        Index = node.Index,
                        MetaFlags = node.MetaFlags,
                        RefTypeHash = node.RefTypeHash,
                    });
                }
            }
            else if (genManager != null && srcInst != null)
            {
                if (type.ScriptTypeIndex == ushort.MaxValue)
                {
                    var typeTreeType = ClassDatabaseToTypeTree.Convert(genManager.ClassDatabase, type.TypeId, false);
                    newNodes.AddRange(typeTreeType.Nodes);
                }
                else
                {
                    var monoTypeTreeNodes = GetMonoBehaviourTypeTreeType(genManager, srcInst, type);
                    newNodes.AddRange(monoTypeTreeNodes.Nodes);
                }
            }
            return newNodes;
        }

        // todo: common strings
        private static TypeTreeType GetMonoBehaviourTypeTreeType(AssetsManager genManager, AssetsFileInstance srcInst, TypeTreeType srcType)
        {
            var monoBehaviourTypeId = (int)AssetClassID.MonoBehaviour;
            var baseMonoBehaviourTemplate = genManager.GetTemplateBaseField(
                srcInst, null, 0, monoBehaviourTypeId, ushort.MaxValue, AssetReadFlags.SkipMonoBehaviourFields);

            var monoScriptPPtr = srcInst.file.Metadata.ScriptTypes[srcType.ScriptTypeIndex];
            var monoScriptExt = genManager.GetExtAsset(srcInst, monoScriptPPtr.FileId, monoScriptPPtr.PathId);
            var monoScriptBaseField = monoScriptExt.baseField;
            var className = monoScriptBaseField["m_ClassName"].AsString;
            var nameSpace = monoScriptBaseField["m_Namespace"].AsString;
            var assemblyName = monoScriptBaseField["m_AssemblyName"].AsString;
            if (assemblyName.EndsWith(".dll"))
            {
                assemblyName = assemblyName[..^4];
            }

            if (monoScriptBaseField == null)
            {
                return TemplateFieldToTypeTree.Convert(baseMonoBehaviourTemplate, monoBehaviourTypeId, ushort.MaxValue);
            }

            var newBaseField = genManager.MonoTempGenerator.GetTemplateField(
                baseMonoBehaviourTemplate, assemblyName, nameSpace, className, new UnityVersion(srcInst.file.Metadata.UnityVersion));

            return TemplateFieldToTypeTree.Convert(newBaseField, monoBehaviourTypeId, srcType.ScriptTypeIndex);
        }

        private static AssetTypeReference CopyTypeReference(AssetTypeReference typeReference)
        {
            return new AssetTypeReference
            {
                ClassName = typeReference.ClassName,
                Namespace = typeReference.Namespace,
                AsmName = typeReference.AsmName
            };
        }
    }
}