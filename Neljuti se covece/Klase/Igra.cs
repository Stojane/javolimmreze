using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece.Klase
{
    public class Igra
    {
        public int trenutniIgracNaPotezu { set; get; } = 1;
        public List<Korisnik> pozicijeFigura { set; get; }
        public int zavrsetakIgre { set; get; }

        public Igra() 
        {
            this.trenutniIgracNaPotezu = 1;
            this.pozicijeFigura = new List<Korisnik>();
            this.zavrsetakIgre = 0;
        }
    }
}
