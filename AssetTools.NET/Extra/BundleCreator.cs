using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra
{
    public class BundleCreator
    {
        public static void CreateBlankAssets(MemoryStream ms, string engineVersion, uint formatVersion, uint typeTreeVersion, bool hasTypeTree = false)
        {
            AssetsFileWriter writer = new AssetsFileWriter(ms);

            AssetsFileHeader header = new AssetsFileHeader()
            {
                MetadataSize = 0,
                FileSize = -1,
                Version = formatVersion,
                DataOffset = -1,
                Endianness = false
            };

            AssetsFileMetadata metadata = new AssetsFileMetadata()
            {
                UnityVersion = engineVersion,
                TargetPlatform = typeTreeVersion,
                TypeTreeEnabled = hasTypeTree,
                TypeTreeTypes = new List<TypeTreeType>(),
                AssetInfos = new List<AssetFileInfo>(),
                ScriptTypes = new List<AssetPPtr>(),
                Externals = new List<AssetsFileExternal>(),
                RefTypes = new List<TypeTreeType>()
            };

            header.Write(writer);
            metadata.Write(writer, formatVersion);

            writer.Write((uint)0); // AssetCount
            writer.Align();

            // preload table and dependencies
            writer.Write((uint)0);
            writer.Write((uint)0);

            // secondaryTypeCount
            if (header.Version >= 0x14)
            {
                writer.Write(0);
            }

            uint metadataSize = (uint)(writer.Position - 0x13);
            if (header.Version >= 0x16)
            {
                metadataSize -= 0x1c;
            }

            if (writer.Position < 0x1000)
            {
                while (writer.Position < 0x1000)
                {
                    writer.Write((byte)0x00);
                }
            }
            else
            {
                if (writer.Position % 16 == 0)
                    writer.Position += 16;
                else
                    writer.Align16();
            }

            long endPosition = writer.Position;

            header.FileSize = endPosition;
            header.DataOffset = endPosition;
            header.MetadataSize = metadataSize;

            writer.Position = 0;
            header.Write(writer);

            writer.Position = endPosition;
        }
    }
}
