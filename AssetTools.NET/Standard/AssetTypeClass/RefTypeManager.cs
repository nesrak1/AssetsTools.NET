using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class RefTypeManager
    {
        private Dictionary<AssetTypeReference, AssetTypeTemplateField> lookup;
        
        public RefTypeManager()
        {
            lookup = new Dictionary<AssetTypeReference, AssetTypeTemplateField>();
        }

        public void Clear()
        {
            lookup.Clear();
        }

        public void FromTypeTree(AssetsFileMetadata metadata)
        {
            foreach (TypeTreeType type in metadata.RefTypes)
            {
                if (!type.IsRefType)
                    continue;

                AssetTypeTemplateField templateField = new AssetTypeTemplateField();
                templateField.FromTypeTree(type);

                lookup[type.TypeReference] = templateField;
            }
        }
        
        public void FromTypeTree(AssetsFileMetadata metadata, TypeTreeType ttType)
        {
            foreach (int dep in ttType.TypeDependencies)
            {
                TypeTreeType type = metadata.FindRefTypeByIndex((ushort)dep);

                if (!type.IsRefType)
                    continue;

                AssetTypeTemplateField templateField = new AssetTypeTemplateField();
                templateField.FromTypeTree(type);

                lookup[type.TypeReference] = templateField;
            }
        }

        public AssetTypeTemplateField GetTemplateField(AssetTypeReference type)
        {
            if (lookup.TryGetValue(type, out AssetTypeTemplateField templateField))
            {
                return templateField;
            }

            return null;
        }
    }
}
