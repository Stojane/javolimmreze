using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece.Klase
{
    public class Izvestaj
    {
        public Igra igra { get; set; }

        public Izvestaj(Igra igra)
        {
            this.igra = igra;
        }
        
        public string PrikaziIzvestaj()
        {
            string izvestajTekst = "";
           izvestajTekst+= "----- IZVESTAJ O IGRI -----\n";
            foreach (var igrac in igra.pozicijeFigura)
            {
                izvestajTekst += $"Igrac: {igrac.ime}\n";
                for (int i = 0; i < igrac.trenutnaPozicijaFigura.Count; i++)
                {
                    var figura = igrac.trenutnaPozicijaFigura[i];
                    izvestajTekst += $"  Figura {i + 1}: Status = {figura.status}, Pozicija = {figura.pozicija}, Udaljenost do cilja = {figura.udaljenostDoCilja}\n";
                }
            }
            izvestajTekst += "---------------------------\n";
            return izvestajTekst;
        }
    }
}