using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileDependency
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

            //because lowercase "resources" is read by unity fine on linux, it either uses
            //hardcoded replaces like below or it has case insensitive pathing somehow
            //this isn't consistent with the original assetstools but it only supported
            //windows anyway, so this will only create issues if more than these three
            //pop up in the future. also, the reason I don't just replace all "library"
            //with "Resources" is so that when saving, I can change it back to the original
            //(like how unity_builtin_extra goes back to "resources", not "library")
            if (assetPath == "resources/unity_builtin_extra")
            {
                assetPath = "Resources/unity_builtin_extra";
            }
            else if (assetPath == "library/unity default resources")
            {
                assetPath = "Resources/unity default resources";
            }
            else if (assetPath == "library/unity editor resources")
            {
                assetPath = "Resources/unity editor resources";
            }
            //todo: the switchero was here for testing purposes, should be handled by application for full control
            //if (assetPath.StartsWith("library/"))
            //{
            //    assetPath = "Resources/" + assetPath.Substring(8);
            //}
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.WriteNullTerminated(bufferedPath);
            guid.Write(writer);
            writer.Write(type);
            string assetPathTemp = assetPath;
            if (assetPath == "Resources/unity_builtin_extra")
            {
                assetPathTemp = "resources/unity_builtin_extra";
            }
            else if (assetPath == "Resources/unity default resources")
            {
                assetPathTemp = "library/unity default resources";
            }
            else if (assetPath == "Resources/unity editor resources")
            {
                assetPathTemp = "library/unity editor resources";
            }
            //if (assetPathTemp.StartsWith("Resources\\") || assetPathTemp.StartsWith("Resources/"))
            //{
            //    assetPathTemp = "library/" + assetPath.Substring(10);
            //}
            writer.WriteNullTerminated(assetPathTemp);
        }
    }
}
