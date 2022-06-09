using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageType
    {
        public int ClassId { get; set; }
        public ushort Name { get; set; }
        public ushort BaseName { get; set; }
        public ClassDatabasePackageTypeFlags Flags { get; set; }
        public ushort EditorRootNode { get; set; }
        public ushort ReleaseRootNode { get; set; }

        public void Read(AssetsFileReader reader, int classId)
        {
            ClassId = classId;

            Name = reader.ReadUInt16();
            BaseName = reader.ReadUInt16();

            Flags = (ClassDatabasePackageTypeFlags)reader.ReadByte();

            EditorRootNode = ushort.MaxValue;
            if (Net35Polyfill.HasFlag(Flags, ClassDatabasePackageTypeFlags.HasEditorRootNode))
                EditorRootNode = reader.ReadUInt16();

            ReleaseRootNode = ushort.MaxValue;
            if (Net35Polyfill.HasFlag(Flags, ClassDatabasePackageTypeFlags.HasReleaseRootNode))
                ReleaseRootNode = reader.ReadUInt16();
        }
    }
}
