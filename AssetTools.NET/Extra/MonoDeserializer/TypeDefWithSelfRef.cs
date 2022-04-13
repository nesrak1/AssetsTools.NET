using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET.Extra
{
    internal struct TypeDefWithSelfRef
    {
        public TypeReference typeRef;
        public TypeDefinition typeDef;

        public TypeDefWithSelfRef(TypeReference typeRef)
        {
            this.typeRef = typeRef;
            this.typeDef = typeRef.Resolve();
        }
    }
}
