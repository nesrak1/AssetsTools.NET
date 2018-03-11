// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static UABE.NET.Winforms.AssetViewer;

namespace UABE.NET.Winforms
{
    public partial class AssetSearch : Form
    {
        List<AssetDetails> details;
        AssetsFile file;
        public AssetSearch(List<AssetDetails> details, AssetsFile file)
        {
            InitializeComponent();
            this.details = details;
            this.file = file;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            searchResults.Items.Clear();
            List<string> ids = GetAssetListFromInput(searchTextBox.Text);
            foreach (string id in ids)
            {
                searchResults.Items.Add(id);
            }
        }

        public List<string> GetAssetListFromInput(string find)
        {
            BinaryReader searcher = new BinaryReader(file.readerPar);
            byte[] findData = Encoding.UTF8.GetBytes(find);
            List<long> findPositions = BinarySearch(searcher, findData);
            List<string> assetNames = new List<string>();
            foreach (long findPosition in findPositions)
            {
                for (int i = 0; i < details.Count - 1; i++)
                {
                    if (details[i].position <= (ulong)findPosition &&
                        (ulong)findPosition < details[i + 1].position)
                    {
                        assetNames.Add(details[i].typeName + " " + details[i].name + " (" + details[i].pathID.ToString() + ")");
                        break;
                    }
                }
            }
            return assetNames;
        }

        public List<string> GetAssetListFromInput(byte[] findData)
        {
            BinaryReader searcher = new BinaryReader(file.readerPar);
            List<long> findPositions = BinarySearch(searcher, findData);
            List<string> assetNames = new List<string>();
            foreach (long findPosition in findPositions)
            {
                for (int i = 0; i < details.Count - 1; i++)
                {
                    if (details[i].position <= (ulong)findPosition &&
                        (ulong)findPosition < details[i + 1].position)
                    {
                        assetNames.Add(details[i].typeName + " " + details[i].name + " (" + details[i].pathID.ToString() + ")");
                        break;
                    }
                }
            }
            return assetNames;
        }

        public List<long> BinarySearch(BinaryReader reader, byte[] input)
        {
            //todo, what happens if text is inbetween buffers?
            long prevPos = reader.BaseStream.Position;
            reader.BaseStream.Position = 0;
            byte[] buffer = new byte[0xFFFF];
            List<long> matches = new List<long>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                long bytesLeft = reader.BaseStream.Length - reader.BaseStream.Position;
                buffer = reader.ReadBytes((int)Math.Min(bytesLeft, 0xFFFFL));
                int search = SearchBytes(buffer, input);
                if (search != -1)
                {
                    long findPosition = reader.BaseStream.Position - buffer.Length + search;
                    matches.Add(findPosition);
                }
            }
            reader.BaseStream.Position = prevPos;
            return matches;
        }

        //https://stackoverflow.com/a/26880541
        public int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (int i = 0; i <= limit; i++)
            {
                int k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        private void searchPosButton_Click(object sender, EventArgs e)
        {
            ulong pos = ulong.Parse(searchTextBox.Text);
            for (int i = 0; i < details.Count - 1; i++)
            {
                if (details[i].position <= pos &&
                    pos < details[i + 1].position)
                {
                    searchResults.Items.Add(details[i].typeName + " " + details[i].name + " (" + details[i].pathID.ToString() + ")");
                    break;
                }
            }
        }

        private void SearchHexButton_Click(object sender, EventArgs e)
        {
            searchResults.Items.Clear();
            byte[] input = StringToByteArrayFast(searchTextBox.Text);
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                Array.Reverse(input, 0, input.Length);
            }
            List<string> ids = GetAssetListFromInput(input);
            foreach (string id in ids)
            {
                searchResults.Items.Add(id);
            }
        }

        //https://stackoverflow.com/a/9995303
        public static byte[] StringToByteArrayFast(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }
    }
}
