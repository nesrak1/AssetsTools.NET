using System;
using System.Collections.Generic;

namespace AssetsTools.NET
{
    public abstract class AssetsReplacer : IDisposable
    {
        public abstract AssetsReplacementType GetReplacementType();
        public abstract long GetPathID();
        public abstract int GetClassID();
        public abstract ushort GetMonoScriptID();
        public abstract long Write(AssetsFileWriter writer);
        public abstract long WriteReplacer(AssetsFileWriter writer);

        public virtual void Dispose()
        {
        }

        #region Unused methods kept for backwards compatibility

        public virtual long GetSize()
        {
            throw new NotImplementedException();
        }

        public virtual void SetMonoScriptID(ushort scriptId)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetPropertiesHash(out Hash128 propertiesHash)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetPropertiesHash(Hash128 propertiesHash)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetScriptIDHash(out Hash128 scriptIdHash)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetScriptIDHash(Hash128 scriptIdHash)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetTypeInfo(out ClassDatabaseFile file, out ClassDatabaseType type)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetTypeInfo(ClassDatabaseFile file, ClassDatabaseType type, bool localCopy)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetPreloadDependencies(out List<AssetPPtr> preloadList)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetPreloadDependencies(List<AssetPPtr> preloadList)
        {
            throw new NotImplementedException();
        }

        public virtual bool AddPreloadDependency(AssetPPtr dependency)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
