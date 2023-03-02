using System;

namespace AssetsTools.NET.Extra
{
    [Flags]
    public enum AssetReadFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// Use the editor version of the TPK instead of the
        /// the player version. Use this if you are generating
        /// new assets for an editor project.
        /// </summary>
        PreferEditor = 1,
        /// <summary>
        /// If the file doesn't have a type tree, decide whether
        /// to skip calling AssetsManager.MonoTempGenerator to add the
        /// MonoBehaviour fields to the end of the base MonoBehaviour
        /// field or not.
        /// </summary>
        SkipMonoBehaviourFields = 2,
        /// <summary>
        /// If the file is using a type tree, force it to use
        /// the loaded class database instead.
        /// </summary>
        ForceFromCldb = 4
    }
}