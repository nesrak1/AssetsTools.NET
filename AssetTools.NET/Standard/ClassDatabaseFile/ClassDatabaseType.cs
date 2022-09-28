using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public class ClassDatabaseType
    {
        public int ClassId { get; set; }
        public ushort Name { get; set; }
        public ushort BaseName { get; set; }
        public ClassFileTypeFlags Flags { get; set; }
        public ClassDatabaseTypeNode EditorRootNode { get; set; }
        public ClassDatabaseTypeNode ReleaseRootNode { get; set; }

        public void Read(AssetsFileReader reader)
        {
            ClassId = reader.ReadInt32();

            Name = reader.ReadUInt16();
            BaseName = reader.ReadUInt16();

            Flags = (ClassFileTypeFlags)reader.ReadByte();

            EditorRootNode = null;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasEditorRootNode))
            {
                EditorRootNode = new ClassDatabaseTypeNode();
                EditorRootNode.Read(reader);
            }

            ReleaseRootNode = null;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasReleaseRootNode))
            {
                ReleaseRootNode = new ClassDatabaseTypeNode();
                ReleaseRootNode.Read(reader);
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.Write(ClassId);

            writer.Write(Name);
            writer.Write(BaseName);

            writer.Write((byte)Flags);

            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasEditorRootNode) && EditorRootNode != null)
            {
                EditorRootNode.Write(writer);
            }

            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasReleaseRootNode) && ReleaseRootNode != null)
            {
                ReleaseRootNode.Write(writer);
            }
        }

        public ClassDatabaseTypeNode GetPreferredNode(bool preferEditor = false)
        {
            if (EditorRootNode != null && ReleaseRootNode != null)
                return preferEditor ? EditorRootNode : ReleaseRootNode;
            else if (EditorRootNode != null)
                return EditorRootNode;
            else if (ReleaseRootNode != null)
                return ReleaseRootNode;
            else
                return null;
        }
    }
}
