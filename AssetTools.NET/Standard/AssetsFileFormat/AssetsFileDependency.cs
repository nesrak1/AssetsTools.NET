using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public struct AssetsFileDependency
    {
        //version < 6 : no bufferedPath
        //version < 5 : no bufferedPath, guid, type
        public struct GUID128
        {
            public long mostSignificant; //64-127 //big-endian
            public long leastSignificant; //0-63  //big-endian
            public void Read(AssetsFileReader reader)
            {
                mostSignificant = reader.ReadInt64();
                leastSignificant = reader.ReadInt64();
            }
            public void Write(AssetsFileWriter writer)
            {
                writer.Write(mostSignificant);
                writer.Write(leastSignificant);
            }
        }
        public string bufferedPath; //for buffered (type=1)
        public GUID128 guid;
        public int type;
        public string assetPath; //path to the .assets file
        public void Read(AssetsFileReader reader)
        {
            bufferedPath = reader.ReadNullTerminated();
            guid = new GUID128();
            guid.Read(reader);
            type = reader.ReadInt32();
            assetPath = reader.ReadNullTerminated();
            //todo: the switchero was here for testing purposes, should be handled by application for full control
            if (assetPath.StartsWith("library/"))
            {
                assetPath = "Resources/" + assetPath.Substring(8);
            }
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(bufferedPath);
            guid.Write(writer);
            writer.Write(type);
            string assetPathTemp = assetPath;
            if (assetPathTemp.StartsWith("Resources\\") || assetPathTemp.StartsWith("Resources/"))
            {
                assetPathTemp = "library/" + assetPath.Substring(10);
            }
            writer.WriteNullTerminated(assetPathTemp);
        }
    }
}
