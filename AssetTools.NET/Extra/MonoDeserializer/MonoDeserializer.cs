////////////////////////////
//   ASSETSTOOLS.NET PLUGINS
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

//see https://docs.unity3d.com/Manual/class-ScriptableObject.html for some serialized data info
//also https://docs.unity3d.com/ScriptReference/SerializeField.html
namespace AssetsTools.NET.Extra
{
    public class MonoClass
    {
        public uint childrenCount;
        public AssetTypeTemplateField[] children;
        private AssemblyDefinition assembly;
        public bool Read(string typeName, string assemblyLocation)
        {
            DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyLocation));
            ReaderParameters readerParameters = new ReaderParameters();
            readerParameters.AssemblyResolver = resolver;
            assembly = AssemblyDefinition.ReadAssembly(assemblyLocation, readerParameters);

            children = new AssetTypeTemplateField[] { };
            children = RecursiveTypeLoad(assembly.MainModule, typeName, children);
            childrenCount = (uint)children.Length;
            return true;
        }
        public static AssetTypeValueField GetMonoBaseField(AssetsManager am, AssetsFileInstance inst, AssetFileInfoEx info, string managedPath)
        {
            AssetsFile file = inst.file;
            AssetTypeTemplateField baseField = new AssetTypeTemplateField();
            baseField.FromClassDatabase(am.classFile, AssetHelper.FindAssetClassByID(am.classFile, info.curFileType), 0);
            AssetTypeInstance mainAti = new AssetTypeInstance(baseField, file.reader, false, info.absoluteFilePos);
            if (file.typeTree.pTypes_Unity5[info.curFileTypeOrIndex].scriptIndex != 0xFFFF)
            {
                AssetTypeInstance scriptAti = am.GetExtAsset(inst, mainAti.GetBaseField().Get("m_Script")).instance;
                string scriptName = scriptAti.GetBaseField().Get("m_Name").GetValue().AsString();
                string assemblyName = scriptAti.GetBaseField().Get("m_AssemblyName").GetValue().AsString();
                string assemblyPath = Path.Combine(managedPath, assemblyName);
                if (File.Exists(assemblyPath))
                {
                    MonoClass mc = new MonoClass();
                    mc.Read(scriptName, assemblyPath);
                    AssetTypeTemplateField[] monoTemplateFields = mc.children;

                    AssetTypeTemplateField[] templateField = baseField.children.Concat(monoTemplateFields).ToArray();
                    baseField.children = templateField;
                    baseField.childrenCount = (uint)baseField.children.Length;

                    mainAti = new AssetTypeInstance(baseField, file.reader, false, info.absoluteFilePos);
                }
            }
            return mainAti.GetBaseField();
        }
        private AssetTypeTemplateField[] RecursiveTypeLoad(ModuleDefinition module, string typeName, AssetTypeTemplateField[] attf)
        {
            TypeDefinition type = module.GetTypes()
                .Where(t => t.Name.Equals(typeName))
                .Select(t => t)
                .First();

            return RecursiveTypeLoad(module, type, attf);
        }
        private AssetTypeTemplateField[] RecursiveTypeLoad(ModuleDefinition module, TypeDefinition type, AssetTypeTemplateField[] attf)
        {
            if (type.BaseType.Name != "Object" &&
                type.BaseType.Name != "MonoBehaviour" &&
                type.BaseType.Name != "ScriptableObject")
            {
                TypeDefinition typeDef = type.BaseType.Resolve();
                attf = RecursiveTypeLoad(typeDef.Module, typeDef, attf);
            }

            AssetTypeTemplateField[] newChildren = ReadTypes(type);
            return attf.Concat(newChildren).ToArray(); //-todo, why would you use arrays?
            //children = children.Concat(newChildren).ToArray();
        }
        private AssetTypeTemplateField[] ReadTypes(TypeDefinition type)
        {
            FieldDefinition[] acceptableFields = GetAcceptableFields(type);
            AssetTypeTemplateField[] localChildren = new AssetTypeTemplateField[acceptableFields.Length];
            for (int i = 0; i < localChildren.Length; i++)
            {
                AssetTypeTemplateField field = new AssetTypeTemplateField();
                FieldDefinition fieldDef = acceptableFields[i];
                TypeDefinition fieldType = fieldDef.FieldType.Resolve();
                if (fieldType.Name.StartsWith("List")) //should always be `1 but 'cha never know
                {
                    fieldType = ((GenericInstanceType)fieldDef.FieldType).GenericArguments[0].Resolve();
                }
                string arrayFixedName = fieldType.Name;
                if ((fieldType.Name.EndsWith("[]") && !fieldType.Name.EndsWith("[][]")) ||
                     fieldType.Name.EndsWith("`1")) {
                    arrayFixedName = arrayFixedName.Substring(0, arrayFixedName.Length - 2);
                }
                field.name = fieldDef.Name;
                field.type = ConvertBaseToPrimitive(arrayFixedName);
                if (fieldType.IsEnum)
                {
                    field.valueType = EnumValueTypes.ValueType_Int32;
                } else
                {
                    field.valueType = AssetTypeValueField.GetValueTypeByTypeName(field.type);
                }
                field.isArray = fieldDef.FieldType.IsArray;
                field.align = IsAlignable(field.valueType);
                field.hasValue = (field.valueType == EnumValueTypes.ValueType_None) ? false : true;
                if (IsAcceptablePrimitiveType(fieldType))
                {
                    field.childrenCount = 0;
                    field.children = new AssetTypeTemplateField[] { };
                } else if (fieldType.Name.Equals("String"))
                {
                    SetString(field);
                } else if (IsAcceptableUnityType(fieldType))
                {
                    SetSpecialUnity(field, fieldType);
                } else if (DerivesFromUEObject(fieldType))
                {
                    SetPPtr(field);
                } else if (fieldType.IsSerializable)
                {
                    SetSerialized(field, fieldType);
                }
                string baseFieldType = fieldDef.FieldType.Name;
                if ((baseFieldType.EndsWith("[]") && !baseFieldType.EndsWith("[][]")) //IsArray won't work here for whatever reason
                    || baseFieldType.StartsWith("List"))
                {
                    field = SetArray(field, fieldType);
                }
                localChildren[i] = field;
            }
            return localChildren;
        }
        //todo- wtf
        //nothing more can be said
        private FieldDefinition[] GetAcceptableFields(TypeDefinition typeDef)
        {
            /*foreach (FieldDefinition def in typeDef.Fields)
            {
                if (def.Name == "m_ObjectArgument")
                {
                    if ((def.Attributes.HasFlag(FieldAttributes.Public) ||
                        def.CustomAttributes.Any(a => a.AttributeType.Name.Equals("SerializeField"))))
                    {
                        Debug.WriteLine("has publiblicity");
                    }
                    if ((def.FieldType.Resolve().IsValueType ||
                        def.FieldType.Resolve().IsSerializable ||
                        DerivesFromUEObject(def.FieldType.Resolve()) ||
                        IsAcceptableUnityType(def.FieldType.Resolve())))
                    {
                        Debug.WriteLine("serializable type");
                    }
                    Debug.WriteLine(def.FieldType.Resolve().FullName);
                    if (!def.Attributes.HasFlag(FieldAttributes.Static) &&
                    !def.Attributes.HasFlag(FieldAttributes.NotSerialized) &&
                    !def.IsInitOnly &&
                    !def.HasConstant)
                    {
                        Debug.WriteLine("misc");
                    }
                }
            }*/
            return typeDef.Fields
                .Where(f =>
                    (f.Attributes.HasFlag(FieldAttributes.Public) ||
                        f.CustomAttributes.Any(a => a.AttributeType.Name.Equals("SerializeField"))) && //will not check for serialized field on things that cannot be serialized so watch out
                    (f.FieldType.Resolve().IsValueType ||
                        f.FieldType.Resolve().IsSerializable ||
                        DerivesFromUEObject(f.FieldType.Resolve()) ||
                        IsAcceptableUnityType(f.FieldType.Resolve())) &&
                    !f.Attributes.HasFlag(FieldAttributes.Static) &&
                    !f.Attributes.HasFlag(FieldAttributes.NotSerialized) &&
                    !f.IsInitOnly &&
                    !f.HasConstant) //&&
                    //!f.CustomAttributes.Any(a => a.AttributeType.Name.Equals("HideInInspector")))
                .Select(f => f)
                .ToArray();
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
            {"Int32","int"}
        };
        private string ConvertBaseToPrimitive(string name)
        {
            if (baseToPrimitive.ContainsKey(name))
            {
                return baseToPrimitive[name];
            }
            return name;
        }
        private bool IsAcceptablePrimitiveType(TypeDefinition typeDef)
        {
            string name = typeDef.Name;
            if (typeDef.IsEnum ||
                name == "Boolean" ||
                name == "Int64" ||
                name == "Int16" ||
                name == "UInt64" ||
                name == "UInt32" ||
                name == "UInt16" ||
                name == "Char" ||
                name == "Byte" ||
                name == "SByte" ||
                name == "Double" ||
                name == "Single" ||
                name == "Int32") return true;
            return false;
        }
        //Not a complete list! Todo!
        private bool IsAcceptableUnityType(TypeDefinition typeDef)
        {
            string name = typeDef.Name;
            if (name == "Color" ||
                name == "Color32" ||
                name == "Gradient" ||
                name == "Vector2" ||
                name == "Vector3" ||
                name == "Vector4" ||
                name == "LayerMask" ||
                name == "Quaternion" ||
                name == "Bounds" ||
                name == "Rect" ||
                name == "Matrix4x4" ||
                name == "AnimationCurve") return true;
            return false;
        }
        private uint GetAcceptableFieldCount(TypeDefinition typeDef)
        {
            return (uint)GetAcceptableFields(typeDef).Length;
        }
        private bool DerivesFromUEObject(TypeDefinition typeDef)
        {
            if (typeDef.BaseType.FullName == "UnityEngine.Object" ||
                typeDef.FullName == "UnityEngine.Object")
                return true;
            if (typeDef.BaseType.FullName != "System.Object")
                return DerivesFromUEObject(typeDef.BaseType.Resolve());
            return false;
        }
        private bool IsAlignable(EnumValueTypes valueType)
        {
            if (valueType.Equals(EnumValueTypes.ValueType_Bool) ||
                valueType.Equals(EnumValueTypes.ValueType_Int8) ||
                valueType.Equals(EnumValueTypes.ValueType_UInt8) ||
                valueType.Equals(EnumValueTypes.ValueType_Int16) ||
                valueType.Equals(EnumValueTypes.ValueType_UInt16))
                return true;
            return false;
        }
        private bool IsValueType(TypeDefinition typeDef)
        {
            if (typeDef.IsValueType || typeDef.Name.Equals("String"))
                return true;
            if (typeDef.IsEnum)
                return true;
            return false;
        }
        private AssetTypeTemplateField SetArray(AssetTypeTemplateField field, TypeDefinition type)
        {
            AssetTypeTemplateField size = new AssetTypeTemplateField();
            size.name = "size";
            size.type = "int";
            size.valueType = EnumValueTypes.ValueType_Int32;
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
            array.valueType = EnumValueTypes.ValueType_None;
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
            size.valueType = EnumValueTypes.ValueType_Int32;
            size.isArray = false;
            size.align = false;
            size.hasValue = true;
            size.childrenCount = 0;
            size.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField data = new AssetTypeTemplateField();
            data.name = "data";
            data.type = "char";
            data.valueType = EnumValueTypes.ValueType_UInt8;
            data.isArray = false;
            data.align = false;
            data.hasValue = true;
            data.childrenCount = 0;
            data.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField array = new AssetTypeTemplateField();
            array.name = "Array";
            array.type = "Array";
            array.valueType = EnumValueTypes.ValueType_None;
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
        private void SetSerialized(AssetTypeTemplateField field, TypeDefinition type)
        {
            AssetTypeTemplateField[] types = new AssetTypeTemplateField[] { };
            types = RecursiveTypeLoad(assembly.MainModule, type, types);
            //AssetTypeTemplateField[] types = ReadTypes(type);
            field.childrenCount = (uint)types.Length;
            field.children = types;
        }
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
                default:
                    SetSerialized(field, type);
                    break;
            }
        }
        //Todo, we may need some new deserializer file like classdatabase
        //Currently, you can generate them from the typetree in bundles generated from the editor
        private void SetGradient(AssetTypeTemplateField field)
        {
            field.childrenCount = 27;
            AssetTypeTemplateField key0 = CreateTemplateField("key0", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key1 = CreateTemplateField("key1", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key2 = CreateTemplateField("key2", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key3 = CreateTemplateField("key3", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key4 = CreateTemplateField("key4", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key5 = CreateTemplateField("key5", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key6 = CreateTemplateField("key6", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField key7 = CreateTemplateField("key7", "ColorRGBA", EnumValueTypes.ValueType_None, 4, RGBAf());
            AssetTypeTemplateField ctime0 = CreateTemplateField("ctime0", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime1 = CreateTemplateField("ctime1", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime2 = CreateTemplateField("ctime2", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime3 = CreateTemplateField("ctime3", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime4 = CreateTemplateField("ctime4", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime5 = CreateTemplateField("ctime5", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime6 = CreateTemplateField("ctime6", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField ctime7 = CreateTemplateField("ctime7", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime0 = CreateTemplateField("atime0", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime1 = CreateTemplateField("atime1", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime2 = CreateTemplateField("atime2", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime3 = CreateTemplateField("atime3", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime4 = CreateTemplateField("atime4", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime5 = CreateTemplateField("atime5", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime6 = CreateTemplateField("atime6", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField atime7 = CreateTemplateField("atime7", "UInt16", EnumValueTypes.ValueType_UInt16);
            AssetTypeTemplateField m_Mode = CreateTemplateField("m_Mode", "int", EnumValueTypes.ValueType_Int32);
            AssetTypeTemplateField m_NumColorKeys = CreateTemplateField("m_NumColorKeys", "UInt8", EnumValueTypes.ValueType_UInt8);
            AssetTypeTemplateField m_NumAlphaKeys = CreateTemplateField("m_NumAlphaKeys", "UInt8", EnumValueTypes.ValueType_UInt8, false, true);
            field.children = new AssetTypeTemplateField[] {
                key0, key1, key2, key3, key4, key5, key6, key7, ctime0, ctime1, ctime2, ctime3, ctime4, ctime5, ctime6, ctime7, atime0, atime1, atime2, atime3, atime4, atime5, atime6, atime7, m_Mode, m_NumColorKeys, m_NumAlphaKeys
            };
        }
        private AssetTypeTemplateField[] RGBAf()
        {
            AssetTypeTemplateField r = CreateTemplateField("r", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField g = CreateTemplateField("g", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField b = CreateTemplateField("b", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField a = CreateTemplateField("a", "float", EnumValueTypes.ValueType_Float);
            return new AssetTypeTemplateField[] { r, g, b, a };
        }
        private void SetAnimationCurve(AssetTypeTemplateField field)
        {
            field.childrenCount = 4;
            AssetTypeTemplateField time = CreateTemplateField("time", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField value = CreateTemplateField("value", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField inSlope = CreateTemplateField("inSlope", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField outSlope = CreateTemplateField("outSlope", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField size = CreateTemplateField("size", "int", EnumValueTypes.ValueType_Int32);
            AssetTypeTemplateField data = CreateTemplateField("data", "Keyframe", EnumValueTypes.ValueType_None, 4, new AssetTypeTemplateField[] {
                time, value, inSlope, outSlope
            });
            AssetTypeTemplateField Array = CreateTemplateField("Array", "Array", EnumValueTypes.ValueType_Array, true, false, 2, new AssetTypeTemplateField[] {
                size, data
            });
            AssetTypeTemplateField m_Curve = CreateTemplateField("m_Curve", "vector", EnumValueTypes.ValueType_None, 1, new AssetTypeTemplateField[] {
                Array
            });
            AssetTypeTemplateField m_PreInfinity = CreateTemplateField("m_PreInfinity", "int", EnumValueTypes.ValueType_Int32);
            AssetTypeTemplateField m_PostInfinity = CreateTemplateField("m_PostInfinity", "int", EnumValueTypes.ValueType_Int32);
            AssetTypeTemplateField m_RotationOrder = CreateTemplateField("m_RotationOrder", "int", EnumValueTypes.ValueType_Int32);
            field.children = new AssetTypeTemplateField[] {
                m_Curve, m_PreInfinity, m_PostInfinity, m_RotationOrder
            };
        }
        private void SetBitField(AssetTypeTemplateField field)
        {
            field.childrenCount = 1;
            AssetTypeTemplateField m_Bits = CreateTemplateField("m_Bits", "unsigned int", EnumValueTypes.ValueType_UInt32);
            field.children = new AssetTypeTemplateField[] {
                m_Bits
            };
        }
        private void SetAABB(AssetTypeTemplateField field)
        {
            field.childrenCount = 2;
            AssetTypeTemplateField m_Center = CreateTemplateField("m_Center", "Vector3f", EnumValueTypes.ValueType_None, 3, VECf());
            AssetTypeTemplateField m_Extent = CreateTemplateField("m_Extent", "Vector3f", EnumValueTypes.ValueType_None, 3, VECf());
            field.children = new AssetTypeTemplateField[] {
                m_Center, m_Extent
            };
        }
        private AssetTypeTemplateField[] VECf()
        {
            AssetTypeTemplateField x = CreateTemplateField("x", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField z = CreateTemplateField("z", "float", EnumValueTypes.ValueType_Float);
            return new AssetTypeTemplateField[] { x, y, z };
        }
        private void SetRectf(AssetTypeTemplateField field)
        {
            field.childrenCount = 4;
            AssetTypeTemplateField x = CreateTemplateField("x", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField y = CreateTemplateField("y", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField width = CreateTemplateField("width", "float", EnumValueTypes.ValueType_Float);
            AssetTypeTemplateField height = CreateTemplateField("height", "float", EnumValueTypes.ValueType_Float);
            field.children = new AssetTypeTemplateField[] {
                x, y, width, height
            };
        }
        private void SetPPtr(AssetTypeTemplateField field)
        {
            field.type = $"PPtr<{field.type}>";
            field.childrenCount = 2;

            AssetTypeTemplateField fileID = new AssetTypeTemplateField();
            fileID.name = "m_FileID";
            fileID.type = "int";
            fileID.valueType = EnumValueTypes.ValueType_Int32;
            fileID.isArray = false;
            fileID.align = false;
            fileID.hasValue = true;
            fileID.childrenCount = 0;
            fileID.children = new AssetTypeTemplateField[] { };

            AssetTypeTemplateField pathID = new AssetTypeTemplateField();
            pathID.name = "m_PathID";
            pathID.type = "SInt64";
            pathID.valueType = EnumValueTypes.ValueType_Int64;
            pathID.isArray = false;
            pathID.align = false;
            pathID.hasValue = true;
            pathID.childrenCount = 0;
            pathID.children = new AssetTypeTemplateField[] { };

            field.children = new AssetTypeTemplateField[] {
                fileID, pathID
            };
        }

        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType)
        {
            return CreateTemplateField(name, type, valueType, false, false, 0, null);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, bool isArray, bool align)
        {
            return CreateTemplateField(name, type, valueType, isArray, align, 0, null);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, uint childrenCount, AssetTypeTemplateField[] children)
        {
            return CreateTemplateField(name, type, valueType, false, false, childrenCount, children);
        }
        private AssetTypeTemplateField CreateTemplateField(string name, string type, EnumValueTypes valueType, bool isArray, bool align, uint childrenCount, AssetTypeTemplateField[] children)
        {
            AssetTypeTemplateField field = new AssetTypeTemplateField();
            field.name = name;
            field.type = type;
            field.valueType = valueType;
            field.isArray = isArray;
            field.align = align;
            field.hasValue = (valueType == EnumValueTypes.ValueType_None) ? false : true;
            field.childrenCount = childrenCount;
            field.children = children;
            
            return field;
        }
    }

    //https://stackoverflow.com/a/4108907
    public static class EnumExt
    {
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool HasFlag(this Enum variable, Enum value)
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
    }
}
