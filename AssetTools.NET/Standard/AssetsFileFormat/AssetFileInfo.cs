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
        /// Address of the asset's data from the header's DataOffset.
        /// Use <see cref="GetAbsoluteByteOffset(AssetsFile)"/> for the real file position.
        /// If the asset has a replacer, this field is ignored.
        /// </summary>
        public long ByteOffset { get; set; }
        /// <summary>
        /// Byte size of the asset data. If the asset has a replacer, this field is ignored.
        /// </summary>
        public uint ByteSize { get; set; }
        /// <summary>
        /// Before version 16 this is the type ID of the asset. In version 16 and later this is the
        /// index into the type tree list. In versions 15 and below, this is the same as TypeId
        /// except in MonoBehaviours where this acts similar to ScriptTypeIndex (negative).
        /// You should use TypeId for the type ID in either version. <see cref="AssetClassID"/>
        /// </summary>
        public int TypeIdOrIndex { get; set; }
        /// <summary>
        /// Old Type ID of the asset (officially called class ID). This field is only used in versions
        /// 15 and below and is the same as TypeId, except when TypeId is negative, in which case
        /// the old type ID will be a MonoBehaviour (0x72) and TypeId will be the same as TypeIdOrIndex.
        /// You should use TypeId for the type ID in either version. <see cref="AssetClassID"/>
        /// </summary>
        public ushort OldTypeId { get; set; }
        /// <summary>
        /// Script type index of the asset. Assets other than MonoBehaviours will have 0xffff for
        /// this field. This value is stored in the type tree starting at version 17. Note this is
        /// not the same as taking
        /// </summary>
        public ushort ScriptTypeIndex { get; set; }
        /// <summary>
        /// Marks if the type in the type tree has been stripped (?)
        /// </summary>
        public byte Stripped { get; set; }

        /// <summary>
        /// The type ID of the asset. This field works in both versions. This field is only for
        /// convenience; modifying the type ID in the type tree in later versions will not update the
        /// ID here, and modifying this field will not update the type ID when saved.
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// Replacer which can be set by the user.
        /// You can use <see cref="SetNewData(byte[])"/> or <see cref="SetNewData(AssetTypeValueField)"/>
        /// for convenience.
        /// </summary>
        public IContentReplacer Replacer { get; set; }
        /// <summary>
        /// Replacer type such as modified or removed.
        /// </summary>
        public ContentReplacerType ReplacerType => Replacer != null ? Replacer.GetReplacerType() : ContentReplacerType.None;
        /// <summary>
        /// Is the replacer non-null and does the replacer has a preview?
        /// </summary>
        public bool IsReplacerPreviewable => Replacer != null && Replacer.HasPreview();

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
                ByteOffset = reader.ReadInt64();
            }
            else
            {
                ByteOffset = reader.ReadUInt32();
            }
            ByteSize = reader.ReadUInt32();
            TypeIdOrIndex = reader.ReadInt32();
            if (version <= 15)
            {
                OldTypeId = reader.ReadUInt16();
            }
            if (version <= 16)
            {
                ScriptTypeIndex = reader.ReadUInt16();
            }
            if (15 <= version && version <= 16)
            {
                Stripped = reader.ReadByte();
            }
            Replacer = null;
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
                writer.Write(ByteOffset);
            }
            else
            {
                writer.Write((uint)ByteOffset);
            }
            writer.Write(ByteSize);
            writer.Write(TypeIdOrIndex);
            if (version <= 15)
            {
                writer.Write(OldTypeId);
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
        public long GetAbsoluteByteOffset(AssetsFile assetsFile)
        {
            return assetsFile.Header.DataOffset + ByteOffset;
        }
        /// <summary>
        /// Address of the asset's data from the start of the file.
        /// </summary>
        public long GetAbsoluteByteOffset(AssetsFileHeader header)
        {
            return header.DataOffset + ByteOffset;
        }
        /// <summary>
        /// Address of the asset's data from the start of the file.
        /// </summary>
        public long GetAbsoluteByteOffset(long dataOffset)
        {
            return dataOffset + ByteOffset;
        }

        /// <summary>
        /// Sets the bytes used when the AssetsFile is written.
        /// </summary>
        public void SetNewData(byte[] newBytes)
        {
            Replacer = new ContentReplacerFromBuffer(newBytes);
        }

        /// <summary>
        /// Sets the bytes to the base field's data used when the AssetsFile is written.
        /// </summary>
        public void SetNewData(AssetTypeValueField baseField)
        {
            Replacer = new ContentReplacerFromBuffer(baseField.WriteToByteArray());
        }
        
        /// <summary>
        /// Set the asset to be removed when the AssetsFile is written.
        /// </summary>
        public void SetRemoved()
        {
            Replacer = new ContentRemover();
        }

        /// <summary>
        /// Creates a new asset info. If the type has not appeared in this file yet, pass
        /// <paramref name="classDatabase"/> to pull new type info from.
        /// </summary>
        /// <param name="assetsFile">The assets file this info will belong to.</param>
        /// <param name="pathId">The path ID to use.</param>
        /// <param name="typeId">The type ID to use.</param>
        /// <param name="classDatabase">The class database to use if the type does not appear in the assets file yet.</param>
        /// <param name="preferEditor">Read from the editor version of this type if available?</param>
        /// <returns>The new asset info, or null if the type can't be found in the type tree or class database.</returns>
        public static AssetFileInfo Create(
            AssetsFile assetsFile, long pathId, int typeId,
            ClassDatabaseFile classDatabase = null, bool preferEditor = false)
        {
            return Create(assetsFile, pathId, typeId, 0xffff, classDatabase, preferEditor);
        }

        /// <summary>
        /// Creates a new asset info. If the type has not appeared in this file yet, pass
        /// <paramref name="classDatabase"/> to pull new type info from. If the asset is
        /// a MonoBehaviour, add the type manually to <see cref="AssetsFileMetadata.TypeTreeTypes"/>
        /// and if version 16 or later, set <paramref name="scriptIndex"/> to the script type index
        /// or if ealier than version 16, set the negative type id.
        /// </summary>
        /// <param name="assetsFile">The assets file this info will belong to.</param>
        /// <param name="pathId">The path ID to use.</param>
        /// <param name="typeId">The type ID to use.</param>
        /// <param name="scriptIndex">The script type index to use.</param>
        /// <param name="classDatabase">The class database to use if the type does not appear in the assets file yet.</param>
        /// <param name="preferEditor">Read from the editor version of this type if available?</param>
        /// <returns>The new asset info, or null if the type can't be found in the type tree or class database.</returns>
        public static AssetFileInfo Create(
            AssetsFile assetsFile, long pathId, int typeId, ushort scriptIndex = 0xffff,
            ClassDatabaseFile classDatabase = null, bool preferEditor = false)
        {
            uint version = assetsFile.Header.Version;

            int typeIdOrIndex;
            ushort oldTypeId;
            ushort scriptTypeIndex;

            if (version >= 16)
            {
                int typeIndex = assetsFile.Metadata.FindTypeTreeTypeIndexByID(typeId, scriptIndex);
                if (typeIndex == -1)
                {
                    if (classDatabase == null)
                    {
                        return null;
                    }

                    TypeTreeType newType;
                    if (assetsFile.Metadata.TypeTreeEnabled)
                    {
                        newType = ClassDatabaseToTypeTree.Convert(classDatabase, typeId, preferEditor);
                        if (newType == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        newType = new TypeTreeType
                        {
                            TypeId = typeId,
                            IsStrippedType = false,
                            ScriptTypeIndex = scriptIndex,
                            ScriptIdHash = Hash128.NewBlankHash(),
                            TypeHash = Hash128.NewBlankHash(),
                            Nodes = new List<TypeTreeNode>(),
                            StringBufferBytes = new byte[0],
                            TypeDependencies = new int[0],
                            IsRefType = false,
                            TypeReference = null,
                        };
                    }

                    typeIdOrIndex = assetsFile.Metadata.TypeTreeTypes.Count;
                    assetsFile.Metadata.TypeTreeTypes.Add(newType);
                }
                else
                {
                    typeIdOrIndex = typeIndex;
                }
                oldTypeId = 0;
            }
            else
            {
                typeIdOrIndex = typeId;
                if (typeId < 0)
                {
                    oldTypeId = 0x72;
                }
                else
                {
                    oldTypeId = (ushort)typeId;
                }
            }

            if (version < 17)
            {
                scriptTypeIndex = scriptIndex;
            }
            else
            {
                scriptTypeIndex = 0;
            }
            
            return new AssetFileInfo()
            {
                PathId = pathId,
                ByteOffset = -1,
                ByteSize = 0,
                TypeIdOrIndex = typeIdOrIndex,
                OldTypeId = oldTypeId,
                ScriptTypeIndex = scriptTypeIndex,
                Stripped = 0,
                TypeId = typeId,
                Replacer = null,
            };
        }
    }
}
