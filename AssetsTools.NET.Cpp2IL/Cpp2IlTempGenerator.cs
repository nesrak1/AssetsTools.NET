﻿using AssetsTools.NET.Extra;
using LibCpp2IL;
using LibCpp2IL.Metadata;
using LibCpp2IL.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ARUnityVersion = AssetRipper.Primitives.UnityVersion;
using ATUnityVersion = AssetsTools.NET.Extra.UnityVersion;

namespace AssetsTools.NET.Cpp2IL
{
    public class Cpp2IlTempGenerator : IMonoBehaviourTemplateGenerator
    {
        private readonly string _globalMetadataPath;
        private readonly string _assemblyPath;
        private ATUnityVersion _unityVersion;
        private bool _initialized;
        private bool anyFieldIsManagedReference;

        public Cpp2IlTempGenerator(string globalMetadataPath, string assemblyPath)
        {
            _globalMetadataPath = globalMetadataPath;
            _assemblyPath = assemblyPath;
            ResetCpp2IL();
        }

        public void Dispose()
        {
            ResetCpp2IL();
        }

        public void ResetCpp2IL()
        {
            LibCpp2IlMain.Reset();
            _unityVersion = null;
            _initialized = false;
        }

        public void SetUnityVersion(ATUnityVersion unityVersion)
        {
            LibCpp2IlMain.Reset();
            _unityVersion = unityVersion;
            _initialized = false;
        }

        public void SetUnityVersion(ARUnityVersion unityVersion)
        {
            LibCpp2IlMain.Reset();
            _unityVersion = new ATUnityVersion(unityVersion.ToString());
            _initialized = false;
        }

        public void SetUnityVersion(int major, int minor, int patch)
        {
            LibCpp2IlMain.Reset();
            _unityVersion = new ATUnityVersion(major + "." + minor + "." + patch);
            _initialized = false;
        }

        public void InitializeCpp2IL()
        {
            ARUnityVersion arUnityVersion = ARUnityVersion.Parse(_unityVersion.ToString());
            if (!LibCpp2IlMain.LoadFromFile(_assemblyPath, _globalMetadataPath, arUnityVersion))
            {
                throw new Exception("Cpp2Il initialization failed");
            }
            _initialized = true;
        }

        public AssetTypeTemplateField GetTemplateField(AssetTypeTemplateField baseField, string assemblyName, string nameSpace, string className, ATUnityVersion unityVersion)
        {
            int[] il2cppUnityVersion = new[] { unityVersion.major, unityVersion.minor, unityVersion.patch };
            if (!_initialized)
            {
                SetUnityVersion(unityVersion);
                InitializeCpp2IL();
            }
            else if (_unityVersion.major != unityVersion.major || _unityVersion.minor != unityVersion.minor || _unityVersion.patch != unityVersion.patch)
            {
                Debug.WriteLine("Warning: This unity version does not match what Cpp2Il was registered with. Call ResetUnityVersion().");
            }

            anyFieldIsManagedReference = false;

            Il2CppMetadata meta = LibCpp2IlMain.TheMetadata;

            Il2CppAssemblyDefinition asm = meta.AssemblyDefinitions.ToList().First(a => a.AssemblyName.Name == assemblyName);
            if (asm == null)
            {
                throw new Exception($"Assembly \"{assemblyName}\" was not found in the IL2CPP metadata.");
            }

            string fullName = $"{nameSpace}{(string.IsNullOrEmpty(nameSpace) ? "" : ".")}{className}";

            Il2CppTypeDefinition type = asm.Image.Types.FirstOrDefault(t => t.FullName == fullName);
            if (type == null)
            {
                throw new Exception($"Type \"{nameSpace}::{className}\" was not found in the IL2CPP metadata.");
            }
            Il2CppTypeReflectionData typeRef = new Il2CppTypeReflectionData
            {
                baseType = type,
                genericParams = Array.Empty<Il2CppTypeReflectionData>(),
                arrayRank = 0,
                arrayType = null,
                isArray = false,
                isGenericType = type.GenericContainer != null,
                isPointer = false,
                isType = true,
                variableGenericParamName = null,
            };

            List<AssetTypeTemplateField> templateFields = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(typeRef, templateFields, CommonMonoTemplateHelper.GetSerializationLimit(_unityVersion), true);

            AssetTypeTemplateField newBaseField = baseField.Clone();
            newBaseField.Children.AddRange(templateFields);

            return newBaseField;
        }

        private List<string> GetAttributeNamesOnField(Il2CppImageDefinition image, Il2CppFieldDefinition field)
        {
            List<string> attributeNames = new List<string>();

            var attributeTypeRange = LibCpp2IlMain.TheMetadata.GetCustomAttributeData(image, field.customAttributeIndex, field.token, out int idx);

            if (attributeTypeRange == null)
            {
                return attributeNames;
            }

            for (int attributeIdx = 0; attributeIdx < attributeTypeRange.count; attributeIdx++)
            {
                var attributeTypeIndex = LibCpp2IlMain.TheMetadata.attributeTypes[attributeTypeRange.start + attributeIdx];
                var attributeTypeDef = LibCpp2IlMain.TheMetadata.typeDefs.FirstOrDefault(td => td.ByvalTypeIndex == attributeTypeIndex);
                if (attributeTypeDef != null)
                {
                    attributeNames.Add(attributeTypeDef.FullName);
                }
            }

            return attributeNames;
        }

        private void RecursiveTypeLoad(TypeDefWithSelfRef type, List<AssetTypeTemplateField> templateFields, int availableDepth, bool isRecursiveCall = false)
        {
            if (!isRecursiveCall)
            {
                availableDepth--;
            }

            string baseName = type.typeDef.BaseType.baseType.FullName;
            if (baseName != "System.Object" &&
                baseName != "UnityEngine.Object" &&
                baseName != "UnityEngine.MonoBehaviour" &&
                baseName != "UnityEngine.ScriptableObject")
            {
                TypeDefWithSelfRef typeDef = type.typeDef.BaseType;
                typeDef.AssignTypeParams(type);
                RecursiveTypeLoad(typeDef, templateFields, availableDepth, true);
            }

            templateFields.AddRange(ReadTypes(type, availableDepth));
        }

        private List<AssetTypeTemplateField> ReadTypes(TypeDefWithSelfRef type, int availableDepth)
        {
            List<Il2CppFieldDefinition> acceptableFields = GetAcceptableFields(type, availableDepth);
            List<AssetTypeTemplateField> localChildren = new List<AssetTypeTemplateField>();
            for (int i = 0; i < acceptableFields.Count; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                Il2CppFieldDefinition fieldDef = acceptableFields[i];
                TypeDefWithSelfRef fieldTypeDef = type.SolidifyType(fieldDef.FieldType);

                bool isPrimitive;
                bool isArrayOrList = false;
                bool derivesFromUEObject = false;
                bool isManagedReference = false;

                if (fieldTypeDef.typeRef.isArray)
                {
                    isArrayOrList = fieldTypeDef.typeRef.arrayRank == 1; // isn't this always true?
                    if (isArrayOrList)
                    {
                        // resolidify the type to match the actual element
                        // back to its original type if it's a generic one
                        fieldTypeDef = type.SolidifyType(fieldTypeDef.typeRef.arrayType);
                    }
                }
                else if (fieldTypeDef.typeDef.FullName == "System.Collections.Generic.List`1")
                {
                    isArrayOrList = true;
                    fieldTypeDef = fieldTypeDef.typeRef.genericParams[0];
                }

                List<string> attributeNames = GetAttributeNamesOnField(type.typeDef.DeclaringAssembly, fieldDef);
                TypeAttributes typeAttrs = (TypeAttributes)fieldTypeDef.typeDef.Flags;
                bool isSerializable = typeAttrs.HasFlag(TypeAttributes.Serializable);

                field.Name = fieldDef.Name;
                if (isPrimitive = fieldTypeDef.typeDef.IsEnumType)
                {
                    var enumType = fieldTypeDef.typeDef.GetEnumUnderlyingType().baseType.FullName;
                    field.Type = CommonMonoTemplateHelper.ConvertBaseToPrimitive(enumType);
                }
                else if (isPrimitive = CommonMonoTemplateHelper.IsPrimitiveType(fieldTypeDef.typeDef.FullName))
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
                else if (isManagedReference = attributeNames.Contains("UnityEngine.SerializeReference"))
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
                    field.Children = CommonMonoTemplateHelper.PPtr(_unityVersion);
                }
                else if (isManagedReference)
                {
                    field.Children = CommonMonoTemplateHelper.ManagedReference(_unityVersion);
                }
                else if (isSerializable)
                {
                    field.Children = Serialized(fieldTypeDef, availableDepth);
                }
                else
                {
                    Console.WriteLine("you wot mate");
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
                localChildren.Add(CommonMonoTemplateHelper.ManagedReferencesRegistry("references", _unityVersion));
            }

            return localChildren;
        }

        private List<Il2CppFieldDefinition> GetAcceptableFields(TypeDefWithSelfRef parentType, int availableDepth)
        {
            List<Il2CppFieldDefinition> validFields = new List<Il2CppFieldDefinition>();
            for (int i = 0; i < parentType.typeDef.FieldCount; i++)
            {
                Il2CppFieldDefinition f = parentType.typeDef.Fields[i];
                FieldAttributes attr = parentType.typeDef.FieldAttributes[i];

                List<string> attributeNames = GetAttributeNamesOnField(parentType.typeDef.DeclaringAssembly, f);

                if (attr.HasFlag(FieldAttributes.Public) ||
                    attributeNames.Contains("UnityEngine.SerializeField") ||
                    attributeNames.Contains("UnityEngine.SerializeReference")) // field is public or has exception attribute
                {
                    if (!attr.HasFlag(FieldAttributes.Static) &&
                        !attr.HasFlag(FieldAttributes.NotSerialized) &&
                        !attr.HasFlag(FieldAttributes.InitOnly) &&
                        !attr.HasFlag(FieldAttributes.Literal)) // field is not public, has exception attribute, readonly, or const
                    {
                        TypeDefWithSelfRef solidifiedFieldType = parentType.SolidifyType(f.FieldType);

                        if (TryGetListOrArrayElement(solidifiedFieldType, out Il2CppTypeReflectionData elemType))
                        {
                            // arrays are not serialized at and past the serialization limit
                            if (availableDepth < 0)
                            {
                                continue;
                            }

                            // unity can't serialize collection of collections, ignore it
                            if (TryGetListOrArrayElement(elemType, out _))
                            {
                                continue;
                            }
                            // resolidify type
                            solidifiedFieldType = parentType.SolidifyType(elemType);
                        }
                        // unity doesn't serialize a field of the same type as declaring type
                        // unless it inherits from UnityEngine.Object
                        else if (parentType.typeDef.FullName == solidifiedFieldType.typeDef.FullName && !DerivesFromUEObject(parentType))
                        {
                            continue;
                        }

                        if (IsValidDef(attributeNames, solidifiedFieldType, availableDepth))
                        {
                            validFields.Add(f);
                        }
                    }
                }
            }
            return validFields;

            bool TryGetListOrArrayElement(TypeDefWithSelfRef fieldType, out Il2CppTypeReflectionData elemType)
            {
                if (fieldType.typeRef.isArray)
                {
                    elemType = fieldType.typeRef.arrayType;
                    return true;
                }
                else if (fieldType.typeRef.isGenericType && fieldType.typeRef.baseType.FullName == "System.Collections.Generic.List`1")
                {
                    elemType = fieldType.typeRef.genericParams[0];
                    return true;
                }

                elemType = default;
                return false;
            }

            bool IsValidDef(List<string> attributeNames, TypeDefWithSelfRef typeDef, int availableDepth)
            {
                // before 2020.1.0 you couldn't have fields of a generic type, so they should be ingored
                // https://unity.com/releases/editor/whats-new/2020.1.0
                if (typeDef.typeDef.GenericContainer != null && _unityVersion.major < 2020)
                {
                    return false;
                }

                if (CommonMonoTemplateHelper.IsPrimitiveType(typeDef.typeDef.FullName) ||
                    typeDef.typeDef.FullName == "System.String")
                {
                    return true;
                }

                // unity doesn't support long enums
                if (typeDef.typeDef.IsEnumType)
                {
                    var enumType = typeDef.typeDef.GetEnumUnderlyingType().baseType.FullName;
                    return enumType != "System.Int64" && enumType != "System.UInt64";
                }


                TypeAttributes typeAttrs = (TypeAttributes)typeDef.typeDef.Flags;
                // value types are not affected by the serialization limit
                if (availableDepth < 0)
                {
                    return typeDef.typeDef.IsValueType && (typeAttrs.HasFlag(TypeAttributes.Serializable) || CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.typeDef.FullName));
                }

                if (DerivesFromUEObject(typeDef) ||
                    CommonMonoTemplateHelper.IsSpecialUnityType(typeDef.typeDef.FullName))
                {
                    return true;
                }

                if (attributeNames.Contains("UnityEngine.SerializeReference"))
                {
                    if (_unityVersion.major == 2019 && _unityVersion.minor == 3 && _unityVersion.patch < 8 && typeDef.typeDef.FullName == "System.Object")
                    {
                        return false;
                    }

                    return !typeDef.typeDef.IsValueType && typeDef.typeDef.GenericContainer == null;
                }

                if (CommonMonoTemplateHelper.IsAssemblyBlacklisted(typeDef.typeDef.DeclaringAssembly.Name, _unityVersion))
                {
                    return false;
                }

                return !typeDef.typeDef.IsAbstract && typeAttrs.HasFlag(TypeAttributes.Serializable);
            }
        }

        private bool DerivesFromUEObject(TypeDefWithSelfRef typeDef)
        {
            TypeAttributes typeAttributes = (TypeAttributes)typeDef.typeDef.Flags;

            if (typeDef.typeDef.BaseType == null)
                return false;
            if (typeAttributes.HasFlag(TypeAttributes.Interface))
                return false;
            if (typeDef.typeDef.BaseType.baseType.FullName == "UnityEngine.Object" ||
                typeDef.typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.typeDef.BaseType.baseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.typeDef.BaseType);
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
                "Gradient" => CommonMonoTemplateHelper.Gradient(_unityVersion),
                "AnimationCurve" => CommonMonoTemplateHelper.AnimationCurve(_unityVersion),
                "LayerMask" => CommonMonoTemplateHelper.BitField(),
                "Bounds" => CommonMonoTemplateHelper.AABB(),
                "BoundsInt" => CommonMonoTemplateHelper.BoundsInt(),
                "Rect" => CommonMonoTemplateHelper.Rectf(),
                "RectOffset" => CommonMonoTemplateHelper.RectOffset(),
                "Color32" => CommonMonoTemplateHelper.RGBAi(),
                "GUIStyle" => CommonMonoTemplateHelper.GUIStyle(_unityVersion),
                "Vector2Int" => CommonMonoTemplateHelper.Vector2Int(),
                "Vector3Int" => CommonMonoTemplateHelper.Vector3Int(),
                "PropertyName" => CommonMonoTemplateHelper.PropertyName(),
                _ => Serialized(type, availableDepth)
            };
        }
    }
}
