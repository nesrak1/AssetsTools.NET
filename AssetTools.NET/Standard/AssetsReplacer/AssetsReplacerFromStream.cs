using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsReplacerFromStream : AssetsReplacer
    {
        private readonly long pathId;
        private readonly int classId;
        private readonly Stream stream;
        private readonly long offset;
        private readonly long size;
        private ushort monoScriptIndex;
        private Hash128 propertiesHash;
        private Hash128 scriptIdHash;
        private ClassDatabaseFile file;
        private ClassDatabaseType type;
        private List<AssetPPtr> preloadList;

        public AssetsReplacerFromStream(long pathId, int classId, ushort monoScriptIndex, Stream stream, long offset, long size)
        {
            this.pathId = pathId;
            this.classId = classId;
            this.monoScriptIndex = monoScriptIndex;
            this.stream = stream;
            this.offset = offset;
            this.size = size;
            this.preloadList = new List<AssetPPtr>();
        }
        public AssetsReplacerFromStream(AssetsFile assetsFile, AssetFileInfo info, Stream stream, long offset, long size)
        {
            this.pathId = info.PathId;
            this.classId = info.TypeId;
            this.monoScriptIndex = assetsFile.GetScriptIndex(info);
            this.stream = stream;
            this.offset = offset;
            this.size = size;
            this.preloadList = new List<AssetPPtr>();
        }
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.AddOrModify;
        }
        public override long GetPathID()
        {
            return pathId;
        }
        public override int GetClassID()
        {
            return classId;
        }
        public override ushort GetMonoScriptID()
        {
            return monoScriptIndex;
        }
        public override void SetMonoScriptID(ushort scriptId)
        {
            monoScriptIndex = scriptId;
        }
        public override long GetSize()
        {
            return size;
        }
        public override bool GetPropertiesHash(out Hash128 propertiesHash)
        {
            propertiesHash = this.propertiesHash;
            return true;
        }
        public override bool SetPropertiesHash(Hash128 propertiesHash)
        {
            this.propertiesHash = propertiesHash;
            return true;
        }
        public override bool GetScriptIDHash(out Hash128 scriptIdHash)
        {
            scriptIdHash = this.scriptIdHash;
            return true;
        }
        public override bool SetScriptIDHash(Hash128 scriptIdHash)
        {
            this.scriptIdHash = scriptIdHash;
            return true;
        }
        public override bool GetTypeInfo(out ClassDatabaseFile file, out ClassDatabaseType type)
        {
            file = this.file;
            type = this.type;
            return true;
        }
        public override bool SetTypeInfo(ClassDatabaseFile file, ClassDatabaseType type, bool localCopy)
        {
            this.file = file;
            this.type = type;
            return true;
        }
        public override bool GetPreloadDependencies(out List<AssetPPtr> preloadList)
        {
            preloadList = new List<AssetPPtr>(this.preloadList);
            return true;
        }
        public override bool SetPreloadDependencies(List<AssetPPtr> preloadList)
        {
            this.preloadList = preloadList;
            return true;
        }
        public override bool AddPreloadDependency(AssetPPtr dependency)
        {
            preloadList.Add(dependency);
            return true;
        }
        public override long Write(AssetsFileWriter writer)
        {
            stream.Position = offset;
            stream.CopyToCompat(writer.BaseStream, size);
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)AssetsReplacerType.AssetModifierFromMemory); // replacer type
            writer.Write((byte)1); // replacer from stream version
            // entry modifier base
            {
                // entry replacer base
                {
                    writer.Write((byte)1); // entry replacer version
                    writer.Write(0); // file id (always 0)
                    writer.Write(pathId);
                    writer.Write(classId);
                    writer.Write(monoScriptIndex);

                    writer.Write(preloadList.Count);
                    for (int i = 0; i < preloadList.Count; i++)
                    {
                        writer.Write(preloadList[i].FileId);
                        writer.Write(preloadList[i].PathId);
                    }
                }

                writer.Write((byte)0); // entry modifier version

                if (propertiesHash.data != null)
                {
                    writer.Write((byte)1);
                    writer.Write(propertiesHash.data);
                }
                else
                {
                    writer.Write((byte)0);
                }

                if (scriptIdHash.data != null)
                {
                    writer.Write((byte)1);
                    writer.Write(scriptIdHash.data);
                }
                else
                {
                    writer.Write((byte)0);
                }

                if (file != null)
                {
                    writer.Write((byte)1);
                    file.Write(writer, ClassFileCompressionType.Uncompressed);
                }
                else
                {
                    writer.Write((byte)0);
                }
            }

            writer.Write(GetSize());
            Write(writer);

            return writer.Position;
        }
        public override void Dispose()
        {
            stream.Dispose();
        }
    }
}
