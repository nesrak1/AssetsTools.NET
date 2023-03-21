using LibCpp2IL.Metadata;
using LibCpp2IL.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AssetsTools.NET.Cpp2IL
{
    internal static class Extensions
    {
        public static Il2CppTypeDefinition Resolve(this Il2CppTypeReflectionData typeRef)
        {
            return typeRef.isArray ? Resolve(typeRef.arrayType) : typeRef.baseType;
        }

        public static Il2CppTypeReflectionData GetEnumUnderlyingType(this Il2CppTypeDefinition self)
        {
            for (int i = 0; i < self.Fields.Length; i++)
            {
                if (!self.FieldAttributes[i].HasFlag(FieldAttributes.Static))
                {
                    return self.Fields[i].FieldType;
                }
            }

            throw new ArgumentException();
        }
    }
}
