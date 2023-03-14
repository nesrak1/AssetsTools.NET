using System;

namespace AssetsTools.NET
{
    [Flags]
    public enum TypeTreeNodeFlags
    {
        None = 0,
        /// <summary>
        /// Type tree node is an array.
        /// </summary>
        Array = 1,
        /// <summary>
        /// Type tree node is a ref type. For example, "managedRefArrayItem" would be an
        /// array item that is a reference to an object in the registry.
        /// </summary>
        Ref = 2,
        /// <summary>
        /// Type tree node is a registry. Should just be "ManagedReferencesRegistry references".
        /// </summary>
        Registry = 4,
        /// <summary>
        /// Type tree node is an array of ref types. This occurs if the SerializeReference was
        /// added to a list or array instead of just a single field. This is not applied to the
        /// Array child of the field, just the field itself.
        /// </summary>
        ArrayOfRefs = 8
    }
}
