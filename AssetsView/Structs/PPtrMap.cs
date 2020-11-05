using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Structs
{
    public class PPtrMap
    {
        public int version;
        public Dictionary<AssetID, long> refTable;
        public List<string> strTable;
        private BinaryReader br;
        public PPtrMap(BinaryReader br)
        {
            this.br = br;
            refTable = new Dictionary<AssetID, long>();
            strTable = new List<string>();

            version = br.ReadInt32();
            br.BaseStream.Position = br.ReadInt32();
            int strTableItems = br.ReadInt32();
            for (int i = 0; i < strTableItems; i++)
            {
                strTable.Add(br.ReadString());
            }
            br.BaseStream.Position = 8;
            int ptrTableItems = br.ReadInt32();
            for (int i = 0; i < ptrTableItems; i++)
            {
                refTable.Add(new AssetID(strTable[br.ReadInt32()], br.ReadInt64()), br.ReadInt32());
            }
        }

        public List<AssetID> GetXRefs(AssetID id)
        {
            AssetID newId = new AssetID(Path.GetFileName(id.fileName), id.pathID);
            if (refTable.ContainsKey(newId))
            {
                List<AssetID> retIds = new List<AssetID>();
                br.BaseStream.Position = refTable[newId];
                int xRefItems = br.ReadInt32();
                for (int i = 0; i < xRefItems; i++)
                {
                    retIds.Add(new AssetID(strTable[br.ReadInt32()], br.ReadInt64()));
                }
                return retIds;
            }
            else
            {
                return null;
            }
        }
    }
}
