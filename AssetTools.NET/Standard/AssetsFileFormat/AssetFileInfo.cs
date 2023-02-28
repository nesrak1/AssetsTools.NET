using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetFileInfo
    {
        /// <summary>
        /// Path ID of the asset.
        /// </summary>
        public long PathId { get; set; }
        /// <summary>
        /// Address of the asset's data from the header's DataOffset. Use GetAbsoluteByteStart for the real file position.
        /// </summary>
        public long ByteStart { get; set; }
        /// <summary>
        /// Byte size of the asset data.
        /// </summary>
        public uint ByteSize { get; set; }
        /// <summary>
        /// Before version 16 this is the type ID of the asset. In version 16 and later this is the index into the type tree list.
        /// In versions 15 and below, this is the same as TypeId except in MonoBehaviours where this acts similar to ScriptTypeIndex (negative).
        /// You should use TypeId for the type ID in either version. <see cref="AssetClassID"/>
        /// </summary>
        public int TypeIdOrIndex { get; set; }
        /// <summary>
        /// Class ID of the asset. This field is only used in versions 15 and below and is the same as TypeId, except when the
        /// Class ID is negative, in which case the TypeId will be a MonoBehaviour and the Class ID will be the negative number.
        /// You should use TypeId for the type ID in either version. <see cref="AssetClassID"/>
        /// </summary>
        public ushort ClassId { get; set; }
        /// <summary>
        /// Script type index of the asset. Assets other than MonoBehaviours will have 0xffff for this field.
        /// This value is stored in the type tree starting at version 17.
        /// </summary>
        public ushort ScriptTypeIndex { get; set; }
        /// <summary>
        /// Marks if the type in the type tree has been stripped (?)
        /// </summary>
        public byte Stripped { get; set; }

        /// <summary>
        /// The type ID of the asset. This field works in both versions. This field is only for convenience; modifying the type
        /// ID in the type tree in later versions will not update the ID here, and modifying this field will not update the
        /// type ID in previous versions.
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// The absolute file position of the asset from the file opened. This field is only for convenience; if you want to
        /// change where this asset is read from, set the ByteStart property instead.
        /// </summary>
        public long AbsoluteByteStart { get; set; }

        public static int GetSize(uint version)
        {
            int size = 0;
            size += 4;
            if (version >= 14) size += 4;
            size += 12;
            if (version >= 22) size += 4;
            if (version <= 15) size += 2;
            if (version <= 16) size += 2;
            if (15 <= version && version <= 16) size += 1;
            return size;
        }

        public void Read(AssetsFileReader reader, uint version)
        {
            reader.Align();
            if (version >= 14)
            {
                PathId = reader.ReadInt64();
            }
            else
            {
                PathId = reader.ReadUInt32();
            }
            if (version >= 22)
            {
                ByteStart = reader.ReadInt64();
            }
            else
            {
                ByteStart = reader.ReadUInt32();
            }
            ByteSize = reader.ReadUInt32();
            TypeIdOrIndex = reader.ReadInt32();
            if (version <= 15)
            {
                ClassId = reader.ReadUInt16();
            }
            if (version <= 16)
            {
                ScriptTypeIndex = reader.ReadUInt16();
            }
            if (15 <= version && version <= 16)
            {
                Stripped = reader.ReadByte();
            }
        }

        public void Write(AssetsFileWriter writer, uint version)
        {
            writer.Align();
            if (version >= 14)
            {
                writer.Write(PathId);
            }
            else
            {
                writer.Write((uint)PathId);
            }
            if (version >= 22)
            {
                writer.Write(ByteStart);
            }
            else
            {
                writer.Write((uint)ByteStart);
            }
            writer.Write(ByteSize);
            writer.Write(TypeIdOrIndex);
            if (version <= 15)
            {
                writer.Write(ClassId);
            }
            if (version <= 16)
            {
                writer.Write(ScriptTypeIndex);
            }
            if (15 <= version && version <= 16)
            {
                writer.Write(Stripped);
            }
        }

        // we may get rid of these and stick with the set-once properties above
        /// <summary>
        /// Get the Type ID of the asset.
        /// </summary>
        public int GetTypeId(AssetsFile assetsFile)
        {
            return GetTypeId(assetsFile.Metadata.TypeTreeTypes, assetsFile.Header.Version);
        }
        /// <summary>
        /// Get the Type ID of the asset.
        /// </summary>
        public int GetTypeId(AssetsFileMetadata metadata, uint version)
        {
            return GetTypeId(metadata.TypeTreeTypes, version);
        }
        /// <summary>
        /// Get the Type ID of the asset.
        /// </summary>
        public int GetTypeId(List<TypeTreeType> typeTreeTypes, uint version)
        {
            if (version < 16)
            {
                return TypeIdOrIndex;
            }
            else
            {
                if (TypeIdOrIndex >= typeTreeTypes.Count)
                {
                    throw new IndexOutOfRangeException("TypeIndex is larger than type tree count!");
                }
                return typeTreeTypes[TypeIdOrIndex].TypeId;
            }
        }

        /// <summary>
        /// Address of the asset's data from the start of the file.
        /// </summary>
        public long GetAbsoluteByteStart(AssetsFile assetsFile)
        {
            return assetsFile.Header.DataOffset + ByteStart;
        }
        /// <summary>
        /// Address of the asset's data from the start of the file.
        /// </summary>
        public long GetAbsoluteByteStart(AssetsFileHeader header)
        {
            return header.DataOffset + ByteStart;
        }
        /// <summary>
        /// Address of the asset's data from the start of the file.
        /// </summary>
        public long GetAbsoluteByteStart(long dataOffset)
        {
            return dataOffset + ByteStart;
        }
    }
}
