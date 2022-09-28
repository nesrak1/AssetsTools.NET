using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    [Flags]
    public enum ClassFileTypeFlags : byte
    {
        /// <summary>
        /// None of the flags apply to this class
        /// </summary>
        None = 0,
        /// <summary>
        /// Is the class abstract?
        /// </summary>
        IsAbstract = 1,
        /// <summary>
        /// Is the class sealed? Not necessarily accurate.
        /// </summary>
        IsSealed = 2,
        /// <summary>
        /// Does the class only appear in the editor?
        /// </summary>
        IsEditorOnly = 4,
        /// <summary>
        /// Does the class only appear in game files? Not currently used
        /// </summary>
        IsReleaseOnly = 8,
        /// <summary>
        /// Is the class stripped?
        /// </summary>
        IsStripped = 16,
        /// <summary>
        /// Not currently used
        /// </summary>
        Reserved = 32,
        /// <summary>
        /// Does the class have an editor root node?
        /// </summary>
        HasEditorRootNode = 64,
        /// <summary>
        /// Does the class have a release root node?
        /// </summary>
        HasReleaseRootNode = 128
    }
}
