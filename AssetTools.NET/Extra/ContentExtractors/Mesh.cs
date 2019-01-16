/* 
   ------------------------
   UNITY CONTENT EXTRACTORS
   ------------------------
*/

using System;
using System.Collections.Generic;

namespace AssetsTools.NET.Extra
{
    public class Mesh
    {
        public string name;
        private List<SubMesh> subMeshes = new List<SubMesh>();
        public void ReadMesh(AssetTypeInstance ati)
        {
            AssetTypeValueField baseField = ati.GetBaseField();
            name = baseField.Get("m_Name").GetValue().AsString();
            AssetTypeValueField channelArray = baseField.Get("m_VertexData")
                                                        .Get("m_Channels")
                                                        .Get("Array");
            ChannelInfo[] channelInfos = new ChannelInfo[channelArray.GetValue().AsArray().size];
            for (uint i = 0; i < channelInfos.Length; i++)
            {
                AssetTypeValueField channelInfo = channelArray.Get(i);
                channelInfos[i].stream    = (byte)channelInfo.Get("stream").GetValue().AsInt();
                channelInfos[i].offset    = (byte)channelInfo.Get("offset").GetValue().AsInt();
                channelInfos[i].format    = (byte)channelInfo.Get("format").GetValue().AsInt();
                channelInfos[i].dimension = (byte)channelInfo.Get("dimension").GetValue().AsInt();
            }
            AssetTypeValueField subMeshArray = baseField.Get("m_SubMeshes")
                                                        .Get("Array");
            SubMeshInfo[] subMeshInfos = new SubMeshInfo[subMeshArray.GetValue().AsArray().size];
            for (uint i = 0; i < subMeshInfos.Length; i++)
            {
                AssetTypeValueField subMeshInfo = subMeshArray.Get(i);
                subMeshInfos[i].firstByte   = (uint)subMeshInfo.Get("firstByte").GetValue().AsInt();
                subMeshInfos[i].indexCount  = (uint)subMeshInfo.Get("indexCount").GetValue().AsInt();
                subMeshInfos[i].topology    =       subMeshInfo.Get("topology").GetValue().AsInt();
                subMeshInfos[i].firstVertex = (uint)subMeshInfo.Get("firstVertex").GetValue().AsInt();
                subMeshInfos[i].vertexCount = (uint)subMeshInfo.Get("vertexCount").GetValue().AsInt();
            }
            SubMesh[] subMeshes = new SubMesh[subMeshInfos.Length];
            int r = 0;
            byte[] vertData = baseField.Get("m_VertexData").Get("m_DataSize").GetValue().AsByteArray().data; //asbytearray wont work in at.net yet rip :(
            for (uint i = 0; i < subMeshInfos.Length; i++)
            {
                for (uint j = 0; j < subMeshInfos[i].vertexCount; j++)
                {
                    for (uint k = 0; k < channelInfos.Length; k++)
                    {
                        for (uint l = 0; l < channelInfos[k].dimension; l++)
                        {
                            switch (k)
                            {
                                case 0:
                                    ReadValue(vertData, r, channelInfos[k].format);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        float ReadValue(byte[] data, int position, int type)
        {
            byte[] bytes;
            switch (type)
            {
                case 0:
                    bytes = new byte[] { data[position], data[position+1], data[position+2], data[position+3] };
                    return BitConverter.ToSingle(bytes, 0);
                case 1:
                    bytes = new byte[] { data[position], data[position+1] };
                    return 0f;//return Half.Half.HalfToSingle(BitConverter.ToUInt16(bytes, 0));
                case 2:
                    return data[position] / 255.0f;
                default:
                    return 0f;
            }
        }

        private class SubMesh
        {
            public List<Vector3> verticies = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector3> uvs = new List<Vector3>();
            public AssetPPtr texture;
        }

        private class SubMeshInfo
        {
            public uint firstByte;
            public uint indexCount;
            public int topology;
            public uint firstVertex;
            public uint vertexCount;
        }

        private struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        private struct ChannelInfo
        {
            public byte stream;
            public byte offset;
            public byte format;
            public byte dimension;
        }
    }
}
