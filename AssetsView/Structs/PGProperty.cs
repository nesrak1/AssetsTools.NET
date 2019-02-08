using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsView.Structs
{
    public class PGProperty
    {
        public string name;
        public object value;
        public PGProperty(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
