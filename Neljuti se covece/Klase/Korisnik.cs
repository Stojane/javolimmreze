using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece.Klase
{
    public class Korisnik
    {
        public static int brojacId = 0;
        public int id { get; set; }
        public string ime { get; set; }
        public List<Figura> trenutnaPozicijaFigura { get; set; }
        public int start { get; set; }
        public int cilj { get; set; }

        public IPEndPoint UdpEndPoint { get; set; }
        public IPEndPoint TcpEndPoint { get; set; }
        public int TcpPort { get; set; }

        public Korisnik(string ime)
        {
            brojacId++;
            this.id = brojacId;
            this.ime = ime;
            this.trenutnaPozicijaFigura = new List<Figura>();
            this.start = 0;
            this.cilj = 0;
            for (int i = 0; i < 4; i++) 
            {
                trenutnaPozicijaFigura.Add(new Figura());
            }
        }
        public Korisnik()
        {
            this.id = -1;
            this.ime = "ZauzimacPozicije";
            this.trenutnaPozicijaFigura = new List<Figura>();
            this.start = 0;
            this.cilj = 0;
        }
    }
}
