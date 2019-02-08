////////////////////////////
//   ASSETSTOOLS.NET        
//   Original by DerPopo    
//   Ported by nesrak1      
//   Hey, watch out! This   
//   library isn't done yet.
//   You've been warned!    

using System;
using System.Runtime.InteropServices;

namespace AssetsTools.NET
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Hash128
    {
        [FieldOffset(0)]
        public byte[] bValue;
        [FieldOffset(0)]
        public ushort[] wValue;
        [FieldOffset(0)]
        public uint[] dValue;
        [FieldOffset(0)]
        public ulong[] qValue;
    }
}
