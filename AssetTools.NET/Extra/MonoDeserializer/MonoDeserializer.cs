////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Extra
{
    public class MonoDeserializer
    {
        public uint format;
        public int childrenCount;
        public List<AssetTypeTemplateField> children;
        private static Dictionary<string, AssemblyDefinition> loadedAssemblies = new Dictionary<string, AssemblyDefinition>();
        public void Read(string typeName, AssemblyDefinition assembly, uint format)
        {
            this.format = format;
            children = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(assembly.MainModule, typeName, children);
            childrenCount = children.Count;
        }
        public void Read(string typeName, string assemblyPath, uint format)
        {
            AssemblyDefinition asmDef = GetAssemblyWithDependencies(assemblyPath);
            Read(typeName, asmDef, format);
        }
        public static AssemblyDefinition GetAssemblyWithDependencies(string path)
        {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(path));
            ReaderParameters readerParameters = new ReaderParameters()
            {
                AssemblyResolver = resolver
            };
            return AssemblyDefinition.ReadAssembly(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), readerParameters);
        }
        public static AssetTypeValueField GetMonoBaseField(AssetsManager am, AssetsFileInstance inst, AssetFileInfoEx info, string managedPath, bool cached = true)
        {
            AssetsFile file = inst.file;
            AssetTypeTemplateField baseField = new AssetTypeTemplateField();
            baseField.FromClassDatabase(am.classFile, AssetHelper.FindAssetClassByID(am.classFile, info.curFileType), 0);
            AssetTypeInstance mainAti = new AssetTypeInstance(baseField, file.reader, info.absoluteFilePos);
            ushort scriptIndex = AssetHelper.GetScriptIndex(file, info);
            if (scriptIndex != 0xFFFF)
            {
                AssetTypeInstance scriptAti = am.GetExtAsset(inst, mainAti.GetBaseField().Get("m_Script")).instance;
                string scriptName = scriptAti.GetBaseField().Get("m_ClassName").GetValue().AsString();
                string scriptNamespace = scriptAti.GetBaseField().Get("m_Namespace").GetValue().AsString();
                string assemblyName = scriptAti.GetBaseField().Get("m_AssemblyName").GetValue().AsString();
                string assemblyPath = Path.Combine(managedPath, assemblyName);

                if (scriptNamespace != string.Empty)
                    scriptName = scriptNamespace + "." + scriptName;

                if (File.Exists(assemblyPath))
                {
                    AssemblyDefinition asmDef;
                    if (cached)
                    {
                        if (!loadedAssemblies.ContainsKey(assemblyName))
                        {
                            loadedAssemblies.Add(assemblyName, GetAssemblyWithDependencies(assemblyPath));
                        }
                        asmDef = loadedAssemblies[assemblyName];
                    }
                    else
                    {
                        asmDef = GetAssemblyWithDependencies(assemblyPath);
                    }

                    MonoDeserializer mc = new MonoDeserializer();
                    mc.Read(scriptName, asmDef, inst.file.header.format);
                    List<AssetTypeTemplateField> monoTemplateFields = mc.children;

                    AssetTypeTemplateField[] templateField = baseField.children.Concat(monoTemplateFields).ToArray();
                    baseField.children = templateField;
                    baseField.childrenCount = baseField.children.Length;

                    mainAti = new AssetTypeInstance(baseField, file.reader, info.absoluteFilePos);
                }
            }
            return mainAti.GetBaseField();
        }
        private void RecursiveTypeLoad(ModuleDefinition module, string typeName, List<AssetTypeTemplateField> attf)
        {
            TypeDefinition type = module.GetTypes().First(t => t.FullName.Equals(typeName));
            RecursiveTypeLoad(type, attf);
        }
        private void RecursiveTypeLoad(TypeDefinition type, List<AssetTypeTemplateField> attf)
        {
            string baseName = type.BaseType.FullName;
            if (baseName != "System.Object" &&
                baseName != "UnityEngine.Object" &&
                baseName != "UnityEngine.MonoBehaviour" &&
                baseName != "UnityEngine.ScriptableObject")
            {
                TypeDefinition typeDef = type.BaseType.Resolve();
                RecursiveTypeLoad(typeDef, attf);
            }

            attf.AddRange(ReadTypes(type));
        }
        private List<AssetTypeTemplateField> ReadTypes(TypeDefinition type)
        {
            List<FieldDefinition> acceptableFields = GetAcceptableFields(type);
            List<AssetTypeTemplateField> localChildren = new List<AssetTypeTemplateField>();
            for (int i = 0; i < acceptableFields.Count; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                FieldDefinition fieldDef = acceptableFields[i];
                TypeReference fieldTypeRef = fieldDef.FieldType;
                TypeDefinition fieldType = fieldTypeRef.Resolve();
                string fieldTypeName = fieldType.Name;
                bool isArrayOrList = false;

                if (fieldTypeRef.MetadataType == MetadataType.Array)
                {
                    ArrayType arrType = (ArrayType)fieldTypeRef;
                    isArrayOrList = arrType.IsVector;
                }
                else if (fieldType.FullName == "System.Collections.Generic.List`1")
                {
                    fieldType = ((GenericInstanceType)fieldDef.FieldType).GenericArguments[0].Resolve();
                    fieldTypeName = fieldType.Name;
                    isArrayOrList = true;
                }

                field.name = fieldDef.Name;
                field.type = ConvertBaseToPrimitive(fieldTypeName);
                if (IsPrimitiveType(fieldType))
                {
                    field.childrenCount = 0;
                    field.children = new AssetTypeTemplateField[] { };
                }
                else if (fieldType.Name.Equals("String"))
                {
                    SetString(field);
                }
                else if (IsSpecialUnityType(fieldType))
                {
                    SetSpecialUnity(field, fieldType);
                }
                else if (DerivesFromUEObject(fieldType))
                {
                    SetPPtr(field, false);
                }
                else if (fieldType.IsSerializable)
                {
                    SetSerialized(field, fieldType);
                }

                if (fieldType.IsEnum)
                {
                    field.valueType = EnumValueTypes.Int32;
                }
                else
                {
                    field.valueType = AssetTypeValueField.GetValueTypeByTypeName(field.type);
                }
                field.align = TypeAligns(field.valueType);
                field.hasValue = field.valueType != EnumValueTypes.None;

                if (isArrayOrList)
                {
                    field = SetArray(field);
                }
                localChildren.Add(field);
            }
            return localChildren;
        }
        private List<FieldDefinition> GetAcceptableFields(TypeDefinition typeDef)
        {
            List<FieldDefinition> validFields = new List<FieldDefinition>();
            foreach (FieldDefinition f in typeDef.Fields)
            {
                if (HasFlag(f.Attributes, FieldAttributes.Public) ||
                    f.CustomAttributes.Any(a => a.AttributeType.Name.Equals("SerializeField"))) //field is public or has exception attribute
                {
                    if (!HasFlag(f.Attributes, FieldAttributes.Static) &&
                        !HasFlag(f.Attributes, FieldAttributes.NotSerialized) &&
                        !f.IsInitOnly &&
                        !f.HasConstant) //field is not public, has exception attribute, readonly, or const
                    {
                        TypeReference ft = f.FieldType;
                        if (f.FieldType.IsArray)
                        {
                            ft = ft.GetElementType();
                        }
                        TypeDefinition ftd = ft.Resolve();
                        if (ftd != null)
                        {
                            if (ftd.IsPrimitive ||
                                ftd.IsEnum ||
                                ftd.IsSerializable ||
                                DerivesFromUEObject(ftd) ||
                                IsSpecialUnityType(ftd)) //field has a serializable type
                            {
                                validFields.Add(f);
                            }
                        }
                    }
                }
            }
            return validFields;
        }
        private Dictionary<string, string> baseToPrimitive = new Dictionary<string, string>()
        {
            {"Boolean","bool"},
            {"Int64","long"},
            {"Int16","short"},
            {"UInt64","ulong"},
            {"UInt32","uint"},
            {"UInt16","ushort"},
            {"Char","char"},
            {"Byte","byte"},
            {"SByte","sbyte"},
            {"Double","double"},
            {"Single","float"},
            {"Int32","int"},
            {"String","string"}
        };
        private string ConvertBaseToPrimitive(string name)
        {
            if (baseToPrimitive.ContainsKey(name))
            {
                return baseToPrimitive[name];
            }
            return name;
        }
        private bool IsPrimitiveType(TypeDefinition typeDef)
        {
            string name = typeDef.FullName;
            if (typeDef.IsEnum ||
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
        private bool IsSpecialUnityType(TypeDefinition typeDef)
        {
            string name = typeDef.FullName;
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
                name == "UnityEngine.GUIStyle") return true;
            return false;
        }
        private bool DerivesFromUEObject(TypeDefinition typeDef)
        {
            if (typeDef.IsInterface)
                return false;
            if (typeDef.BaseType.FullName == "UnityEngine.Object" ||
                typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.BaseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.BaseType.Resolve());
            return false;
        }
        private bool TypeAligns(EnumValueTypes valueType)
        {
            if (valueType.Equals(EnumValueTypes.Bool) ||
                valueType.Equals(EnumValueTypes.Int8) ||
                valueType.Equals(EnumValueTypes.UInt8) ||
                valueType.Equals(EnumValueTypes.Int16) ||
                valueType.Equals(EnumValueTypes.UInt16))
                return true;
            return false;
        }
        private AssetTypeTemplateField SetArray(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField size = new AssetTypeTemplateField();
            size.name = "size";
            size.type = "int";
            size.valueType = EnumValueTypes.Int32;
            size.isArray = false;
            size.align = false;
            size.hasValue = true;
            size.childrenCount = 0;
            size.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField data = new AssetTypeTemplateField();
            data.name = string.Copy(field.name);
            data.type = string.Copy(field.type);
            data.valueType = field.valueType;
            data.isArray = false;
            data.align = false;//IsAlignable(field.valueType);
            data.hasValue = field.hasValue;
            data.childrenCount = field.childrenCount;
            data.children = field.children;

            AssetTypeTemplateField array = new AssetTypeTemplateField();
            array.name = string.Copy(field.name);
            array.type = "Array";
            array.valueType = EnumValueTypes.None;
            array.isArray = true;
            array.align = true;
            array.hasValue = false;
            array.childrenCount = 2;
            array.children = new AssetTypeTemplateField[] {
                size, data
            };

            return array;
        }
        private void SetString(AssetTypeTemplateField field)
        {
            field.childrenCount = 1;

            AssetTypeTemplateField size = new AssetTypeTemplateField();
            size.name = "size";
            size.type = "int";
            size.valueType = EnumValueTypes.Int32;
            size.isArray = false;
            size.align = false;
            size.hasValue = true;
            size.childrenCount = 0;
            size.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField data = new AssetTypeTemplateField();
            data.name = "data";
            data.type = "char";
            data.valueType = EnumValueTypes.UInt8;
            data.isArray = false;
            data.align = false;
            data.hasValue = true;
            data.childrenCount = 0;
            data.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField array = new AssetTypeTemplateField();
            array.name = "Array";
            array.type = "Array";
            array.valueType = EnumValueTypes.None;
            array.isArray = true;
            array.align = true;
            array.hasValue = false;
            array.childrenCount = 2;
            array.children = new AssetTypeTemplateField[] {
                size, data
            };

            field.children = new AssetTypeTemplateField[] {
                array
            };
        }
        private void SetPPtr(AssetTypeTemplateField field, bool dollar)
        {
            if (dollar)
                field.type = $"PPtr<${field.type}>";
            else
                field.type = $"PPtr<{field.type}>";
            field.childrenCount = 2;

            AssetTypeTemplateField fileID = new AssetTypeTemplateField();
            fileID.name = "m_FileID";
            fileID.type = "int";
            fileID.valueType = EnumValueTypes.Int32;
            fileID.isArray = false;
            fileID.align = false;
            fileID.hasValue = true;
            fileID.childrenCount = 0;
            fileID.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField pathID = new AssetTypeTemplateField();
            pathID.name = "m_PathID";
            if (format < 0x0E)
            {
                pathID.type = "int";
                pathID.valueType = EnumValueTypes.Int32;
            }
            else
            {
                pathID.type = "SInt64";
                pathID.valueType = EnumValueTypes.Int64;
            }
            pathID.isArray = false;
            pathID.align = false;
            pathID.hasValue = true;
            pathID.childrenCount = 0;
            pathID.children = new AssetTypeTemplateField[] { };
            field.children = new AssetTypeTemplateField[] {
                fileID, pathID
            };
        }
        private void SetSerialized(AssetTypeTemplateField field, TypeDefinition type)
        {
            List<AssetTypeTemplateField> types = new List<AssetTypeTemplateField>();
            RecursiveTypeLoad(type, types);
            field.childrenCount = types.Count;
            field.children = types.ToArray();
        }
        #region special unity serialization
        private void SetSpecialUnity(AssetTypeTemplateField field, TypeDefinition type)
        {
            switch (type.Name)
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
                default:
                    SetSerialized(field, type);
                    break;
            }
        }
        private void SetGradient(AssetTypeTemplateField field)
        {
            field.childrenCount = 27;
            AssetTypeTemplateField key0 = CreateTemplateField("key0", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key1 = CreateTemplateField("key1", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key2 = CreateTemplateField("key2", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key3 = CreateTemplateField("key3", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key4 = CreateTemplateField("key4", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key5 = CreateTemplateField("key5", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key6 = CreateTemplateField("key6", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField key7 = CreateTemplateField("key7", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            AssetTypeTemplateField ctime0 = CreateTemplateField("ctime0", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime1 = CreateTemplateField("ctime1", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime2 = CreateTemplateField("ctime2", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime3 = CreateTemplateField("ctime3", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime4 = CreateTemplateField("ctime4", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime5 = CreateTemplateField("ctime5", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime6 = CreateTemplateField("ctime6", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField ctime7 = CreateTemplateField("ctime7", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime0 = CreateTemplateField("atime0", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime1 = CreateTemplateField("atime1", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime2 = CreateTemplateField("atime2", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime3 = CreateTemplateField("atime3", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime4 = CreateTemplateField("atime4", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime5 = CreateTemplateField("atime5", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime6 = CreateTemplateField("atime6", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField atime7 = CreateTemplateField("atime7", "UInt16", EnumValueTypes.UInt16);
            AssetTypeTemplateField m_Mode = CreateTemplateField("m_Mode", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_NumColorKeys = CreateTemplateField("m_NumColorKeys", "UInt8", EnumValueTypes.UInt8);
            AssetTypeTemplateField m_NumAlphaKeys = CreateTemplateField("m_NumAlphaKeys", "UInt8", EnumValueTypes.UInt8, false, true);
            field.children = new AssetTypeTemplateField[] {
                key0, key1, key2, key3, key4, key5, key6, key7, ctime0, ctime1, ctime2, ctime3, ctime4, ctime5, ctime6, ctime7, atime0, atime1, atime2, atime3, atime4, atime5, atime6, atime7, m_Mode, m_NumColorKeys, m_NumAlphaKeys
            };
        }
        private AssetTypeTemplateField[] RGBAf()
        {
            AssetTypeTemplateField r = CreateTemplateField("r", "float", EnumValueTypes.Float);
            AssetTypeTemplateField g = CreateTemplateField("g", "float", EnumValueTypes.Float);
            AssetTypeTemplateField b = CreateTemplateField("b", "float", EnumValueTypes.Float);
            AssetTypeTemplateField a = CreateTemplateField("a", "float", EnumValueTypes.Float);
            return new AssetTypeTemplateField[] { r, g, b, a };
        }
        private void SetAnimationCurve(AssetTypeTemplateField field)
        {
            field.childrenCount = 4;
            AssetTypeTemplateField time = CreateTemplateField("time", "float", EnumValueTypes.Float);
            AssetTypeTemplateField value = CreateTemplateField("value", "float", EnumValueTypes.Float);
            AssetTypeTemplateField inSlope = CreateTemplateField("inSlope", "float", EnumValueTypes.Float);
            AssetTypeTemplateField outSlope = CreateTemplateField("outSlope", "float", EnumValueTypes.Float);
            //new in 2019
            AssetTypeTemplateField weightedMode = CreateTemplateField("weightedMode", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField inWeight = CreateTemplateField("inWeight", "float", EnumValueTypes.Float);
            AssetTypeTemplateField outWeight = CreateTemplateField("outWeight", "float", EnumValueTypes.Float);
            /////////////
            AssetTypeTemplateField size = CreateTemplateField("size", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField data;
            if (format >= 0x13)
            {
                data = CreateTemplateField("data", "Keyframe", EnumValueTypes.None, 7, new AssetTypeTemplateField[] {
                    time, value, inSlope, outSlope, weightedMode, inWeight, outWeight
                });
            }
            else
            {
                data = CreateTemplateField("data", "Keyframe", EnumValueTypes.None, 4, new AssetTypeTemplateField[] {
                    time, value, inSlope, outSlope
                });
            }
            AssetTypeTemplateField Array = CreateTemplateField("Array", "Array", EnumValueTypes.Array, true, false, 2, new AssetTypeTemplateField[] {
                size, data
            });
            AssetTypeTemplateField m_Curve = CreateTemplateField("m_Curve", "vector", EnumValueTypes.None, 1, new AssetTypeTemplateField[] {
                Array
            });
            AssetTypeTemplateField m_PreInfinity = CreateTemplateField("m_PreInfinity", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_PostInfinity = CreateTemplateField("m_PostInfinity", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_RotationOrder = CreateTemplateField("m_RotationOrder", "int", EnumValueTypes.Int32);
            field.children = new AssetTypeTemplateField[] {
                m_Curve, m_PreInfinity, m_PostInfinity, m_RotationOrder
            };
        }
        private void SetBitField(AssetTypeTemplateField field)
        {
            field.childrenCount = 1;
            AssetTypeTemplateField m_Bits = CreateTemplateField("m_Bits", "unsigned int", EnumValueTypes.UInt32);
            field.children = new AssetTypeTemplateField[] {
                m_Bits
            };
        }
        private void SetAABB(AssetTypeTemplateField field)
        {
            field.childrenCount = 2;
            AssetTypeTemplateField m_Center = CreateTemplateField("m_Center", "Vector3f", EnumValueTypes.None, 3, Vec3f());
            AssetTypeTemplateField m_Extent = CreateTemplateField("m_Extent", "Vector3f", EnumValueTypes.None, 3, Vec3f());
            field.children = new AssetTypeTemplateField[] {
                m_Center, m_Extent
            };
        }
        private AssetTypeTemplateField[] Vec3f()
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", EnumValueTypes.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", EnumValueTypes.Float);
            AssetTypeTemplateField z = CreateTemplateField("z", "float", EnumValueTypes.Float);
            return new AssetTypeTemplateField[] { x, y, z };
        }
        private void SetRectf(AssetTypeTemplateField field)
        {
            field.childrenCount = 4;
            AssetTypeTemplateField x = CreateTemplateField("x", "float", EnumValueTypes.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", EnumValueTypes.Float);
            AssetTypeTemplateField width = CreateTemplateField("width", "float", EnumValueTypes.Float);
            AssetTypeTemplateField height = CreateTemplateField("height", "float", EnumValueTypes.Float);
            field.children = new AssetTypeTemplateField[] {
                x, y, width, height
            };
        }
        private void SetGradientRGBAb(AssetTypeTemplateField field)
        {
            field.childrenCount = 1;
            AssetTypeTemplateField rgba = CreateTemplateField("rgba", "unsigned int", EnumValueTypes.UInt32);
            field.children = new AssetTypeTemplateField[] {
                rgba
            };
        }
        //only supports 2019 right now
        private void SetGUIStyle(AssetTypeTemplateField field)
        {
            field.childrenCount = 26;
            AssetTypeTemplateField m_Name = CreateTemplateField("m_Name", "string", EnumValueTypes.String, 1, String());
            AssetTypeTemplateField m_Normal = CreateTemplateField("m_Normal", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_Hover = CreateTemplateField("m_Hover", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_Active = CreateTemplateField("m_Active", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_Focused = CreateTemplateField("m_Focused", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_OnNormal = CreateTemplateField("m_OnNormal", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_OnHover = CreateTemplateField("m_OnHover", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_OnActive = CreateTemplateField("m_OnActive", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_OnFocused = CreateTemplateField("m_OnFocused", "GUIStyleState", EnumValueTypes.None, 2, GUIStyleState());
            AssetTypeTemplateField m_Border = CreateTemplateField("m_Border", "RectOffset", EnumValueTypes.None, 4, RectOffset());
            AssetTypeTemplateField m_Margin = CreateTemplateField("m_Margin", "RectOffset", EnumValueTypes.None, 4, RectOffset());
            AssetTypeTemplateField m_Padding = CreateTemplateField("m_Padding", "RectOffset", EnumValueTypes.None, 4, RectOffset());
            AssetTypeTemplateField m_Overflow = CreateTemplateField("m_Overflow", "RectOffset", EnumValueTypes.None, 4, RectOffset());
            AssetTypeTemplateField m_Font = CreateTemplateField("m_Font", "PPtr<Font>", EnumValueTypes.None, 2, PPtr());
            AssetTypeTemplateField m_FontSize = CreateTemplateField("m_FontSize", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_FontStyle = CreateTemplateField("m_FontStyle", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_Alignment = CreateTemplateField("m_Alignment", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_WordWrap = CreateTemplateField("m_WordWrap", "bool", EnumValueTypes.Bool);
            AssetTypeTemplateField m_RichText = CreateTemplateField("m_RichText", "bool", EnumValueTypes.Bool, false, true);
            AssetTypeTemplateField m_TextClipping = CreateTemplateField("m_TextClipping", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_ImagePosition = CreateTemplateField("m_ImagePosition", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_ContentOffset = CreateTemplateField("m_ContentOffset", "Vector2f", EnumValueTypes.None, 2, Vec2f());
            AssetTypeTemplateField m_FixedWidth = CreateTemplateField("m_FixedWidth", "float", EnumValueTypes.Float);
            AssetTypeTemplateField m_FixedHeight = CreateTemplateField("m_FixedHeight", "float", EnumValueTypes.Float);
            AssetTypeTemplateField m_StretchWidth = CreateTemplateField("m_StretchWidth", "bool", EnumValueTypes.Bool);
            AssetTypeTemplateField m_StretchHeight = CreateTemplateField("m_StretchHeight", "bool", EnumValueTypes.Bool, false, true);
            field.children = new AssetTypeTemplateField[] {
                m_Name, m_Normal, m_Hover, m_Active, m_Focused, m_OnNormal, m_OnHover, m_OnActive, m_OnFocused, m_Border, m_Margin, m_Padding, m_Overflow, m_Font, m_FontSize, m_FontStyle, m_Alignment, m_WordWrap, m_RichText, m_TextClipping, m_ImagePosition, m_ContentOffset, m_FixedWidth, m_FixedHeight, m_StretchWidth, m_StretchHeight
            };
        }
        private AssetTypeTemplateField[] String()
        {
            AssetTypeTemplateField size = CreateTemplateField("size", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField data = CreateTemplateField("char", "data", EnumValueTypes.UInt8);
            AssetTypeTemplateField Array = CreateTemplateField("Array", "Array", EnumValueTypes.Array, true, true, 2, new AssetTypeTemplateField[] {
                size, data
            });
            return new AssetTypeTemplateField[] { Array };
        }
        private AssetTypeTemplateField[] GUIStyleState()
        {
            AssetTypeTemplateField m_Background = CreateTemplateField("m_Background", "PPtr<Texture2D>", EnumValueTypes.None, 2, PPtr());
            AssetTypeTemplateField m_TextColor = CreateTemplateField("m_TextColor", "ColorRGBA", EnumValueTypes.None, 4, RGBAf());
            return new AssetTypeTemplateField[] { m_Background, m_TextColor };
        }
        private AssetTypeTemplateField[] RectOffset()
        {
            AssetTypeTemplateField m_Left = CreateTemplateField("m_Left", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_Right = CreateTemplateField("m_Right", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_Top = CreateTemplateField("m_Top", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_Bottom = CreateTemplateField("m_Bottom", "int", EnumValueTypes.Int32);
            return new AssetTypeTemplateField[] { m_Left, m_Right, m_Top, m_Bottom };
        }
        private AssetTypeTemplateField[] PPtr()
        {
            AssetTypeTemplateField m_FileID = CreateTemplateField("m_FileID", "int", EnumValueTypes.Int32);
            AssetTypeTemplateField m_PathID = CreateTemplateField("m_PathID", "SInt64", EnumValueTypes.Int64);
            return new AssetTypeTemplateField[] { m_FileID, m_PathID };
        }
        private AssetTypeTemplateField[] Vec2f()
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", EnumValueTypes.Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", EnumValueTypes.Float);
            return new AssetTypeTemplateField[] { x, y };
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType)
        {
            return CreateTemplateField(name, type, valueType, false, false, 0, null);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, bool isArray, bool align)
        {
            return CreateTemplateField(name, type, valueType, isArray, align, 0, null);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, int childrenCount, AssetTypeTemplateField[] children)
        {
            return CreateTemplateField(name, type, valueType, false, false, childrenCount, children);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, bool isArray, bool align, int childrenCount, AssetTypeTemplateField[] children)
        {
            AssetTypeTemplateField field = new AssetTypeTemplateField();
            field.name = name;
            field.type = type;
            field.valueType = valueType;
            field.isArray = isArray;
            field.align = align;
            field.hasValue = valueType != EnumValueTypes.None;
            field.childrenCount = childrenCount;
            field.children = children;
            
            return field;
        }
        #endregion
        #region .net polyfill
        //https://stackoverflow.com/a/4108907
        private static bool HasFlag(Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
        #endregion
    }
}
