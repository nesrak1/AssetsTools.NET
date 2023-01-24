using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public class MonoCecilTempGenerator : IMonoBehaviourTemplateGenerator
    {
        private UnityVersion unityVersion;
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
            baseField.Children.AddRange(newFields);
            return baseField;
        }

        public List<AssetTypeTemplateField> Read(string assemblyPath, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            AssemblyDefinition asmDef = GetAssemblyWithDependencies(assemblyPath);
            return Read(asmDef, nameSpace, typeName, unityVersion);
        }

        public List<AssetTypeTemplateField> Read(AssemblyDefinition assembly, string nameSpace, string typeName, UnityVersion unityVersion)
        {
            this.unityVersion = unityVersion;
            List<AssetTypeTemplateField> children = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(assembly.MainModule, nameSpace, typeName, children);
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

        private void RecursiveTypeLoad(ModuleDefinition module, string nameSpace, string typeName, List<AssetTypeTemplateField> attf)
        {
            // TypeReference needed for TypeForwardedTo in UnityEngine (and others)
            TypeDefinition type = new TypeReference(nameSpace, typeName, module, module).Resolve();
            RecursiveTypeLoad(type, attf);
        }

        private void RecursiveTypeLoad(TypeDefWithSelfRef type, List<AssetTypeTemplateField> attf)
        {
            string baseName = type.typeDef.BaseType.FullName;
            if (baseName != "System.Object" &&
                baseName != "UnityEngine.Object" &&
                baseName != "UnityEngine.MonoBehaviour" &&
                baseName != "UnityEngine.ScriptableObject")
            {
                TypeDefWithSelfRef typeDef = type.typeDef.BaseType;
                typeDef.AssignTypeParams(type);
                RecursiveTypeLoad(typeDef, attf);
            }

            attf.AddRange(ReadTypes(type));
        }

        private List<AssetTypeTemplateField> ReadTypes(TypeDefWithSelfRef type)
        {
            List<FieldDefinition> acceptableFields = GetAcceptableFields(type);
            List<AssetTypeTemplateField> localChildren = new List<AssetTypeTemplateField>();
            for (int i = 0; i < acceptableFields.Count; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                FieldDefinition fieldDef = acceptableFields[i];
                TypeDefWithSelfRef fieldTypeDef = type.SolidifyType(fieldDef.FieldType);

                bool isArrayOrList = false;

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
                if (baseToPrimitive.ContainsKey(fieldTypeDef.typeDef.FullName))
                {
                    field.Type = baseToPrimitive[fieldTypeDef.typeDef.FullName];
                }
                else
                {
                    field.Type = fieldTypeDef.typeDef.Name;
                }

                if (IsPrimitiveType(fieldTypeDef))
                {
                    field.Children = new List<AssetTypeTemplateField>();
                }
                else if (fieldTypeDef.typeDef.Name.Equals("String"))
                {
                    SetString(field);
                }
                else if (IsSpecialUnityType(fieldTypeDef))
                {
                    SetSpecialUnity(field, fieldTypeDef);
                }
                else if (DerivesFromUEObject(fieldTypeDef))
                {
                    SetPPtr(field, true);
                }
                else if (fieldTypeDef.typeDef.IsSerializable)
                {
                    SetSerialized(field, fieldTypeDef);
                }

                if (fieldTypeDef.typeDef.IsEnum)
                {
                    field.ValueType = AssetValueType.Int32;
                }
                else
                {
                    field.ValueType = AssetTypeValueField.GetValueTypeByTypeName(field.Type);
                }
                field.IsAligned = TypeAligns(field.ValueType);
                field.HasValue = field.ValueType != AssetValueType.None;

                if (isArrayOrList)
                {
                    field = SetArray(field);
                }
                localChildren.Add(field);
            }
            return localChildren;
        }

        private List<FieldDefinition> GetAcceptableFields(TypeDefWithSelfRef typeDef)
        {
            List<FieldDefinition> validFields = new List<FieldDefinition>();
            foreach (FieldDefinition f in typeDef.typeDef.Fields)
            {
                if (Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.Public) ||
                    f.CustomAttributes.Any(a => a.AttributeType.Name == "SerializeField")) //field is public or has exception attribute
                {
                    if (!Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.Static) &&
                        !Net35Polyfill.HasFlag(f.Attributes, FieldAttributes.NotSerialized) &&
                        !f.IsInitOnly &&
                        !f.HasConstant) //field is not public, has exception attribute, readonly, or const
                    {
                        TypeDefWithSelfRef ft = typeDef.SolidifyType(f.FieldType);

                        TypeDefinition ftd = ft.typeDef;

                        if (f.FieldType is GenericInstanceType gft)
                        {
                            //Unity can't serialize list of collections, ignoring it
                            if (gft.ElementType.FullName == "System.Collections.Generic.List`1")
                            {
                                TypeDefWithSelfRef elem = ft.typeParamToArg["T"];
                                if (elem.typeRef.IsArray || elem.typeDef.FullName == "System.Collections.Generic.List`1" || !IsValidDef(elem.typeDef))
                                {
                                    continue;
                                }
                            }

                            //Before 2020.1.0 you couldn't have fields of a generic type, so they should be ingored
                            //https://unity.com/releases/editor/whats-new/2020.1.0
                            else if (unityVersion.major < 2020)
                            {
                                continue;
                            }
                        }
                        //Unity can't serialize array of collections, ignoring it
                        else if (f.FieldType is ArrayType aft)
                        {
                            TypeDefWithSelfRef elem = aft.ElementType;
                            if (aft.ElementType.IsArray || elem.typeDef.FullName == "System.Collections.Generic.List`1" || !IsValidDef(elem.typeDef))
                            {
                                continue;
                            }
                        }

                        if (ftd != null && IsValidDef(ftd))
                        {
                            validFields.Add(f);
                        }
                    }
                }
            }
            return validFields;
            
            bool IsValidDef(TypeDefinition def)
            {
                //object has IsSerializable=true, which means it passes other check while it shouldn't
                if (def.FullName == "System.Object")
                {
                    return false;
                }

                return def.IsPrimitive ||
                       def.IsEnum ||
                       def.IsSerializable ||
                       DerivesFromUEObject(def) ||
                       IsSpecialUnityType(def); //field has a serializable type
            }
        }

        private Dictionary<string, string> baseToPrimitive = new Dictionary<string, string>()
        {
            {"System.Boolean","bool"},
            {"System.Int64","SInt64"},
            {"System.Int16","SInt16"},
            {"System.UInt64","SInt64"},
            {"System.UInt32","unsigned int"},
            {"System.UInt16","UInt16"},
            {"System.Char","char"},
            {"System.Byte","UInt8"},
            {"System.SByte","SInt8"},
            {"System.Double","double"},
            {"System.Single","float"},
            {"System.Int32","int"},
            {"System.String","string"}
        };

        private string ConvertBaseToPrimitive(string name)
        {
            if (baseToPrimitive.ContainsKey(name))
            {
                return baseToPrimitive[name];
            }
            return name;
        }

        private bool IsPrimitiveType(TypeDefWithSelfRef typeDef)
        {
            string name = typeDef.typeDef.FullName;
            if (typeDef.typeDef.IsEnum ||
                name == "System.Boolean" ||
                name == "System.Int64" ||
                name == "System.Int16" ||
                name == "System.UInt64" ||
                name == "System.UInt32" ||
                name == "System.UInt16" ||
                name == "System.Char" ||
                name == "System.Byte" ||
                name == "System.SByte" ||
                name == "System.Double" ||
                name == "System.Single" ||
                name == "System.Int32") return true;
            return false;
        }

        private bool IsSpecialUnityType(TypeDefWithSelfRef typeDef)
        {
            string name = typeDef.typeDef.FullName;
            if (name == "UnityEngine.Color" ||
                name == "UnityEngine.Color32" ||
                name == "UnityEngine.Gradient" ||
                name == "UnityEngine.Vector2" ||
                name == "UnityEngine.Vector3" ||
                name == "UnityEngine.Vector4" ||
                name == "UnityEngine.LayerMask" ||
                name == "UnityEngine.Quaternion" ||
                name == "UnityEngine.Bounds" ||
                name == "UnityEngine.Rect" ||
                name == "UnityEngine.Matrix4x4" ||
                name == "UnityEngine.AnimationCurve" ||
                name == "UnityEngine.GUIStyle" ||
                name == "UnityEngine.Vector2Int" ||
                name == "UnityEngine.Vector3Int" ||
                name == "UnityEngine.BoundsInt") return true;
            return false;
        }

        private bool DerivesFromUEObject(TypeDefWithSelfRef typeDef)
        {
            if (typeDef.typeDef.IsInterface)
                return false;
            if (typeDef.typeDef.BaseType.FullName == "UnityEngine.Object" ||
                typeDef.typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.typeDef.BaseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.typeDef.BaseType.Resolve());
            return false;
        }

        private bool TypeAligns(AssetValueType valueType)
        {
            if (valueType.Equals(AssetValueType.Bool) ||
                valueType.Equals(AssetValueType.Int8) ||
                valueType.Equals(AssetValueType.UInt8) ||
                valueType.Equals(AssetValueType.Int16) ||
                valueType.Equals(AssetValueType.UInt16))
                return true;
            return false;
        }


        private AssetTypeTemplateField SetArray(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField size = new AssetTypeTemplateField();
            size.Name = "size";
            size.Type = "int";
            size.ValueType = AssetValueType.Int32;
            size.IsArray = false;
            size.IsAligned = false;
            size.HasValue = true;
            size.Children = new List<AssetTypeTemplateField>(0);

            AssetTypeTemplateField data = new AssetTypeTemplateField();
            data.Name = string.Copy(field.Name);
            data.Type = string.Copy(field.Type);
            data.ValueType = field.ValueType;
            data.IsArray = false;
            data.IsAligned = false;//IsAlignable(field.valueType);
            data.HasValue = field.HasValue;
            data.Children = field.Children;

            AssetTypeTemplateField array = new AssetTypeTemplateField();
            array.Name = string.Copy(field.Name);
            array.Type = "Array";
            array.ValueType = field.ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array;
            array.IsArray = true;
            array.IsAligned = true;
            array.HasValue = true;
            array.Children = new List<AssetTypeTemplateField> {
                size, data
            };

            return array;
        }

        private void SetString(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField size = new AssetTypeTemplateField();
            size.Name = "size";
            size.Type = "int";
            size.ValueType = AssetValueType.Int32;
            size.IsArray = false;
            size.IsAligned = false;
            size.HasValue = true;
            size.Children = new List<AssetTypeTemplateField>(0);

            AssetTypeTemplateField data = new AssetTypeTemplateField();
            data.Name = "data";
            data.Type = "char";
            data.ValueType = AssetValueType.UInt8;
            data.IsArray = false;
            data.IsAligned = false;
            data.HasValue = true;
            data.Children = new List<AssetTypeTemplateField>(0);

            AssetTypeTemplateField array = new AssetTypeTemplateField();
            array.Name = "Array";
            array.Type = "Array";
            array.ValueType = AssetValueType.ByteArray;
            array.IsArray = true;
            array.IsAligned = true;
            array.HasValue = true;
            array.Children = new List<AssetTypeTemplateField> {
                size, data
            };

            field.Children = new List<AssetTypeTemplateField> {
                array
            };
        }

        private void SetPPtr(AssetTypeTemplateField field, bool dollar)
        {
            if (dollar)
                field.Type = $"PPtr<${field.Type}>";
            else
                field.Type = $"PPtr<{field.Type}>";

            AssetTypeTemplateField fileID = new AssetTypeTemplateField();
            fileID.Name = "m_FileID";
            fileID.Type = "int";
            fileID.ValueType = AssetValueType.Int32;
            fileID.IsArray = false;
            fileID.IsAligned = false;
            fileID.HasValue = true;
            fileID.Children = new List<AssetTypeTemplateField>(0);

            AssetTypeTemplateField pathID = new AssetTypeTemplateField();
            pathID.Name = "m_PathID";
            if (unityVersion.major >= 5)
            {
                pathID.Type = "SInt64";
                pathID.ValueType = AssetValueType.Int64;
            }
            else
            {
                pathID.Type = "int";
                pathID.ValueType = AssetValueType.Int32;
            }
            pathID.IsArray = false;
            pathID.IsAligned = false;
            pathID.HasValue = true;
            pathID.Children = new List<AssetTypeTemplateField>(0);

            field.Children = new List<AssetTypeTemplateField> {
                fileID, pathID
            };
        }

        private void SetSerialized(AssetTypeTemplateField field, TypeDefWithSelfRef type)
        {
            List<AssetTypeTemplateField> types = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(type, types);
            field.Children = types;
        }

        #region special unity serialization
        private void SetSpecialUnity(AssetTypeTemplateField field, TypeDefWithSelfRef type)
        {
            switch (type.typeDef.Name)
            {
                case "Gradient":
                    SetGradient(field);
                    break;
                case "AnimationCurve":
                    SetAnimationCurve(field);
                    break;
                case "LayerMask":
                    SetBitField(field);
                    break;
                case "Bounds":
                    SetAABB(field);
                    break;
                case "Rect":
                    SetRectf(field);
                    break;
                case "Color32":
                    SetGradientRGBAb(field);
                    break;
                case "GUIStyle":
                    SetGUIStyle(field);
                    break;
                case "BoundsInt":
                    SetAABBInt(field);
                    break;
                case "Vector2Int":
                    SetVec2Int(field);
                    break;
                case "Vector3Int":
                    SetVec3Int(field);
                    break;
                default:
                    SetSerialized(field, type);
                    break;
            }
        }

        private void SetGradient(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField key0 = CreateTemplateField("key0", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key1 = CreateTemplateField("key1", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key2 = CreateTemplateField("key2", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key3 = CreateTemplateField("key3", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key4 = CreateTemplateField("key4", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key5 = CreateTemplateField("key5", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key6 = CreateTemplateField("key6", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField key7 = CreateTemplateField("key7", "ColorRGBA", AssetValueType.None, RGBAf());
            AssetTypeTemplateField ctime0 = CreateTemplateField("ctime0", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime1 = CreateTemplateField("ctime1", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime2 = CreateTemplateField("ctime2", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime3 = CreateTemplateField("ctime3", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime4 = CreateTemplateField("ctime4", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime5 = CreateTemplateField("ctime5", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime6 = CreateTemplateField("ctime6", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField ctime7 = CreateTemplateField("ctime7", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime0 = CreateTemplateField("atime0", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime1 = CreateTemplateField("atime1", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime2 = CreateTemplateField("atime2", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime3 = CreateTemplateField("atime3", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime4 = CreateTemplateField("atime4", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime5 = CreateTemplateField("atime5", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime6 = CreateTemplateField("atime6", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField atime7 = CreateTemplateField("atime7", "UInt16", AssetValueType.UInt16);
            AssetTypeTemplateField m_Mode = CreateTemplateField("m_Mode", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_NumColorKeys = CreateTemplateField("m_NumColorKeys", "UInt8", AssetValueType.UInt8);
            AssetTypeTemplateField m_NumAlphaKeys = CreateTemplateField("m_NumAlphaKeys", "UInt8", AssetValueType.UInt8, false, true);
            field.Children = new List<AssetTypeTemplateField> {
                key0, key1, key2, key3, key4, key5, key6, key7, ctime0, ctime1, ctime2, ctime3, ctime4, ctime5, ctime6, ctime7, atime0, atime1, atime2, atime3, atime4, atime5, atime6, atime7, m_Mode, m_NumColorKeys, m_NumAlphaKeys
            };
        }

        private List<AssetTypeTemplateField> RGBAf()
        {
            AssetTypeTemplateField r = CreateTemplateField("r", "float", AssetValueType.Float);
            AssetTypeTemplateField g = CreateTemplateField("g", "float", AssetValueType.Float);
            AssetTypeTemplateField b = CreateTemplateField("b", "float", AssetValueType.Float);
            AssetTypeTemplateField a = CreateTemplateField("a", "float", AssetValueType.Float);
            return new List<AssetTypeTemplateField> { r, g, b, a };
        }

        private void SetAnimationCurve(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField time = CreateTemplateField("time", "float", AssetValueType.Float);
            AssetTypeTemplateField value = CreateTemplateField("value", "float", AssetValueType.Float);
            AssetTypeTemplateField inSlope = CreateTemplateField("inSlope", "float", AssetValueType.Float);
            AssetTypeTemplateField outSlope = CreateTemplateField("outSlope", "float", AssetValueType.Float);
            //new in 2019
            AssetTypeTemplateField weightedMode = CreateTemplateField("weightedMode", "int", AssetValueType.Int32);
            AssetTypeTemplateField inWeight = CreateTemplateField("inWeight", "float", AssetValueType.Float);
            AssetTypeTemplateField outWeight = CreateTemplateField("outWeight", "float", AssetValueType.Float);
            /////////////
            AssetTypeTemplateField size = CreateTemplateField("size", "int", AssetValueType.Int32);
            AssetTypeTemplateField data;
            if (unityVersion.major >= 2018)
            {
                data = CreateTemplateField("data", "Keyframe", AssetValueType.None, new List<AssetTypeTemplateField> {
                    time, value, inSlope, outSlope, weightedMode, inWeight, outWeight
                });
            }
            else
            {
                data = CreateTemplateField("data", "Keyframe", AssetValueType.None, new List<AssetTypeTemplateField> {
                    time, value, inSlope, outSlope
                });
            }
            AssetTypeTemplateField Array = CreateTemplateField("Array", "Array", AssetValueType.Array, true, false, new List<AssetTypeTemplateField> {
                size, data
            });
            AssetTypeTemplateField m_Curve = CreateTemplateField("m_Curve", "vector", AssetValueType.None, new List<AssetTypeTemplateField> {
                Array
            });
            AssetTypeTemplateField m_PreInfinity = CreateTemplateField("m_PreInfinity", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_PostInfinity = CreateTemplateField("m_PostInfinity", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_RotationOrder = CreateTemplateField("m_RotationOrder", "int", AssetValueType.Int32);
            field.Children = new List<AssetTypeTemplateField> {
                m_Curve, m_PreInfinity, m_PostInfinity, m_RotationOrder
            };
        }

        private void SetBitField(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_Bits = CreateTemplateField("m_Bits", "unsigned int", AssetValueType.UInt32);
            field.Children = new List<AssetTypeTemplateField> {
                m_Bits
            };
        }

        private void SetAABB(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_Center = CreateTemplateField("m_Center", "Vector3f", AssetValueType.None, Vec3f());
            AssetTypeTemplateField m_Extent = CreateTemplateField("m_Extent", "Vector3f", AssetValueType.None, Vec3f());
            field.Children = new List<AssetTypeTemplateField> {
                m_Center, m_Extent
            };
        }

        private List<AssetTypeTemplateField> Vec3f()
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", AssetValueType.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", AssetValueType.Float);
            AssetTypeTemplateField z = CreateTemplateField("z", "float", AssetValueType.Float);
            return new List<AssetTypeTemplateField> { x, y, z };
        }

        private void SetRectf(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", AssetValueType.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", AssetValueType.Float);
            AssetTypeTemplateField width = CreateTemplateField("width", "float", AssetValueType.Float);
            AssetTypeTemplateField height = CreateTemplateField("height", "float", AssetValueType.Float);
            field.Children = new List<AssetTypeTemplateField> {
                x, y, width, height
            };
        }

        private void SetGradientRGBAb(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField rgba = CreateTemplateField("rgba", "unsigned int", AssetValueType.UInt32);
            field.Children = new List<AssetTypeTemplateField> {
                rgba
            };
        }

        //only supports 2019 right now
        private void SetGUIStyle(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_Name = CreateTemplateField("m_Name", "string", AssetValueType.String, String());
            AssetTypeTemplateField m_Normal = CreateTemplateField("m_Normal", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_Hover = CreateTemplateField("m_Hover", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_Active = CreateTemplateField("m_Active", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_Focused = CreateTemplateField("m_Focused", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_OnNormal = CreateTemplateField("m_OnNormal", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_OnHover = CreateTemplateField("m_OnHover", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_OnActive = CreateTemplateField("m_OnActive", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_OnFocused = CreateTemplateField("m_OnFocused", "GUIStyleState", AssetValueType.None, GUIStyleState());
            AssetTypeTemplateField m_Border = CreateTemplateField("m_Border", "RectOffset", AssetValueType.None, RectOffset());
            AssetTypeTemplateField m_Margin = CreateTemplateField("m_Margin", "RectOffset", AssetValueType.None, RectOffset());
            AssetTypeTemplateField m_Padding = CreateTemplateField("m_Padding", "RectOffset", AssetValueType.None, RectOffset());
            AssetTypeTemplateField m_Overflow = CreateTemplateField("m_Overflow", "RectOffset", AssetValueType.None, RectOffset());
            AssetTypeTemplateField m_Font = CreateTemplateField("m_Font", "PPtr<Font>", AssetValueType.None, PPtr());
            AssetTypeTemplateField m_FontSize = CreateTemplateField("m_FontSize", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_FontStyle = CreateTemplateField("m_FontStyle", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Alignment = CreateTemplateField("m_Alignment", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_WordWrap = CreateTemplateField("m_WordWrap", "bool", AssetValueType.Bool);
            AssetTypeTemplateField m_RichText = CreateTemplateField("m_RichText", "bool", AssetValueType.Bool, false, true);
            AssetTypeTemplateField m_TextClipping = CreateTemplateField("m_TextClipping", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_ImagePosition = CreateTemplateField("m_ImagePosition", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_ContentOffset = CreateTemplateField("m_ContentOffset", "Vector2f", AssetValueType.None, Vec2f());
            AssetTypeTemplateField m_FixedWidth = CreateTemplateField("m_FixedWidth", "float", AssetValueType.Float);
            AssetTypeTemplateField m_FixedHeight = CreateTemplateField("m_FixedHeight", "float", AssetValueType.Float);
            AssetTypeTemplateField m_StretchWidth = CreateTemplateField("m_StretchWidth", "bool", AssetValueType.Bool);
            AssetTypeTemplateField m_StretchHeight = CreateTemplateField("m_StretchHeight", "bool", AssetValueType.Bool, false, true);
            field.Children = new List<AssetTypeTemplateField> {
                m_Name, m_Normal, m_Hover, m_Active, m_Focused, m_OnNormal, m_OnHover, m_OnActive, m_OnFocused, m_Border, m_Margin, m_Padding, m_Overflow, m_Font, m_FontSize, m_FontStyle, m_Alignment, m_WordWrap, m_RichText, m_TextClipping, m_ImagePosition, m_ContentOffset, m_FixedWidth, m_FixedHeight, m_StretchWidth, m_StretchHeight
            };
        }

        private void SetAABBInt(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_Center = CreateTemplateField("m_Center", "Vector3Int", AssetValueType.None, Vec3Int());
            AssetTypeTemplateField m_Extent = CreateTemplateField("m_Extent", "Vector3Int", AssetValueType.None, Vec3Int());
            field.Children = new List<AssetTypeTemplateField> {
                m_Center, m_Extent
            };
        }

        private List<AssetTypeTemplateField> Vec3Int()
        {
            AssetTypeTemplateField m_X = CreateTemplateField("m_X", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Y = CreateTemplateField("m_Y", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Z = CreateTemplateField("m_Z", "int", AssetValueType.Int32);
            return new List<AssetTypeTemplateField> { m_X, m_Y, m_Z };
        }

        private void SetVec2Int(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_X = CreateTemplateField("m_X", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Y = CreateTemplateField("m_Y", "int", AssetValueType.Int32);
            field.Children = new List<AssetTypeTemplateField> {
                m_X, m_Y
            };
        }

        private void SetVec3Int(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField m_X = CreateTemplateField("m_X", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Y = CreateTemplateField("m_Y", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Z = CreateTemplateField("m_Z", "int", AssetValueType.Int32);
            field.Children = new List<AssetTypeTemplateField> {
                m_X, m_Y, m_Z
            };
        }

        private List<AssetTypeTemplateField> String()
        {
            AssetTypeTemplateField size = CreateTemplateField("size", "int", AssetValueType.Int32);
            AssetTypeTemplateField data = CreateTemplateField("char", "data", AssetValueType.UInt8);
            AssetTypeTemplateField Array = CreateTemplateField("Array", "Array", AssetValueType.Array, true, true, new List<AssetTypeTemplateField> {
                size, data
            });
            return new List<AssetTypeTemplateField> { Array };
        }

        private List<AssetTypeTemplateField> GUIStyleState()
        {
            AssetTypeTemplateField m_Background = CreateTemplateField("m_Background", "PPtr<Texture2D>", AssetValueType.None, PPtr());
            AssetTypeTemplateField m_TextColor = CreateTemplateField("m_TextColor", "ColorRGBA", AssetValueType.None, RGBAf());
            return new List<AssetTypeTemplateField> { m_Background, m_TextColor };
        }

        private List<AssetTypeTemplateField> RectOffset()
        {
            AssetTypeTemplateField m_Left = CreateTemplateField("m_Left", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Right = CreateTemplateField("m_Right", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Top = CreateTemplateField("m_Top", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_Bottom = CreateTemplateField("m_Bottom", "int", AssetValueType.Int32);
            return new List<AssetTypeTemplateField> { m_Left, m_Right, m_Top, m_Bottom };
        }

        private List<AssetTypeTemplateField> PPtr()
        {
            AssetTypeTemplateField m_FileID = CreateTemplateField("m_FileID", "int", AssetValueType.Int32);
            AssetTypeTemplateField m_PathID = CreateTemplateField("m_PathID", "SInt64", AssetValueType.Int64);
            return new List<AssetTypeTemplateField> { m_FileID, m_PathID };
        }

        private List<AssetTypeTemplateField> Vec2f()
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", AssetValueType.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", AssetValueType.Float);
            return new List<AssetTypeTemplateField> { x, y };
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType)
        {
            return CreateTemplateField(name, type, valueType, false, false, new List<AssetTypeTemplateField>(0));
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, bool isArray, bool align)
        {
            return CreateTemplateField(name, type, valueType, isArray, align, new List<AssetTypeTemplateField>(0));
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, List<AssetTypeTemplateField> children)
        {
            return CreateTemplateField(name, type, valueType, false, false, children);
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, bool isArray, bool align, List<AssetTypeTemplateField> children)
        {
            AssetTypeTemplateField field = new AssetTypeTemplateField();
            field.Name = name;
            field.Type = type;
            field.ValueType = valueType;
            field.IsArray = isArray;
            field.IsAligned = align;
            field.HasValue = valueType != AssetValueType.None;
            field.Children = children;

            return field;
        }
        #endregion
    }
}