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
    public partial class LoadingBar : Form
    {
        public ProgressBar pb;
        public LoadingBar()
        {
            InitializeComponent();
            pb = progressBar;
        }
    }
}
