using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public ulong Read(ulong absFilePos, AssetsFileReader reader)
            {
                mostSignificant = reader.ReadInt64();
                leastSignificant = reader.ReadInt64();
                return reader.Position;
            }
            public ulong Write(ulong absFilePos, AssetsFileWriter writer)
            {
                writer.Write(mostSignificant);
                writer.Write(leastSignificant);
                return writer.Position;
            }
        }
        public GUID128 guid;
        public int type;
        public string assetPath; //path to the .assets file
        public byte[] bufferedPath; //for buffered (type=1)
        public ulong Read(ulong absFilePos, AssetsFileReader reader, bool bigEndian)
        {
            bufferedPath = new byte[] { reader.ReadByte() }; //-Dunno why it's here but it is
            guid = new GUID128();
            guid.Read(reader.Position, reader);
            type = reader.ReadInt32();
            assetPath = reader.ReadNullTerminated();
            //todo: the switchero was here for testing purposes, should be handled by application for full control
            if (assetPath.StartsWith("library/"))
            {
                assetPath = "Resources\\" + assetPath.Substring(8);
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer)
        {
            writer.Write(bufferedPath);
            guid.Write(writer.Position, writer);
            writer.Write(type);
            string assetPathTemp = assetPath;
            if (assetPathTemp.StartsWith("Resources\\"))
            {
                assetPathTemp = "library/" + assetPath.Substring(10);
            }
            writer.WriteNullTerminated(assetPathTemp);
            return writer.Position;
        }
    }
}
