using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece.Klase
{
    public class Figura
    {
        public string status { get; set; } 
        public int pozicija { get; set; }
        public int udaljenostDoCilja { get; set; }

        public Figura()
        {
            this.status = "neaktivna";
            this.pozicija = -1;
            this.udaljenostDoCilja = 44;
        }

    }
}
