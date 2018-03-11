// ASSETSTOOLS.NET v1

using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static UABE.NET.Winforms.AssetViewer;

namespace UABE.NET.Winforms
{
    public partial class AssetNameSearch : Form
    {
        List<AssetDetails> details;
        AssetsFile file;
        public AssetNameSearch(List<AssetDetails> details, AssetsFile file)
        {
            InitializeComponent();
            this.details = details;
            this.file = file;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            searchResults.Items.Clear();
            foreach (AssetDetails detail in details)
            {
                if (Regex.IsMatch(detail.name, WildcardToRegex(searchTextBox.Text),
                    ((caseSensitive.Checked) ? RegexOptions.None : RegexOptions.IgnoreCase))) { //todo, too ugly for my taste
                    searchResults.Items.Add(detail.typeName + " " + detail.name + " (" + detail.pathID.ToString() + ")");
                }
            }
        }

        private string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }
    }
}
