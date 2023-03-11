using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public class MonoCecilTempGenerator : IMonoBehaviourTemplateGenerator
    {
        private UnityVersion unityVersion;
        private bool anyFieldIsManagedReference;

        public string managedPath;
        public Dictionary<string, AssemblyDefinition> loadedAssemblies = new Dictionary<string, AssemblyDefinition>();

        public MonoCecilTempGenerator(string managedPath)
        {
            this.managedPath = managedPath;
        }

        public void Dispose()
        {
            foreach (AssemblyDefinition assembly in loadedAssemblies.Values)
            {
                assembly.Dispose();
            }

            loadedAssemblies.Clear();
        }

        public AssetTypeTemplateField GetTemplateField(AssetTypeTemplateField baseField, string assemblyName, string nameSpace, string className, UnityVersion unityVersion)
        {
            // newer games don't have .dll
            if (!assemblyName.EndsWith(".dll"))
            {
                assemblyName += ".dll";
            }

            string assemblyPath = Path.Combine(managedPath, assemblyName);
            if (!File.Exists(assemblyPath))
            {
                return null;
            }

            List<AssetTypeTemplateField> newFields = Read(assemblyPath, nameSpace, className, unityVersion);

            AssetTypeTemplateField newBaseField = baseField.Clone();
            newBaseField.Children.AddRange(newFields);

            return newBaseField;
        }

        public List<AssetTypeTemplateField> Read(string assemblyPath, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            AssemblyDefinition asmDef = GetAssemblyWithDependencies(assemblyPath);
            return Read(asmDef, nameSpace, typeName, unityVersion);
        }

        public List<AssetTypeTemplateField> Read(AssemblyDefinition assembly, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            this.unityVersion = unityVersion;
            anyFieldIsManagedReference = false;
            List<AssetTypeTemplateField> children = new List<AssetTypeTemplateField>();

            RecursiveTypeLoad(assembly.MainModule, nameSpace, typeName, children, CommonMonoTemplateHelper.GetSerializationLimit(unityVersion));
            return children;
        }

        private AssemblyDefinition GetAssemblyWithDependencies(string path)
        {
            string assemblyName = Path.GetFileName(path);
            if (loadedAssemblies.ContainsKey(assemblyName))
            {
                return loadedAssemblies[assemblyName];
            }

            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(path));

            ReaderParameters readerParameters = new ReaderParameters()
            {
                AssemblyResolver = resolver
            };

            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(path, readerParameters);
            loadedAssemblies[assemblyName] = asmDef;

            return asmDef;
        }

        private void RecursiveTypeLoad(ModuleDefinition module, string nameSpace, string typeName, List<AssetTypeTemplateField> attf, int availableDepth)
        {
            // TypeReference needed for TypeForwardedTo in UnityEngine (and others)
            TypeReference typeRef;
            TypeDefinition type = null;

            if (typeName.Contains('/'))
            {
                string[] types = typeName.Split('/');
                type = new TypeReference(nameSpace, types[0], module, module).Resolve();
                for (int i = 1; i < types.Length; i++)
                {
                    typeRef = new TypeReference("", types[i], module, module)
                    {
                        DeclaringType = type
                    };
                    type = typeRef.Resolve();
                }
            }
            else
            {
                typeRef = new TypeReference(nameSpace, typeName, module, module);
                type = typeRef.Resolve();
            }

            RecursiveTypeLoad(type, attf, availableDepth, true);
        }

        private void RecursiveTypeLoad(TypeDefWithSelfRef type, List<AssetTypeTemplateField> attf, int availableDepth, bool isRecursiveCall = false)
        {
            if (!isRecursiveCall)
            {
                availableDepth--;
            }

            string baseName = type.typeDef.BaseType.FullName;
            if (baseName != "System.Object" &&
                baseName != "UnityEngine.Object" &&
                baseName != "UnityEngine.MonoBehaviour" &&
                baseName != "UnityEngine.ScriptableObject")
            {
                TypeDefWithSelfRef typeDef = type.typeDef.BaseType;
                typeDef.AssignTypeParams(type);
                RecursiveTypeLoad(typeDef, attf, availableDepth, true);
            }

            attf.AddRange(ReadTypes(type, availableDepth));
        }

        private List<AssetTypeTemplateField> ReadTypes(TypeDefWithSelfRef type, int availableDepth)
        {
            List<FieldDefinition> acceptableFields = GetAcceptableFields(type, availableDepth);
            List<AssetTypeTemplateField> localChildren = new List<AssetTypeTemplateField>();
            for (int i = 0; i < acceptableFields.Count; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                FieldDefinition fieldDef = acceptableFields[i];
                TypeDefWithSelfRef fieldTypeDef = type.SolidifyType(fieldDef.FieldType);

                bool isArrayOrList = false;
                bool isPrimitive = false;
                bool derivesFromUEObject = false;
                bool isManagedReference = false;

                if (fieldTypeDef.typeRef.MetadataType == MetadataType.Array)
                {
                    ArrayType arrType = (ArrayType)fieldTypeDef.typeRef;
                    isArrayOrList = arrType.IsVector;
                }
                else if (fieldTypeDef.typeDef.FullName == "System.Collections.Generic.List`1")
                {
                    fieldTypeDef = fieldTypeDef.typeParamToArg.First().Value;
                    isArrayOrList = true;
                }

                field.Name = fieldDef.Name;
                if (isPrimitive = fieldTypeDef.typeDef.IsEnum)
                {
                    var enumType = fieldTypeDef.typeDef.GetEnumUnderlyingType().FullName;
                    field.Type = CommonMonoTemplateHelper.ConvertBaseToPrimitive(enumType);
                }
                else if (isPrimitive = fieldTypeDef.typeDef.IsPrimitive)
                {
                    field.Type = CommonMonoTemplateHelper.ConvertBaseToPrimitive(fieldTypeDef.typeDef.FullName);
                }
                else if (fieldTypeDef.typeDef.FullName == "System.String")
                {
                    field.Type = "string";
                }
                else if (derivesFromUEObject = DerivesFromUEObject(fieldTypeDef))
                {
                    field.Type = $"PPtr<${fieldTypeDef.typeDef.Name}>";
                }
                else if (isManagedReference = fieldDef.CustomAttributes.Any(a => a.AttributeType.Name == "SerializeReference"))
                {
                    anyFieldIsManagedReference = true;
                    field.Type = "managedReference";
                }
                else 
                {
                    field.Type = fieldTypeDef.typeDef.Name;
                }

                if (isPrimitive)
                {
                    field.Children = new List<AssetTypeTemplateField>();
                }
                else if (fieldTypeDef.typeDef.FullName == "System.String")
                {
                    field.Children = CommonMonoTemplateHelper.String();
                }
                else if (CommonMonoTemplateHelper.IsSpecialUnityType(fieldTypeDef.typeDef.FullName))
                {
                    field.Children = SpecialUnity(fieldTypeDef, availableDepth);
                }
                else if (derivesFromUEObject)
                {
                    field.Children = CommonMonoTemplateHelper.PPtr(unityVersion);
                }
                else if (isManagedReference)
                {
                    field.Children = CommonMonoTemplateHelper.ManagedReference(unityVersion);
                }
                else if (fieldTypeDef.typeDef.IsSerializable)
                {
                    field.Children = Serialized(fieldTypeDef, availableDepth);
                }

                field.ValueType = AssetTypeValueField.GetValueTypeByTypeName(field.Type);
                field.IsAligned = CommonMonoTemplateHelper.TypeAligns(field.ValueType);
                field.HasValue = field.ValueType != AssetValueType.None;

                if (isArrayOrList)
                {
                    if (isPrimitive || derivesFromUEObject)
                    {
                        field = CommonMonoTemplateHelper.Vector(field);
                    }
                    else
                    {
                        field = CommonMonoTemplateHelper.VectorWithType(field);
                    }
                }
                localChildren.Add(field);
            }

            if (anyFieldIsManagedReference && DerivesFromUEObject(type))
            {
                localChildren.Add(CommonMonoTemplateHelper.ManagedReferencesRegistry("references", unityVersion));
            }

            return localChildren;
        }

        private List<FieldDefinition> GetAcceptableFields(TypeDefWithSelfRef typeDef, int availableDepth)
        {
            List<FieldDefinition> validFields = new List<FieldDefinition>();
            foreach (FieldDefinition f in typeDef.typeDef.Fields)
            {
                if (Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.Public) ||
                    f.CustomAttributes.Any(a => a.AttributeType.FullName == "UnityEngine.SerializeField") ||
                    f.CustomAttributes.Any(a => a.AttributeType.FullName == "UnityEngine.SerializeReference")) //field is public or has exception attribute
                {
                    if (!Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.Static) &&
                        !Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.NotSerialized) &&
                        !f.IsInitOnly &&
                        !f.HasConstant) //field is not public, has exception attribute, readonly, or const
                    {
                        TypeDefWithSelfRef ft = typeDef.SolidifyType(f.FieldType);

                        if (TryGetListOrArrayElement(ft.typeRef, out TypeDefWithSelfRef elemType))
                        {
                            //Array are not serialized at and past the serialization limit
                            if (availableDepth < 0)
                            {
                                continue;
                            }

                            //Unity can't serialize collection of collections, ignoring it
                            if (TryGetListOrArrayElement(elemType, out _))
                            {
                                continue;
                            }
                            ft = elemType;
                        }
                        //Unity doesn't serialize a field of the same type as declaring type
                        //unless it inherits from UnityEngine.Object
                        else if (typeDef.typeDef.FullName == ft.typeDef.FullName && !DerivesFromUEObject(typeDef))
                        {
                            continue;
                        }

                        TypeDefinition ftd = ft.typeDef;
                        if (ftd != null && IsValidDef(f, ftd, availableDepth))
                        {
                            validFields.Add(f);
                        }
                    }
                }
            }
            return validFields;

            bool TryGetListOrArrayElement(TypeDefWithSelfRef fieldType, out TypeDefWithSelfRef elemType)
            {
                if (fieldType.typeRef is ArrayType aft)
                {
                    elemType = aft.ElementType;
                    return true;
                }
                else if (fieldType.typeRef is GenericInstanceType gft && gft.ElementType.FullName == "System.Collections.Generic.List`1")
                {
                    elemType = fieldType.typeParamToArg["T"];
                    return true;
                }

                elemType = default;
                return false;
            }
            
            bool IsValidDef(FieldDefinition fieldDef, TypeDefinition typeDef, int availableDepth)
            {
                //Before 2020.1.0 you couldn't have fields of a generic type, so they should be ingored
                //https://unity.com/releases/editor/whats-new/2020.1.0
                if (typeDef.HasGenericParameters && unityVersion.major < 2020)
                {
                    return false;
                }

                if (typeDef.IsPrimitive ||
                    typeDef.FullName == "System.String")
                {
                    return true;
                }

                //Unity doesn't support long enums
                if (typeDef.IsEnum)
                {
                    var enumType = typeDef.GetEnumUnderlyingType().FullName;
                    return enumType != "System.Int64" && enumType != "System.UInt64";
                }

                //Value types are not affected by the serialization limit
                if (availableDepth < 0)
                {
                    return typeDef.IsValueType && (typeDef.IsSerializable || CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.FullName));
                }

                if (DerivesFromUEObject(typeDef) ||
                    CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.FullName))
                {
                    return true;
                }
                
                if (fieldDef.CustomAttributes.Any(a => a.AttributeType.Name == "SerializeReference"))
                {
                    if (unityVersion.major == 2019 && unityVersion.minor == 3 && unityVersion.patch < 8 && typeDef.FullName == "System.Object")
                    {
                        return false;
                    }

                    return !typeDef.IsValueType && !typeDef.HasGenericParameters;
                }

                if (CommonMonoTemplateHelper.IsAssemblyBlacklisted((typeDef.Scope as ModuleDefinition)?.Assembly.Name.Name ?? typeDef.Scope.Name, unityVersion))
                {
                    return false;
                }

                return !typeDef.IsAbstract && typeDef.IsSerializable;
            }
        }

        private bool DerivesFromUEObject(TypeDefWithSelfRef typeDef)
        {
            if (typeDef.typeDef.BaseType == null)
                return false;
            if (typeDef.typeDef.IsInterface)
                return false;
            if (typeDef.typeDef.BaseType.FullName == "UnityEngine.Object" ||
                typeDef.typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.typeDef.BaseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.typeDef.BaseType.Resolve());
            return false;
        }

        private List<AssetTypeTemplateField> Serialized(TypeDefWithSelfRef type, int availableDepth)
        {
            List<AssetTypeTemplateField> types = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(type, types, availableDepth);
            return types;
        }

        private List<AssetTypeTemplateField> SpecialUnity(TypeDefWithSelfRef type, int availableDepth)
        {
            return type.typeDef.Name switch
            {
                "Gradient" => CommonMonoTemplateHelper.Gradient(unityVersion),
                "AnimationCurve" => CommonMonoTemplateHelper.AnimationCurve(unityVersion),
                "LayerMask" => CommonMonoTemplateHelper.BitField(),
                "Bounds" => CommonMonoTemplateHelper.AABB(),
                "BoundsInt" => CommonMonoTemplateHelper.BoundsInt(),
                "Rect" => CommonMonoTemplateHelper.Rectf(),
                "RectOffset" => CommonMonoTemplateHelper.RectOffset(),
                "Color32" => CommonMonoTemplateHelper.RGBAi(),
                "GUIStyle" => CommonMonoTemplateHelper.GUIStyle(unityVersion),
                "Vector2Int" => CommonMonoTemplateHelper.Vector2Int(),
                "Vector3Int" => CommonMonoTemplateHelper.Vector3Int(),
                _ => Serialized(type, availableDepth)
            };
        }
    }
}