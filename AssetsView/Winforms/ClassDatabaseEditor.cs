using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class ClassDatabaseEditor : Form
    {
        private ClassDatabaseFile classFile;
        public ClassDatabaseEditor(ClassDatabaseFile classFile)
        {
            this.classFile = classFile;
            InitializeComponent();
        }

        private void ClassDatabaseEditor_Load(object sender, EventArgs e)
        {
            foreach (ClassDatabaseType type in classFile.Classes)
            {
                typeListBox.Items.Add(classFile.GetString(type.Name));
            }
        }

        string lastSearchedText = "";
        int lastSearchedIndex = -1;
        private void SearchText()
        {
            string searchText = searchBox.Text;
            if (searchText != lastSearchedText)
            {
                lastSearchedText = searchText;
                lastSearchedIndex = -1;
            }
            for (int i = lastSearchedIndex + 1; i < typeListBox.Items.Count; i++)
            {
                string itemText = typeListBox.Items[i] as string;
                if (itemText.ToLower().Contains(searchText.ToLower()))
                {
                    typeListBox.SelectedIndex = i;
                    lastSearchedIndex = i;
                    return;
                }
            }
            lastSearchedIndex = -1;
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            SearchText();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchText();
                e.Handled = true;
            }
        }
    }
}
