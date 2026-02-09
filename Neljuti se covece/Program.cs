using Neljuti_se_covece.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Igra igra = new Igra();
            Random random = new Random();
            Izvestaj izvestaj = new Izvestaj(igra);
            /*
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("Unesite ime korisnika");
                string imeKorisnika = Console.ReadLine();
                igra.pozicijeFigura.Add(new Korisnik(imeKorisnika));
            }
            */
            while (igra.zavrsetakIgre == 0)
            {
                izvestaj.PrikaziIzvestaj();
                Korisnik trenutniIgrac = igra.pozicijeFigura[igra.trenutniIgracNaPotezu - 1];
                Console.WriteLine($"Na potezu je igrac: {trenutniIgrac.ime}");
                int brojBaceneKockice = (int)Math.Ceiling((double)random.Next(1, 7));
                if (brojBaceneKockice == 6) 
                {
                    igra.trenutniIgracNaPotezu -= 1;
                }
                Console.WriteLine($"Broj kockice koju ste dobili: {brojBaceneKockice}");
                int brojNeaktivnih = 0;
                if (brojBaceneKockice!=6) {
                    for (int i = 0; i < 4; i++)
                    {
                        if (trenutniIgrac.trenutnaPozicijaFigura[i].status == "neaktivna")
                        {
                            brojNeaktivnih++;
                        }
                    }
                    if (brojNeaktivnih == 4)
                    {
                        Console.WriteLine("Morate baciti sesticu da biste aktivirali figuru! Preskacete potez.");
                        igra.trenutniIgracNaPotezu = (igra.trenutniIgracNaPotezu % 4) + 1;
                        continue;
                    }
                }
                for (int j = 1; j < 5; j++)
                {
                    for (int i = 0; i < 4; i++)
                    {

                        igra.pozicijeFigura[i].trenutnaPozicijaFigura.ForEach(figura =>
                        {
                            if (figura.pozicija == (trenutniIgrac.trenutnaPozicijaFigura[j - 1].pozicija - (i - trenutniIgrac.id)) && figura.status == "aktivna" && i != trenutniIgrac.id && figura.pozicija > 3)
                            {
                                Console.WriteLine($"Igrac {trenutniIgrac.ime} moze da izbaci figuru igraca {igra.pozicijeFigura[i].ime} u koliko pomeri figuru {j}!");


                            }
                        });
                    }
                }
                int brojFigure;
                while (true) 
                {
                    Console.WriteLine("Unesite broj figure nad kojom zelite da izvrsite akciju");
                     brojFigure = int.Parse(Console.ReadLine());
                    if (brojFigure < 1 || brojFigure > 4) 
                    {
                        Console.WriteLine("Neispravan broj figure! Pokusajte ponovo.");
                    }
                    else 
                    {
                        break;
                    }
                }
                Console.WriteLine("Unesite akciju koju zelite da izvrsite (aktivacija, pomeri)");
                string akcija = Console.ReadLine();
                Potez potez = new Potez
                {
                    igra = igra,
                    igrac = trenutniIgrac,
                    brojBaceneKockice = brojBaceneKockice,
                    brojFigure = brojFigure,
                    akcija = akcija
                };
                igra.trenutniIgracNaPotezu = (igra.trenutniIgracNaPotezu % 4) + 1;
               // potez.IzvrsiPotez();
            }
        }
    }
}
