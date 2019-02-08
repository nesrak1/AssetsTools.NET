using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Util
{
    public class ImprovedListView : ListView
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr hwndForm;
            public uint idFrom;
            public uint code;
        }

        private const uint NM_CUSTOMDRAW = unchecked((uint)-12);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x204E)
            {
                NMHDR hdr = (NMHDR)m.GetLParam(typeof(NMHDR));
                if (hdr.code == NM_CUSTOMDRAW)
                {
                    m.Result = (IntPtr)0;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
