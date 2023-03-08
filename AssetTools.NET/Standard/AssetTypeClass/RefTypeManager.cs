using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class RefTypeManager
    {
        private Dictionary<AssetTypeReference, AssetTypeTemplateField> typeTreeLookup;
        private Dictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateLookup;
        private IMonoBehaviourTemplateGenerator monoTemplateGenerator;
        private UnityVersion unityVersion;
        private bool isSharedMonoLookup;

        public RefTypeManager()
        {
            typeTreeLookup = new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
        }

        public void Clear()
        {
            typeTreeLookup.Clear();
            if (!isSharedMonoLookup)
            {
                monoTemplateLookup.Clear();
            }
        }

        public void FromTypeTree(AssetsFileMetadata metadata)
        {
            if (!metadata.TypeTreeEnabled || metadata.RefTypes == null)
            {
                return;
            }

            foreach (TypeTreeType type in metadata.RefTypes)
            {
                if (!type.IsRefType)
                    continue;

                AssetTypeTemplateField templateField = new AssetTypeTemplateField();
                templateField.FromTypeTree(type);
                //If RefType has fields with [SerializeReference] it will contain its own registry,
                //but it shouldn't be there, as the registry is only available at the root type
                if (templateField.Children.Count > 0 && templateField.Children[templateField.Children.Count - 1].ValueType == AssetValueType.ManagedReferencesRegistry)
                {
                    templateField.Children.RemoveAt(templateField.Children.Count - 1);
                }

                typeTreeLookup[type.TypeReference] = templateField;
            }
        }

        public void WithMonoTemplateGenerator(AssetsFileMetadata metadata, IMonoBehaviourTemplateGenerator monoTemplateGenerator, Dictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = null)
        {
            this.monoTemplateGenerator = monoTemplateGenerator;
            unityVersion = new UnityVersion(metadata.UnityVersion);
            monoTemplateLookup = monoTemplateFieldCache ?? new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
            isSharedMonoLookup = monoTemplateLookup != null;
        }

        public AssetTypeTemplateField GetTemplateField(AssetTypeReference type)
        {
            if (type == null || (string.IsNullOrEmpty(type.ClassName) && string.IsNullOrEmpty(type.Namespace) && string.IsNullOrEmpty(type.AsmName)) || type.Equals(AssetTypeReference.TERMINUS))
            {
                return null;
            }

            if (typeTreeLookup.TryGetValue(type, out AssetTypeTemplateField templateField))
            {
                return templateField;
            }

            if (monoTemplateGenerator != null)
            {
                if (monoTemplateLookup.TryGetValue(type, out templateField))
                {
                    return templateField;
                }

                templateField = new AssetTypeTemplateField
                {
                    Name = "Base",
                    Type = type.ClassName,
                    ValueType = AssetValueType.None,
                    IsArray = false,
                    IsAligned = false,
                    HasValue = false,
                    Children = new List<AssetTypeTemplateField>(0)
                };
                templateField = monoTemplateGenerator.GetTemplateField(templateField, type.AsmName, type.Namespace, type.ClassName, unityVersion);
                if (templateField != null)
                {
                    monoTemplateLookup[type] = templateField;
                    return templateField;
                }
            }

            return null;
        }
    }
}
