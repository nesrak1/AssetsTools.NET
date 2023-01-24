using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class AssetsRemover : AssetsReplacer
    {
        private readonly long pathID;

        public AssetsRemover(long pathID)
        {
            this.pathID = pathID;
        }
        public override AssetsReplacementType GetReplacementType()
        {
            return AssetsReplacementType.Remove;
        }
        public override long GetPathID()
        {
            return pathID;
        }
        public override int GetClassID()
        {
            return 0;
        }
        public override ushort GetMonoScriptID()
        {
            return 0;
        }
        public override void SetMonoScriptID(ushort scriptId)
        {
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
            writer.Write((short)AssetsReplacerType.AssetRemover); // replacer type
            writer.Write((byte)1); // remover version
            // entry modifier base
            {
                // entry replacer base
                {
                    writer.Write((byte)1); // entry replacer version
                    writer.Write(0); // file id (always 0)
                    writer.Write(pathID);
                    writer.Write(0); // class id
                    writer.Write((ushort)0); // mono script index
                }

                writer.Write(0); //flags, which are ignored since this is a remover
            }

            return writer.Position;
        }
        public override void Dispose() {}
    }
}
