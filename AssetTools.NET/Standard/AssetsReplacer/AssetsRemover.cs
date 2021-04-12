using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsRemover : AssetsReplacer
    {
        private readonly int fileID;
        private readonly long pathID;
        private readonly int classID;
        private ushort monoScriptIndex;

        //why is classid and monoscriptindex in the constructor if they go unused?
        public AssetsRemover(int fileID, long pathID, int classID, ushort monoScriptIndex = 0xFFFF)
        {
            this.fileID = fileID;
            this.pathID = pathID;
            this.classID = classID;
            this.monoScriptIndex = monoScriptIndex;
        }
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.Remove;
        }
        public override int GetFileID()
        {
            return fileID;
        }
        public override long GetPathID()
        {
            return pathID;
        }
        public override int GetClassID()
        {
            return classID;
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
            return 0;
        }
        public override bool GetPropertiesHash(out Hash128 propertiesHash)
        {
            propertiesHash = new Hash128();
            return false;
        }
        public override bool SetPropertiesHash(Hash128 propertiesHash)
        {
            return false;
        }
        public override bool GetScriptIDHash(out Hash128 scriptIdHash)
        {
            scriptIdHash = new Hash128();
            return false;
        }
        public override bool SetScriptIDHash(Hash128 scriptIdHash)
        {
            return false;
        }
        public override bool GetTypeInfo(out ClassDatabaseFile file, out ClassDatabaseType type)
        {
            file = null;
            type = null;
            return false;
        }
        public override bool SetTypeInfo(ClassDatabaseFile file, ClassDatabaseType type, bool localCopy)
        {
            return false;
        }
        public override bool GetPreloadDependencies(out List<AssetPPtr> preloadList)
        {
            preloadList = null;
            return false;
        }
        public override bool SetPreloadDependencies(List<AssetPPtr> preloadList)
        {
            return false;
        }
        public override bool AddPreloadDependency(AssetPPtr dependency)
        {
            return false;
        }
        public override long Write(AssetsFileWriter writer)
        {
            return writer.Position;
        }
        public override long WriteReplacer(AssetsFileWriter writer)
        {
            writer.Write((short)0); //replacer type
            writer.Write((byte)1); //file type (0 bundle, 1 assets)
            writer.Write((byte)1); //idk, always 1
            writer.Write(0); //always 0 even when fileid is something else
            writer.Write(pathID);
            writer.Write(classID);
            writer.Write(monoScriptIndex);
            writer.Write(0); //flags, which are ignored since this is a remover

            return writer.Position;
        }
    }
}
