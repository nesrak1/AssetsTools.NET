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
                metadataSize = 0,
                fileSize = -1,
                format = formatVersion,
                firstFileOffset = -1,
                endianness = 0,
                unknown = new byte[] { 0, 0, 0 }
            };

            TypeTree typeTree = new TypeTree()
            {
                unityVersion = engineVersion,
                version = typeTreeVersion,
                hasTypeTree = hasTypeTree,
                fieldCount = 0,
                unity5Types = new List<Type_0D>()
            };

            header.Write(writer);
            typeTree.Write(writer, formatVersion);

            writer.Write((uint)0); //AssetCount
            writer.Align();

            //preload table and dependencies
            writer.Write((uint)0);
            writer.Write((uint)0);

            //secondaryTypeCount
            if (header.format >= 0x14)
            {
                writer.Write(0);
            }

            uint metadataSize = (uint)(writer.Position - 0x13);
            if (header.format >= 0x16)
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

            header.fileSize = endPosition;
            header.firstFileOffset = endPosition;
            header.metadataSize = metadataSize;

            writer.Position = 0;
            header.Write(writer);

            writer.Position = endPosition;
        }
    }
}
