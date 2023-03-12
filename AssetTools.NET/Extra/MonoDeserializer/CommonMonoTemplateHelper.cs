using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public static class CommonMonoTemplateHelper
    {
        #region Collections

        //Unity definitly excludes some assemblies from being serialized
        //but I couldn't find the definitive list, plus it changes from version-to-version
        //Those should be enough for most cases
        //but need more research when it will cause problems for some games
        private static readonly string[] blacklistedAssemblies = new[]
        {
            "mscorlib",
            "mscorlib.dll",
            "netstandard",
            "netstandard.dll",
            "System.Core",
            "System.Core.dll",
            "System",
            "System.dll",

            //Got these when testing assembly directly from Library\ScriptAssemblies
            //Discovered from dotnet by cecil because it couldn't find system libs in the same folder as assembly
            //Should be safe to also exclude them
            "System.Private.CoreLib",
            "System.Private.CoreLib.dll",
            "System.Collections",
            "System.Collections.dll",
            "System.Collections.NonGeneric",
            "System.Collections.NonGeneric.dll"
        };

        private static readonly string[] specialUnityTypes = new[]
        {
            "UnityEngine.Color",
            "UnityEngine.Color32",
            "UnityEngine.Gradient",
            "UnityEngine.Vector2",
            "UnityEngine.Vector3",
            "UnityEngine.Vector4",
            "UnityEngine.LayerMask",
            "UnityEngine.Quaternion",
            "UnityEngine.Bounds",
            "UnityEngine.Rect",
            "UnityEngine.RectOffset",
            "UnityEngine.Matrix4x4",
            "UnityEngine.AnimationCurve",
            "UnityEngine.GUIStyle",
            "UnityEngine.Vector2Int",
            "UnityEngine.Vector3Int",
            "UnityEngine.BoundsInt"
        };

        private static readonly string[] primitiveTypes = new[]
        {
            "System.Boolean",
            "System.SByte",
            "System.Byte",
            "System.Char",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Int64",
            "System.UInt64",
            "System.Double",
            "System.Single"
        };

        private static readonly Dictionary<string, string> baseToPrimitive = new Dictionary<string, string>()
        {
            ["System.Boolean"] = "UInt8",
            ["System.SByte"] = "SInt8",
            ["System.Byte"] = "UInt8",
            ["System.Char"] = "UInt16",
            ["System.Int16"] = "SInt16",
            ["System.UInt16"] = "UInt16",
            ["System.Int32"] = "int",
            ["System.UInt32"] = "unsigned int",
            ["System.Int64"] = "SInt64",
            ["System.UInt64"] = "UInt64",
            ["System.Double"] = "double",
            ["System.Single"] = "float",
            ["System.String"] = "string"
        };

        private static readonly Dictionary<string, AssetValueType> baseToAssetValueType = new Dictionary<string, AssetValueType>()
        {
            ["System.Boolean"] = AssetValueType.Bool,
            ["System.SByte"] = AssetValueType.Int8,
            ["System.Byte"] = AssetValueType.UInt8,
            ["System.Char"] = AssetValueType.UInt16,
            ["System.Int16"] = AssetValueType.Int16,
            ["System.UInt16"] = AssetValueType.UInt16,
            ["System.Int32"] = AssetValueType.Int32,
            ["System.UInt32"] = AssetValueType.UInt32,
            ["System.Int64"] = AssetValueType.Int64,
            ["System.UInt64"] = AssetValueType.UInt64,
            ["System.Double"] = AssetValueType.Double,
            ["System.Single"] = AssetValueType.Float,
            ["System.String"] = AssetValueType.String
        };

        #endregion

        #region Checks


        public static string ConvertBaseToPrimitive(string name)
        {
            if (baseToPrimitive.TryGetValue(name, out string primitiveName))
            {
                return primitiveName;
            }
            return name;
        }

        public static AssetValueType ConvertBaseToAssetValueType(string name)
        {
            if (baseToAssetValueType.TryGetValue(name, out AssetValueType value))
            {
                return value;
            }
            return AssetValueType.None;
        }

        public static bool IsSpecialUnityType(string fullName)
        {
            return specialUnityTypes.Contains(fullName);
        }

        public static bool IsAssemblyBlacklisted(string assembly, UnityVersion unityVersion)
        {
            return blacklistedAssemblies.Contains(assembly);
        }

        public static bool IsPrimitiveType(string fullName)
        {
            return primitiveTypes.Contains(fullName);
        }

        public static bool TypeAligns(AssetValueType valueType)
        {
            if (valueType.Equals(AssetValueType.Bool) ||
                valueType.Equals(AssetValueType.Int8) ||
                valueType.Equals(AssetValueType.UInt8) ||
                valueType.Equals(AssetValueType.Int16) ||
                valueType.Equals(AssetValueType.UInt16))
                return true;
            return false;
        }

        public static int GetSerializationLimit(UnityVersion unityVersion)
        {
            if (unityVersion.major > 2020 ||
                (unityVersion.major == 2020 && (unityVersion.minor >= 2 || (unityVersion.minor == 1 && unityVersion.patch >= 4))) ||
                (unityVersion.major == 2019 && unityVersion.minor == 4 && unityVersion.patch >= 9))
            {
                return 10;
            }

            return 7;
        }

        #endregion

        #region Basic types

        public static AssetTypeTemplateField Bool(string name, bool align = false) => CreateTemplateField(name, "bool", AssetValueType.Bool, false, align);
        public static AssetTypeTemplateField SByte(string name, bool align = false) => CreateTemplateField(name, "SInt8", AssetValueType.Int8, false, align);
        public static AssetTypeTemplateField Byte(string name, bool align = false) => CreateTemplateField(name, "UInt8", AssetValueType.UInt8, false, align);
        public static AssetTypeTemplateField CChar(string name, bool align = false) => CreateTemplateField(name, "char", AssetValueType.UInt8, false, align);
        public static AssetTypeTemplateField Char(string name, bool align = false) => CreateTemplateField(name, "UInt16", AssetValueType.UInt16, false, align);
        public static AssetTypeTemplateField Short(string name, bool align = false) => CreateTemplateField(name, "SInt16", AssetValueType.Int16, false, align);
        public static AssetTypeTemplateField UShort(string name, bool align = false) => CreateTemplateField(name, "UInt16", AssetValueType.UInt16, false, align);
        public static AssetTypeTemplateField Int(string name) => CreateTemplateField(name, "int", AssetValueType.Int32);
        public static AssetTypeTemplateField UInt(string name) => CreateTemplateField(name, "unsigned int", AssetValueType.UInt32);
        public static AssetTypeTemplateField Long(string name) => CreateTemplateField(name, "SInt64", AssetValueType.Int64);
        public static AssetTypeTemplateField ULong(string name) => CreateTemplateField(name, "UInt64", AssetValueType.UInt64);
        public static AssetTypeTemplateField Float(string name) => CreateTemplateField(name, "float", AssetValueType.Float);
        public static AssetTypeTemplateField Double(string name) => CreateTemplateField(name, "double", AssetValueType.Double);
        
        public static AssetTypeTemplateField String(string name)
        {
            return CreateTemplateField(name, "string", AssetValueType.String, String());
        }

        public static List<AssetTypeTemplateField> String()
        {
            return Array(CChar("data"));
        }

        public static AssetTypeTemplateField Vector(AssetTypeTemplateField field)
        {
            return CreateTemplateField(field.Name, "vector", Array(field));
        }

        public static AssetTypeTemplateField VectorWithType(AssetTypeTemplateField field)
        {
            return CreateTemplateField(field.Name, field.Type, Array(field));
        }

        public static List<AssetTypeTemplateField> Array(AssetTypeTemplateField field)
        {
            AssetTypeTemplateField array = new AssetTypeTemplateField
            {
                Name = "Array",
                Type = "Array",
                ValueType = field.ValueType == AssetValueType.UInt8 ? AssetValueType.ByteArray : AssetValueType.Array,
                IsArray = true,
                IsAligned = true,
                HasValue = true,
                Children = new List<AssetTypeTemplateField>
                {
                    Int("size"), CreateTemplateField("data", field.Type, field.ValueType, field.Children)
                }
            };

            return new List<AssetTypeTemplateField> { array };
        }

        #endregion

        #region ManagedReferencesRegistry

        public static AssetTypeTemplateField ManagedReference(string name, UnityVersion unityVersion) => CreateTemplateField(name, "managedReference", ManagedReference(unityVersion));
        public static List<AssetTypeTemplateField> ManagedReference(UnityVersion unityVersion)
        {
            if (unityVersion.major > 2021 || (unityVersion.major == 2021 && unityVersion.minor >= 2))
            {
                return new List<AssetTypeTemplateField> { Long("rid") };
            }

            return new List<AssetTypeTemplateField> { Int("id") };
        }

        public static AssetTypeTemplateField ManagedReferencesRegistry(string name, UnityVersion unityVersion) =>
            CreateTemplateField(name, "ManagedReferencesRegistry", AssetValueType.ManagedReferencesRegistry, ManagedReferencesRegistry(unityVersion));
        public static List<AssetTypeTemplateField> ManagedReferencesRegistry(UnityVersion unityVersion)
        {
            if (unityVersion.major > 2021 || (unityVersion.major == 2021 && unityVersion.minor >= 2))
            {
                return new List<AssetTypeTemplateField> { Int("version"), Vector(ReferencedObject("RefIds", unityVersion)) };
            }

            return new List<AssetTypeTemplateField> { Int("version"), ReferencedObject("00000000", unityVersion) };
        }

        public static AssetTypeTemplateField ReferencedObject(string name, UnityVersion unityVersion) => CreateTemplateField(name, "ReferencedObject", ReferencedObject(unityVersion));
        public static List<AssetTypeTemplateField> ReferencedObject(UnityVersion unityVersion)
        {
            if (unityVersion.major > 2021 || (unityVersion.major == 2021 && unityVersion.minor >= 2))
            {
                return new List<AssetTypeTemplateField> { Long("rid"), ReferencedManagedType("type"), CreateTemplateField("data", "ReferencedObjectData", AssetValueType.None) };
            }

            return new List<AssetTypeTemplateField> { ReferencedManagedType("type"), CreateTemplateField("data", "ReferencedObjectData", AssetValueType.None) };
        }

        public static AssetTypeTemplateField ReferencedManagedType(string name) => CreateTemplateField(name, "ReferencedManagedType", ReferencedManagedType());
        public static List<AssetTypeTemplateField> ReferencedManagedType()
        {
            return new List<AssetTypeTemplateField> { String("class"), String("ns"), String("asm") };
        }

        #endregion

        #region Special Unity types

        public static AssetTypeTemplateField Gradient(string name, UnityVersion unityVersion) => CreateTemplateField(name, "Gradient", Gradient(unityVersion));
        public static List<AssetTypeTemplateField> Gradient(UnityVersion unityVersion)
        {
            if (unityVersion.major > 2022 || (unityVersion.major == 2022 && unityVersion.minor >= 2))
            {
                return new List<AssetTypeTemplateField> {
                    RGBAf("key0"), RGBAf("key1"), RGBAf("key2"), RGBAf("key3"), RGBAf("key4"), RGBAf("key5"), RGBAf("key6"), RGBAf("key7"),
                    UShort("ctime0"), UShort("ctime1"), UShort("ctime2"), UShort("ctime3"), UShort("ctime4"), UShort("ctime5"), UShort("ctime6"), UShort("ctime7"),
                    UShort("atime0"), UShort("atime1"), UShort("atime2"), UShort("atime3"), UShort("atime4"), UShort("atime5"), UShort("atime6"), UShort("atime7"),
                    Byte("m_Mode"), SByte("m_ColorSpace"), Byte("m_NumColorKeys"), Byte("m_NumAlphaKeys", true)
                };
            }

            return new List<AssetTypeTemplateField> {
                RGBAf("key0"), RGBAf("key1"), RGBAf("key2"), RGBAf("key3"), RGBAf("key4"), RGBAf("key5"), RGBAf("key6"), RGBAf("key7"),
                UShort("ctime0"), UShort("ctime1"), UShort("ctime2"), UShort("ctime3"), UShort("ctime4"), UShort("ctime5"), UShort("ctime6"), UShort("ctime7"),
                UShort("atime0"), UShort("atime1"), UShort("atime2"), UShort("atime3"), UShort("atime4"), UShort("atime5"), UShort("atime6"), UShort("atime7"),
                Int("m_Mode"), Byte("m_NumColorKeys"), Byte("m_NumAlphaKeys", true)
            };
        }

        public static AssetTypeTemplateField AnimationCurve(string name, UnityVersion unityVersion) => CreateTemplateField(name, "AnimationCurve", AnimationCurve(unityVersion));
        public static List<AssetTypeTemplateField> AnimationCurve(UnityVersion unityVersion)
        {
            return new List<AssetTypeTemplateField> {
                Vector(Keyframe("m_Curve", unityVersion)), Int("m_PreInfinity"), Int("m_PostInfinity"), Int("m_RotationOrder")
            };
        }

        //only supports 2019 right now
        public static AssetTypeTemplateField GUIStyle(string name, UnityVersion unityVersion) => CreateTemplateField(name, "GUIStyle", GUIStyle(unityVersion));
        public static List<AssetTypeTemplateField> GUIStyle(UnityVersion unityVersion)
        {
            return new List<AssetTypeTemplateField> {
                String("m_Name"),
                GUIStyleState("m_Normal", unityVersion), GUIStyleState("m_Hover", unityVersion), GUIStyleState("m_Active", unityVersion), GUIStyleState("m_Focused", unityVersion),
                GUIStyleState("m_OnNormal", unityVersion), GUIStyleState("m_OnHover", unityVersion), GUIStyleState("m_OnActive", unityVersion), GUIStyleState("m_OnFocused", unityVersion),
                RectOffset("m_Border"), RectOffset("m_Margin"), RectOffset("m_Padding"), RectOffset("m_Overflow"),
                PPtr("m_Font", "Font", unityVersion), Int("m_FontSize"), Int("m_FontStyle"),
                Int("m_Alignment"), Bool("m_WordWrap"), Bool("m_RichText", true),
                Int("m_TextClipping"), Int("m_ImagePosition"), Vector2f("m_ContentOffset"),
                Float("m_FixedWidth"), Float("m_FixedHeight"), Bool("m_StretchWidth"), Bool("m_StretchHeight", true)
            };
        }

        public static AssetTypeTemplateField Keyframe(string name, UnityVersion unityVersion) => CreateTemplateField(name, "Keyframe", Keyframe(unityVersion));
        public static List<AssetTypeTemplateField> Keyframe(UnityVersion unityVersion)
        {
            if (unityVersion.major >= 2018)
            {
                return new List<AssetTypeTemplateField> {
                    Float("time"), Float("value"), Float("inSlope"), Float("outSlope"),
                    Int("weightedMode"), Float("inWeight"), Float("outWeight")
                };
            }
            return new List<AssetTypeTemplateField> { Float("time"), Float("value"), Float("inSlope"), Float("outSlope") };
        }

        public static AssetTypeTemplateField GUIStyleState(string name, UnityVersion unityVersion) => CreateTemplateField(name, "GUIStyleState", GUIStyleState(unityVersion));
        public static List<AssetTypeTemplateField> GUIStyleState(UnityVersion unityVersion)
        {
            return new List<AssetTypeTemplateField> { PPtr("m_Background", "Texture2D", unityVersion), RGBAf("m_TextColor") };
        }

        public static AssetTypeTemplateField RGBAf(string name) => CreateTemplateField(name, "ColorRGBA", RGBAf());
        public static List<AssetTypeTemplateField> RGBAf()
        {
            return new List<AssetTypeTemplateField> { Float("r"), Float("g"), Float("b"), Float("a") };
        }

        public static AssetTypeTemplateField RGBAi(string name) => CreateTemplateField(name, "ColorRGBA", RGBAi());
        public static List<AssetTypeTemplateField> RGBAi()
        {
            return new List<AssetTypeTemplateField> { UInt("rgba") };
        }

        public static AssetTypeTemplateField AABB(string name) => CreateTemplateField(name, "AABB", AABB());
        public static List<AssetTypeTemplateField> AABB()
        {
            return new List<AssetTypeTemplateField> { Vector3f("m_Center"), Vector3f("m_Extent") };
        }

        public static AssetTypeTemplateField BoundsInt(string name) => CreateTemplateField(name, "BoundsInt", BoundsInt());
        public static List<AssetTypeTemplateField> BoundsInt()
        {
            return new List<AssetTypeTemplateField> { Vector3Int("m_Position"), Vector3Int("m_Size") };
        }

        public static AssetTypeTemplateField BitField(string name) => CreateTemplateField(name, "BitField", BitField());
        public static List<AssetTypeTemplateField> BitField()
        {
            return new List<AssetTypeTemplateField> { UInt("m_Bits") };
        }

        public static AssetTypeTemplateField Rectf(string name) => CreateTemplateField(name, "Rectf", Rectf());
        public static List<AssetTypeTemplateField> Rectf()
        {
            return new List<AssetTypeTemplateField> { Float("x"), Float("y"), Float("width"), Float("height") };
        }

        public static AssetTypeTemplateField RectOffset(string name) => CreateTemplateField(name, "RectOffset", RectOffset());
        public static List<AssetTypeTemplateField> RectOffset()
        {
            return new List<AssetTypeTemplateField> { Int("m_Left"), Int("m_Right"), Int("m_Top"), Int("m_Bottom") };
        }

        public static AssetTypeTemplateField Vector2Int(string name) => CreateTemplateField(name, "int2_storage", Vector2Int());
        public static List<AssetTypeTemplateField> Vector2Int()
        {
            return new List<AssetTypeTemplateField> { Int("x"), Int("y") };
        }

        public static AssetTypeTemplateField Vector3Int(string name) => CreateTemplateField(name, "int3_storage", Vector3Int());
        public static List<AssetTypeTemplateField> Vector3Int()
        {
            return new List<AssetTypeTemplateField> { Int("x"), Int("y"), Int("z") };
        }

        public static AssetTypeTemplateField Vector2f(string name) => CreateTemplateField(name, "Vector2f", Vector2f());
        public static List<AssetTypeTemplateField> Vector2f()
        {
            return new List<AssetTypeTemplateField> { Float("x"), Float("y") };
        }

        public static AssetTypeTemplateField Vector3f(string name) => CreateTemplateField(name, "Vector3f", Vector3f());
        public static List<AssetTypeTemplateField> Vector3f()
        {
            return new List<AssetTypeTemplateField> { Float("x"), Float("y"), Float("z") };
        }

        public static AssetTypeTemplateField PPtr(string name, string typeName, UnityVersion unityVersion) => CreateTemplateField(name, $"PPtr<{typeName}>", PPtr(unityVersion));
        public static List<AssetTypeTemplateField> PPtr(UnityVersion unityVersion)
        {
            if (unityVersion.major >= 5)
            {
                return new List<AssetTypeTemplateField> { Int("m_FileID"), Long("m_PathID") };
            }

            return new List<AssetTypeTemplateField> { Int("m_FileID"), Int("m_PathID") };
        }

        #endregion

        #region CreateTemplateField

        public static AssetTypeTemplateField CreateTemplateField(string name, string type, List<AssetTypeTemplateField> children)
        {
            return CreateTemplateField(name, type, AssetValueType.None, false, false, children);
        }
        public static AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType)
        {
            return CreateTemplateField(name, type, valueType, false, false, new List<AssetTypeTemplateField>(0));
        }

        public static AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, bool isArray, bool align)
        {
            return CreateTemplateField(name, type, valueType, isArray, align, new List<AssetTypeTemplateField>(0));
        }

        public static AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, List<AssetTypeTemplateField> children)
        {
            return CreateTemplateField(name, type, valueType, false, false, children);
        }

        public static AssetTypeTemplateField CreateTemplateField(string name, string type, AssetValueType valueType, bool isArray, bool align, List<AssetTypeTemplateField> children)
        {
            AssetTypeTemplateField field = new AssetTypeTemplateField
            {
                Name = name,
                Type = type,
                ValueType = valueType,
                IsArray = isArray,
                IsAligned = align,
                HasValue = valueType != AssetValueType.None,
                Children = children ?? new List<AssetTypeTemplateField>(0)
            };

            return field;
        }

        #endregion
    }
}
